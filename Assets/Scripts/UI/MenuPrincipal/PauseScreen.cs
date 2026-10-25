using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// este script es la pantalla de pausa del nuevo sistema de menus: muestra los botones
// de reanudar, guardar, ajustes y salir, y tambien maneja el panel de confirmacion antes de salir
public class PauseScreen : MenuScreen
{
    [Header("Menú")]
    [SerializeField] private GameObject menuContent; // contenedor con los 4 botones
    [SerializeField] private Button resumeButton; // boton para reanudar la partida
    [SerializeField] private Button saveButton; // boton para guardar
    [SerializeField] private Button settingsButton; // boton para abrir los ajustes
    [SerializeField] private Button quitButton; // boton para salir del juego

    [Header("Pantallas")]
    [SerializeField] private MenuScreen settingsScreen; // pantalla de ajustes que se abre desde aqui

    [Header("Salir")]
    [SerializeField] private string mainMenuScene = ""; // si esta vacio cierra el juego, si tiene nombre carga esa escena
    [SerializeField] private GameObject confirmPanel; // panel de confirmacion (hermano de menuContent, no hijo)
    [SerializeField] private Button confirmYes; // boton "Si" del panel de confirmacion
    [SerializeField] private Button confirmNo; // boton "No" del panel de confirmacion

    public event Action SaveRequested; // aqui se conectara el sistema de guardado

    private void Awake()
    {
        // conectamos cada boton con su funcion correspondiente
        resumeButton.onClick.AddListener(() => Manager.CloseAll()); // reanudar = cerrar todo el menu
        saveButton.onClick.AddListener(OnSave);
        settingsButton.onClick.AddListener(() => Manager.Open(settingsScreen)); // abrir ajustes
        quitButton.onClick.AddListener(ShowConfirm); // salir = mostrar confirmacion primero
        confirmYes.onClick.AddListener(Quit);
        confirmNo.onClick.AddListener(HideConfirm);
    }

    // se llama cuando se muestra la pantalla
    public override void OnShown()
    {
        menuContent.SetActive(true); // mostramos el menu de botones
        confirmPanel.SetActive(false); // el panel de confirmacion empieza oculto
    }

    // devuelve el primer elemento seleccionable (el boton de reanudar)
    public override GameObject GetFirstSelected()
    {
        return resumeButton.gameObject;
    }

    // se llama al pulsar cancelar/volver: si el panel de confirmacion esta abierto lo cierra,
    // si no, devolvemos false para que el manager cierre la pausa y reanude el juego
    public override bool OnCancel()
    {
        if (confirmPanel.activeSelf)
        {
            HideConfirm();
            return true; // nosotros hemos gestionado el volver
        }

        return false; // que lo gestione el manager
    }

    // se llama al pulsar guardar
    private void OnSave()
    {
        SaveRequested?.Invoke(); // aqui se conectara el sistema de guardado
        Debug.Log("Guardar: pendiente. El estado está en GameSession.Data.");
    }

    // muestra el panel de confirmacion antes de salir
    private void ShowConfirm()
    {
        menuContent.SetActive(false); // ocultamos los botones
        confirmPanel.SetActive(true); // mostramos el panel de confirmacion

        // seleccionamos "No" por defecto para que no se salga sin querer
        EventSystem.current.SetSelectedGameObject(confirmNo.gameObject);
    }

    // oculta el panel de confirmacion y vuelve a los botones
    private void HideConfirm()
    {
        confirmPanel.SetActive(false); // ocultamos el panel
        menuContent.SetActive(true); // mostramos otra vez los botones

        // seleccionamos el boton de salir otra vez
        EventSystem.current.SetSelectedGameObject(quitButton.gameObject);
    }

    // funcion para salir del juego (o cargar la escena del menu principal)
    private void Quit()
    {
        // si hay una escena de menu principal asignada, la cargamos
        if (!string.IsNullOrEmpty(mainMenuScene))
        {
            SceneManager.LoadScene(mainMenuScene);
            return;
        }

#if UNITY_EDITOR
        // en el editor paramos el modo play para no tener que salir del editor
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit(); // en una build de verdad, cerramos el juego
#endif
    }
}