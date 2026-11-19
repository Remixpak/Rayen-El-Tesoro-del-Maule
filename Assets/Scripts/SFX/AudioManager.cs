using UnityEngine;
using UnityEngine.Audio;


public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioMixerGroup sfxGroup; // opcional
    [SerializeField, Min(1)] private int voices = 12;

    private AudioSource[] sources;
    private int next;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        sources = new AudioSource[voices];
        for (int i = 0; i < voices; i++)
        {
            var s = gameObject.AddComponent<AudioSource>();
            s.playOnAwake = false;
            s.spatialBlend = 0f; // 2D
            s.outputAudioMixerGroup = sfxGroup;
            sources[i] = s;
        }
    }

    public static void Play(SfxCue cue)
    {
        if (Instance == null || cue == null) return;
        if (!cue.TryGet(out var clip, out var vol, out var pitch)) return;

        var s = Instance.sources[Instance.next];
        Instance.next = (Instance.next + 1) % Instance.sources.Length;

        s.clip = clip;
        s.volume = vol;
        s.pitch = pitch;
        s.Play();
    }
}