using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ControlCamionGiroscopio : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidad = 5f;
    public float aceleracion = 12f;
    public float limiteVelocidad = 6f;

    [Header("Pruebas")]
    public bool permitirTeclado = true;
    public bool activarAlIniciar = false;

    [Header("Giroscopio / acelerómetro")]
    public bool usarGiroscopio = true;
    public float sensibilidadGiro = 1.5f;

    [Header("Rotación visual")]
    public bool rotarHaciaMovimiento = true;
    public float suavizadoRotacion = 10f;

    private Rigidbody2D rb;
    private bool puedeMoverse = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

   void Start()
{
    rb.gravityScale = 0f;
    rb.linearDamping = 3f;
    rb.angularDamping = 5f;

    rb.linearVelocity = Vector2.zero;
    rb.angularVelocity = 0f;

    if (activarAlIniciar)
    {
        ActivarMovimiento();
        Debug.Log("Camión inicia con movimiento activado.");
    }
    else
    {
        Debug.Log("Camión inicia quieto, esperando botón.");
    }
}

    void FixedUpdate()
    {
        if (!puedeMoverse)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 input = ObtenerInput();

        if (input.magnitude > 1f)
            input.Normalize();

        rb.AddForce(input * aceleracion, ForceMode2D.Force);

        if (rb.linearVelocity.magnitude > limiteVelocidad)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * limiteVelocidad;
        }

        if (rotarHaciaMovimiento && rb.linearVelocity.sqrMagnitude > 0.05f)
        {
            float angulo = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg - 90f;

            Quaternion rotacionObjetivo = Quaternion.Euler(0f, 0f, angulo);

            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                rotacionObjetivo,
                Time.fixedDeltaTime * suavizadoRotacion
            );
        }
    }

    Vector2 ObtenerInput()
    {
        Vector2 input = Vector2.zero;

        if (permitirTeclado)
        {
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                input.x -= 1f;

            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                input.x += 1f;

            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
                input.y += 1f;

            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
                input.y -= 1f;

            if (input.magnitude > 0.1f)
                return input;
        }

        if (usarGiroscopio)
        {
            float xGiro = Input.acceleration.x;
            float yGiro = Input.acceleration.y;

            input = new Vector2(xGiro, yGiro) * sensibilidadGiro;
        }

        return input;
    }

    public void ActivarMovimiento()
    {
        puedeMoverse = true;
        Debug.Log("Movimiento del camión ACTIVADO.");
    }

    public void DetenerMovimiento()
    {
        puedeMoverse = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        Debug.Log("Movimiento del camión DETENIDO.");
    }
}