using UnityEngine;

public class GravedadTopDown : MonoBehaviour
{
    [Header("Configuración Top-Down")]
    [Tooltip("Fuerza con la que el agua se mueve al inclinar el celular")]
    public float fuerzaInclinacion = 20f;
    
    [Tooltip("Suavizado para que el cambio de dirección no sea brusco (simula inercia)")]
    public float suavizado = 5f;

    private Vector2 gravedadObjetivo;

    void Update()
    {
        // 1. Leemos la inclinación del celular (Ejes X e Y)
        // En PC usamos las flechas. En celular el acelerómetro.
        float inputX = Input.GetAxis("Horizontal");
        float inputY = Input.GetAxis("Vertical");
        
        float giroX = Input.acceleration.x;
        float giroY = Input.acceleration.y;

        // Mezclamos (para poder probar en PC y Celular)
        float finalX = (Mathf.Abs(inputX) > 0.1f) ? inputX : giroX;
        float finalY = (Mathf.Abs(inputY) > 0.1f) ? inputY : giroY;

        // 2. Calculamos hacia dónde debería ir el agua
        gravedadObjetivo = new Vector2(finalX, finalY) * fuerzaInclinacion;

        // 3. Aplicamos la gravedad globalmente, pero con un Lerp (suavizado)
        // Esto hace que el agua se sienta más como un líquido espeso sobre una mesa
        Physics2D.gravity = Vector2.Lerp(Physics2D.gravity, gravedadObjetivo, Time.deltaTime * suavizado);
    }

    void OnDestroy()
    {
        Physics2D.gravity = new Vector2(0, -9.81f);
    }
}