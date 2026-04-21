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
        // 🔥 NUEVOS TIPOS: La Tubería en T según qué lado tiene "plano" (cerrado)
        Bifurcacion_SinArriba, Bifurcacion_SinDer, Bifurcacion_SinAbajo, Bifurcacion_SinIzq,
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
    public Sprite spriteBifurcacion; // 🔥 NUEVO SPRITE: Tubería en forma de T

    [Header("Flujo (Color y Parpadeo)")]
    public Color colorVacio = Color.white;
    public Color colorLleno = Color.cyan;
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
    private bool llenadoCompletado = false; 

    void Start()
    {
        if (visualTuberia != null) sr = visualTuberia.GetComponent<SpriteRenderer>();
        else Debug.LogError("¡Falta asignar el Visual Tuberia en la celda!");
        
        ActualizarVisual();
    }

    void OnMouseDown()
    {
        if (estaActiva) return; 

        if (seleccionada == null)
        {
            seleccionada = this;
            moviendose = true;
            anguloActual = visualTuberia.eulerAngles.z;
        }
        else if (seleccionada == this)
        {
            seleccionada = null;
        }
    }

    void Update()
    {
        if (seleccionada == this && !estaActiva)
        {
            float factorDeOnda = (Mathf.Sin(Time.time * velocidadParpadeo) + 1f) / 2f;
            sr.color = Color.Lerp(colorVacio, colorParpadeo, factorDeOnda);

            float inclinacion = -Input.acceleration.x;
            anguloActual += inclinacion * velocidadGiro * Time.deltaTime;
            visualTuberia.rotation = Quaternion.Euler(0, 0, anguloActual); 
        }
        else if (moviendose && seleccionada != this)
        {
            moviendose = false;
            sr.color = colorVacio;
            
            if (GestorGrilla.instancia != null) GestorGrilla.instancia.RegistrarMovimiento();

            AplicarSnapYActualizarTipo();
        }
        else if (!estaActiva && seleccionada != this) sr.color = colorVacio;
        else if (llenadoCompletado) sr.color = colorLleno;
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
        // 🔥 Lógica de Snap para la Bifurcación
        else if (tipo == TipoCelda.Bifurcacion_SinArriba || tipo == TipoCelda.Bifurcacion_SinIzq || tipo == TipoCelda.Bifurcacion_SinAbajo || tipo == TipoCelda.Bifurcacion_SinDer)
        {
            if (anguloNormalizado == 0) tipo = TipoCelda.Bifurcacion_SinArriba; // 0º
            else if (anguloNormalizado == 90) tipo = TipoCelda.Bifurcacion_SinIzq; // 90º
            else if (anguloNormalizado == 180) tipo = TipoCelda.Bifurcacion_SinAbajo; // 180º
            else if (anguloNormalizado == 270) tipo = TipoCelda.Bifurcacion_SinDer; // 270º
        }

        ActualizarVisual();
    }

    public void ActualizarVisual()
    {
        if (sr == null && visualTuberia != null) sr = visualTuberia.GetComponent<SpriteRenderer>();
        if (sr == null) return;

        visualTuberia.localEulerAngles = Vector3.zero;
        if (!llenadoCompletado) sr.color = estaActiva ? colorLleno : colorVacio;

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
            case TipoCelda.CurvaIzqArriba:
                sr.sprite = spriteCurva; visualTuberia.localEulerAngles = new Vector3(0, 0, 90); break;
            case TipoCelda.CurvaAbajoIzq:
                sr.sprite = spriteCurva; visualTuberia.localEulerAngles = new Vector3(0, 0, 180); break;
            case TipoCelda.CurvaDerAbajo:
                sr.sprite = spriteCurva; visualTuberia.localEulerAngles = new Vector3(0, 0, 270); break;

            // 🔥 Visuales de la Bifurcación
            case TipoCelda.Bifurcacion_SinArriba:
                sr.sprite = spriteBifurcacion; visualTuberia.localEulerAngles = new Vector3(0, 0, 0); break;
            case TipoCelda.Bifurcacion_SinIzq:
                sr.sprite = spriteBifurcacion; visualTuberia.localEulerAngles = new Vector3(0, 0, 90); break;
            case TipoCelda.Bifurcacion_SinAbajo:
                sr.sprite = spriteBifurcacion; visualTuberia.localEulerAngles = new Vector3(0, 0, 180); break;
            case TipoCelda.Bifurcacion_SinDer:
                sr.sprite = spriteBifurcacion; visualTuberia.localEulerAngles = new Vector3(0, 0, 270); break;
        }
    }

    public IEnumerator AnimarLlenado(float tiempoTotal, Direccion entrada)
    {
        estaActiva = true;
        if (seleccionada == this) seleccionada = null; 

        Color colorInicial = sr.color;
        float tiempo = 0f;

        while (tiempo < tiempoTotal)
        {
            tiempo += Time.deltaTime;
            sr.color = Color.Lerp(colorInicial, colorLleno, tiempo / tiempoTotal);
            yield return null;
        }

        sr.color = colorLleno;
        llenadoCompletado = true; 
    }

    public bool TieneConexion(Direccion dir)
    {
        if (tipo == TipoCelda.Fuente || tipo == TipoCelda.Objetivo) return true;

        switch (tipo)
        {
            case TipoCelda.RectaHorizontal: return dir == Direccion.Izquierda || dir == Direccion.Derecha;
            case TipoCelda.RectaVertical: return dir == Direccion.Arriba || dir == Direccion.Abajo;
            case TipoCelda.CurvaArribaDer: return dir == Direccion.Arriba || dir == Direccion.Derecha;
            case TipoCelda.CurvaDerAbajo: return dir == Direccion.Derecha || dir == Direccion.Abajo;
            case TipoCelda.CurvaAbajoIzq: return dir == Direccion.Abajo || dir == Direccion.Izquierda;
            case TipoCelda.CurvaIzqArriba: return dir == Direccion.Izquierda || dir == Direccion.Arriba;
            
            // 🔥 Conexiones de la Bifurcación (3 caminos abiertos, 1 cerrado)
            case TipoCelda.Bifurcacion_SinArriba: return dir != Direccion.Arriba;
            case TipoCelda.Bifurcacion_SinIzq: return dir != Direccion.Izquierda;
            case TipoCelda.Bifurcacion_SinAbajo: return dir != Direccion.Abajo;
            case TipoCelda.Bifurcacion_SinDer: return dir != Direccion.Derecha;
        }
        return false;
    }
}