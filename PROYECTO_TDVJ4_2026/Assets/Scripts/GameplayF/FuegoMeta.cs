using UnityEngine;
using System.Collections;

public class FuegoMeta : MonoBehaviour
{
    [Header("Interfaz Final")]
    public GameObject popUpFinal; 
    public GameObject botonSiguiente;  

    [Header("Conexión")]
    public EmisorCaudal emisor;

    [Header("Estadísticas")]
    public int gotasAtrapadas = 0;
    private bool nivelTerminado = false;

    void Start()
    {
        // 1. Apagamos el Panel y el Botón desde el inicio para que no haya errores
        if (popUpFinal != null) popUpFinal.SetActive(false);
        if (botonSiguiente != null) botonSiguiente.SetActive(false); 
        
        StartCoroutine(RadarDeGotasUltraRapido());
    }

    void OnTriggerEnter2D(Collider2D otro)
    {
        if (otro.CompareTag("Gota"))
        {
            gotasAtrapadas++;
            EmisorCaudal.gotasActivas--;
            Destroy(otro.gameObject);
        }
    }

    IEnumerator RadarDeGotasUltraRapido()
    {
        yield return new WaitForSeconds(2f);

        while (!nivelTerminado)
        {
            if (EmisorCaudal.terminoDeEmitir && EmisorCaudal.gotasActivas <= 0)
            {
                nivelTerminado = true;
                EvaluarResultado();
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    void EvaluarResultado()
    {
        if (emisor == null)
        {
            Debug.LogError("❌ Falta conectar el Generador de Agua en el Fuego.");
            return;
        }

        float gotasRequeridas = emisor.cantidadTotal / 2f; 

        // 2. Encendemos el Panel de fondo
        if (popUpFinal != null) popUpFinal.SetActive(true);

        // 3. Verificamos el botón y lo encendemos SOLO si ganamos
        if (botonSiguiente != null)
        {
            if (gotasAtrapadas >= gotasRequeridas)
            {
                Debug.Log("✅ ¡Superaste el 50%! Encendiendo botón.");
                botonSiguiente.SetActive(true); 
            }
            else
            {
                Debug.Log("❌ No llegaste al 50%. El botón se queda apagado.");
                botonSiguiente.SetActive(false); 
            }
        }
        else
        {
            Debug.LogError("❌ ¡OJO! El hueco de 'Boton Siguiente' en el Inspector está vacío.");
        }
    }
}