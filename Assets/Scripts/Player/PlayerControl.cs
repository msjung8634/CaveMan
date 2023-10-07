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

        // Indicator�� Player Control�� �����ִ°� ���� ������?
        [SerializeField]
        private Image attackIndicator;
        [SerializeField]
        private float attackDelay = .5f;
        private float attackCoolDown = 0f;
        [SerializeField]
        private float attackDetectionDuration = .3f;

        // Indicator�� Player Control�� �����ִ°� ���� ������?
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
			// �Է��� ����, �ӵ��� ����� ������ ����
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
            // ������ �ƴ� ��� �¿��̵� ����
            if (!stateMachine.IsGrounded && Mathf.Abs(inputHorizontal) < 0.01f)
            {
                // ����ӷ�, ������ �� ���� ũ��
                float amount = Mathf.Min(Mathf.Abs(rigidbody2D.velocity.x), Mathf.Abs(frictionAmount));
                // �̵������� �ݴ� ����
                amount *= Mathf.Sign(rigidbody2D.velocity.x) * -1;
                // ����
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

                // ���� ���� ���� ��� ����
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
                    // ���� �Ұ��̸� inputHorizontal�� 0���� ����
					if (stateMachine.ControlState == ControlState.Uncontrollable)
                        inputHorizontal = 0;

                    // ��ǥ �ӵ�
                    float targetVelocity = inputHorizontal * moveSpeed;
                    // �ӵ� ���� (����)
                    float velocityDif = targetVelocity - rigidbody2D.velocity.x;
                    // ���� ����
                    float accelRate = (Mathf.Abs(targetVelocity) > 0.01f) ? acceleration : decceleration;
                    // ������ �ӵ� ���
                    float movement = Mathf.Pow(Mathf.Abs(velocityDif) * accelRate, velPower) * Mathf.Sign(velocityDif);

                    //Debug.Log($"target : {targetVelocity} / dif : {velocityDif} / accelRate : {accelRate} / movement : {movement}");

                    rigidbody2D.AddForce(movement * Vector2.right);
                    #endregion
                    #region Jump
                    // ���� �Ұ��̸� Jump �Ұ�
                    if (stateMachine.ControlState == ControlState.Uncontrollable)
                        return;

                    if (stateMachine.IsGrounded
                        && inputJump > 0)
					{
                        // ���� �� y�� �ӵ��� 0���� �ʱ�ȭ
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
            // ���� �浹�� ���
            // ������ ���
            // ��� ������ �Ǹ�, �˹�
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

                    // Sprite ������ ���� ������ �Է¹��� ����
                    Vector2 horizontalDirection = Mathf.Sign(inputHorizontal) * Vector2.right;
                    if (inputHorizontal != 0 && horizontalDirection != animation.LastDirection)
                        animation.LastDirection = horizontalDirection;

                    // Animation�� ���� ������ �Է� ����
                    animation.LastInput = new Vector2(inputHorizontal, 0);
                    break;
				case MoveType.Horizontal_Vertical:
                    inputHorizontal = Input.GetAxisRaw("Horizontal");
                    inputVertical = Input.GetAxisRaw("Vertical");

                    // X, Y�� ���밪�� ū �͸� ����
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
