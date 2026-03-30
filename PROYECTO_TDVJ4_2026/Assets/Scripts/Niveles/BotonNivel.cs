using UnityEngine;
using UnityEngine.UI; // Obligatorio para manejar Botones de UI
using UnityEngine.SceneManagement; // Obligatorio para cargar escenas

public class BotonNivel : MonoBehaviour
{
    [Header("Configuración del Nivel")]
    [Tooltip("¿Qué número de nivel representa este botón? (Ej: 1, 2, 3...)")]
    public int numeroNivel; 

    [Tooltip("Escribe EXACTAMENTE el nombre de la Escena de juego que debe cargar este botón")]
    public string nombreDeLaEscenaAJugar;

    private Button miBoton;
    private Image imagenBoton; // Referencia opcional si quieres cambiar el aspecto visual

    void Start()
    {
        miBoton = GetComponent<Button>();
        
        // 🛑 NUEVO: Si no es la UI Clásica y usas TextMeshPro, a veces está en un hijo.
        imagenBoton = GetComponent<Image>();

        ActualizarEstadoBoton();
    }

    // 🔥 Esta es la función mágica que revisa el candado
    void ActualizarEstadoBoton()
    {
        // Preguntamos a PlayerPrefs cuál es el máximo nivel desbloqueado actual.
        // Si nunca hemos jugado, el valor por defecto será el Nivel 1.
        int maxNivelDesbloqueadoActual = PlayerPrefs.GetInt("MaxNivelDesbloqueado", 1);

        // Lógica del candado:
        // Si mi número de nivel es menor o igual al máximo desbloqueado, estoy activo.
        if (numeroNivel <= maxNivelDesbloqueadoActual)
        {
            miBoton.interactable = true; // El botón se puede presionar
            if (imagenBoton != null) imagenBoton.color = Color.white; // Color normal
        }
        else
        {
            miBoton.interactable = false; // El botón está grisado y no funciona
            if (imagenBoton != null) imagenBoton.color = new Color(0.5f, 0.5f, 0.5f, 0.8f); // Color gris y semitransparente (estilo bloqueado)
        }
    }

    // Esta función se enlazará en el evento 'On Click()' del botón en Unity
    public void CargarNivel()
    {
        // Solo cargamos la escena si PlayerPrefs nos confirma que está desbloqueado
        // (es una doble seguridad por si acaso)
        if (numeroNivel <= PlayerPrefs.GetInt("MaxNivelDesbloqueado", 1))
        {
            SceneManager.LoadScene(nombreDeLaEscenaAJugar);
        }
    }
}