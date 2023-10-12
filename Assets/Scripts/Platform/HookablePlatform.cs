using Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookablePlatform : MonoBehaviour, IRaycastable
{
    [field:SerializeField]
    public Vector3 HookPoint { get; set; } = Vector3.zero;

    public CursorType GetCursorType()
    {
        return CursorType.Hookable;
    }

    public bool HandleRaycast(PlayerControl playerControl)
    {
        return false;
    }

    private void Update()
    {
        if (TryGetComponent(out SpriteRenderer renderer))
        {
            renderer.color = Color.white;
        }
    }

    public void Hook(PlayerControl playerControl, HookablePlatform platform)
    {
        if (playerControl.LastHookTarget == null)
        {
            playerControl.TryGetComponent(out PlayerStateMachine stateMachine);
            stateMachine.SetControlState(FSM.ControlState.Grappling);

            playerControl.TryGetComponent(out GrapplingHook grappling);
            grappling.Hook(playerControl, this);

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
            grappling.UnHook(playerControl, this);

            playerControl.LastHookTarget = null;
            playerControl.ResetGrappleCoolDown();
        }
    }
}
