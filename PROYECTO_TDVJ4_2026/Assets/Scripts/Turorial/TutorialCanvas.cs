using UnityEngine;

public class TutorialCanvas : MonoBehaviour
{
    [Header("Referencias UI")]
    public GameObject imagenDedo;
    public GameObject imagenGiroscopio;

    [Header("Configuración Tutorial")]
    public Celda celdaTutorial;

    [Header("Animación")]
    public float velocidadFlotacion = 5f; 
    public float alturaFlotacion = 15f;   
    
    private Vector3 posicionOriginalDedo;
    private bool tutorialTerminado = false;

    void Start()
    {
        if (imagenDedo != null)
        {
            posicionOriginalDedo = imagenDedo.transform.localPosition;
            imagenDedo.SetActive(true);
        }
        if (imagenGiroscopio != null) imagenGiroscopio.SetActive(false);
        
        if (celdaTutorial == null) BuscarTuberiaTutorial();
    }

    void BuscarTuberiaTutorial()
    {
       Celda[] todas = FindObjectsByType<Celda>(FindObjectsSortMode.None);
        foreach (Celda c in todas)
        {
            if (c.tipo != Celda.TipoCelda.Fuente && c.tipo != Celda.TipoCelda.Objetivo)
            {
                celdaTutorial = c;
                break;
            }
        }
    }

    void Update()
    {
        if (tutorialTerminado) return;

        if (celdaTutorial == null)
        {
            BuscarTuberiaTutorial();
            return; 
        }

        if (imagenDedo != null && imagenDedo.activeSelf)
        {
            float nuevaY = posicionOriginalDedo.y + (Mathf.Sin(Time.time * velocidadFlotacion) * alturaFlotacion);
            imagenDedo.transform.localPosition = new Vector3(posicionOriginalDedo.x, nuevaY, posicionOriginalDedo.z);
        }

        if (celdaTutorial.tipo == Celda.TipoCelda.RectaHorizontal && Celda.seleccionada == null)
        {
            FinalizarTutorial();
            return;
        }

        if (Celda.seleccionada == celdaTutorial)
        {
            float anguloZ = celdaTutorial.visualTuberia.eulerAngles.z;
            float anguloNormalizado = (anguloZ % 180 + 180) % 180; 
            bool estaCasiDerecha = anguloNormalizado < 15f || anguloNormalizado > 165f;

            if (estaCasiDerecha)
            {
                if (imagenGiroscopio.activeSelf) imagenGiroscopio.SetActive(false);
                if (!imagenDedo.activeSelf) imagenDedo.SetActive(true);
            }
            else
            {
                if (imagenDedo.activeSelf) imagenDedo.SetActive(false);
                if (!imagenGiroscopio.activeSelf) imagenGiroscopio.SetActive(true);
            }
        }
        else
        {
            if (imagenGiroscopio.activeSelf) imagenGiroscopio.SetActive(false);
            if (!imagenDedo.activeSelf) imagenDedo.SetActive(true);
        }
    }

    void FinalizarTutorial()
    {
        tutorialTerminado = true;
        if (imagenDedo != null) imagenDedo.SetActive(false);
        if (imagenGiroscopio != null) imagenGiroscopio.SetActive(false);
    }
}