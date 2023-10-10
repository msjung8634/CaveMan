using Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookablePlatform : MonoBehaviour, IRaycastable
{
    public CursorType GetCursorType()
    {
        return CursorType.Hookable;
    }

    public bool HandleRaycast(PlayerControl playerControl)
    {
        return false;
    }

    public void Hook(PlayerControl playerControl, HookablePlatform platform)
    {
        if (playerControl.LastHookTarget == null)
        {
            playerControl.TryGetComponent(out PlayerStateMachine stateMachine);
            stateMachine.SetControlState(FSM.ControlState.Grappling);

            playerControl.TryGetComponent(out GrapplingHook grappling);
            grappling.Hook(this);

            playerControl.LastHookTarget = platform;
        }
    }

    public void Unhook(PlayerControl playerControl)
    {
        if (playerControl.LastHookTarget != null)
        {
            playerControl.TryGetComponent(out PlayerStateMachine stateMachine);
            stateMachine.SetControlState(FSM.ControlState.Controllable);

            playerControl.TryGetComponent(out GrapplingHook grappling);
            grappling.UnHook();

            playerControl.LastHookTarget = null;
        }
    }
}