
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class TeclasManager : MonoBehaviour
{
    [SerializeField] private GameObject canvasMenu;

    void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.aKey.wasPressedThisFrame)
            AccionA();

        if (Keyboard.current.sKey.wasPressedThisFrame)
            AccionS();

        if (Keyboard.current.dKey.wasPressedThisFrame)
            AccionD();

        if (Keyboard.current.fKey.wasPressedThisFrame)
            AccionF();

        if (Keyboard.current.qKey.wasPressedThisFrame)
        {
            canvasMenu.SetActive(!canvasMenu.activeSelf);
        }
    }

    void AccionA()
    {
        SceneManager.LoadScene("Rio");
    }

    void AccionS()
    {
        SceneManager.LoadScene("Bosque");
    }

    void AccionD()
    {
        SceneManager.LoadScene("Pueblo");
    }

    void AccionF()
    {
        SceneManager.LoadScene("Pueblo2");
    }
}
