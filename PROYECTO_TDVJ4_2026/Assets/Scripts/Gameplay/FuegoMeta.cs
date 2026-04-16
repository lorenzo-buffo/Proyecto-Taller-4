using UnityEngine;
using System.Collections;

public class FuegoMeta : MonoBehaviour
{
    [Header("Interfaz")]
    public GameObject popUpFinal; // Arrastra tu panel de botones aquí

    [Header("Estadísticas")]
    public int gotasAtrapadas = 0;
    private bool nivelTerminado = false;

    void Start()
    {
        // Nos aseguramos de que el pop-up esté oculto al empezar
        if (popUpFinal != null) popUpFinal.SetActive(false);
        
        // Iniciamos el radar que buscará si quedan gotas
        StartCoroutine(RadarDeGotas());
    }

    void OnTriggerEnter2D(Collider2D otro)
    {
        if (otro.CompareTag("Gota"))
        {
            gotasAtrapadas++;
            // ¡Aquí el agua toca el fuego! Destruimos la gota.
            Destroy(otro.gameObject);
        }
    }

    IEnumerator RadarDeGotas()
    {
        // Le damos 3 segundos de ventaja al emisor para que empiece a soltar el agua
        // antes de empezar a buscar si el nivel terminó.
        yield return new WaitForSeconds(3f);

        while (!nivelTerminado)
        {
            // Buscamos cuántas gotas existen en toda la escena en este momento
            GameObject[] gotasRestantes = GameObject.FindGameObjectsWithTag("Gota");

            // Si el arreglo está vacío (no hay gotas)...
            if (gotasRestantes.Length == 0)
            {
                nivelTerminado = true;
                MostrarPopUp();
            }

            // Pausa el radar medio segundo para no consumir batería del celular
            yield return new WaitForSeconds(0.5f);
        }
    }

    void MostrarPopUp()
    {
        // Activamos el menú
        if (popUpFinal != null) popUpFinal.SetActive(true);
        
        Debug.Log("Nivel finalizado. Agua salvada: " + gotasAtrapadas);
        // Más adelante, aquí puedes calcular si ganaste 1, 2 o 3 estrellas
        // dependiendo del número de 'gotasAtrapadas'.
    }
}