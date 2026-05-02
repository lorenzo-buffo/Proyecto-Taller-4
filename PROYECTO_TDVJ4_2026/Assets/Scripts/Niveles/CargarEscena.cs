using UnityEngine;
using UnityEngine.SceneManagement;

public class CargarEscena : MonoBehaviour
{
    // Nombre de la escena a cargar (lo escribís en el Inspector)
    public string nombreEscena;

    public void Cargar()
    {
        if (!string.IsNullOrEmpty(nombreEscena))
        {
            SceneManager.LoadScene(nombreEscena);
        }
        else
        {
            Debug.LogWarning("No se asignó nombre de escena.");
        }
    }
}