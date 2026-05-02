using UnityEngine;
using UnityEngine.UI;

public class BotonSoltarAgua : MonoBehaviour
{
    [Header("Conexión")]
    public EmisorCaudal generadorAgua;
    
    private Button miBoton;

    void Start()
    {
        miBoton = GetComponent<Button>();
    }

    public void EjecutarPuzzle()
    {
        // 1. Encendemos el agua directamente (¡sin congelar nada!)
        if (generadorAgua != null) 
        {
            generadorAgua.IniciarEmision();
        }

        // 2. Apagamos el botón visualmente para que no se presione dos veces
        if (miBoton != null) 
        {
            miBoton.interactable = false;
        }
    }
}