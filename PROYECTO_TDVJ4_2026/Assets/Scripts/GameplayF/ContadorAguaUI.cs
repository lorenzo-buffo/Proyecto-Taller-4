using UnityEngine;
using UnityEngine.UI;

public class ContadorAguaUI : MonoBehaviour
{
    public static ContadorAguaUI instancia;

    [Header("Conexiones")]
    [Tooltip("Arrastra aquí tu texto del Canvas.")]
    public Text textoPorcentaje;

    [Tooltip("Arrastra aquí tu Generador de Agua.")]
    public EmisorCaudal emisor;

    [Header("Configuración visual")]
    public string sufijo = "%";
    public bool actualizarEnUpdate = true;

    private void Awake()
    {
        instancia = this;
    }

    private void Start()
    {
        ActualizarUI();
    }

    private void Update()
    {
        if (actualizarEnUpdate)
        {
            ActualizarUI();
        }
    }

    public void ActualizarUI()
    {
        if (emisor == null || textoPorcentaje == null)
            return;

        textoPorcentaje.text = ObtenerPorcentajeAguaRestante() + sufijo;
    }

    public int ObtenerPorcentajeAguaRestante()
    {
        if (emisor == null || emisor.cantidadTotal <= 0)
            return 0;

        int porcentaje = 100 - ((EmisorCaudal.gotasDestruidas * 100) / emisor.cantidadTotal);
        return Mathf.Clamp(porcentaje, 0, 100);
    }

    public void PerderAgua(int cantidad)
    {
        EmisorCaudal.gotasDestruidas += Mathf.Max(1, cantidad);
        ActualizarUI();
    }
}
