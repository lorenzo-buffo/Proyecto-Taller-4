using UnityEngine;

public class TableroElectrico : MonoBehaviour
{
    [Header("Conexión")]
    [Tooltip("Arrastra aquí el objeto de tu Portón Eléctrico")]
    public PortonElectrico portonConectado;

    [Header("Efectos Visuales")]
    public Color colorApagado = Color.red;
    public Color colorEncendido = Color.green;
    
    private SpriteRenderer sr;
    private bool yaActivado = false;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = colorApagado;
    }

    void OnTriggerEnter2D(Collider2D otro)
    {
        // Si ya se activó antes, ignoramos todo
        if (yaActivado) return;

        // Si lo que nos tocó fue una Gota de agua...
        if (otro.CompareTag("Gota"))
        {
            yaActivado = true;
            
            // 1. Cambiamos el color visualmente
            if (sr != null) sr.color = colorEncendido;

            // 2. Le damos el "grito" al portón para que se abra
            if (portonConectado != null)
            {
                portonConectado.AbrirPorton();
                Debug.Log("¡Tablero activado! El portón se está abriendo.");
            }
        }
    }
}