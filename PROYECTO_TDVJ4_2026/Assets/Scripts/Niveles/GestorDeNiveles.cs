using UnityEngine;
using UnityEngine.SceneManagement;

public class GestorDeNiveles : MonoBehaviour
{
    [Header("Configuración de Navegación")]
    [Tooltip("Escribe EXACTAMENTE el nombre de la escena del siguiente nivel de este modo (Ej: ModoFisico_Nivel2)")]
    public string nombreSiguienteNivel;

    public void ReiniciarNivel()
    {
        // Carga la escena actual otra vez
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void SiguienteNivel()
    {
        // 1. Revisamos si el diseñador escribió un nombre específico en el Inspector
        if (!string.IsNullOrEmpty(nombreSiguienteNivel))
        {
            SceneManager.LoadScene(nombreSiguienteNivel);
        }
        else
        {
            // 2. Si lo dejaste en blanco por error, usamos el sistema viejo de sumar 1 (como plan B)
            Debug.LogWarning("⚠️ No escribiste el nombre del siguiente nivel. Usando índice + 1.");
            int nivelActual = SceneManager.GetActiveScene().buildIndex;
            SceneManager.LoadScene(nivelActual + 1); 
        }
    }

    public void IrAlMenuPrincipal()
    {
        // Escribe aquí exactamente cómo se llama tu escena de menú
        SceneManager.LoadScene("Menu"); 
    }
}