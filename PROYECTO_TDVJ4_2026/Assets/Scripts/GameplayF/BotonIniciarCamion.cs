using UnityEngine;
using UnityEngine.UI;

public class BotonIniciarCamion : MonoBehaviour
{
    [Header("Conexión")]
    public ControlCamionGiroscopio camion;

    private Button miBoton;

    void Start()
    {
        miBoton = GetComponent<Button>();
    }

    public void EjecutarPuzzle()
    {
        if (camion != null)
        {
            camion.ActivarMovimiento();
        }

        if (miBoton != null)
        {
            miBoton.interactable = false;
        }
    }
}