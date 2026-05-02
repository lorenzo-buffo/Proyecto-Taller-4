using UnityEngine;

public class PlataformaGiroscopio : MonoBehaviour
{
    [Header("Configuración de Rotación")]
    public float velocidadDeGiro = 150f;
    public float limiteAngulo = 60f; 

    [Header("Mecánica de Puzle")]
    [Tooltip("Si marcas esto, la plataforma girará al revés que el celular")]
    public bool giroInvertido = false; 

    private float anguloActual = 0f;

    void Start()
    {
        anguloActual = transform.eulerAngles.z;
    }

    void Update()
    {
        // Leemos los controles TODO EL TIEMPO
        float inclinacionTeclado = Input.GetAxis("Horizontal"); 
        float inclinacionGiroscopio = Input.acceleration.x; 
        float inclinacion = (Mathf.Abs(inclinacionTeclado) > 0.1f) ? inclinacionTeclado : inclinacionGiroscopio;

        // Invertimos si es necesario
        if (giroInvertido)
        {
            inclinacion = inclinacion * -1f; 
        }

        // Calculamos y aplicamos la rotación
        anguloActual -= inclinacion * velocidadDeGiro * Time.deltaTime;
        anguloActual = Mathf.Clamp(anguloActual, -limiteAngulo, limiteAngulo);
        
        transform.rotation = Quaternion.Euler(0, 0, anguloActual); 
    }
}