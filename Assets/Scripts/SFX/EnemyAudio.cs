using UnityEngine;

//script que maneja el audio del enemigo
public class EnemyAudio : MonoBehaviour
{
    [Header("Estado")]
    [SerializeField] private SfxCue hurt;
    [SerializeField] private SfxCue death;

    [Header("Combate")]
    [SerializeField] private SfxCue attackMelee;
    [SerializeField] private SfxCue attackRanged;
    [SerializeField] private SfxCue alert; // cuando te ve

    public void PlayHurt() => AudioManager.Play(hurt);
    public void PlayDeath() => AudioManager.Play(death);
    public void PlayAttackRanged() => AudioManager.Play(attackRanged);
    public void PlayAttackMelee() => AudioManager.Play(attackMelee);
    public void PlayAlert() => AudioManager.Play(alert);
}