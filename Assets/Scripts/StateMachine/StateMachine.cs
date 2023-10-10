using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FSM
{
    /// <summary>
    /// 물리처리 방식
    /// </summary>
    public enum PhysicsType
	{
        /// <summary>
        /// rb.AddForce 사용
        /// </summary>
        Force,
        /// <summary>
        /// rb.velocity 사용
        /// </summary>
        Velocity,
	}

    /// <summary>
    /// 이동 방식
    /// </summary>
    public enum MoveType
    {
        /// <summary>
        /// A/D
        /// </summary>
        Horizontal,
        /// <summary>
        /// W/A/S/D
        /// </summary>
        Horizontal_Vertical,
    }

    /// <summary>
    /// 제어가능 여부
    /// </summary>
    public enum ControlState
    {
        Controllable,
        Uncontrollable,
        Grappling,
    }

    /// <summary>
    /// 피격판정 여부
    /// </summary>
    public enum HitState
	{
        Hittable,
        Unhittable,
    }

    public class StateMachine : MonoBehaviour
    {
        [field: SerializeField]
        public PhysicsType PhysicsType { get; private set; } = PhysicsType.Velocity;

        [field: SerializeField]
        public MoveType MoveType { get; private set; } = MoveType.Horizontal;

        [field: SerializeField]
        public ControlState ControlState { get; private set; } = ControlState.Controllable;

        [field: SerializeField]
        public HitState HitState { get; private set; } = HitState.Hittable;

        public virtual void SetPhysicsType(PhysicsType type)
        {
            PhysicsType = type;
        }

        public virtual void SetMoveType(MoveType type)
        {
            MoveType = type;
        }

        public virtual void SetControlState(ControlState state)
		{
            ControlState = state;
		}

        public virtual void SetHitState(HitState state)
        {
            HitState = state;
        }

        protected virtual void InitializeStateMachine()
		{
            SetMoveType(MoveType.Horizontal);
            SetControlState(ControlState.Controllable);
            SetHitState(HitState.Hittable);
        }
    }
}
