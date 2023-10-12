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
	[RequireComponent(typeof(BoxCollider2D))]
	[RequireComponent(typeof(Rigidbody2D))]
	public class EnemyStateMachine : StateMachine
	{
		#region Animation Parameters
		private readonly int isAttackToHash = Animator.StringToHash("isAttack");
		private readonly int isDeadToHash = Animator.StringToHash("isDead");
		#endregion

		public Vector2 LastDirection { get; set; } = Vector2.zero;

		public bool IsAttack { get; set; } = false;

		[SerializeField]
		private float dieDelay = .5f;

		private SpriteRenderer spriteRenderer;
		private Animator animator;
		private Health health;
		private BoxCollider2D collider;
		private Rigidbody2D rigidbody;

		private void Awake()
		{
			TryGetComponent(out spriteRenderer);
			TryGetComponent(out animator);
			TryGetComponent(out health);
			TryGetComponent(out collider);
			TryGetComponent(out rigidbody);
		}

		private void Start()
		{
			InitializeStateMachine();
		}

		private void Update()
		{
			Animate();

			if (health.CurrentHealth <= 0)
				StartCoroutine(Die(dieDelay));
		}

		private void Animate()
		{
			if (LastDirection == Vector2.left)
				spriteRenderer.flipX = true;
			else if (LastDirection == Vector2.right)
				spriteRenderer.flipX = false;

			animator.SetBool(isDeadToHash, health.CurrentHealth <= 0);
			animator.SetBool(isAttackToHash, IsAttack);
		}

		private IEnumerator Die(float duration)
        {
			animator.SetBool(isAttackToHash, false);
			SetControlState(ControlState.Uncontrollable);
			rigidbody.constraints = RigidbodyConstraints2D.FreezePosition;
			collider.enabled = false;

			float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
				elapsedTime += Time.deltaTime;

				// alpha값 1 -> 0 으로 서서히 감소
				float alpha = Mathf.Lerp(0f, 1f, 1 - elapsedTime/duration);
				spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, alpha);

				yield return null;
            }

			Destroy(gameObject);
		}
	}
}
