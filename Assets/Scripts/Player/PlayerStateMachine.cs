using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FSM;

namespace Player
{
    public class PlayerStateMachine : StateMachine
    {
        // 체력을 여기에 갖고 있는게 낫지 않을까?

        // ControlState
        [field: SerializeField]
        public float LastGroundTime { get; private set; }
        [field:SerializeField]
		public bool IsGrounded { get; private set; }
        [field: SerializeField]
        public bool IsAgainstLeftWall { get; private set; }
        [field: SerializeField]
        public bool IsAgainstRightWall { get; private set; }

        public Vector3 leftRayOffset = new Vector3(-.2f, -.3f, 0);
        public Vector3 rightRayOffset = new Vector3(.2f, -.3f, 0);
        public Vector3 downRayOffset = new Vector3(0, -.3f, 0);

        public float horizontalRayDistance = .6f;
        public float verticalRayDistance = .6f;

        private void Awake()
		{
			base.InitializeStateMachine();
		}

		private void FixedUpdate()
		{
            CheckObstacleHorizontal();
            CheckObstacleVertical();
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
            }
        }
    }
}
