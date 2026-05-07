using UnityEngine;
using UnityEngine.SceneManagement;

public class DestructorGotas : MonoBehaviour
{
    [Header("Derrota")]
    public bool reiniciarEscena = true;
    public GameObject panelDerrota;

    void OnTriggerEnter2D(Collider2D otro)
    {
        if (otro.CompareTag("Camion"))
        {
            Perder(otro.gameObject);
        }
    }

    void Perder(GameObject camion)
    {
        ControlCamionGiroscopio control = camion.GetComponent<ControlCamionGiroscopio>();

        if (control != null)
            control.DetenerMovimiento();

        if (panelDerrota != null)
        {
            panelDerrota.SetActive(true);
            Time.timeScale = 0f;
        }
        else if (reiniciarEscena)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}