using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Audio : MonoBehaviour
{
    [Header("Sound references:")]
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip landingSound;
    [SerializeField] private AudioClip bounceSound;
    [SerializeField] private AudioClip windSound;

    public void JumpSound()
    {
        SoundManager.instance.PlaySound(jumpSound);
    }

    public void LandingSound()
    {
        SoundManager.instance.PlaySound(landingSound);
    } 

    public void BounceSound()
    {
        SoundManager.instance.PlaySound(bounceSound);
    }

    public void WindSound()
    {
        SoundManager.instance.PlaySound(windSound);
    }
}
