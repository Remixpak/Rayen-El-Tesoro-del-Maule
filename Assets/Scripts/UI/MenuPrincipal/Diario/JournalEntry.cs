using System.Runtime.CompilerServices;
using UnityEngine;

//este script define una entrada del diario/libro (lo que incluye el titulo, texto e imagen) como un scriptableobject
//se puede crear una entrada mediante el editor waju

//recordar usar el tema de ulocked para desbloquear luego entradas en el diario al conversar con cierto npc o algo

//opcion del menu para crear una entrada nueva al crear un nuevo objeto
[CreateAssetMenu(fileName = "NewJournalEntry", menuName = "Game/Journal Entry")]
public class JournalEntry : ScriptableObject
{
    [SerializeField] private string id; //identificador para guardar (si esta vacio se usa el nombre del asset)
    [SerializeField] private string title;//titulo de la entrada
    [SerializeField, TextArea(5, 15)] private string body;//texto de la entrada
    [SerializeField] private Sprite image;//imagen de la entrada 
    [SerializeField] private bool unlockedFromStart; // entrada desbloqueada desde el inicio de la partida

    //propiedades de solo lectura para que otros scripts puedan leer los datos sin modificarlos

    public string Id => string.IsNullOrEmpty(id) ? name : id;
    public string Title => title;
    public string Body => body;
    public Sprite Image => image;
    public bool UnlockedFromStart => unlockedFromStart;



}
