using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private SfxCue footstep;
    [SerializeField] private SfxCue jump;
    [SerializeField] private SfxCue land;

    [Header("Combate")]
    [SerializeField] private SfxCue attackSwing;
    [SerializeField] private SfxCue shoot;
    [SerializeField] private SfxCue emptyAmmo;
    [SerializeField] private SfxCue switchWeapon;

    [Header("Estado")]
    [SerializeField] private SfxCue hurt;
    [SerializeField] private SfxCue death;
    [SerializeField] private SfxCue heal;

    [Header("Otros")]
    [SerializeField] private SfxCue lockOn;
    [SerializeField] private SfxCue lockOff;
    [SerializeField] private SfxCue interact;
    [SerializeField] private SfxCue interactChest;
    [SerializeField] private SfxCue pickup;


    // Públicos y sin parámetros: se pueden llamar desde Animation Events
    public void Footstep() => AudioManager.Play(footstep);
    public void Jump() => AudioManager.Play(jump);
    public void Land() => AudioManager.Play(land);
    public void AttackSwing() => AudioManager.Play(attackSwing);
    public void Shoot() => AudioManager.Play(shoot);
    public void EmptyAmmo() => AudioManager.Play(emptyAmmo);
    public void SwitchWeapon() => AudioManager.Play(switchWeapon);
    public void Hurt() => AudioManager.Play(hurt);
    public void Death() => AudioManager.Play(death);
    public void Heal() => AudioManager.Play(heal);
    public void LockOn() => AudioManager.Play(lockOn);
    public void LockOff() => AudioManager.Play(lockOff);
    public void Interact() => AudioManager.Play(interact);
    public void InteractChest() => AudioManager.Play(interactChest);
    public void Pickup() => AudioManager.Play(pickup);
}