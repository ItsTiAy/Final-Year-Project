using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    public Sound[] sounds;
    private Dictionary<string, Sound> audioSources = new Dictionary<string, Sound>();

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        // Checks to make sure there is only 1 instance of the audio manager
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // Sets all the sounds values from the editor
        foreach (Sound sound in sounds)
        {
            sound.source = gameObject.AddComponent<AudioSource>();
            sound.source.clip = sound.clip;

            sound.source.volume = sound.volume;
            sound.source.pitch = sound.pitch;
            sound.source.loop = sound.loop;
            sound.source.outputAudioMixerGroup = sound.mixerGroup;

            audioSources.Add(sound.name, sound);
        }
    }

    public void Play(string name)
    {
        Sound sound = audioSources[name];

        if (sound != null)
        {
            sound.source.PlayOneShot(sound.source.clip);
        }
    }

    public void Stop(string name)
    {
        Sound sound = audioSources[name];

        if (sound != null)
        {
            sound.source.Stop();
        }
    }

    public void PlayTrack(int trackNum)
    {
        Sound sound = audioSources["Track" + trackNum];

        if (sound != null)
        {
            sound.source.Play();
        }
    }

    public bool IsPlaying(string name)
    {
        Sound sound = audioSources[name];

        return sound.source.isPlaying;
    }

    public IEnumerator FadeOutTrack(int trackNum)
    {
        Sound sound = audioSources["Track" + trackNum];

        float FadeTime = 1;

        float startVolume = sound.source.volume;

        // Lowers volume of sound over time
        while (sound.source.volume > 0)
        {
            sound.source.volume -= startVolume * Time.unscaledDeltaTime / FadeTime;

            yield return null;
        }

        sound.source.Stop();
        sound.source.volume = startVolume;
    }
}
