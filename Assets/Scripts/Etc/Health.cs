using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(DamagePopUp))]
public class Health : MonoBehaviour
{
    [field:SerializeField]
    [field: Range(10, 200)]
    public int MaxHealth { get; private set; } = 10;
    [field:SerializeField]
    public int CurrentHealth { get; private set; }

    private DamagePopUp damagePopUp;

    private void Awake()
    {
        TryGetComponent(out damagePopUp);
    }

    void Start()
    {
        CurrentHealth = MaxHealth;
    }

    public void GetDamage(int value)
    {
        CurrentHealth = Mathf.Max(CurrentHealth - value, 0);

        // 피격 데미지 표시
        damagePopUp.PopUp(value);

        if (CurrentHealth == 0)
        {
            //gameObject.SetActive(false);
            Debug.Log($"{gameObject.name} is Dead");
        }
    }

    public void Heal(int value)
    {
        CurrentHealth = Mathf.Min(CurrentHealth + value, MaxHealth);
    }
}
