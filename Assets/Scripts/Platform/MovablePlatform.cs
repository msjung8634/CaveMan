using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovablePlatform : MonoBehaviour
{
    [Header("Preset")]
    [SerializeField]
    private bool isLeftStart = true;
    [SerializeField]
    private float moveRange = 10f;
    [Header("Modifiable")]
    [SerializeField]
    private float frequency = 1f;
    [field:SerializeField]
    public bool IsStopped { get; set; } = false;
    [field: SerializeField]
    public bool IsReversed { get; set; } = false;

    private float tau = Mathf.PI * 2;
    private Vector3 startPos;
    private Vector3 endPos;

    private void Start()
    {
        startPos = transform.position - Vector3.right * moveRange;
        endPos = transform.position + Vector3.right * moveRange;
        StartCoroutine(Oscillate());
    }

    private IEnumerator Oscillate()
    {
        float elapsedTime = -Mathf.PI / 2;
        if (isLeftStart)
        {
            elapsedTime = Mathf.PI / 2;
        }
        
        float value = 0;
        while (true)
        {
            if (!IsStopped)
            {
                elapsedTime += frequency * tau * Time.deltaTime;
                value = IsReversed ? Mathf.Sin(elapsedTime) / 2 + 0.5f : Mathf.Cos(elapsedTime) / 2 + 0.5f;
                transform.position = Vector3.Lerp(startPos, endPos, value);
            }

            // Grappling Hook을 FixedUpdate에서 관리하므로 동기화
            yield return new WaitForFixedUpdate();
        }
    }
}
