using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBar : MonoBehaviour
{
    [SerializeField]
    private Slider slider;
    [SerializeField]
    private Gradient gradient;
    [SerializeField]
    private Image fill;
    [SerializeField]
    private Health playerHealth;

    private void Start()
    {
        SetMaxHealth(playerHealth.MaxHealth);
    }

    private void Update()
    {
        SetHealth(playerHealth.CurrentHealth);
    }

    public void SetMaxHealth(int value)
    {
        slider.maxValue = value;
        slider.value = value;

        fill.color = gradient.Evaluate(1f);
    }

    public void SetHealth(int value)
    {
        slider.value = value;

        fill.color = gradient.Evaluate(slider.normalizedValue);
    }
}
