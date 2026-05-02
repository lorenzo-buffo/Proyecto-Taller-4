using UnityEngine;

public class ControlProPipe : MonoBehaviour
{
    public static ControlProPipe seleccionado;

    [Header("Sensibilidad")]
    public float sensibilidad = 2f;
    public float suavizado = 10f;

    [Header("Visual")]
    public Color colorActivo = Color.cyan;
    private Color colorOriginal;
    private SpriteRenderer sr;

    [Header("Efectos (Juice)")]
    public ParticleSystem chispasSnap; // Arrastra tu sistema de partículas aquí
    public AudioSource sonidoSnap;     // Arrastra tu componente de audio aquí
    private bool yaHizoSnap = true;    // Seguro para no repetir el efecto infinitamente

    private float anguloActual;
    private Quaternion rotacionMeta;

    void Start() {
        sr = GetComponent<SpriteRenderer>();
        colorOriginal = sr.color;
        anguloActual = transform.eulerAngles.z;
        rotacionMeta = transform.rotation;
    }

    void OnMouseDown() {
        seleccionado = this;
    }

    void Update() {
        if (seleccionado == this) {
            sr.color = colorActivo;
            yaHizoSnap = false; // Como lo estamos moviendo, reseteamos el seguro
            
            // 1. Leemos el giro (tipo volante)
            float giro = Input.acceleration.x * -90f * sensibilidad;

            // 2. Aplicamos suavizado (Lerp)
            rotacionMeta = Quaternion.Euler(0, 0, giro);
            transform.rotation = Quaternion.Lerp(transform.rotation, rotacionMeta, Time.deltaTime * suavizado);
        } else {
            sr.color = colorOriginal;
            
            // 3. ¡EL SNAP CON JUICE!
            float z = transform.eulerAngles.z;
            float snapZ = Mathf.Round(z / 90f) * 90f; 
            
            // Calculamos cuántos grados faltan para llegar al ángulo perfecto
            float distanciaAlSnap = Mathf.Abs(Mathf.DeltaAngle(z, snapZ));

            // Si todavía le falta un poquito para llegar, sigue moviéndose
            if (distanciaAlSnap > 0.5f) {
                // Aceleré un poco el Lerp aquí (a 15f) para que el "encaje" sea más seco y satisfactorio
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 0, snapZ), Time.deltaTime * 15f);
            } 
            // Si ya llegó (o está a menos de medio grado), activamos los efectos
            else if (!yaHizoSnap) {
                transform.rotation = Quaternion.Euler(0, 0, snapZ); // Lo clavamos en el ángulo exacto
                
                // Disparamos los efectos visuales y sonoros
                if (chispasSnap != null) chispasSnap.Play();
                if (sonidoSnap != null) sonidoSnap.Play();
                
                yaHizoSnap = true; // Ponemos el seguro para que no vuelva a sonar hasta que lo toquemos de nuevo
            }
        }
    }
}