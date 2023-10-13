using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Player;

[RequireComponent(typeof(PlayerControl))]
public class CameraShake : MonoBehaviour
{
    [SerializeField]
    private CinemachineVirtualCamera virtualCamera;
    private CinemachineBasicMultiChannelPerlin noise;

    [Header("OnAttack")]
    [SerializeField]
    private float attackAmplitude = 1f;
    [SerializeField]
    private float attackFrequency = 1f;

    [Header("OnHit")]
    [SerializeField]
    private float hitAmplitude = 1f;
    [SerializeField]
    private float hitFrequency = 1f;

    [Header("OnDodge")]
    [SerializeField]
    private float dodgeAmplitude = 1f;
    [SerializeField]
    private float dodgeFrequency = 1f;

    [Header("OnGrapple")]
    [SerializeField]
    private float grappleAmplitude = 1f;
    [SerializeField]
    private float grappleFrequency = 1f;


    private PlayerControl playerControl;

    private void Awake()
    {
        TryGetComponent(out playerControl);
        noise = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }

    private void Start()
    {
        playerControl.OnAttack += OnAttack;
        playerControl.OnHit += OnHit;
        playerControl.OnDodge += OnDodge;
        playerControl.OnGrapple += OnGrapple;

        playerControl.OnAttackFinish += ResetNoise;
        playerControl.OnHitFinish += ResetNoise;
        playerControl.OnDodgeFinish += ResetNoise;
        playerControl.OnGrappleFinish += ResetNoise;
    }

    private void OnAttack()
    {
        noise.m_AmplitudeGain = attackAmplitude;
        noise.m_FrequencyGain = attackFrequency;
    }
    private void OnHit()
    {
        noise.m_AmplitudeGain = hitAmplitude;
        noise.m_FrequencyGain = hitFrequency;
    }
    private void OnDodge()
    {
        noise.m_AmplitudeGain = dodgeAmplitude;
        noise.m_FrequencyGain = dodgeFrequency;
    }
    private void OnGrapple()
    {
        noise.m_AmplitudeGain = grappleAmplitude;
        noise.m_FrequencyGain = grappleFrequency;
    }

    private void ResetNoise()
    {
        noise.m_AmplitudeGain = 0;
        noise.m_FrequencyGain = 0;
    }
}
