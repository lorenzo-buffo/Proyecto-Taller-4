using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SelectorModos : MonoBehaviour
{
    [Header("Configuración de Escenas")]
    public string escenaModoFisico = "ModoFisicoNiveles";
    public string escenaModoLogico = "ModoLogicoNiveles";

    [Header("Efectos de Botón (Opcional)")]
    public float escalaAlPulsar = 0.95f;
    public float velocidadEscala = 10f;

    // Función para el botón superior
    public void SeleccionarModoFisico()
    {
        // Aquí podrías guardar una preferencia si quieres
        PlayerPrefs.SetString("UltimoModoJugado", "Fisico");
        SceneManager.LoadScene(escenaModoFisico);
    }

    // Función para el botón inferior
    public void SeleccionarModoLogico()
    {
        PlayerPrefs.SetString("UltimoModoJugado", "Logico");
        SceneManager.LoadScene(escenaModoLogico);
    }
}