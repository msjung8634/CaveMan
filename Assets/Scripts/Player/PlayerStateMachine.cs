using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FSM;
using System;

namespace Player
{
    public class PlayerStateMachine : StateMachine
    {
        // 체력을 여기에 갖고 있는게 낫지 않을까?

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
        public HookablePlatform NearestHookTarget { get; set; } = null;

        public Vector3 leftRayOffset = new Vector3(-.2f, -.3f, 0);
        public Vector3 rightRayOffset = new Vector3(.2f, -.3f, 0);
        public Vector3 downRayOffset = new Vector3(0, -.3f, 0);

        public float horizontalRayDistance = .6f;
        public float verticalRayDistance = .6f;
        public float platformCheckRadius = 5f;

        private void Awake()
		{
			base.InitializeStateMachine();
		}

		private void FixedUpdate()
		{
            CheckObstacleHorizontal();
            CheckObstacleVertical();
            CheckHookablePlatform();
            LastGroundTime += Time.fixedDeltaTime;
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
            NearestHookTarget = null;

            float minDistance = float.MaxValue;
            Collider[] colliders = Physics.OverlapSphere(transform.position, platformCheckRadius, LayerMask.GetMask("Hookable"));
            foreach (Collider collider in colliders)
            {
                // 플레이어보다 위에 있는 Platform만 검사
                if (collider.transform.position.y <= transform.position.y)
                    continue;

                collider.gameObject.TryGetComponent(out HookablePlatform platform);
                float distance = Vector3.Distance(transform.position, collider.transform.position);
                if (platform != null && distance < minDistance)
                {
                    NearestHookTarget = platform;
                    minDistance = distance;
                }
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = debugColor;
            Gizmos.DrawSphere(transform.position, platformCheckRadius);
        }
    }
}
