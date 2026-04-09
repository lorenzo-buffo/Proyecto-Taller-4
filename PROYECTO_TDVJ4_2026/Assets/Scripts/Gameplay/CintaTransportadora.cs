using UnityEngine;

public class CintaTransportadora : MonoBehaviour
{
    [Header("Referencias de UI")]
    [Tooltip("Arrastra aquí el objeto vacío PuntoSpawn")]
    public Transform puntoDeSpawn; 
    [Tooltip("Arrastra aquí tu PanelCinta")]
    public Transform contenedorCinta; // 🔥 NUEVO: Necesitamos decirle dónde meter las piezas
    
    [Header("Configuración")]
    public GameObject[] prefabsPiezasUI; 
    public float tiempoEntrePiezas = 2.0f;
    private float temporizador;

    void Update() 
    {
        temporizador += Time.deltaTime;
        if (temporizador >= tiempoEntrePiezas) 
        {
            GenerarNuevaPieza();
            temporizador = 0;
        }
    }

    void GenerarNuevaPieza() 
    {
        int indiceAleatorio = Random.Range(0, prefabsPiezasUI.Length);
        
        // 🔥 1. Creamos la pieza y la hacemos hija directamente del PanelCinta
        GameObject nuevaPieza = Instantiate(prefabsPiezasUI[indiceAleatorio], contenedorCinta);
        
        // 🔥 2. Movemos la pieza a la posición exacta del Punto de Spawn
        nuevaPieza.transform.position = puntoDeSpawn.position;
        
        // 🔥 3. Ajustamos su escala para que no se deforme
        nuevaPieza.GetComponent<RectTransform>().localScale = Vector3.one;
    }
}