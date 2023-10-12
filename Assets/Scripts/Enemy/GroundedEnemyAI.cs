using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Enemy
{
    [RequireComponent(typeof(BoxCollider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(EnemyStateMachine))]
    public class GroundedEnemyAI : MonoBehaviour
    {
		private BoxCollider2D boxCollider2D;
        private Rigidbody2D rigidbody2D;
        private EnemyStateMachine stateMachine;

        private bool isAttacking = false;
        private bool isHit = false;
        private bool isDead = false;

        [Header("Patrol")]
        [SerializeField]
        private float accelRate = 10f;
        [SerializeField]
        private float maxVelocity = 10f;
        private float firstVelocity;
        [SerializeField]
        private float reverseDelay = 1f;
        [SerializeField]
        private float reverseCoolDown = 0f;

        [Header("Attack Property")]
        [SerializeField]
        private float attackCoolDown = 1f;
        [SerializeField]
        private float attackDelay = 1f;

        [Header("Dash Attack")]
        [SerializeField]
        private float dashDistance = 1f;
        [SerializeField]
        private float dashMaxVelocity = 30f;
        [SerializeField]
        private float dashCastDelay = .5f;
        [SerializeField]
        private float revertVelocityDelay = .5f;

        [Header("Jump Attack")]
        private float jumpHeight = 10f;
        private float jumpSpeed = 10f;

        [Header("SelfDestruct Attack")]
        private float destructDelay = 2f;
        private float destructRadius = 2f;

        [Header("Ground Detection")]
        [SerializeField]
        private float groundRayLength = 1f;
        [SerializeField]
        private float groundRaySpacing;
        [SerializeField]
        private Vector2 groundRayOffset = Vector2.zero;
        
        [Header("Player Detection")]
        [SerializeField]
        private float detectRayLength = 5f;
        [SerializeField]
        private Vector2 detectRayOffset = Vector2.zero;
        private Vector2 detectDirection = Vector2.right;

        private void Awake()
        {
            TryGetComponent(out boxCollider2D);
            TryGetComponent(out rigidbody2D);
            TryGetComponent(out stateMachine);
        }

		private void Start()
		{
            firstVelocity = maxVelocity;
            groundRaySpacing = transform.localScale.x;
            stateMachine.LastDirection = Vector2.left;
        }

		private void FixedUpdate()
        {
            reverseCoolDown = Mathf.Max(0, reverseCoolDown - Time.fixedDeltaTime);
            attackCoolDown = Mathf.Max(0, attackCoolDown - Time.fixedDeltaTime);

			if (stateMachine.ControlState == FSM.ControlState.Uncontrollable)
			{
                Stop(out Vector2 originVelocity);
                return;
            }

            if (IsGrounded() && !isAttacking && attackCoolDown <= 0)
            {
                if(TryDetectPlayerHitBox(out float distance))
                {
                    Attack(distance);
                }
                else
                {
                    Patrol();
                }
            }
        }

        public void Die()
        {
            StopAllCoroutines();
        }

		private bool IsGrounded()
        {
            Vector2 origin = new Vector2(transform.position.x, transform.position.y) + groundRayOffset;
            Vector2 spacing = new Vector2(groundRaySpacing, 0);

            RaycastHit2D hitLeft = Physics2D.Raycast(origin - spacing, Vector2.down, groundRayLength, LayerMask.GetMask("Obstacle"));
            Debug.DrawLine(transform.position + new Vector3(groundRayOffset.x - spacing.x, groundRayOffset.y, 0), transform.position + new Vector3(groundRayOffset.x - spacing.x, groundRayOffset.y, 0) + Vector3.down * groundRayLength, Color.red);

            RaycastHit2D hitRight = Physics2D.Raycast(origin + spacing, Vector2.down, groundRayLength, LayerMask.GetMask("Obstacle"));
            Debug.DrawLine(transform.position + new Vector3(groundRayOffset.x + spacing.x, groundRayOffset.y, 0), transform.position + new Vector3(groundRayOffset.x + spacing.x, groundRayOffset.y, 0) + Vector3.down * groundRayLength, Color.red);

            // 끝이 낭떠러지인 경우
            if ((hitLeft && !hitRight)
                || (!hitLeft && hitRight))
            {
                ReverseMovement();
            }

            return hitLeft || hitRight;
        }

        private void ReverseMovement()
        {
            if (reverseCoolDown > 0)
                return;

            reverseCoolDown = reverseDelay;

            Stop(out Vector2 originVelocity);
            ReverseDirection(originVelocity);
        }

        private void ReverseDirection(Vector2 originVelocity)
        {
            accelRate *= -1;
            detectDirection *= -1;
            rigidbody2D.velocity = -originVelocity;
            if (rigidbody2D.velocity.x != 0)
            {
                stateMachine.LastDirection = Vector2.right * Mathf.Sign(-rigidbody2D.velocity.x);
            }
        }

        private void Stop(out Vector2 originVelocity)
        {
            originVelocity = rigidbody2D.velocity;
            rigidbody2D.velocity = Vector2.zero;
        }

        private bool TryDetectPlayerHitBox(out float distance)
        {
            Vector2 origin = new Vector2(transform.position.x, transform.position.y) + detectRayOffset;

            RaycastHit2D hit = Physics2D.Raycast(origin, detectDirection, detectRayLength, LayerMask.GetMask("HitBox"));
            Debug.DrawLine(origin, origin + detectDirection * detectRayLength, Color.yellow);
            if (hit && hit.collider.CompareTag("PlayerHitBox"))
            {
                distance = hit.distance * Mathf.Sign(hit.point.x - transform.position.x);
                return true;
            }

            distance = 0;
            return false;
        }

        private void Patrol()
        {
            rigidbody2D.AddForce(Vector2.right * accelRate * rigidbody2D.mass);
            if (Mathf.Abs(rigidbody2D.velocity.x) > maxVelocity)
                rigidbody2D.velocity = new Vector2(maxVelocity * Mathf.Sign(rigidbody2D.velocity.x), rigidbody2D.velocity.y);
        }

        private void Attack(float distance)
        {
            if (isAttacking || attackCoolDown > 0
                && stateMachine.ControlState == FSM.ControlState.Uncontrollable)
                return;
            
            // 공격
            if (Mathf.Abs(distance) <= dashDistance)
            {
                attackCoolDown = attackDelay;
                StartCoroutine(DashAttack(distance));
            }
        }

        private IEnumerator DashAttack(float distance)
        {
            isAttacking = true;
            stateMachine.IsAttack = true;

            // 잠시 정지
            Stop(out Vector2 originVelocity);
            yield return StartCoroutine(Delay(dashCastDelay));

            // 대쉬
            maxVelocity = dashMaxVelocity;
            rigidbody2D.velocity = dashMaxVelocity * Vector2.right * Mathf.Sign(distance);

            // 속도 복원
            yield return StartCoroutine(RevertVelocity(originVelocity, revertVelocityDelay));

            isAttacking = false;
            stateMachine.IsAttack = false;
        }

        private IEnumerator RevertVelocity(Vector2 originVelocity, float delay)
        {
            while (stateMachine.ControlState != FSM.ControlState.Uncontrollable)
            {
                float elapsedTime = 0f;
                while (elapsedTime < delay)
                {
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }

                maxVelocity = firstVelocity;
                rigidbody2D.velocity = originVelocity;

                break;
            }
        }

        private IEnumerator Delay(float duration)
        {
            while (stateMachine.ControlState != FSM.ControlState.Uncontrollable)
            {
                float elapsedTime = 0f;
                while (elapsedTime < duration)
                {
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }

                break;
            }
        }
    }
}
