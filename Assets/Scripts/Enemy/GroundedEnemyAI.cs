using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Enemy
{
    [RequireComponent(typeof(BoxCollider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class GroundedEnemyAI : MonoBehaviour
    {
        private BoxCollider2D boxCollider2D;
        private Rigidbody2D rigidbody2D;

        private bool isAttacking = false;

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

            firstVelocity = maxVelocity;
            groundRaySpacing = boxCollider2D.size.x;
        }

        private void FixedUpdate()
        {
            reverseCoolDown = Mathf.Max(0, reverseCoolDown - Time.fixedDeltaTime);
            attackCoolDown = Mathf.Max(0, attackCoolDown - Time.fixedDeltaTime);

            if (!isAttacking && CheckGround())
            {
                if(TryDetectPlayer(out float distance))
                {
                    Attack(distance);
                }
                else
                {
                    Patrol();
                }
            }
        }

        private bool CheckGround()
        {
            Vector2 origin = new Vector2(transform.position.x, transform.position.y) + groundRayOffset;
            Vector2 spacing = new Vector2(groundRaySpacing, 0);

            RaycastHit2D hitLeft = Physics2D.Raycast(origin - spacing, Vector2.down, groundRayLength, LayerMask.GetMask("Obstacle"));
            Debug.DrawLine(transform.position + new Vector3(groundRayOffset.x - spacing.x, groundRayOffset.y, 0), transform.position + new Vector3(groundRayOffset.x - spacing.x, groundRayOffset.y, 0) + Vector3.down * groundRayLength, Color.red);

            RaycastHit2D hitCenter = Physics2D.Raycast(origin, Vector2.down, groundRayLength, LayerMask.GetMask("Obstacle"));
            Debug.DrawLine(transform.position + new Vector3(groundRayOffset.x, groundRayOffset.y, 0), transform.position + new Vector3(groundRayOffset.x, groundRayOffset.y, 0) + Vector3.down * groundRayLength, Color.red);

            RaycastHit2D hitRight = Physics2D.Raycast(origin + spacing, Vector2.down, groundRayLength, LayerMask.GetMask("Obstacle"));
            Debug.DrawLine(transform.position + new Vector3(groundRayOffset.x + spacing.x, groundRayOffset.y, 0), transform.position + new Vector3(groundRayOffset.x + spacing.x, groundRayOffset.y, 0) + Vector3.down * groundRayLength, Color.red);

            // ���� ���������� ���
            if ((hitLeft && hitCenter && !hitRight)
                || (!hitLeft && hitCenter && hitRight))
            {
                ReverseMovement();
            }

            return hitLeft || hitCenter || hitRight;
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
        }

        private void Stop(out Vector2 originVelocity)
        {
            originVelocity = rigidbody2D.velocity;
            //rigidbody2D.AddForce(-rigidbody2D.velocity * accelRate * rigidbody2D.mass, ForceMode2D.Impulse);
            rigidbody2D.velocity = Vector2.zero;
        }

        private bool TryDetectPlayer(out float distance)
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
            if (isAttacking || attackCoolDown > 0)
                return;
            
            // ����
            if (distance <= dashDistance)
            {
                attackCoolDown = attackDelay;
                StartCoroutine(DashAttack(distance));
            }
        }

        private IEnumerator DashAttack(float distance)
        {
            isAttacking = true;

            // ��� ����
            Stop(out Vector2 originVelocity);
            yield return StartCoroutine(Delay(dashCastDelay));

            // �뽬
            maxVelocity = dashMaxVelocity;
            rigidbody2D.velocity = dashMaxVelocity * Vector2.right * Mathf.Sign(distance);

            // �ӵ� ����
            yield return StartCoroutine(RevertVelocity(originVelocity, .5f));

            isAttacking = false;
        }

        private IEnumerator RevertVelocity(Vector2 originVelocity, float delay)
        {
            float elapsedTime = 0f;
            while (elapsedTime < delay)
            {
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            maxVelocity = firstVelocity;
            rigidbody2D.velocity = originVelocity;
        }

        private IEnumerator Delay(float duration)
        {
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }
    }
}