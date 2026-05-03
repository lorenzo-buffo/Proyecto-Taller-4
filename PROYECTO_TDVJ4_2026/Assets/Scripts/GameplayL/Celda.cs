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
        Bifurcacion_SinArriba, Bifurcacion_SinDer, Bifurcacion_SinAbajo, Bifurcacion_SinIzq,
        ValvulaHorizontal,
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
    public Sprite spriteBifurcacion;

    [Header("Objetivo Final")]
    [Tooltip("El objetivo funciona como una entrada universal: acepta flujo desde arriba, abajo, izquierda o derecha. No rota y no se destruye.")]
    public bool objetivoAceptaTodosLosLados = true;

    [Header("Válvula")]
    public Sprite spriteValvulaAbierta;
    public Sprite spriteValvulaCerrada;
    public bool valvulaAbierta = true;
    public bool valvulaVertical = false;
    public float inclinacionValvula = 0.35f;

    [Header("Tubería Giratoria Automática")]
    public bool giraAutomaticamente = false;
    public float tiempoEntreGiros = 2f;

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

        if (giraAutomaticamente)
        {
            StartCoroutine(RotarAutomaticamente());
        }
    }

    void OnMouseDown()
    {
        if (giraAutomaticamente)
{
    return;
}
        if (estaActiva) return;
        if (tipo == TipoCelda.Fuente || tipo == TipoCelda.Objetivo) return;

        // La válvula NO rota. Solo se selecciona para abrir/cerrar con la inclinación.
        if (tipo == TipoCelda.ValvulaHorizontal)
        {
            seleccionada = (seleccionada == this) ? null : this;
            moviendose = false;
            ActualizarVisual();
            return;
        }

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
        if (sr == null) return;

        // Control especial de la válvula:
        // inclinar izquierda = abrir, inclinar derecha = cerrar.
        if (seleccionada == this && tipo == TipoCelda.ValvulaHorizontal && !estaActiva)
        {
            float factorDeOnda = (Mathf.Sin(Time.time * velocidadParpadeo) + 1f) / 2f;
            sr.color = Color.Lerp(colorVacio, colorParpadeo, factorDeOnda);

            float inclinacion = Input.acceleration.x;
            bool estadoAnterior = valvulaAbierta;

            if (inclinacion < -inclinacionValvula)
            {
                valvulaAbierta = true;
            }
            else if (inclinacion > inclinacionValvula)
            {
                valvulaAbierta = false;
            }

            if (estadoAnterior != valvulaAbierta)
            {
                ActualizarVisual();
            }

            return;
        }

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
        else if (!estaActiva && seleccionada != this)
        {
            sr.color = colorVacio;
        }
        else if (llenadoCompletado)
        {
            sr.color = colorLleno;
        }
    }

IEnumerator RotarAutomaticamente()
{
    while (true)
    {
        yield return new WaitForSeconds(tiempoEntreGiros);

        if (estaActiva)
            yield break;

        if (seleccionada != this)
        {
            visualTuberia.Rotate(0, 0, 90f);
            AplicarSnapYActualizarTipo();
        }
    }
}

    public void AplicarSnapYActualizarTipo()
    {
        if (tipo == TipoCelda.ValvulaHorizontal || tipo == TipoCelda.Fuente || tipo == TipoCelda.Objetivo || tipo == TipoCelda.Vacia)
        {
            ActualizarVisual();
            return;
        }

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
        else if (tipo == TipoCelda.Bifurcacion_SinArriba || tipo == TipoCelda.Bifurcacion_SinIzq || tipo == TipoCelda.Bifurcacion_SinAbajo || tipo == TipoCelda.Bifurcacion_SinDer)
        {
            if (anguloNormalizado == 0) tipo = TipoCelda.Bifurcacion_SinArriba;
            else if (anguloNormalizado == 90) tipo = TipoCelda.Bifurcacion_SinIzq;
            else if (anguloNormalizado == 180) tipo = TipoCelda.Bifurcacion_SinAbajo;
            else if (anguloNormalizado == 270) tipo = TipoCelda.Bifurcacion_SinDer;
        }

        ActualizarVisual();
    }

    public void ActualizarVisual()
    {
        if (sr == null && visualTuberia != null) sr = visualTuberia.GetComponent<SpriteRenderer>();
        if (sr == null || visualTuberia == null) return;

        visualTuberia.localEulerAngles = Vector3.zero;
        if (!llenadoCompletado) sr.color = estaActiva ? colorLleno : colorVacio;

        switch (tipo)
        {
            case TipoCelda.Vacia:
                sr.sprite = spriteVacia;
                break;

            case TipoCelda.Fuente:
                sr.sprite = spriteFuente;
                break;

            case TipoCelda.Objetivo:
                // Ahora el objetivo es una tubería fija dentro de la grilla, no el fuego.
                // No se rota. La conexión lógica acepta flujo desde cualquier lado.
                sr.sprite = spriteObjetivo != null ? spriteObjetivo : spriteRecta;
                visualTuberia.localEulerAngles = Vector3.zero;
                break;

           case TipoCelda.ValvulaHorizontal:
    sr.sprite = valvulaAbierta ? spriteValvulaAbierta : spriteValvulaCerrada;
    visualTuberia.localEulerAngles = valvulaVertical ? new Vector3(0, 0, 90) : Vector3.zero;
    break;

            case TipoCelda.RectaHorizontal:
                sr.sprite = spriteRecta;
                visualTuberia.localEulerAngles = new Vector3(0, 0, 0);
                break;

            case TipoCelda.RectaVertical:
                sr.sprite = spriteRecta;
                visualTuberia.localEulerAngles = new Vector3(0, 0, 90);
                break;

            case TipoCelda.CurvaArribaDer:
                sr.sprite = spriteCurva;
                visualTuberia.localEulerAngles = new Vector3(0, 0, 0);
                break;

            case TipoCelda.CurvaIzqArriba:
                sr.sprite = spriteCurva;
                visualTuberia.localEulerAngles = new Vector3(0, 0, 90);
                break;

            case TipoCelda.CurvaAbajoIzq:
                sr.sprite = spriteCurva;
                visualTuberia.localEulerAngles = new Vector3(0, 0, 180);
                break;

            case TipoCelda.CurvaDerAbajo:
                sr.sprite = spriteCurva;
                visualTuberia.localEulerAngles = new Vector3(0, 0, 270);
                break;

            case TipoCelda.Bifurcacion_SinArriba:
                sr.sprite = spriteBifurcacion;
                visualTuberia.localEulerAngles = new Vector3(0, 0, 0);
                break;

            case TipoCelda.Bifurcacion_SinIzq:
                sr.sprite = spriteBifurcacion;
                visualTuberia.localEulerAngles = new Vector3(0, 0, 90);
                break;

            case TipoCelda.Bifurcacion_SinAbajo:
                sr.sprite = spriteBifurcacion;
                visualTuberia.localEulerAngles = new Vector3(0, 0, 180);
                break;

            case TipoCelda.Bifurcacion_SinDer:
                sr.sprite = spriteBifurcacion;
                visualTuberia.localEulerAngles = new Vector3(0, 0, 270);
                break;
        }
    }

    public IEnumerator AnimarLlenado(float tiempoTotal, Direccion entrada)
    {
        estaActiva = true;
        if (seleccionada == this) seleccionada = null;

        // El objetivo ya no se destruye ni cambia a celda vacía.
        // Solo se pinta como lleno cuando el flujo llega.
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
        if (tipo == TipoCelda.Fuente)
            return dir == Direccion.Derecha;

        if (tipo == TipoCelda.Objetivo)
        {
            // El objetivo debe poder recibir flujo desde cualquier dirección.
            // Esto evita que falle si el agua entra por abajo, arriba, izquierda o derecha.
            return dir == Direccion.Arriba ||
                   dir == Direccion.Abajo ||
                   dir == Direccion.Izquierda ||
                   dir == Direccion.Derecha;
        }

        switch (tipo)
        {
          case TipoCelda.ValvulaHorizontal:
    if (valvulaVertical)
        return valvulaAbierta && (dir == Direccion.Arriba || dir == Direccion.Abajo);
    else
        return valvulaAbierta && (dir == Direccion.Izquierda || dir == Direccion.Derecha);

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

            case TipoCelda.Bifurcacion_SinArriba:
                return dir != Direccion.Arriba;

            case TipoCelda.Bifurcacion_SinIzq:
                return dir != Direccion.Izquierda;

            case TipoCelda.Bifurcacion_SinAbajo:
                return dir != Direccion.Abajo;

            case TipoCelda.Bifurcacion_SinDer:
                return dir != Direccion.Derecha;
        }

        return false;
    }
}
