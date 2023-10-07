using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lockPositionToPlayer : MonoBehaviour
{
    [SerializeField]
    [Range(1f, 10f)]
    private float dampRate = 2f;

    private Transform player;
    private Vector3 lastPosition;
    private bool isFollowing = false;

    private void Awake()
    {
        GameObject.FindGameObjectWithTag("Player").TryGetComponent(out player);
    }

    private void Update()
    {
        if (!isFollowing
            && lastPosition.x != player.position.x)
        {
            lastPosition = new Vector3(player.position.x, 0, 0);
            StartCoroutine(FollowPosition());
        }
    }

    private IEnumerator FollowPosition()
    {
        isFollowing = true;

        float progress = 0f;
        while (progress < 1f)
        {
            progress += Time.deltaTime;
            var start = transform.position;
            var target = new Vector3(player.position.x / dampRate, transform.position.y, 0);
            transform.position = Vector3.Lerp(start, target, progress);
            
            yield return null;
        }

        isFollowing = false;
    }
}
