using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    #region Variables

    public AudioSource Music;

    public AudioSource Sound;

    /// <summary>
    /// List with all music clips
    /// 0 - main menu music (while user is in main menu)
    /// 1 - game music (while match is during)
    /// </summary>
    public List<AudioClip> MusicClips;

    /// <summary>
    /// List with all sound clips
    /// 0 - button click
    /// 1 - dices throwing
    /// 2 - dice select
    /// 3 - pen scratch
    /// 4 - game found
    /// 5 - victory
    /// 6 - defeat
    /// </summary>
    public List<AudioClip> SoundClips;

    /// <summary>
    /// Music which is keep playing
    /// </summary>
    private AudioClip PlayingMusic;

    #endregion

    #region Monodevelop constructions

    private void FixedUpdate()
    {
        if (!Music.isPlaying)
        {
            Music.PlayOneShot(PlayingMusic);
        }
    }

    #endregion

    /// <summary>
    /// Sets music volume
    /// </summary>
    /// <param name="value">Music volume (0.0 - 1.0)</param>
    public void SetMusicVolume(float value)
    {
        Music.volume = value;
    }

    /// <summary>
    /// Sets sound volume
    /// </summary>
    /// <param name="value">Sound volume (0.0 - 1.0)</param>
    public void SetSoundVolume(float value)
    {
        Sound.volume = value;
    }

    public void PlayMenuMusic()
    {
        if (Music.isPlaying)
        {
            Music.Stop();
        }
        PlayingMusic = MusicClips[0];
        Music.PlayOneShot(PlayingMusic);
    }

    public void PlayGameMusic()
    {
        if (Music.isPlaying)
        {
            Music.Stop();
        }
        PlayingMusic = MusicClips[1];
        Music.PlayOneShot(PlayingMusic);
    }

    public void PlayButtonClick()
    {
        Sound.PlayOneShot(SoundClips[0]);
    }

    public void PlayDiceThrow()
    {
        Sound.PlayOneShot(SoundClips[1]);
    }

    public void PlayDiceSelect()
    {
        Sound.PlayOneShot(SoundClips[2]);
    }

    public void PlayPenScratch()
    {
        Sound.PlayOneShot(SoundClips[3]);
    }

    public void PlayGameFound()
    {
        Sound.PlayOneShot(SoundClips[4]);
    }
    public void PlayVictorySound()
    {
        Sound.PlayOneShot(SoundClips[5]);
    }
    public void PlayDefeatSound()
    {
        Sound.PlayOneShot(SoundClips[6]);
    }
}
