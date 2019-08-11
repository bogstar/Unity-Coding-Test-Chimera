using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manager class used for handling audio.
/// </summary>
public class AudioManager : Manager<AudioManager>
{
    #region Private fields
    private AudioSource sfxSource;
    private AudioSource combatSource;
    #endregion

    #region Public methods
    /// <summary>
    /// Initialize Manager.
    /// </summary>
    public void Initialize()
    {
        combatSource = gameObject.AddComponent<AudioSource>();
        sfxSource = gameObject.AddComponent<AudioSource>();
    }
    
    /// <summary>
    /// Play sound using the combat audio source. Use during combat.
    /// </summary>
    /// <param name="clips"></param>
    public void PlayCombatSound(AudioClip[] clips)
    {
        combatSource.clip = clips[Random.Range(0, clips.Length)];
        combatSource.Play();
    }

    /// <summary>
    /// Play sound using the sfx audio source. Use during Game Over sequence.
    /// </summary>
    /// <param name="clips"></param>
    public void PlaySfxSound(AudioClip[] clips)
    {
        sfxSource.clip = clips[Random.Range(0, clips.Length)];
        sfxSource.Play();
    }
    #endregion
}