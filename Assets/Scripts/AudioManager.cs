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

        foreach (Sound sound in sounds)
        {
            sound.source = gameObject.AddComponent<AudioSource>();
            sound.source.clip = sound.clip;

            sound.source.volume = sound.volume;
            sound.source.pitch = sound.pitch;
            sound.source.loop = sound.loop;

            audioSources.Add(sound.name, sound);
        }
    }

    public void Play(string name)
    {
        Sound sound = audioSources[name];

        //sound.source.pitch = Random.Range(0.95f, 1.05f);

        if (sound != null)
        {
            sound.source.PlayOneShot(sound.source.clip);
        }
    }
}
