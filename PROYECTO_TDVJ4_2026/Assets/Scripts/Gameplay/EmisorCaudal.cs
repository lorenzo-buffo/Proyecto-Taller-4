using UnityEngine;
using System.Collections;

public class EmisorCaudal : MonoBehaviour
{
    [Header("Configuración del Agua")]
    public GameObject prefabGota;
    public int cantidadTotal = 150; // El 100% de tu agua
    public float velocidadDeSalida = 0.02f; // Qué tan rápido salen las gotas

    void Start()
    {
        StartCoroutine(SoltarAgua());
    }

    IEnumerator SoltarAgua()
    {
        for (int i = 0; i < cantidadTotal; i++)
        {
            // Crea una gota en la posición exacta de este generador
            Instantiate(prefabGota, transform.position, Quaternion.identity);
            
            // Espera una fracción de segundo antes de soltar la siguiente
            yield return new WaitForSeconds(velocidadDeSalida);
        }
    }
}