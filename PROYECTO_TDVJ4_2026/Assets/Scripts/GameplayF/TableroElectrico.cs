using UnityEngine;

public class TableroElectrico : MonoBehaviour
{
    [Header("Conexión")]
    [Tooltip("Arrastra aquí el objeto de tu Portón Eléctrico")]
    public PortonElectrico portonConectado;

    [Header("Detección")]
    public string tagActivador = "Camion";

    [Header("Efectos Visuales")]
    public Color colorApagado = Color.red;
    public Color colorEncendido = Color.green;

    private SpriteRenderer sr;
    private bool yaActivado = false;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();

        if (sr != null)
            sr.color = colorApagado;
    }

    void OnTriggerEnter2D(Collider2D otro)
    {
        if (yaActivado) return;

        if (otro.CompareTag(tagActivador))
        {
            yaActivado = true;

            if (sr != null)
                sr.color = colorEncendido;

            if (portonConectado != null)
            {
                portonConectado.AbrirPorton();
                Debug.Log("Tablero activado por el camión. El portón se está abriendo.");
            }
            else
            {
                Debug.LogWarning("El tablero no tiene un portón conectado.");
            }
        }
    }
}