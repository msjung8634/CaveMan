using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Player;

[RequireComponent(typeof(DistanceJoint2D))]
[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(PlayerControl))]
public class GrapplingHook : MonoBehaviour
{
    private DistanceJoint2D distanceJoint = null;
    private LineRenderer lineRenderer = null;
    private PlayerControl playerControl = null;

    private void Awake()
    {
        TryGetComponent(out distanceJoint);
        TryGetComponent(out lineRenderer);
        TryGetComponent(out playerControl);
    }

    private void Start()
    {
        distanceJoint.enabled = false;
        lineRenderer.enabled = false;
    }

    private void Update()
    {
        if (playerControl.LastHookTarget != null)
        {
            Vector3 playerPos = transform.position;
            playerPos = new Vector3(playerPos.x, playerPos.y, -1);
            lineRenderer.SetPosition(0, playerPos);
        }
    }

    public void Hook(HookablePlatform platform)
    {
        // 선 연결
        lineRenderer.positionCount = 2;
        Vector3 platformPos = platform.transform.position;
        platformPos = new Vector3(platformPos.x, platformPos.y, -1);
        Vector3 playerPos = transform.position;
        playerPos = new Vector3(playerPos.x, playerPos.y, -1);
        lineRenderer.SetPosition(0, playerPos);
        lineRenderer.SetPosition(1, platformPos);

        distanceJoint.distance = Vector3.Distance(transform.position, platformPos);
        distanceJoint.connectedAnchor = platformPos;
        playerControl.LastHookTarget = platform;

        distanceJoint.enabled = true;
        lineRenderer.enabled = true;
    }

    public void UnHook()
    {
        // 단선
        lineRenderer.positionCount = 0;
        playerControl.LastHookTarget = null;

        distanceJoint.enabled = false;
        lineRenderer.enabled = false;
    }
}
