using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FSM;
using System;

namespace Player
{
    public class PlayerStateMachine : StateMachine
    {
        [SerializeField]
        private Color debugColor;

        // ControlState
        [field: SerializeField]
        public float LastGroundTime { get; private set; }
        [field:SerializeField]
		public bool IsGrounded { get; private set; }
        [field: SerializeField]
        public bool IsAgainstLeftWall { get; private set; }
        [field: SerializeField]
        public bool IsAgainstRightWall { get; private set; }
        [field: SerializeField]
        public HookablePlatform NearestHookablePlatform { get; set; } = null;

        [field: SerializeField]
        public bool IsTalking { get; set; } = false;

        public Vector3 leftRayOffset = new Vector3(-.2f, -.3f, 0);
        public Vector3 rightRayOffset = new Vector3(.2f, -.3f, 0);
        public Vector3 downRayOffset = new Vector3(0, -.3f, 0);

        public float horizontalRayDistance = .6f;
        public float verticalRayDistance = .6f;
        public float platformCheckRadius = 5f;

        private int EnemyLayer;
        private int DefaultLayer;

        private void Awake()
		{
			base.InitializeStateMachine();
            EnemyLayer = LayerMask.NameToLayer("Enemy");
            DefaultLayer = LayerMask.NameToLayer("Default");
        }

		private void FixedUpdate()
		{
            CheckObstacleHorizontal();
            CheckObstacleVertical();
            CheckHookablePlatform();
            LastGroundTime += Time.fixedDeltaTime;
        }

        private void Update()
        {
            gameObject.layer = HitState == HitState.Unhittable ? EnemyLayer : DefaultLayer;
        }

        private void LateUpdate()
        {
            HighlightNearestHookablePlatform();
        }

        private void HighlightNearestHookablePlatform()
        {
            if (NearestHookablePlatform != null
                && NearestHookablePlatform.TryGetComponent(out SpriteRenderer renderer))
            {
                renderer.color = Color.red;
            }
        }

        private void CheckObstacleHorizontal()
        {
            RaycastHit2D leftHit1 = Physics2D.Raycast(transform.position + leftRayOffset,
                                                     Vector3.left,
                                                     horizontalRayDistance,
                                                     LayerMask.GetMask("Obstacle"));
            Debug.DrawLine(transform.position + leftRayOffset,
                           transform.position + leftRayOffset + Vector3.left * horizontalRayDistance,
                           Color.red);

            RaycastHit2D leftHit2 = Physics2D.Raycast(transform.position + leftRayOffset + Vector3.up * 0.2f,
                                                     Vector3.left,
                                                     horizontalRayDistance,
                                                     LayerMask.GetMask("Obstacle"));
            Debug.DrawLine(transform.position + leftRayOffset + Vector3.up * 0.2f,
                           transform.position + leftRayOffset + Vector3.left * horizontalRayDistance + Vector3.up * 0.2f,
                           Color.red);

            RaycastHit2D leftHit3 = Physics2D.Raycast(transform.position + leftRayOffset - Vector3.up * 0.2f,
                                                     Vector3.left,
                                                     horizontalRayDistance,
                                                     LayerMask.GetMask("Obstacle"));
            Debug.DrawLine(transform.position + leftRayOffset - Vector3.up * 0.2f,
                           transform.position + leftRayOffset + Vector3.left * horizontalRayDistance - Vector3.up * 0.2f,
                           Color.red);

            RaycastHit2D rightHit1 = Physics2D.Raycast(transform.position + rightRayOffset,
                                                      Vector3.right + rightRayOffset,
                                                      horizontalRayDistance,
                                                      LayerMask.GetMask("Obstacle"));
            Debug.DrawLine(transform.position + rightRayOffset,
                           transform.position + rightRayOffset + Vector3.right * horizontalRayDistance,
                           Color.blue);

            RaycastHit2D rightHit2 = Physics2D.Raycast(transform.position + rightRayOffset + rightRayOffset + Vector3.up * 0.2f,
                                                      Vector3.right,
                                                      horizontalRayDistance,
                                                      LayerMask.GetMask("Obstacle"));
            Debug.DrawLine(transform.position + rightRayOffset + Vector3.up * 0.2f,
                           transform.position + rightRayOffset + Vector3.right * horizontalRayDistance + Vector3.up * 0.2f,
                           Color.blue);

            RaycastHit2D rightHit3 = Physics2D.Raycast(transform.position + rightRayOffset - rightRayOffset - Vector3.up * 0.2f,
                                                      Vector3.right,
                                                      horizontalRayDistance,
                                                      LayerMask.GetMask("Obstacle"));
            Debug.DrawLine(transform.position + rightRayOffset - Vector3.up * 0.2f,
                           transform.position + rightRayOffset + Vector3.right * horizontalRayDistance - Vector3.up * 0.2f,
                           Color.blue);


            IsAgainstLeftWall = false;
            if (leftHit1 || leftHit2 || leftHit3)
                IsAgainstLeftWall = true;

            IsAgainstRightWall = false;
            if (rightHit1 || rightHit2 || rightHit3)
                IsAgainstRightWall = true;
        }
        private void CheckObstacleVertical()
        {
            RaycastHit2D downHit = Physics2D.Raycast(transform.position + downRayOffset,
                                                     Vector3.down,
                                                     verticalRayDistance,
                                                     LayerMask.GetMask("Obstacle"));
            Debug.DrawLine(transform.position + downRayOffset,
                           transform.position + downRayOffset + Vector3.down * verticalRayDistance,
                           Color.cyan);

            RaycastHit2D downHit2 = Physics2D.Raycast(transform.position + downRayOffset + Vector3.left * 0.2f,
                                                     Vector3.down,
                                                     verticalRayDistance,
                                                     LayerMask.GetMask("Obstacle"));
            Debug.DrawLine(transform.position + downRayOffset + Vector3.left * 0.2f,
                           transform.position + downRayOffset + Vector3.down * verticalRayDistance + Vector3.left * 0.2f,
                           Color.cyan);

            RaycastHit2D downHit3 = Physics2D.Raycast(transform.position + downRayOffset + Vector3.right * 0.2f,
                                                     Vector3.down,
                                                     verticalRayDistance,
                                                     LayerMask.GetMask("Obstacle"));
            Debug.DrawLine(transform.position + downRayOffset + Vector3.right * 0.2f,
                           transform.position + downRayOffset + Vector3.down * verticalRayDistance + Vector3.right * 0.2f,
                           Color.cyan);

            IsGrounded = false;
            if (downHit || downHit2 || downHit3)
			{
                IsGrounded = true;
                LastGroundTime = 0;

                if (downHit)
                    SetParentMovablePlatform(downHit);
                else if (downHit2)
                    SetParentMovablePlatform(downHit2);
                else if (downHit3)
                    SetParentMovablePlatform(downHit3);
            }
            else
            {
                transform.SetParent(null);
            }
        }

        private void SetParentMovablePlatform(RaycastHit2D downHit)
        {
            Collider2D collider2D = downHit.collider;
            if (collider2D.gameObject.transform.parent.TryGetComponent(out MovablePlatform movable))
            {
                transform.SetParent(collider2D.transform.parent);
            }
        }

        private void CheckHookablePlatform()
        {
            NearestHookablePlatform = null;

            float minDistance = float.MaxValue;
            //Collider[] colliders = Physics.OverlapSphere(transform.position, platformCheckRadius, LayerMask.GetMask("Hookable"));
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos = new Vector3(mousePos.x, mousePos.y, 0);
            Collider[] colliders = Physics.OverlapSphere(mousePos, platformCheckRadius, LayerMask.GetMask("Hookable"));
            foreach (Collider collider in colliders)
            {
                // �÷��̾�� ���� �ִ� Platform�� �˻�
                if (collider.transform.position.y <= transform.position.y)
                    continue;

                mousePos = new Vector3(mousePos.x, mousePos.y, 0);
                Vector3 playerToPlatform = collider.transform.position - transform.position;
                Vector3 mouseToPlatform = collider.transform.position - mousePos;

                collider.gameObject.TryGetComponent(out HookablePlatform platform);
                //float distance = Vector3.Distance(transform.position, collider.transform.position);
                float distance = playerToPlatform.sqrMagnitude * mouseToPlatform.sqrMagnitude;
                if (platform != null && distance < minDistance)
                {
                    NearestHookablePlatform = platform;
                    minDistance = distance;
                }
            }
        }

        private void OnDrawGizmos()
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos = new Vector3(mousePos.x, mousePos.y, 0);
            Gizmos.color = debugColor;
            Gizmos.DrawSphere(mousePos, platformCheckRadius);
        }
    }
}
