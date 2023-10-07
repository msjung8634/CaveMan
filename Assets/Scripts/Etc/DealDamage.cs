using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DealDamage : MonoBehaviour
{
    [SerializeField]
    private string targetTag;

    [SerializeField]
    private int damage = 10;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(targetTag))
        {
            Health targetHealth = collision.GetComponent<Health>();
            targetHealth.GetDamage(damage);
        }
    }
}
