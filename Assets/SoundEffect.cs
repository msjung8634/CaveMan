using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Player;

[RequireComponent(typeof(PlayerStateMachine))]
[RequireComponent(typeof(PlayerControl))]
[RequireComponent(typeof(AudioSource))]
public class SoundEffect : MonoBehaviour
{
    [SerializeField] private AudioClip attackClip;
    [SerializeField] private AudioClip dodgeClip;
    [SerializeField] private AudioClip grappleClip;
    [SerializeField] private AudioClip hitClip;

    private PlayerStateMachine stateMachine;
    private PlayerControl control;
    private AudioSource source;

    private void Awake()
    {
        TryGetComponent(out stateMachine);
        TryGetComponent(out control);
        TryGetComponent(out source);
    }

    private void Start()
    {
        // Add to event
        control.OnAttack += OnAttack;
        control.OnHit += OnHit;
        control.OnDodge += OnDodge;
        control.OnGrapple += OnGrapple;
    }
    
    private void OnAttack()
    {
        source.PlayOneShot(attackClip);
    }
    private void OnHit()
    {
        source.PlayOneShot(hitClip);
    }
    private void OnDodge()
    {
        source.PlayOneShot(dodgeClip);
    }
    private void OnGrapple()
    {
        if (stateMachine.ControlState != FSM.ControlState.Grappling)
        {
            source.PlayOneShot(grappleClip);
        }
    }
}
