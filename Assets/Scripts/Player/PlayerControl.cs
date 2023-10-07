using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FSM;

namespace Player
{
    [RequireComponent(typeof(PlayerStateMachine))]
    [RequireComponent(typeof(PlayerAnimation))]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Health))]
    public class PlayerControl : MonoBehaviour
    {
        [Header("Move")]
        [SerializeField]
        private float moveSpeed = 10f;
        [SerializeField]
        [Range(0f, 4f)]
        private float stopDif = 1f;
        [SerializeField]
        private float acceleration = 1f;
        [SerializeField]
        private float decceleration = 1f;
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

        private float jumpCoolDown = 0f;
        [Header("Fall")]
        [SerializeField]
        private float gravityScale = 1f;
        [SerializeField]
        private float fallGravityMultiplier = 1f;

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

        private PlayerStateMachine stateMachine;
        private PlayerAnimation animation;
        private Rigidbody2D rigidbody2D;
        private Health health;

        private void Awake()
		{
            TryGetComponent(out stateMachine);
            TryGetComponent(out animation);
            TryGetComponent(out rigidbody2D);
            TryGetComponent(out health);
		}

        private void FixedUpdate()
        {
            attackCoolDown = Mathf.Max(0, attackCoolDown - Time.fixedDeltaTime);
            dodgeCoolDown = Mathf.Max(0, dodgeCoolDown - Time.fixedDeltaTime);
            jumpCoolDown = Mathf.Max(0, jumpCoolDown - Time.fixedDeltaTime);

            if (animation.IsDodge)
                StartCoroutine(Dodge(dodgeDuration));

			#region Ground Friction
			// 입력이 없고, 속도가 충분히 낮으면 정지
			if (stateMachine.IsGrounded
                && inputHorizontal == 0
                && Mathf.Abs(rigidbody2D.velocity.x) < stopDif)
                rigidbody2D.velocity = Vector2.zero;
            #endregion
            #region Fall
            if (!stateMachine.IsGrounded)
            {
                rigidbody2D.gravityScale *= gravityScale * fallGravityMultiplier;
            }
            else
            {
                rigidbody2D.gravityScale = gravityScale;
            }
            #endregion
            #region Air Friction
            // 지면이 아닌 경우 좌우이동 정지
            if (!stateMachine.IsGrounded && Mathf.Abs(inputHorizontal) < 0.01f)
            {
                // 현재속력, 마찰력 중 작은 크기
                float amount = Mathf.Min(Mathf.Abs(rigidbody2D.velocity.x), Mathf.Abs(frictionAmount));
                // 이동방향의 반대 방향
                amount *= Mathf.Sign(rigidbody2D.velocity.x) * -1;
                // 적용
                rigidbody2D.AddForce(Vector2.right * amount, ForceMode2D.Impulse);
            }
            #endregion
            Move();
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
            playerHitBox.SetActive(false);

            rigidbody2D.velocity = dodgeForce * -animation.LastDirection;

            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.fixedDeltaTime;

                // 벽에 닿은 경우는 즉시 정지
                if (stateMachine.IsAgainstLeftWall
                    || stateMachine.IsAgainstRightWall)
                {
                    Debug.Log("Dodge(dodgeDuration) : Hit Wall");
                    rigidbody2D.velocity = Vector2.zero;
                    break;
                }
                yield return new WaitForFixedUpdate();
            }

            playerHitBox.SetActive(true);
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
                    float accelRate = (Mathf.Abs(targetVelocity) > 0.01f) ? acceleration : decceleration;
                    // 적용할 속도 계산
                    float movement = Mathf.Pow(Mathf.Abs(velocityDif) * accelRate, velPower) * Mathf.Sign(velocityDif);

                    //Debug.Log($"target : {targetVelocity} / dif : {velocityDif} / accelRate : {accelRate} / movement : {movement}");

                    rigidbody2D.AddForce(movement * Vector2.right);
                    #endregion
                    #region Jump
                    // 제어 불가이면 Jump 불가
                    if (stateMachine.ControlState == ControlState.Uncontrollable)
                        return;

                    if (stateMachine.IsGrounded
                        && inputJump > 0)
					{
                        // 점프 전 y축 속도를 0으로 초기화
                        rigidbody2D.velocity = new Vector2(rigidbody2D.velocity.x, 0);
                        rigidbody2D.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                        
                        //Debug.Log($"inputJump : {inputJump}");
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

        private void OnTriggerEnter2D(Collider2D collision)
        {
            // 적과 충돌한 경우
            // 데미지 계산
            // 잠시 무적이 되며, 넉백
        }

        private void Update()
        {
            attackIndicator.fillAmount = (attackDelay - attackCoolDown) / attackDelay;
            dodgeIndicator.fillAmount = (dodgeDelay - dodgeCoolDown) / dodgeDelay;

            if (health.CurrentHealth == 0)
                animation.IsDead = true;

            ProcessCombatInput();
            ProcessMoveInput();
        }

        float inputDodge;
        float inputAttack;
        private void ProcessCombatInput()
		{
            inputDodge = 0;
            inputAttack = 0;

            if (stateMachine.ControlState == ControlState.Uncontrollable)
                return;

            inputDodge = Input.GetAxisRaw("Fire3");
            inputAttack = Input.GetAxisRaw("Fire1");

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
                    inputJump = Input.GetAxisRaw("Jump");

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

        private void LateUpdate()
        {
            // Camera
        }
    }
}
