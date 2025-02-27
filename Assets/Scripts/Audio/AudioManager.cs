using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    private static AudioManager _instance;
    public static AudioManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<AudioManager>();
            }
            return _instance;
        }
    }

    [System.Serializable]
    public class MusicTrack
    {
        public string name;
        public AudioClip clip;
        public float volume = 1f;
        public bool loop = true;
    }

    [Header("Music Settings")]
    [SerializeField] private MusicTrack[] musicTracks;
    [SerializeField] private float crossFadeDuration = 1f;
    [SerializeField] private float masterVolume = 1f;
    
    private AudioSource[] musicSources; // Two sources for crossfading
    private int activeMusicIndex = 0;
    private float musicTransitionTime = 0f;
    private bool isTransitioning = false;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            SetupAudioSources();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void SetupAudioSources()
    {
        musicSources = new AudioSource[2];
        for (int i = 0; i < 2; i++)
        {
            musicSources[i] = gameObject.AddComponent<AudioSource>();
            musicSources[i].playOnAwake = false;
            musicSources[i].loop = true;
        }
    }

    public void PlayMusic(string trackName)
    {
        MusicTrack track = System.Array.Find(musicTracks, t => t.name == trackName);
        if (track == null)
        {
            Debug.LogWarning($"Music track {trackName} not found!");
            return;
        }

        // If same track is already playing, don't restart
        if (musicSources[activeMusicIndex].clip == track.clip && 
            musicSources[activeMusicIndex].isPlaying)
            return;

        StartCoroutine(CrossFadeMusic(track));
    }

    private System.Collections.IEnumerator CrossFadeMusic(MusicTrack newTrack)
    {
        isTransitioning = true;
        int nextSource = 1 - activeMusicIndex;

        // Setup next track
        musicSources[nextSource].clip = newTrack.clip;
        musicSources[nextSource].volume = 0f;
        musicSources[nextSource].loop = newTrack.loop;
        musicSources[nextSource].Play();

        // Crossfade
        float transitionTime = 0f;
        while (transitionTime < crossFadeDuration)
        {
            transitionTime += Time.deltaTime;
            float t = transitionTime / crossFadeDuration;

            musicSources[activeMusicIndex].volume = Mathf.Lerp(masterVolume, 0f, t);
            musicSources[nextSource].volume = Mathf.Lerp(0f, newTrack.volume * masterVolume, t);

            yield return null;
        }

        // Stop old track
        musicSources[activeMusicIndex].Stop();
        activeMusicIndex = nextSource;
        isTransitioning = false;
    }

    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        if (!isTransitioning)
        {
            musicSources[activeMusicIndex].volume = masterVolume;
        }
    }
} 