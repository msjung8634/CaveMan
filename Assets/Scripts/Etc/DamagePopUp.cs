using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DamagePopUp : MonoBehaviour
{
    [Header("피격 데미지 표시")]
    [SerializeField]
    private GameObject damagePopupPrefab;
    [SerializeField]
    private float offsetX = 0f;
    [SerializeField]
    private float offsetY = 1.2f;
    [SerializeField]
    [Tooltip("회전 각도")]
    private float angle = 5f;
    [SerializeField]
    [Tooltip("지속 시간")]
    private float duration = .2f;

    private Canvas canvas;

    private void Awake()
    {
        canvas = FindObjectOfType<Canvas>();
    }

    public void PopUp(int damage)
    {
        StartCoroutine(ShowDamage(damage));
    }

    private IEnumerator ShowDamage(int damage)
    {
        if (damagePopupPrefab == null)
            yield break;

        Vector3 pos = Camera.main.WorldToScreenPoint(transform.position + new Vector3(offsetX, offsetY, 0));
        GameObject obj = Instantiate(damagePopupPrefab, pos, Quaternion.Euler(0, 0, angle));
        obj.GetComponent<Text>().text = damage.ToString();
        obj.transform.SetParent(canvas.transform);

        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Destroy(obj);
    }
}
