using System.Collections;
using UnityEngine;

public enum Direccion
{
    Arriba, Abajo, Izquierda, Derecha, Ninguna
}

public class Celda : MonoBehaviour
{
    public enum TipoCelda
    {
        Vacia, 
        RectaHorizontal, RectaVertical, 
        CurvaArribaDer, CurvaDerAbajo, CurvaAbajoIzq, CurvaIzqArriba, 
        Fuente, Objetivo
    }

    [Header("Sprites Base")]
    public Sprite spriteVacia; 
    public Sprite spriteRecta;
    public Sprite spriteCurva;
    public Sprite spriteFuente;
    public Sprite spriteObjetivo;

    [Header("Animaciones de Llenado")]
    public Sprite[] animacionRecta;
    public Sprite[] animacionCurva;
    // 🔥 NUEVO: Espacios dedicados para la Fuente y el Objetivo
    public Sprite[] animacionFuente; 
    public Sprite[] animacionObjetivo;

    public TipoCelda tipo;
    public bool estaActiva = false;
    public int x;
    public int y;

    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        ActualizarVisual();
    }

    public void Refrescar()
    {
        ActualizarVisual();
    }

    public void ActualizarVisual()
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();

        transform.localEulerAngles = Vector3.zero;
        sr.color = Color.white;

        switch (tipo)
        {
            case TipoCelda.Vacia: sr.sprite = spriteVacia; break;
            case TipoCelda.Fuente: sr.sprite = spriteFuente; break;
            case TipoCelda.Objetivo: sr.sprite = spriteObjetivo; break;
            
            case TipoCelda.RectaHorizontal: 
                sr.sprite = spriteRecta; 
                transform.localEulerAngles = new Vector3(0, 0, 0); 
                break;
            case TipoCelda.RectaVertical: 
                sr.sprite = spriteRecta; 
                transform.localEulerAngles = new Vector3(0, 0, 90); 
                break;
            
            case TipoCelda.CurvaArribaDer: 
                sr.sprite = spriteCurva; 
                transform.localEulerAngles = new Vector3(0, 0, 0); 
                break;
            case TipoCelda.CurvaDerAbajo: 
                sr.sprite = spriteCurva; 
                transform.localEulerAngles = new Vector3(0, 0, -90); 
                break;
            case TipoCelda.CurvaAbajoIzq: 
                sr.sprite = spriteCurva; 
                transform.localEulerAngles = new Vector3(0, 0, -180); 
                break;
            case TipoCelda.CurvaIzqArriba: 
                sr.sprite = spriteCurva; 
                transform.localEulerAngles = new Vector3(0, 0, -270); 
                break;
        }
    }

    public bool TieneConexion(Direccion dir)
    {
        switch (tipo)
        {
            case TipoCelda.Fuente:
            case TipoCelda.Objetivo:
                return true; 
            case TipoCelda.RectaHorizontal: return dir == Direccion.Izquierda || dir == Direccion.Derecha;
            case TipoCelda.RectaVertical: return dir == Direccion.Arriba || dir == Direccion.Abajo;
            case TipoCelda.CurvaArribaDer: return dir == Direccion.Arriba || dir == Direccion.Derecha;
            case TipoCelda.CurvaDerAbajo: return dir == Direccion.Derecha || dir == Direccion.Abajo;
            case TipoCelda.CurvaAbajoIzq: return dir == Direccion.Abajo || dir == Direccion.Izquierda;
            case TipoCelda.CurvaIzqArriba: return dir == Direccion.Izquierda || dir == Direccion.Arriba;
        }
        return false;
    }

    public IEnumerator AnimarLlenado(float tiempoTotal, Direccion entrada)
    {
        estaActiva = true;

        if (entrada != Direccion.Ninguna)
        {
            if (tipo == TipoCelda.RectaHorizontal && entrada == Direccion.Derecha) transform.localEulerAngles = new Vector3(0, 0, 180);
            else if (tipo == TipoCelda.RectaVertical && entrada == Direccion.Arriba) transform.localEulerAngles = new Vector3(0, 0, -90);
        }

        Sprite[] frames = null;

        // 🔥 MODIFICADO: Ahora cada tipo busca su propia lista de imágenes
        if (tipo == TipoCelda.RectaHorizontal || tipo == TipoCelda.RectaVertical) 
            frames = animacionRecta;
        else if (tipo == TipoCelda.CurvaArribaDer || tipo == TipoCelda.CurvaDerAbajo || tipo == TipoCelda.CurvaAbajoIzq || tipo == TipoCelda.CurvaIzqArriba) 
            frames = animacionCurva;
        else if (tipo == TipoCelda.Fuente)
            frames = animacionFuente;
        else if (tipo == TipoCelda.Objetivo)
            frames = animacionObjetivo;

        if (frames != null && frames.Length > 0)
        {
            float tiempoPorFrame = tiempoTotal / frames.Length;

            bool reversa = false;
            if (tipo == TipoCelda.CurvaArribaDer && entrada == Direccion.Derecha) reversa = true;
            if (tipo == TipoCelda.CurvaDerAbajo && entrada == Direccion.Abajo) reversa = true;
            if (tipo == TipoCelda.CurvaAbajoIzq && entrada == Direccion.Izquierda) reversa = true;
            if (tipo == TipoCelda.CurvaIzqArriba && entrada == Direccion.Arriba) reversa = true;

            if (!reversa)
            {
                for (int i = 0; i < frames.Length; i++)
                {
                    sr.sprite = frames[i];
                    yield return new WaitForSeconds(tiempoPorFrame);
                }
            }
            else
            {
                for (int i = frames.Length - 1; i >= 0; i--)
                {
                    sr.sprite = frames[i];
                    yield return new WaitForSeconds(tiempoPorFrame);
                }
            }
        }
        else
        {
            // Si no le pusiste animación a la fuente/objetivo, simplemente espera el tiempo sin cambiar el sprite ni el color
            if (tipo == TipoCelda.Fuente || tipo == TipoCelda.Objetivo)
            {
                yield return new WaitForSeconds(tiempoTotal);
            }
            else
            {
                // Solo pinta de cyan las tuberías normales a las que les falte animación
                sr.color = Color.cyan;
                yield return new WaitForSeconds(tiempoTotal);
            }
        }
    }
}