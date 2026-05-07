using UnityEngine;

public class FuegoMetaCamion : MonoBehaviour
{
    [Header("Interfaz Final")]
    public GameObject popUpFinal;
    public GameObject botonSiguiente;

    [Header("Objeto controlado")]
    public ControlCamionGiroscopio camion;

    private bool nivelTerminado = false;

    void Start()
    {
        if (popUpFinal != null)
            popUpFinal.SetActive(false);

        if (botonSiguiente != null)
            botonSiguiente.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D otro)
    {
        if (nivelTerminado) return;

        if (otro.CompareTag("Camion"))
        {
            nivelTerminado = true;

            if (camion != null)
                camion.DetenerMovimiento();

            if (popUpFinal != null)
                popUpFinal.SetActive(true);

            if (botonSiguiente != null)
                botonSiguiente.SetActive(true);

            Debug.Log("Nivel completado: el camión llegó al incendio.");
        }
    }
}