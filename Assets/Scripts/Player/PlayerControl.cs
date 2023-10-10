using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FSM;
using Dialogue;

namespace Player
{
    [RequireComponent(typeof(PlayerStateMachine))]
    [RequireComponent(typeof(PlayerAnimation))]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(PlayerConversant))]
    public class PlayerControl : MonoBehaviour
    {
        [Header("Move")]
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
        private float jumpDelay = .5f;
        [SerializeField]
        private float jumpForce = 50f;
        [SerializeField]
        private float coyoteTime = .5f;
        [SerializeField]
        private float jumpCutMultiplier = .5f;

        private float jumpCoolDown = 0f;
        [Header("Fall")]
        [SerializeField]
        private float gravityScale = 1f;
        [SerializeField]
        private float fallGravityMultiplier = 1f;

        [Header("Grappling")]
        [SerializeField]
        private Image grappleIndicator;
        [SerializeField]
        public HookablePlatform LastHookTarget { get; set; } = null;
        [SerializeField]
        private float ropeReelSpeed = 5f;
        [SerializeField]
        private float grappleAcceleration = 1f;
        [SerializeField]
        private float grappleDeceleration = 2f;
        [SerializeField]
        private float grappleDelay = 1f;
        private float grappleCoolDown = 0f;

        [Header("Combat")]
        // Indicator가 Player Control을 갖고있는게 낫지 않을까?
        [SerializeField]
        private Image attackIndicator;
        [SerializeField]
        private float attackDelay = .5f;
        private float attackCoolDown = 0f;
        [SerializeField]
        private float attackDetectionDuration = .3f;

        // Indicator가 Player Control을 갖고있는게 낫지 않을까?
        [SerializeField]
        private Image dodgeIndicator;
        [SerializeField]
        private float dodgeDelay = 2f;
        private float dodgeCoolDown = 0f;
        [SerializeField]
        private float dodgeForce = 40f;
        [SerializeField]
        private float dodgeDuration = .5f;

        [SerializeField]
        private float knockBackForce = 60f;
        [SerializeField]
        private float knockBackDuration = .2f;

        [SerializeField]
        private GameObject playerHitBox;
        [SerializeField]
        private GameObject leftAttackHitBox;
        [SerializeField]
        private GameObject rightAttackHitBox;

        [System.Serializable]
		struct CursorMapping
        {
            public CursorType type;
            public Texture2D texture;
            public Vector2 hotspot;
        }

        [SerializeField]
        CursorMapping[] cursorMappings = null;

        private PlayerStateMachine stateMachine;
        private PlayerAnimation animation;
        private Rigidbody2D rigidbody2D;
        private Health health;
        private PlayerConversant conversant;

        private void Awake()
		{
            TryGetComponent(out stateMachine);
            TryGetComponent(out animation);
            TryGetComponent(out rigidbody2D);
            TryGetComponent(out health);
            TryGetComponent(out conversant);
		}

        private void FixedUpdate()
        {
            attackCoolDown = Mathf.Max(0, attackCoolDown - Time.fixedDeltaTime);
            dodgeCoolDown = Mathf.Max(0, dodgeCoolDown - Time.fixedDeltaTime);
            jumpCoolDown = Mathf.Max(0, jumpCoolDown - Time.fixedDeltaTime);
            grappleCoolDown = Mathf.Max(0, grappleCoolDown - Time.fixedDeltaTime);

            if (animation.IsDodge)
                StartCoroutine(Dodge(dodgeDuration));

            #region Fall
            if (stateMachine.ControlState != ControlState.Grappling
                && !stateMachine.IsGrounded)
            {
                rigidbody2D.gravityScale *= gravityScale * fallGravityMultiplier;
            }
            else
            {
                rigidbody2D.gravityScale = gravityScale;
            }
            #endregion
            if (stateMachine.ControlState != ControlState.Grappling)
            {
                #region Ground Friction
                // 입력이 없고, 속도가 충분히 낮으면 정지
                if (stateMachine.IsGrounded
                    && inputHorizontal == 0
                    && Mathf.Abs(rigidbody2D.velocity.x) < stopDif)
                    rigidbody2D.velocity = Vector2.zero;
                #endregion
                #region Air Friction
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
                #endregion
            }
            Move();
            Grapple();
        }
        #region Animation Event
        private void Attack()
        {
            if (animation.LastDirection == Vector2.left)
            {
                StartCoroutine(ToggleHitBox(leftAttackHitBox, attackDetectionDuration));
            }
            else if (animation.LastDirection == Vector2.right)
            {
                StartCoroutine(ToggleHitBox(rightAttackHitBox, attackDetectionDuration));
            }
        }
        private IEnumerator ToggleHitBox(GameObject hitBox, float duration)
        {
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
        }
        #endregion
        private IEnumerator Dodge(float duration)
        {
            if (stateMachine.ControlState == ControlState.Uncontrollable)
                yield break;

            dodgeCoolDown = dodgeDelay;

            stateMachine.SetControlState(ControlState.Uncontrollable);
            stateMachine.SetHitState(HitState.Unhittable);
            playerHitBox.SetActive(false);
            
            if (stateMachine.PhysicsType == PhysicsType.Velocity)
            {
                rigidbody2D.velocity = dodgeForce * -animation.LastDirection;
            }

            float decceleration = moveDecceleration;
            if (stateMachine.PhysicsType == PhysicsType.Force)
            {
                moveDecceleration = 0;
                rigidbody2D.AddForce(dodgeForce * -animation.LastDirection, ForceMode2D.Impulse);
            }

            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.fixedDeltaTime;

                // 벽에 닿은 경우는 즉시 정지
                if (stateMachine.IsAgainstLeftWall
                    || stateMachine.IsAgainstRightWall)
                {
                    rigidbody2D.velocity = Vector2.zero;
                    break;
                }

                moveDecceleration = decceleration / (duration / elapsedTime);

                yield return new WaitForFixedUpdate();
            }

            moveDecceleration = decceleration;

            playerHitBox.SetActive(true);
            stateMachine.SetHitState(HitState.Hittable);
            stateMachine.SetControlState(ControlState.Controllable);
        }
        private void Move()
        {
            switch (stateMachine.PhysicsType)
            {
                case PhysicsType.Force:
                    #region Run
                    // 제어 불가이면 inputHorizontal을 0으로 간주
                    if (stateMachine.ControlState == ControlState.Uncontrollable)
                        inputHorizontal = 0;

                    // 목표 속도
                    float targetVelocity = inputHorizontal * moveSpeed;
                    // 속도 차이 (벡터)
                    float velocityDif = targetVelocity - rigidbody2D.velocity.x;
                    // 가속 비율
                    float accelRate = (Mathf.Abs(targetVelocity) > 0.01f) ? moveAcceleration : moveDecceleration;

                    // Grappling이면 가속/감속비율 낮춤
                    if (stateMachine.ControlState == ControlState.Grappling)
                    {
                        accelRate = (Mathf.Abs(targetVelocity) > 0.01f) ? grappleAcceleration : grappleDeceleration;
                    }

                    // 적용할 속도 계산
                    float movement = Mathf.Pow(Mathf.Abs(velocityDif) * accelRate, velPower) * Mathf.Sign(velocityDif);

                    rigidbody2D.AddForce(movement * Vector2.right);
                    #endregion
                    #region Jump
                    // 제어 불가이면 Jump 불가
                    if (stateMachine.ControlState == ControlState.Uncontrollable
                        || stateMachine.ControlState == ControlState.Grappling)
                        return;

                    // Jump Cut 적용
                    if (!stateMachine.IsGrounded && rigidbody2D.velocity.y > 0
                        && inputJump < 0)
                    {
                        rigidbody2D.AddForce(Vector2.down * rigidbody2D.velocity.y * (1 - jumpCutMultiplier), ForceMode2D.Impulse);
                    }

                    // Coyote Time 적용
                    if ((stateMachine.IsGrounded || CheckCoyoteTime())
                        && inputJump > 0)
                    {
                        // 점프 전 y축 속도를 0으로 초기화
                        rigidbody2D.velocity = new Vector2(rigidbody2D.velocity.x, 0);
                        rigidbody2D.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                    }
                    #endregion
                    break;
                case PhysicsType.Velocity:
                    #region Run
                    rigidbody2D.velocity = new Vector2(inputHorizontal * moveSpeed, 0);
                    #endregion
                    #region Jump
                    if (inputJump > 0
                        && stateMachine.IsGrounded)
                    {
                        rigidbody2D.velocity += new Vector2(0, inputJump * jumpForce);
                    }
                    #endregion
                    break;
                default:
                    break;
            }
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

            stateMachine.NearestHookablePlatform.TryGetComponent(out HookablePlatform platform);
            // Hook
            if (inputHook > 0 && grappleCoolDown == 0)
            {
                platform.Hook(this, platform);
            }
            // Unhook
            else
            {
                platform.Unhook(this);
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
            // 적과 충돌한 경우
            // 데미지 계산
            // 잠시 무적이 되며, 넉백
        }

        private void Update()
        {
            grappleIndicator.fillAmount = (grappleDelay - grappleCoolDown) / grappleDelay;
            attackIndicator.fillAmount = (attackDelay - attackCoolDown) / attackDelay;
            dodgeIndicator.fillAmount = (dodgeDelay - dodgeCoolDown) / dodgeDelay;

            if (health.CurrentHealth == 0)
                animation.IsDead = true;

            if (InteractWithCursor())
                return;

            ProcessCombatInput();
            ProcessMoveInput();
            ProcessGrappleInput();
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
        float inputVertical;
        float inputJump;
        private void ProcessMoveInput()
        {
            inputHorizontal = 0;
            inputVertical = 0;
            inputJump = 0;

            if (stateMachine.ControlState == ControlState.Uncontrollable)
                return;

            switch (stateMachine.MoveType)
			{
				case MoveType.Horizontal:
                    inputHorizontal = Input.GetAxisRaw("Horizontal");
                    if (stateMachine.ControlState != ControlState.Grappling)
                    {
                        inputJump = Input.GetAxisRaw("Jump");
                    }

                    // Sprite 반전을 위해 마지막 입력방향 저장
                    Vector2 horizontalDirection = Mathf.Sign(inputHorizontal) * Vector2.right;
                    if (inputHorizontal != 0 && horizontalDirection != animation.LastDirection)
                        animation.LastDirection = horizontalDirection;

                    // Animation을 위해 마지막 입력 저장
                    animation.LastInput = new Vector2(inputHorizontal, 0);
                    break;
				case MoveType.Horizontal_Vertical:
                    inputHorizontal = Input.GetAxisRaw("Horizontal");
                    inputVertical = Input.GetAxisRaw("Vertical");

                    // X, Y중 절대값이 큰 것만 적용
                    if (Mathf.Abs(inputHorizontal) > Mathf.Abs(inputVertical))
                    {
                        inputHorizontal = Mathf.Sign(inputHorizontal);
                        inputVertical = 0;
                    }
                    else if (Mathf.Abs(inputHorizontal) < Mathf.Abs(inputVertical))
                    {
                        inputHorizontal = 0;
                        inputVertical = Mathf.Sign(inputVertical);
                    }
                    break;
				default:
                    Debug.Log("Undefined MoveType. Check StateMachine.");
					break;
			}

            inputHorizontal = Mathf.Abs(inputHorizontal) > 0 ? Mathf.Sign(inputHorizontal) : 0;
            inputVertical = Mathf.Abs(inputVertical) > 0 ? Mathf.Sign(inputVertical) : 0;
            inputJump = Mathf.Abs(inputJump) > 0 ? Mathf.Sign(inputJump) : 0;
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

        private void LateUpdate()
        {
            // Camera
        }

        private bool InteractWithCursor()
        {
            if (RaycastInteractableTarget(out IRaycastable target))
            {
                if (Input.GetMouseButtonDown(0))
                {
                    target.HandleRaycast(this);
                }

                SetCursor(target.GetCursorType());
                return true;
            }
            else if (stateMachine.NearestHookablePlatform != null)
            {
                SetCursor(stateMachine.NearestHookablePlatform.GetCursorType());
                return false;
            }

            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
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
    }
}
