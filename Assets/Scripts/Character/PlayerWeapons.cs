using UnityEngine;
using System;

public enum WeaponType { Scythe, gun} // enumerator con los dos tipos de armas que puede tener el jugador

[RequireComponent(typeof(PlayerInputHandler))]
public class PlayerWeapons : MonoBehaviour
{
    [Header("Armas")]
    [SerializeField] private WeaponType startWeapon = WeaponType.Scythe; // iniciamos con la hoz como arma principal
    [SerializeField] private GameObject ScytheModel; // objeto visual hoz (poner sprite real ya que es un cubo mientras)
    [SerializeField] private GameObject gunModel;// mismo caso

    private PlayerInputHandler inputs; // referencia al script de inputs del jugador 
    private PlayerAttack scythe; //referencia al scrypt que maneja el ataque melee
    private PlayerShooting gun; // referencia al script que maneja el disparo del arma

    public WeaponType Current { get; private set; } // arma actual del jugador 

    //para que el UI o las animaciones sepan del cambio de arma
    public event Action<WeaponType> WeaponChanged;

    private void Awake()
    {
        //tomamos las referencias  a los scripts que estan en el mismo gameobject
        inputs = GetComponent<PlayerInputHandler>();
        scythe = GetComponent<PlayerAttack>();
        gun = GetComponent<PlayerShooting>();
    }

    //al empezar el juego ponemos esa arma inicial
    private void Start() => SetWeapon(startWeapon);

    private void Update()
    {
        if (inputs.swtichWeapon)//si el jugador presiona el boton cambia el arma 
        {
            inputs.swtichWeapon = false;

            if (scythe == null || !scythe.isAttacking) //solo permitimos cambiar el arma si es que no esta atacando 
            {
                SwitchWeapon();
            }
        }

        if (inputs.attack) // si el jugador ataca se consume el input para no repetirlo 
        {
            inputs.attack = false;

            switch (Current) //segun el arma actual se ataca de una forma u otra 
            {
                case WeaponType.Scythe:
                    scythe?.TryAttack();
                    break;
                case WeaponType.gun:
                    gun?.TryShoot();
                    break;
            }
        }
    }

    //cambia el arma 
    private void SwitchWeapon()
    {
        SetWeapon(Current == WeaponType.Scythe ? WeaponType.gun : WeaponType.Scythe);
    }

    //aplica el arma nueva cambiando su modelo y avisa a quien escuche el evento
    private void SetWeapon(WeaponType type)
    {
        Current = type; //guardamos el arma actual 


        //activamos el modelo del arma 
        if (ScytheModel != null)
        {
            ScytheModel.SetActive(type == WeaponType.Scythe);
        }

        if (gunModel != null)
        {
            gunModel.SetActive(type == WeaponType.gun);
        }

        Debug.Log($"Se cambió el arma actual a: {type}"); // feedback para asegurarse que se cambio

        WeaponChanged?.Invoke(type); //lanzamos el evento para que otros scripts lo escuchen 
    }


}
