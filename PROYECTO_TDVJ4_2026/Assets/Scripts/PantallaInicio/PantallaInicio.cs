using UnityEngine;
using UnityEngine.SceneManagement; // Obligatorio para cambiar de escenas

public class PantallaInicio : MonoBehaviour
{
    [Header("Configuración de Escena")]
    [Tooltip("Escribe EXACTAMENTE el nombre de la escena a la que querés ir (ej: 'Menu' o 'Nivel1')")]
    public string nombreEscenaSiguiente = "Menu";

    [Header("Efecto de Texto")]
    [Tooltip("Arrastra aquí el objeto de texto que tiene el componente Canvas Group")]
    public CanvasGroup grupoTexto; 
    public float velocidadParpadeo = 1.5f;

    // Seguro para no intentar cargar la escena dos veces si el jugador toca muy rápido
    private bool cargandoEscena = false; 

    void Update()
    {
        // 1. Efecto Fade In / Fade Out automático
        if (grupoTexto != null)
        {
            // Mathf.PingPong va de 0 a 1 y vuelve a bajar a 0 infinitamente, 
            // creando un efecto de latido o parpadeo suave perfecto para el alpha (transparencia).
            float transparencia = Mathf.PingPong(Time.time * velocidadParpadeo, 1f);
            
            // Para que no desaparezca AL 100% (y el jugador siempre vea algo), 
            // le sumamos un mínimo (ej. 0.2f) y lo limitamos.
            grupoTexto.alpha = Mathf.Clamp(transparencia + 0.2f, 0f, 1f); 
        }

        // 2. Detectar el toque en la pantalla
        // Funciona tanto con el click del mouse en la PC como con el dedo en el celular
        if (!cargandoEscena && (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)))
        {
            cargandoEscena = true;
            CargarSiguienteEscena();
        }
    }

    void CargarSiguienteEscena()
    {
        SceneManager.LoadScene(nombreEscenaSiguiente);
    }
}