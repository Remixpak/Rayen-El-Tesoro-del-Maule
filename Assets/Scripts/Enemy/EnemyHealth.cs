using UnityEngine;
using System.Collections;


//este script se eencarga de la vida del enemigo y el feedback visual al golpearlo ademas de su muerte (todo super basico como prototipo mientras tenemos assets)
public class EnemyHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth = 100f; // vida maxima del enemigo
    [SerializeField] private Renderer rend; // referencia al renderer del enemigo para cambiar el color al recibir daño(cuando se tenga)
    [SerializeField] private Color hitColor = Color.red; // color que se mostrara al recibir daño 
    [SerializeField] private float hitDuration = 0.1f; // duracion del color de golpe

    private float health; // vida actual del enemigo
    private Color baseColor; // color base del enemigo
    private Coroutine hit; // referencia a la corrutina del golpe para poder detenerla si se recibe otro golpe antes de que termine

    private Material mat; // referencia al material del enemigo para cambiar el color al recibir daño



    private void Awake()
    {
        health = maxHealth;
        if (rend == null) rend = GetComponentInChildren<Renderer>(); // si no se asigna el renderer en el inspector, se busca en los hijos del enemigo
        if (rend != null) // si se encuentra el renderer se obtiene el color del material despues de recibir daño
        {
            mat = rend.material;
            baseColor = mat.color;
        }
    }

    //funcion que maneja el daño que recibe el enemigo
    public void TakeDamage(float amount, Vector3 hitDirection)
    {
        health -= amount;
        Debug.Log($"{name} recibió {amount} de daño. Vida: {health}/{maxHealth}");

        //si el enemigo tiene un renderer este cambia de color al recibir daño y vuelve a su color original despues de un tiempo
        if (rend != null)
        {
            if (hit != null)
            {
                StopCoroutine(hit);
                hit = null;
            }
            hit = StartCoroutine(Hit());
        }

        //si el enemigo no tiene vida se muere basicamente
        if (health <= 0)
        {
            Die();
        }

    }

    // corrutina que cambia el color del enemigo al recibir daño y vuelve a su color original despues de un tiempo
    private IEnumerator Hit ()
    {
        mat.color = hitColor;
        yield return new WaitForSeconds(hitDuration);
        mat.color = baseColor;
        hit = null;
    }

    //funcion para la muerte del enemigo
    private void Die()
    {
        Debug.Log($"{name} ha muerto.");
        Destroy(gameObject);
    }

}
