using UnityEngine;

public class PortonElectrico : MonoBehaviour
{
    [Header("Configuración del Movimiento")]
    [Tooltip("Hacia dónde se mueve al abrirse. (Ej: X: 2 se mueve a la derecha, Y: 2 hacia arriba)")]
    public Vector3 desplazamientoAlAbrir = new Vector3(0f, 2f, 0f); 
    public float velocidadApertura = 3f;

    private Vector3 posicionCerrada;
    private Vector3 posicionAbierta;
    private bool estaAbierto = false;

    void Start()
    {
        // Guardamos dónde empieza el portón y calculamos hasta dónde debe llegar
        posicionCerrada = transform.position;
        posicionAbierta = posicionCerrada + desplazamientoAlAbrir;
    }

    void Update()
    {
        // Si recibió la orden de abrirse, se mueve suavemente hacia la meta
        if (estaAbierto)
        {
            transform.position = Vector3.MoveTowards(transform.position, posicionAbierta, velocidadApertura * Time.deltaTime);
        }
    }

    // Esta función es pública para que el Tablero pueda llamarla
    public void AbrirPorton()
    {
        estaAbierto = true;
    }
}