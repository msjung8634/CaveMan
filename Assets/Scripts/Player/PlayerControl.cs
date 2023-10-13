using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FSM;
using Dialogue;
using System;

namespace Player
{
    [RequireComponent(typeof(PlayerStateMachine))]
    [RequireComponent(typeof(PlayerAnimation))]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CapsuleCollider2D))]
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(PlayerConversant))]
    [RequireComponent(typeof(SpriteRenderer))]
    public class PlayerControl : MonoBehaviour
    {
        [Header("Velocity Limit")]
        [SerializeField]
        [Range(5f, 100f)]
        private float velocityLimitX = 10f;
        [SerializeField]
        [Range(5f, 100f)]
        private float velocityLimitY = 10f;

        [Header("Run")]
        [SerializeField]
        private float moveSpeed = 10f;
        [SerializeField]
        [Range(0f, 4f)]
        private float stopDif = 1f;
        [SerializeField]
        private float moveAcceleration = 2f;
        [SerializeField]
        private float moveDecceleration = 6f;
        [SerializeField]
        [Range(2f, 4f)]
        private float velPower = 2f;
        [Range(0f, 0.2f)]
        private float frictionAmount = 0.2f;

        [Header("Jump")]
        [SerializeField]
        [Range(0.1f, 2f)]
        private float jumpDelay = .5f;
        private float jumpCoolDown = 0f;
        [SerializeField]
        [Range(0f, 500f)]
        private float jumpForce = 50f;
        [SerializeField]
        [Range(0f, 1f)]
        private float coyoteTime = .5f;
		[SerializeField]
		private float jumpCutMultiplier = .5f;
        [SerializeField]
        private bool isWallJump = false;
        
        [Header("Fall")]
        [SerializeField]
        [Range(1f, 5f)]
        private float gravityScale = 1f;
        [SerializeField]
        [Range(1f, 2f)]
        private float fallGravityMultiplier = 1f;
        [SerializeField]
        private bool isWallSlide = false;
        [SerializeField]
        [Range(0f, 1f)]
        private float wallSlideGravityScale = 0.3f;

        [Header("Grappling")]
        [SerializeField]
        private Image grappleIndicator;
        [SerializeField]
        private Image grappleIndicatorBG;
        [SerializeField]
        [Range(0f, 1f)]
        private float grappleDelay = .1f;
        private float grappleCoolDown = 0f;
        [SerializeField]
        private float ropeReelSpeed = 5f;
        [SerializeField]
        private float grappleAcceleration = 1f;
        [SerializeField]
        private float grappleDeceleration = 2f;
        public HookablePlatform LastHookTarget { get; set; } = null;

        [Header("Attack")]
        [SerializeField]
        private GameObject leftAttackHitBox;
        [SerializeField]
        private GameObject rightAttackHitBox;
        // Indicator가 Player Control을 갖고있는게 낫지 않을까?
        [SerializeField]
        private Image attackIndicator;
        [SerializeField]
        private Image attackIndicatorBG;
        [SerializeField]
        private float attackDelay = .5f;
        private float attackCoolDown = 0f;
        [SerializeField]
        private float attackDetectionDuration = .3f;

        [Header("Dodge")]
        // Indicator가 Player Control을 갖고있는게 낫지 않을까?
        [SerializeField]
        private Image dodgeIndicator;
        [SerializeField]
        private Image dodgeIndicatorBG;
        [SerializeField]
        private float dodgeDelay = 2f;
        private float dodgeCoolDown = 0f;
        [SerializeField]
        private float dodgeForce = 40f;
        [SerializeField]
        private float dodgeDuration = .5f;

        [Header("KnockBack")]
        [SerializeField]
        private GameObject playerHitBox;
        [SerializeField]
        private float knockBackForce = 60f;
        [SerializeField]
        private float knockBackDuration = .2f;
        [SerializeField]
        private float knockBackInvincibleDuration = 1f;

        [System.Serializable]
        struct CursorMapping
        {
            public CursorType type;
            public Texture2D texture;
            public Vector2 hotspot;
        }

        [Header("CursorMapping")]
        [SerializeField]
        CursorMapping[] cursorMappings = null;

        public event Action OnAttack;
        public event Action OnDodge;
        public event Action OnGrapple;
        public event Action OnHit;

        public event Action OnAttackFinish;
        public event Action OnDodgeFinish;
        public event Action OnGrappleFinish;
        public event Action OnHitFinish;

        private PlayerStateMachine stateMachine;
        private PlayerAnimation animation;
        private Rigidbody2D rigidbody2D;
        private CapsuleCollider2D capsuleCollider2D;
        private Health health;
        private PlayerConversant conversant;
        private SpriteRenderer spriteRenderer;

        private void Awake()
        {
            TryGetComponent(out stateMachine);
            TryGetComponent(out animation);
            TryGetComponent(out rigidbody2D);
            TryGetComponent(out capsuleCollider2D);
            TryGetComponent(out health);
            TryGetComponent(out conversant);
            TryGetComponent(out spriteRenderer);
        }

        private void FixedUpdate()
        {
            attackCoolDown = Mathf.Max(0, attackCoolDown - Time.fixedDeltaTime);
            dodgeCoolDown = Mathf.Max(0, dodgeCoolDown - Time.fixedDeltaTime);
            jumpCoolDown = Mathf.Max(0, jumpCoolDown - Time.fixedDeltaTime);
            grappleCoolDown = Mathf.Max(0, grappleCoolDown - Time.fixedDeltaTime);

            if (animation.IsDodge && dodgeCoolDown == 0 && stateMachine.ControlState == ControlState.Controllable)
                StartCoroutine(Dodge(dodgeDuration));

            Run();
            Jump();
            Grapple();
            
            Fall();
            if (stateMachine.ControlState != ControlState.Grappling)
            {
                GroundFriction();
                AirFriction();
            }

            LimitVelocity();
        }

		private void LimitVelocity()
		{
            if (stateMachine.ControlState == ControlState.Controllable)
            {
                float velocityX = Mathf.Min(Mathf.Abs(rigidbody2D.velocity.x), velocityLimitX) * Mathf.Sign(rigidbody2D.velocity.x);

                float velocityY = rigidbody2D.velocity.y;
				if (!stateMachine.IsGrounded)
				{
                    // 상승 할때
					if (rigidbody2D.velocity.y > 0)
					{
                        velocityY = Mathf.Min(Mathf.Abs(rigidbody2D.velocity.y), velocityLimitY) * Mathf.Sign(rigidbody2D.velocity.y);
                    }
                    // 하강 할때
                    else
                    {
                        velocityY = Mathf.Min(Mathf.Abs(rigidbody2D.velocity.y), velocityLimitY * 2) * Mathf.Sign(rigidbody2D.velocity.y);
                    }
                }
                rigidbody2D.velocity = new Vector2(velocityX, velocityY);
            }
		}

		private IEnumerator Dodge(float duration)
        {
            OnDodge();

            dodgeCoolDown = dodgeDelay;

            TogglePlayerHitbox(false);
            // 감속계수 제거
            float decceleration = moveDecceleration;
            moveDecceleration = 0;
            rigidbody2D.AddForce(dodgeForce * -animation.LastDirection, ForceMode2D.Impulse);

            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.fixedDeltaTime;

                // 감속계수 서서히 복원
                moveDecceleration = decceleration / (duration / elapsedTime);
                yield return new WaitForFixedUpdate();
            }

            // 감속계수 복원
            moveDecceleration = decceleration;
            TogglePlayerHitbox(true);

            OnDodgeFinish();
        }

		private void TogglePlayerHitbox(bool isHitable)
		{
            // Toggle hitbox
            playerHitBox.SetActive(isHitable);

            // Change HitState
            HitState hitState = isHitable ? HitState.Hittable : HitState.Unhittable;
            stateMachine.SetHitState(hitState);

            // Change ControlState
            ControlState controlState = isHitable ? ControlState.Controllable : ControlState.Uncontrollable;
            stateMachine.SetControlState(controlState);
		}

		private void Fall()
        {
            // 지면에 있거나 그래플링 중인 경우
            if (stateMachine.IsGrounded
                || stateMachine.ControlState == ControlState.Grappling)
            {
                rigidbody2D.gravityScale = gravityScale;
                return;
            }

            // 허공에 있는 경우
            if (!stateMachine.IsGrounded)
            {
                // 벽을 붙잡지 않은 경우
                rigidbody2D.gravityScale *= gravityScale * fallGravityMultiplier;

                // 벽을 붙잡은 채 추락하는 경우
                if (CheckWallSliding() && isWallSlide)
                {
                    rigidbody2D.gravityScale = gravityScale * wallSlideGravityScale;
                }
            }
        }

        private bool CheckWallSliding()
		{
			return ((CheckWallHolding())
					&& rigidbody2D.velocity.y < 0);
		}

		private bool CheckWallHolding()
		{
			return (stateMachine.IsAgainstLeftWall && inputHorizontal < 0)
				    || (stateMachine.IsAgainstRightWall && inputHorizontal > 0);
		}

		private void GroundFriction()
        {
            // 입력이 없고, 속도가 충분히 낮으면 정지
            if (stateMachine.IsGrounded
                && inputHorizontal == 0
                && Mathf.Abs(rigidbody2D.velocity.x) < stopDif)
                rigidbody2D.velocity = new Vector2(0, rigidbody2D.velocity.y);
        }

        private void AirFriction()
        {
            // 공중에서 키입력 안하면 좌우이동 정지
            if (!stateMachine.IsGrounded && Mathf.Abs(inputHorizontal) < 0.01f)
            {
                // 현재속력, 마찰력 중 작은 크기
                float amount = Mathf.Min(Mathf.Abs(rigidbody2D.velocity.x), Mathf.Abs(frictionAmount));
                // 이동방향의 반대 방향
                amount *= Mathf.Sign(rigidbody2D.velocity.x) * -1;
                // 외력 적용
                rigidbody2D.AddForce(Vector2.right * amount, ForceMode2D.Impulse);
            }
        }

        #region Animation Event
        private void Attack()
        {
            if (animation.LastDirection == Vector2.left)
            {
                StartCoroutine(ToggleAttackHitBox(leftAttackHitBox, attackDetectionDuration));
            }
            else if (animation.LastDirection == Vector2.right)
            {
                StartCoroutine(ToggleAttackHitBox(rightAttackHitBox, attackDetectionDuration));
            }
        }
        private IEnumerator ToggleAttackHitBox(GameObject hitBox, float duration)
        {
            OnAttack();

            attackCoolDown = attackDelay;

            stateMachine.SetControlState(ControlState.Uncontrollable);
            hitBox.SetActive(true);

            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }

            hitBox.SetActive(false);
            stateMachine.SetControlState(ControlState.Controllable);

            OnAttackFinish();
        }
        #endregion

        private void Run()
        {
            // 제어 불가이면 inputHorizontal은 0으로 간주
            if (stateMachine.ControlState == ControlState.Uncontrollable)
                inputHorizontal = 0;

            // 목표 속도
            float targetVelocity = inputHorizontal * moveSpeed;
            // 속도 차이 (이동해야할 속도 계산)
            float velocityDif = targetVelocity - rigidbody2D.velocity.x;
            // 가속 비율 조정
            float accelRate = (Mathf.Abs(targetVelocity) > 0.01f) ? moveAcceleration : moveDecceleration;
            if (stateMachine.ControlState == ControlState.Grappling)
            {
                // Grappling이면 그에 맞게 조정
                accelRate = (Mathf.Abs(targetVelocity) > 0.01f) ? grappleAcceleration : grappleDeceleration;
            }

            // 수평으로 외력 적용
            float movement = Mathf.Pow(Mathf.Abs(velocityDif) * accelRate, velPower) * Mathf.Sign(velocityDif);
            rigidbody2D.AddForce(movement * Vector2.right);
        }

        private void Jump()
		{
			// 제어 불가이면 Jump 불가
			if (stateMachine.ControlState == ControlState.Uncontrollable
				|| stateMachine.ControlState == ControlState.Grappling)
				return;

			// Jump
			if (inputJump > 0 && jumpCoolDown == 0 && CheckJumpCondition())
			{
				jumpCoolDown = jumpDelay;
				rigidbody2D.velocity = new Vector2(rigidbody2D.velocity.x, 0);
				rigidbody2D.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            }

            JumpCut();
        }

        private void JumpCut()
        {
            if (rigidbody2D.velocity.y > 0 && isJumpCut)
            {
                Debug.Log("Jump Cut");
                rigidbody2D.AddForce(Vector2.down * rigidbody2D.velocity.y * (1 - jumpCutMultiplier), ForceMode2D.Impulse);
            }
        }

        private bool CheckJumpCondition()
		{
            // 지면에 있는 경우
			if (stateMachine.IsGrounded)
			{
                return true;
			}
            // CoyoteTime인 경우
            else if (CheckCoyoteTime())
			{
                return true;
			}
            // 벽을 붙잡고 있는 경우
			else if (CheckWallHolding() && isWallJump)
			{
                return true;
			}

            return false;
		}

		private bool CheckCoyoteTime()
        {
            return (stateMachine.LastGroundTime > Time.fixedDeltaTime
                    && stateMachine.LastGroundTime < Time.fixedDeltaTime + coyoteTime);
        }

        private void Grapple()
        {
            // 불가피하게 범위를 벗어난 경우 강제로 Grapple Rope를 끊는다
            if (stateMachine.NearestHookablePlatform == null)
            {
                if (LastHookTarget != null)
				{
                    LastHookTarget.Unhook(this);
                }

                return;
            }

            // 대화 중에는 Grappling을 하지 않는다
            if (stateMachine.IsTalking)
                return;

            // Hook
            if (stateMachine.NearestHookablePlatform.TryGetComponent(out HookablePlatform platform)
                && inputHook > 0 && grappleCoolDown == 0)
            {
                OnGrapple();
                platform.Hook(this, platform);
            }
            // Unhook
            else
            {
                platform.Unhook(this);
                OnGrappleFinish();
            }

            // Reel Rope
            if (TryGetComponent(out DistanceJoint2D joint))
            {
                joint.distance += inputReelRope * ropeReelSpeed * Time.fixedDeltaTime;
            }
        }

        public void ResetGrappleCoolDown()
		{
            grappleCoolDown = grappleDelay;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            // 피격불가 상태에서는 KnockBack 하지 않음
            if (collision.gameObject.CompareTag("Enemy")
                && stateMachine.HitState == HitState.Hittable)
			{
                Vector2 direction;
                // 적 왼쪽에서 부딪힌 경우
                if (transform.position.x < collision.transform.position.x)
                    direction = Vector2.left + Vector2.up * .3f;
                else
                    direction = Vector2.right + Vector2.up * .3f;

                StartCoroutine(KnockBack(direction));
            }
        }

        private IEnumerator KnockBack(Vector2 direction)
        {
            StartCoroutine(TogglePlayerHitBox(knockBackInvincibleDuration));
            StartCoroutine(Blink(knockBackInvincibleDuration, Color.red));

            OnHit();

            // 데미지 계산
            health.GetDamage(10);

            // 감속계수 제거
            float decceleration = moveDecceleration;
            moveDecceleration = 0;
            // 속도 제거
            rigidbody2D.velocity = Vector2.zero;
            // 외력 적용
            rigidbody2D.AddForce(knockBackForce * direction, ForceMode2D.Impulse);
            
            float elapsedTime = 0f;
			while (elapsedTime < knockBackDuration)
			{
                elapsedTime += Time.deltaTime;
                yield return null;
			}

            // 감속계수 복원
            moveDecceleration = decceleration;

            OnHitFinish();
        }

        private IEnumerator Blink(float duration, Color color)
        {
            float elapsedTime = 0f;
            int frameCount = 0;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                spriteRenderer.color = frameCount % 20 < 10 ? color : Color.white;
                frameCount++;
                yield return null;
            }
            spriteRenderer.color = Color.white;
        }

        private IEnumerator TogglePlayerHitBox(float duration)
        {
            playerHitBox.SetActive(false);
            stateMachine.SetHitState(HitState.Unhittable);

            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            playerHitBox.SetActive(true);
            stateMachine.SetHitState(HitState.Hittable);
        }

        private void Update()
        {
            grappleIndicator.fillAmount = (grappleDelay - grappleCoolDown) / grappleDelay;
            grappleIndicatorBG.fillAmount = (grappleDelay - grappleCoolDown) / grappleDelay;
            attackIndicator.fillAmount = (attackDelay - attackCoolDown) / attackDelay;
            attackIndicatorBG.fillAmount = (attackDelay - attackCoolDown) / attackDelay;
            dodgeIndicator.fillAmount = (dodgeDelay - dodgeCoolDown) / dodgeDelay;
            dodgeIndicatorBG.fillAmount = (dodgeDelay - dodgeCoolDown) / dodgeDelay;

            if (health.CurrentHealth <= 0)
            {
                animation.IsDead = true;
                rigidbody2D.constraints = RigidbodyConstraints2D.FreezeAll;
                capsuleCollider2D.enabled = false;
                TogglePlayerHitbox(false);
            }

            InteractWithCursor();
            //if (InteractWithCursor())
            //    return;

            ProcessCombatInput();
            ProcessMoveInput();
            ProcessJumpInput();
            ProcessGrappleInput();

            CheckDodgeToWall();
        }

        private void CheckDodgeToWall()
		{
            // 제어불가 상태(Dodge / KnockBack)에서 벽에 닿은 경우 즉시 정지
            if (stateMachine.ControlState == ControlState.Uncontrollable
                && (stateMachine.IsAgainstLeftWall || stateMachine.IsAgainstRightWall))
            {
                rigidbody2D.velocity = new Vector2(0, rigidbody2D.velocity.y);
            }
        }
        
        float inputDodge;
        float inputAttack;
        private void ProcessCombatInput()
		{
            inputDodge = 0;
            inputAttack = 0;

            if (stateMachine.ControlState == ControlState.Uncontrollable
                || stateMachine.ControlState == ControlState.Grappling)
                return;

            inputDodge = Input.GetAxisRaw("Dodge");
            inputAttack = Input.GetAxisRaw("Attack");

            animation.IsDodge = false;
            if (stateMachine.IsGrounded
                && inputDodge > 0
                && dodgeCoolDown == 0)
            {
                animation.IsDodge = true;
            }

            animation.IsAttack = false;
            if (inputAttack > 0
                && attackCoolDown == 0)
            {
                animation.IsAttack = true;
            }
        }

        float inputHorizontal;
        
        private void ProcessMoveInput()
        {
            inputHorizontal = 0;

            if (stateMachine.ControlState == ControlState.Uncontrollable)
                return;
            
            inputHorizontal = Input.GetAxisRaw("Horizontal");
            inputHorizontal = Mathf.Abs(inputHorizontal) > 0 ? Mathf.Sign(inputHorizontal) : 0;

            // Animation을 위해 마지막 입력 방향 저장
            Vector2 horizontalDirection = Mathf.Sign(inputHorizontal) * Vector2.right;
            if (inputHorizontal != 0 && horizontalDirection != animation.LastDirection)
                animation.LastDirection = horizontalDirection;
			{
                animation.LastInput = new Vector2(inputHorizontal, 0);
            }
        }

        float inputJump;
        bool isJumpCut;
        private void ProcessJumpInput()
		{
			inputJump = 0;

			if (stateMachine.ControlState == ControlState.Uncontrollable)
				return;

			if (stateMachine.ControlState != ControlState.Grappling)
			{
				inputJump = Input.GetAxisRaw("Jump");
			}

			inputJump = Mathf.Abs(inputJump) > 0 ? Mathf.Sign(inputJump) : 0;

            isJumpCut = false;
            if (Input.GetButtonUp("Jump"))
            {
                isJumpCut = true;
            }
        }

		

		float inputHook;
        float inputReelRope;
        private void ProcessGrappleInput()
        {
            inputHook = 0;
            inputReelRope = 0;

            inputHook = Input.GetAxisRaw("Hook");
            inputReelRope = -Input.GetAxisRaw("ReelRope");
        }

		#region Cursor Interaction
		private bool InteractWithCursor()
        {
            if (RaycastInteractableTarget(out IRaycastable target))
            {
                if (target == null)
                    return false;

                if (Input.GetMouseButtonDown(0))
                {
                    target.HandleRaycast(this);
                }
                SetCursor(target.GetCursorType());

                return true;
            }
            else if (stateMachine.NearestHookablePlatform != null
                     && !stateMachine.IsTalking)
            {
                SetCursor(stateMachine.NearestHookablePlatform.GetCursorType());
                return false;
            }

            //Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            SetCursor(CursorType.Default);
            return false;
        }

        // IRaycastable을 상속받은 Script를 가진 gameObject 검출
        private bool RaycastInteractableTarget(out IRaycastable target)
        {
            target = null;

            bool hasHit = Physics.Raycast(GetMousRay(), out RaycastHit hit);
            if (!hasHit)
                return false;

            hit.transform.gameObject.TryGetComponent(out IRaycastable conversant);
            target = conversant;

            return true;
        }

        private void SetCursor(CursorType type)
        {
            CursorMapping mapping = GetCursorMapping(type);
            Cursor.SetCursor(mapping.texture, mapping.hotspot, CursorMode.Auto);
        }

        private CursorMapping GetCursorMapping(CursorType type)
        {
            foreach (CursorMapping mapping in cursorMappings)
            {
                if (mapping.type == type)
                {
                    return mapping;
                }
            }
            return cursorMappings[0];
        }

        public static Ray GetMousRay()
        {
            float distance = (Mathf.Abs(Camera.main.transform.position.z) + 1);
            return new Ray(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector3.forward * distance);
        }
		#endregion
	}
}
