using UnityEngine;
using UnityEngine.SceneManagement;

public class GestorDeNiveles : MonoBehaviour
{
    public void ReiniciarNivel()
    {
        // Carga la escena actual otra vez
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void SiguienteNivel()
    {
        // 1. Averiguamos el número de nuestra escena actual
        int nivelActual = SceneManager.GetActiveScene().buildIndex;
        
        // 2. Le sumamos 1 para saber cuál es el siguiente
        int siguienteNivel = nivelActual + 1;

        // 3. Cargamos ese número
        SceneManager.LoadScene(siguienteNivel); 
    }

    public void IrAlMenuPrincipal()
    {
        // Escribe aquí exactamente cómo se llama tu escena de menú
        SceneManager.LoadScene("Menu"); 
    }
}