using System.Collections;
using UnityEngine;

public enum Direccion { Arriba, Abajo, Izquierda, Derecha, Ninguna }

public class Celda : MonoBehaviour
{
    public static Celda seleccionada;

    public enum TipoCelda
    {
        Vacia,
        RectaHorizontal, RectaVertical,
        CurvaArribaDer, CurvaDerAbajo, CurvaAbajoIzq, CurvaIzqArriba,
        Fuente, Objetivo
    }

    [Header("Referencias Visuales")]
    public Transform visualTuberia; 

    [Header("Sprites Base")]
    public Sprite spriteVacia;
    public Sprite spriteRecta;
    public Sprite spriteCurva;
    public Sprite spriteFuente;
    public Sprite spriteObjetivo;

    [Header("Flujo (Color y Parpadeo)")]
    public Color colorVacio = Color.white;
    public Color colorLleno = Color.cyan;
    [Tooltip("El color al que cambia cuando palpita (Ej: Blanco o Cian más oscuro)")]
    public Color colorParpadeo = new Color(0.8f, 1f, 1f, 1f); 
    public float velocidadParpadeo = 4f;

    [Header("Control Móvil")]
    public float velocidadGiro = 250f;

    public TipoCelda tipo;
    public bool estaActiva = false;
    public int x;
    public int y;

    private SpriteRenderer sr;
    private float anguloActual;
    private bool moviendose = false;
    
    // 🔥 NUEVA VARIABLE: Nos avisa cuándo terminó de llenarse para empezar a latir
    private bool llenadoCompletado = false; 

    void Start()
    {
        if (visualTuberia != null) sr = visualTuberia.GetComponent<SpriteRenderer>();
        else Debug.LogError("¡Falta asignar el Visual Tuberia en la celda!");
        
        ActualizarVisual();
    }

    void OnMouseDown()
    {
        // Si ya tiene energía, bloqueamos el toque
        if (estaActiva) return; 

        // Tocar para agarrar
        if (seleccionada == null)
        {
            seleccionada = this;
            moviendose = true;
            anguloActual = visualTuberia.eulerAngles.z;
        }
        // Tocar para soltar (TOGGLE)
        else if (seleccionada == this)
        {
            seleccionada = null;
        }
    }

void Update()
    {
        // 1. Rotación y Parpadeo de Feedback cuando está SELECCIONADA
        if (seleccionada == this && !estaActiva)
        {
            // 🔥 EFECTO DE LATIDO DE SELECCIÓN 🔥
            // Usamos la onda matemática para hacerla palpitar
            float factorDeOnda = (Mathf.Sin(Time.time * velocidadParpadeo) + 1f) / 2f;
            
            // Alternamos suavemente entre el color apagado y el color de parpadeo
            sr.color = Color.Lerp(colorVacio, colorParpadeo, factorDeOnda);

            // Rotación con giroscopio
            float inclinacion = -Input.acceleration.x;
            anguloActual += inclinacion * velocidadGiro * Time.deltaTime;
            visualTuberia.rotation = Quaternion.Euler(0, 0, anguloActual); 
        }
        // 2. Soltamos la pieza y registramos el movimiento
        else if (moviendose && seleccionada != this)
        {
            moviendose = false;
            
            // 🔥 Apagamos el parpadeo al soltarla devolviéndole su color normal 🔥
            sr.color = colorVacio;
            
            if (GestorGrilla.instancia != null)
            {
                GestorGrilla.instancia.RegistrarMovimiento();
            }

            AplicarSnapYActualizarTipo();
        }
        // 3. Si no la estamos tocando y no tiene energía, aseguramos su color normal
        else if (!estaActiva && seleccionada != this)
        {
            sr.color = colorVacio;
        }
        // 4. Si ya se llenó de energía, la dejamos del color lleno sólido
        else if (llenadoCompletado)
        {
            sr.color = colorLleno;
        }
    }

    public void AplicarSnapYActualizarTipo()
    {
        float z = visualTuberia.eulerAngles.z;
        int anguloSnap = Mathf.RoundToInt(z / 90f) * 90;
        int anguloNormalizado = (anguloSnap % 360 + 360) % 360;

        if (tipo == TipoCelda.RectaHorizontal || tipo == TipoCelda.RectaVertical)
        {
            if (anguloNormalizado == 0 || anguloNormalizado == 180) tipo = TipoCelda.RectaHorizontal;
            else tipo = TipoCelda.RectaVertical;
        }
        else if (tipo == TipoCelda.CurvaArribaDer || tipo == TipoCelda.CurvaDerAbajo || tipo == TipoCelda.CurvaAbajoIzq || tipo == TipoCelda.CurvaIzqArriba)
        {
            if (anguloNormalizado == 0) tipo = TipoCelda.CurvaArribaDer;
            else if (anguloNormalizado == 90) tipo = TipoCelda.CurvaIzqArriba;
            else if (anguloNormalizado == 180) tipo = TipoCelda.CurvaAbajoIzq;
            else if (anguloNormalizado == 270) tipo = TipoCelda.CurvaDerAbajo;
        }

        ActualizarVisual();
    }

    public void ActualizarVisual()
    {
        if (sr == null && visualTuberia != null) sr = visualTuberia.GetComponent<SpriteRenderer>();
        if (sr == null) return;

        visualTuberia.localEulerAngles = Vector3.zero;
        
        // Solo asignamos el color estático si la pieza AÚN NO está palpitando
        if (!llenadoCompletado) 
        {
            sr.color = estaActiva ? colorLleno : colorVacio;
        }

        switch (tipo)
        {
            case TipoCelda.Vacia: sr.sprite = spriteVacia; break;
            case TipoCelda.Fuente: sr.sprite = spriteFuente; break;
            case TipoCelda.Objetivo: sr.sprite = spriteObjetivo; break;

            case TipoCelda.RectaHorizontal:
                sr.sprite = spriteRecta; visualTuberia.localEulerAngles = new Vector3(0, 0, 0); break;
            case TipoCelda.RectaVertical:
                sr.sprite = spriteRecta; visualTuberia.localEulerAngles = new Vector3(0, 0, 90); break;

            case TipoCelda.CurvaArribaDer:
                sr.sprite = spriteCurva; visualTuberia.localEulerAngles = new Vector3(0, 0, 0); break;
            case TipoCelda.CurvaDerAbajo:
                sr.sprite = spriteCurva; visualTuberia.localEulerAngles = new Vector3(0, 0, -90); break;
            case TipoCelda.CurvaAbajoIzq:
                sr.sprite = spriteCurva; visualTuberia.localEulerAngles = new Vector3(0, 0, -180); break;
            case TipoCelda.CurvaIzqArriba:
                sr.sprite = spriteCurva; visualTuberia.localEulerAngles = new Vector3(0, 0, -270); break;
        }
    }

    public IEnumerator AnimarLlenado(float tiempoTotal, Direccion entrada)
    {
        estaActiva = true;
        
        // Si el jugador justo la estaba tocando cuando le llegó el agua, la soltamos a la fuerza
        if (seleccionada == this) seleccionada = null; 

        Color colorInicial = sr.color;
        float tiempo = 0f;

        // Fase 1: Transición suave de Vacio a Lleno
        while (tiempo < tiempoTotal)
        {
            tiempo += Time.deltaTime;
            sr.color = Color.Lerp(colorInicial, colorLleno, tiempo / tiempoTotal);
            yield return null;
        }

        sr.color = colorLleno;
        
        // Fase 2: ¡Arranca el latido en el Update!
        llenadoCompletado = true; 
    }

    public bool TieneConexion(Direccion dir)
    {
        // La fuente y el objetivo se conectan con cualquier lado
        if (tipo == TipoCelda.Fuente || tipo == TipoCelda.Objetivo) return true;

        switch (tipo)
        {
            case TipoCelda.RectaHorizontal:
                return dir == Direccion.Izquierda || dir == Direccion.Derecha;
            case TipoCelda.RectaVertical:
                return dir == Direccion.Arriba || dir == Direccion.Abajo;
            case TipoCelda.CurvaArribaDer:
                return dir == Direccion.Arriba || dir == Direccion.Derecha;
            case TipoCelda.CurvaDerAbajo:
                return dir == Direccion.Derecha || dir == Direccion.Abajo;
            case TipoCelda.CurvaAbajoIzq:
                return dir == Direccion.Abajo || dir == Direccion.Izquierda;
            case TipoCelda.CurvaIzqArriba:
                return dir == Direccion.Izquierda || dir == Direccion.Arriba;
        }
        return false;
    }
}