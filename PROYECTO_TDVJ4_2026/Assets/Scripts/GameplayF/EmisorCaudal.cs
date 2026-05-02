using UnityEngine;
using System.Collections;

public class EmisorCaudal : MonoBehaviour
{
    [Header("Configuración del Agua")]
    public GameObject prefabGota;
    public int cantidadTotal = 100; // 🔥 Ajustado a 100
    public float velocidadDeSalida = 0.02f; 

    public static int gotasActivas = 0;
    public static bool terminoDeEmitir = false;
    
    // 🔥 NUEVA VARIABLE: Llevará la cuenta de las gotas que cayeron al vacío
    public static int gotasDestruidas = 0; 

    void Start()
    {
        gotasActivas = 0;
        terminoDeEmitir = false;
        gotasDestruidas = 0; // Reseteamos el contador al reiniciar el nivel
    }

    public void IniciarEmision()
    {
        StartCoroutine(SoltarAgua());
    }

    IEnumerator SoltarAgua()
    {
        for (int i = 0; i < cantidadTotal; i++)
        {
            Instantiate(prefabGota, transform.position, Quaternion.identity);
            gotasActivas++; 
            yield return new WaitForSeconds(velocidadDeSalida);
        }
        
        terminoDeEmitir = true;
    }
}