using UnityEngine;

//este escrit se encarga basicamente de ser la interfaz de daño para los enemigos, para que puedan recibir daño de los ataques del jugador


//este script define una interfaz que contiene un metodo TakeDamage que recibe un float como el amount (daño) y un vector3 como la direccion del golpe
public interface IDamageable
{
    void TakeDamage(float amount, Vector3 hitDirection); // un enemigo recibe daño y la direccion del golpe
}