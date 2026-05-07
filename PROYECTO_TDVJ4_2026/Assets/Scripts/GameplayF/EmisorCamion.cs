using System.Collections;
using UnityEngine;

public class EmisorCamion : MonoBehaviour
{
    [Header("Prefab del camión")]
    public GameObject prefabCamion;

    [Header("Punto donde aparece")]
    public Transform puntoSpawn;

    private GameObject camionActual;

    public void GenerarCamion()
    {
        Time.timeScale = 1f;

        if (camionActual != null)
        {
            Debug.Log("Ya existe un camión en la escena.");
            return;
        }

        if (prefabCamion == null)
        {
            Debug.LogError("No asignaste el prefab del camión en EmisorCamion.");
            return;
        }

        Transform spawn = puntoSpawn != null ? puntoSpawn : transform;

        camionActual = Instantiate(
            prefabCamion,
            spawn.position,
            spawn.rotation
        );

        camionActual.SetActive(true);

        StartCoroutine(ActivarCamionDespuesDeCrear());
    }

    private IEnumerator ActivarCamionDespuesDeCrear()
    {
        yield return null;

        ControlCamionGiroscopio control = camionActual.GetComponent<ControlCamionGiroscopio>();

        if (control == null)
            control = camionActual.GetComponentInChildren<ControlCamionGiroscopio>();

        if (control != null)
        {
            control.ActivarMovimiento();
            Debug.Log("Camión generado y movimiento activado después del Start.");
        }
        else
        {
            Debug.LogError("El prefab del camión NO tiene ControlCamionGiroscopio.");
        }
    }
}