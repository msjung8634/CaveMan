using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FSM;
using System;

namespace Enemy
{
	[RequireComponent(typeof(SpriteRenderer))]
	[RequireComponent(typeof(Animator))]
	[RequireComponent(typeof(Health))]
	public class EnemyStateMachine : StateMachine
	{
		#region Animation Parameters
		private readonly int isAttackToHash = Animator.StringToHash("isAttack");
		private readonly int isDeadToHash = Animator.StringToHash("isDead");
		#endregion

		public Vector2 LastDirection { get; set; } = Vector2.zero;

		public bool IsAttack { get; set; } = false;

		private SpriteRenderer spriteRenderer;
		private Animator animator;
		private Health health;

		private void Awake()
		{
			TryGetComponent(out spriteRenderer);
			TryGetComponent(out animator);
			TryGetComponent(out health);
		}

		private void Start()
		{
			InitializeStateMachine();
		}

		private void Update()
		{
			if (LastDirection == Vector2.left)
				spriteRenderer.flipX = true;
			else if (LastDirection == Vector2.right)
				spriteRenderer.flipX = false;

			if (health.CurrentHealth <= 0)
			{
				SetControlState(ControlState.Uncontrollable);
			}

			Animate();
		}

		private void Animate()
		{
			animator.SetBool(isDeadToHash, health.CurrentHealth <= 0);
			animator.SetBool(isAttackToHash, IsAttack);
		}
	}
}
