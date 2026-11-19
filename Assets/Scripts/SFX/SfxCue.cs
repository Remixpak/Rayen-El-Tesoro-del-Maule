using UnityEngine;

[CreateAssetMenu(menuName = "Audio/Sfx Cue")]
public class SfxCue : ScriptableObject
{
    public AudioClip[] clips;
    [Range(0f, 1f)] public float volume = 1f;
    public Vector2 pitchRange = new Vector2(0.95f, 1.05f);
    public float cooldown = 0.05f; // evita que suene 5 veces en el mismo instante

    [System.NonSerialized] private float lastPlayTime;

    private void OnEnable() { lastPlayTime = -999f; }

    public bool TryGet(out AudioClip clip, out float vol, out float pitch)
    {
        clip = null; vol = volume; pitch = 1f;

        if (clips == null || clips.Length == 0) return false;
        if (Time.unscaledTime - lastPlayTime < cooldown) return false;

        lastPlayTime = Time.unscaledTime;
        clip = clips[Random.Range(0, clips.Length)];
        pitch = Random.Range(pitchRange.x, pitchRange.y);
        return clip != null;
    }
}