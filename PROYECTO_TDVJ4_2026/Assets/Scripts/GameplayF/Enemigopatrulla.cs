using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemigoPatrulla : MonoBehaviour
{
    [Header("Ruta de patrulla")]
    public Transform[] waypoints;
    public float velocidad = 2f;
    public float distanciaUmbral = 0.1f;

    [Header("Derrota")]
    public bool reiniciarEscenaAlTocarCamion = true;
    public GameObject panelDerrota;

    private int indiceActual = 0;

    void Update()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        Transform destino = waypoints[indiceActual];

        transform.position = Vector2.MoveTowards(
            transform.position,
            destino.position,
            velocidad * Time.deltaTime
        );

        if (Vector2.Distance(transform.position, destino.position) < distanciaUmbral)
        {
            indiceActual = (indiceActual + 1) % waypoints.Length;
        }
    }

    void OnTriggerEnter2D(Collider2D otro)
    {
        if (otro.CompareTag("Camion"))
        {
            PerderNivel(otro.gameObject);
        }
    }

    void PerderNivel(GameObject camion)
    {
        ControlCamionGiroscopio control = camion.GetComponent<ControlCamionGiroscopio>();

        if (control != null)
            control.DetenerMovimiento();

        if (panelDerrota != null)
        {
            panelDerrota.SetActive(true);
            Time.timeScale = 0f;
        }
        else if (reiniciarEscenaAlTocarCamion)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}