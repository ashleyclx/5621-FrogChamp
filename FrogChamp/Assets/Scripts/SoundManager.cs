using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance { get; private set; }
    private AudioSource audioSource;

    private void Awake()
    {
        instance = this;
        audioSource = GetComponent<AudioSource>();
    }

    public void PlaySound(AudioClip _sound)
    {
        audioSource.PlayOneShot(_sound);
    }
    
}
