using UnityEngine;
using UnityEngine.UI;

// este script es la pantalla de ajustes del nuevo sistema de menus: de momento solo tiene
// un slider de volumen y un boton para volver a la pantalla anterior

//igual no se si tendremos configuraciones pero esto existira igual :p
public class SettingsScreen : MenuScreen
{
    [SerializeField] private Slider volumeSlider; // slider para ajustar el volumen
    [SerializeField] private Button backButton; // boton para volver atras

    private const string VolumeKey = "MasterVolume"; // clave con la que se guarda el volumen en PlayerPrefs

    private void Awake()
    {
        // conectamos el slider y el boton con sus funciones
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        backButton.onClick.AddListener(() => Manager.Back()); // volver = pedirle al manager que retroceda
    }

    // se llama cuando se muestra la pantalla
    public override void OnShown()
    {
        // recuperamos el volumen guardado (si no hay nada guardado, ponemos 1)
        float volume = PlayerPrefs.GetFloat(VolumeKey, 1f);

        // ponemos el slider en esa posicion SIN disparar el evento (para no guardar dos veces)
        volumeSlider.SetValueWithoutNotify(volume);
        AudioListener.volume = volume; // aplicamos el volumen
    }

    // devuelve el primer elemento seleccionable (el slider de volumen)
    public override GameObject GetFirstSelected() => volumeSlider.gameObject;

    // se llama cuando el jugador mueve el slider
    private void OnVolumeChanged(float value)
    {
        AudioListener.volume = value; // aplicamos el volumen
        PlayerPrefs.SetFloat(VolumeKey, value); // lo guardamos para la proxima vez
    }
}