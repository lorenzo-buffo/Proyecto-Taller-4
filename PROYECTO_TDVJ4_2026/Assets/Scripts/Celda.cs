using System.Collections;
using UnityEngine;

public enum Direccion
{
    Arriba, Abajo, Izquierda, Derecha
}

public class Celda : MonoBehaviour
{
    public enum TipoCelda
    {
        Vacia, Recta, Curva, Fuente, Objetivo
    }

    [Header("Sprites Base")]
    public Sprite spriteRecta;
    public Sprite spriteCurva;
    public Sprite spriteFuente;
    public Sprite spriteObjetivo;

    [Header("Animaciones de Llenado")]
    [Tooltip("Arrastra aquí los sprites de la recta llenándose en orden")]
    public Sprite[] animacionRecta;
    [Tooltip("Arrastra aquí los sprites de la curva llenándose en orden")]
    public Sprite[] animacionCurva;

    [Header("Sonidos")]
    [Tooltip("Arrastra aquí tus sonidos de rotación en el orden que quieres que suenen")]
    public AudioClip[] clipsRotacion; 
    private AudioSource reproductorAudio;
    
    // Esta variable es estática, lo que significa que TODAS las celdas comparten este mismo número
    private static int indiceSonidoActual = 0; 

    public TipoCelda tipo;
    public int rotacion; // 0, 90, 180, 270
    public bool estaActiva = false;

    public int x;
    public int y;

    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        reproductorAudio = GetComponent<AudioSource>();
        ActualizarVisual();
    }

    public void Refrescar()
    {
        ActualizarVisual();
    }

    void ActualizarVisual()
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();

        switch (tipo)
        {
            case TipoCelda.Recta: sr.sprite = spriteRecta; break;
            case TipoCelda.Curva: sr.sprite = spriteCurva; break;
            case TipoCelda.Fuente: sr.sprite = spriteFuente; break;
            case TipoCelda.Objetivo: sr.sprite = spriteObjetivo; break;
        }
    }

    void OnMouseDown()
    {
        // Si ya pasó la electricidad, bloqueamos la rotación
        if (estaActiva) return;

        Rotar();
    }

    public void Rotar()
    {
        rotacion = (rotacion + 90) % 360;
        transform.Rotate(0, 0, -90);

        // 🔥 Reproducimos el sonido en orden secuencial
        if (reproductorAudio != null && clipsRotacion != null && clipsRotacion.Length > 0)
        {
            reproductorAudio.PlayOneShot(clipsRotacion[indiceSonidoActual]);
            
            // Avanzamos al siguiente sonido. Si llega al final de la lista, vuelve a 0.
            indiceSonidoActual = (indiceSonidoActual + 1) % clipsRotacion.Length;
        }
    }

    public bool TieneConexion(Direccion dir)
    {
        int rot = rotacion / 90;

        switch (tipo)
        {
            case TipoCelda.Recta:
            case TipoCelda.Fuente:
            case TipoCelda.Objetivo:
                if (rot % 2 == 0) return dir == Direccion.Izquierda || dir == Direccion.Derecha;
                else return dir == Direccion.Arriba || dir == Direccion.Abajo;

            case TipoCelda.Curva:
                if (rot == 0) return dir == Direccion.Arriba || dir == Direccion.Derecha;
                if (rot == 1) return dir == Direccion.Derecha || dir == Direccion.Abajo;
                if (rot == 2) return dir == Direccion.Abajo || dir == Direccion.Izquierda;
                if (rot == 3) return dir == Direccion.Izquierda || dir == Direccion.Arriba;
                break;
        }
        return false;
    }

    public IEnumerator AnimarLlenado(float tiempoTotal)
    {
        estaActiva = true;
        Sprite[] frames = null;

        if (tipo == TipoCelda.Recta || tipo == TipoCelda.Fuente || tipo == TipoCelda.Objetivo) 
            frames = animacionRecta;
        else if (tipo == TipoCelda.Curva) 
            frames = animacionCurva;

        // Si tenemos frames configurados, los animamos
        if (frames != null && frames.Length > 0)
        {
            float tiempoPorFrame = tiempoTotal / frames.Length;

            for (int i = 0; i < frames.Length; i++)
            {
                sr.sprite = frames[i];
                yield return new WaitForSeconds(tiempoPorFrame);
            }
        }
        else
        {
            // Plan B temporal: Si falta configurar los frames de la curva, lo pinta
            sr.color = Color.cyan;
            yield return new WaitForSeconds(tiempoTotal);
        }
    }
}