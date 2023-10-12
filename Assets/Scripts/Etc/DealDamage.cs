using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FSM;

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
            StateMachine targetFSM = collision.gameObject.GetComponent<StateMachine>();
            if (targetFSM.HitState == FSM.HitState.Hittable)
            {
                Health targetHealth = collision.GetComponent<Health>();
                targetHealth.GetDamage(damage);
            }
        }
    }
}
