using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FSM
{
    /// <summary>
    /// �̵� ���
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
    /// ����� ����
    /// </summary>
    public enum ControlState
    {
        Controllable,
        Uncontrollable,
        Grappling,
    }

    /// <summary>
    /// �ǰ����� ����
    /// </summary>
    public enum HitState
	{
        Hittable,
        Unhittable,
    }

    public class StateMachine : MonoBehaviour
    {
        [field: SerializeField]
        public ControlState ControlState { get; private set; } = ControlState.Controllable;

        [field: SerializeField]
        public HitState HitState { get; private set; } = HitState.Hittable;

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
            SetControlState(ControlState.Controllable);
            SetHitState(HitState.Hittable);
        }
    }
}
