using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(PlayerStateMachine))]
	[RequireComponent(typeof(SpriteRenderer))]
	[RequireComponent(typeof(Animator))]
	public class PlayerAnimation : MonoBehaviour
    {
        private readonly int moveXToHash = Animator.StringToHash("moveX");
        private readonly int moveYToHash = Animator.StringToHash("moveY");
        private readonly int isDeadToHash = Animator.StringToHash("isDead");
		private readonly int isAttackToHash = Animator.StringToHash("isAttack");
		private readonly int isDodgeToHash = Animator.StringToHash("isDodge");

		public Vector2 LastInput { get; set; } = Vector2.zero;
		public Vector2 LastDirection { get; set; } = Vector2.zero;
		public bool IsDead { get; set; } = false;
		public bool IsAttack { get; set; } = false;
		public bool IsDodge { get; set; } = false;

		private PlayerStateMachine stateMachine;
		private SpriteRenderer spriteRenderer;
		private Animator animator;
		private void Awake()
		{
            TryGetComponent(out stateMachine);
			TryGetComponent(out spriteRenderer);
			TryGetComponent(out animator);
		}

		void Update()
        {
			if (LastDirection == Vector2.left)
				spriteRenderer.flipX = true;
			else if (LastDirection == Vector2.right)
				spriteRenderer.flipX = false;

			Animate();
		}

		private void Animate()
		{
			animator.SetFloat(moveXToHash, LastInput.x);
			animator.SetFloat(moveYToHash, LastInput.y);
			animator.SetBool(isDeadToHash, IsDead);
			animator.SetBool(isAttackToHash, IsAttack);
			animator.SetBool(isDodgeToHash, IsDodge);
		}
	}
}
