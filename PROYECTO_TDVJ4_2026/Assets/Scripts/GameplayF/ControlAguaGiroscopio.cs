using UnityEngine;

public class ControlAguaGiroscopio : MonoBehaviour
{
    [Header("Control de la Corriente")]
    [Tooltip("La fuerza normal con la que el agua cae hacia abajo")]
    public float caidaFuerza = -9.81f; 
    
    [Tooltip("Qué tan fuerte el celular empuja el agua hacia los lados")]
    public float fuerzaGiro = 15f; 

    void Start()
    {
        // Al empezar, nos aseguramos de que el agua caiga normal
        Physics2D.gravity = new Vector2(0, caidaFuerza);
    }

    void Update()
    {
        // 1. Leemos el control
        float inclinacionTeclado = Input.GetAxis("Horizontal"); 
        float inclinacionGiroscopio = Input.acceleration.x; 
        
        float inclinacion = (Mathf.Abs(inclinacionTeclado) > 0.1f) ? inclinacionTeclado : inclinacionGiroscopio;

        // 2. Modificamos la gravedad (El Eje X cambia con el celular, el Eje Y siempre es hacia abajo)
        Physics2D.gravity = new Vector2(inclinacion * fuerzaGiro, caidaFuerza);
    }

    // Es importante que si salimos del nivel, la gravedad vuelva a la normalidad para el Nivel 1
    void OnDestroy()
    {
        Physics2D.gravity = new Vector2(0, -9.81f);
    }
}