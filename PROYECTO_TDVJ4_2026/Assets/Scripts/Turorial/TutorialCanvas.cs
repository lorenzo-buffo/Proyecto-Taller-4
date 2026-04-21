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

        // Si por alguna razón perdimos la tubería, la volvemos a buscar
        if (celdaTutorial == null)
        {
            BuscarTuberiaTutorial();
            return; 
        }

        // 1. Animación del dedo flotante (solo se mueve si está visible)
        if (imagenDedo != null && imagenDedo.activeSelf)
        {
            float nuevaY = posicionOriginalDedo.y + (Mathf.Sin(Time.time * velocidadFlotacion) * alturaFlotacion);
            imagenDedo.transform.localPosition = new Vector3(posicionOriginalDedo.x, nuevaY, posicionOriginalDedo.z);
        }

        // 2. Condición de Victoria: La soltó (seleccionada == null) y ya es RectaHorizontal
        if (celdaTutorial.tipo == Celda.TipoCelda.RectaHorizontal && Celda.seleccionada == null)
        {
            FinalizarTutorial();
            return;
        }

        // 3. LA MÁQUINA DE ESTADOS VISUAL: ¿Qué imagen mostramos?
        if (Celda.seleccionada == celdaTutorial)
        {
            // ESTÁ TOCANDO LA TUBERÍA: Ahora calculamos si ya la giró bien
            float anguloZ = celdaTutorial.visualTuberia.eulerAngles.z;
            float anguloNormalizado = (anguloZ % 180 + 180) % 180; 
            
            // Le damos un margen de 15 grados para que no tenga que ser matemáticamente perfecta
            bool estaCasiDerecha = anguloNormalizado < 15f || anguloNormalizado > 165f;

            if (estaCasiDerecha)
            {
                // ESTADO A: Ya la giró correctamente -> Mostramos DEDO para que la suelte (toque de nuevo)
                ActivarImagenTutorial(mostrarDedo: true, mostrarGiro: false);
            }
            else
            {
                // ESTADO B: Todavía está chueca -> Mostramos GIROSCOPIO para que incline el celular
                ActivarImagenTutorial(mostrarDedo: false, mostrarGiro: true);
            }
        }
        else
        {
            // ESTADO C: NO LA ESTÁ TOCANDO -> Mostramos DEDO para que la seleccione
            ActivarImagenTutorial(mostrarDedo: true, mostrarGiro: false);
        }
    }

    // 🔥 FUNCIÓN NUEVA: Nos ayuda a cambiar las imágenes sin errores y sin parpadeos
    void ActivarImagenTutorial(bool mostrarDedo, bool mostrarGiro)
    {
        if (imagenDedo != null && imagenDedo.activeSelf != mostrarDedo) 
            imagenDedo.SetActive(mostrarDedo);
            
        if (imagenGiroscopio != null && imagenGiroscopio.activeSelf != mostrarGiro) 
            imagenGiroscopio.SetActive(mostrarGiro);
    }

    void FinalizarTutorial()
    {
        tutorialTerminado = true;
        if (imagenDedo != null) imagenDedo.SetActive(false);
        if (imagenGiroscopio != null) imagenGiroscopio.SetActive(false);
    }
}