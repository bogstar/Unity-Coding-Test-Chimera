using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : Manager<AudioManager>
{
    private AudioSource sfxSource;
    private AudioSource combatSource;

    public void Initialize()
    {
        combatSource = gameObject.AddComponent<AudioSource>();
        sfxSource = gameObject.AddComponent<AudioSource>();
    }

    public void PlayCombatSound(AudioClip[] clips)
    {
        combatSource.clip = clips[Random.Range(0, clips.Length)];
        combatSource.Play();
    }

    public void PlaySfxSound(AudioClip[] clips)
    {
        sfxSource.clip = clips[Random.Range(0, clips.Length)];
        sfxSource.Play();
    }
}