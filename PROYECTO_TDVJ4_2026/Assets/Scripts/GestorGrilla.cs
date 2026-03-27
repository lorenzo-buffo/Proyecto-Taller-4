using System.Collections;
using UnityEngine;

public class GestorGrilla : MonoBehaviour
{
    [Header("Configuración de Grilla")]
    public int ancho = 6;
    public int alto = 6;
    public GameObject prefabCelda;
    public float tamañoCelda = 1.1f;

    [Header("Configuración del Flujo")]
    public float tiempoLlenadoPorCelda = 1.0f; // Segundos que tarda en llenarse CADA tubería
    private bool juegoTerminado = false;

    private Celda[,] grilla;

    void Start()
    {
        grilla = new Celda[ancho, alto];

        GenerarGrilla();
        GenerarCamino();
        RellenarCeldas();
        MezclarCeldas();
        RefrescarCeldas();

        // Iniciamos el flujo constante en lugar del cálculo instantáneo
        IniciarFlujo();
    }

    void GenerarGrilla()
    {
        float offsetX = (ancho - 1) * tamañoCelda / 2f;
        float offsetY = (alto - 1) * tamañoCelda / 2f;

        for (int x = 0; x < ancho; x++)
        {
            for (int y = 0; y < alto; y++)
            {
                Vector3 posicion = new Vector3(
                    x * tamañoCelda - offsetX,
                    y * tamañoCelda - offsetY,
                    0
                );

                GameObject celdaObj = Instantiate(prefabCelda, posicion, Quaternion.identity);
                Celda celda = celdaObj.GetComponent<Celda>();
                celda.x = x;
                celda.y = y;
                celda.tipo = Celda.TipoCelda.Vacia;

                grilla[x, y] = celda;
            }
        }
    }

    void GenerarCamino()
    {
        int x = 0;
        int y = 0;

        grilla[x, y].tipo = Celda.TipoCelda.Fuente;

        while (x < ancho - 1)
        {
            x++;
            grilla[x, y].tipo = Celda.TipoCelda.Recta;
        }

        while (y < alto - 1)
        {
            y++;
            grilla[x, y].tipo = Celda.TipoCelda.Recta;
        }

        grilla[x, y].tipo = Celda.TipoCelda.Objetivo;
    }

    void RellenarCeldas()
    {
        for (int x = 0; x < ancho; x++)
        {
            for (int y = 0; y < alto; y++)
            {
                if (grilla[x, y].tipo == Celda.TipoCelda.Vacia)
                {
                    if (Random.value > 0.5f)
                        grilla[x, y].tipo = Celda.TipoCelda.Recta;
                    else
                        grilla[x, y].tipo = Celda.TipoCelda.Curva;
                }
            }
        }
    }

    void MezclarCeldas()
    {
        foreach (Celda celda in grilla)
        {
            int rotaciones = Random.Range(0, 4);
            for (int i = 0; i < rotaciones; i++)
            {
                celda.Rotar();
            }
        }
    }

    void RefrescarCeldas()
    {
        foreach (Celda celda in grilla)
        {
            celda.Refrescar();
        }
    }

    // ⚡ ================= SISTEMA DE FLUJO CONSTANTE =================

    public void IniciarFlujo()
    {
        StartCoroutine(RutinaFlujo());
    }

    IEnumerator RutinaFlujo()
    {
        Celda actual = ObtenerFuente();
        if (actual == null) yield break;

        // Pequeña pausa al iniciar el nivel para que el jugador se prepare
        yield return new WaitForSeconds(2.0f);

        // Animar la fuente inicial
        yield return StartCoroutine(actual.AnimarLlenado(tiempoLlenadoPorCelda));

        while (!juegoTerminado)
        {
            Celda siguiente = null;

            foreach (Direccion dir in System.Enum.GetValues(typeof(Direccion)))
            {
                if (actual.TieneConexion(dir))
                {
                    Celda vecino = ObtenerVecino(actual.x, actual.y, dir);

                    if (vecino != null && !vecino.estaActiva && vecino.TieneConexion(Opuesta(dir)))
                    {
                        siguiente = vecino;
                        break;
                    }
                }
            }

            if (siguiente != null)
            {
                // La magia ocurre aquí: espera a que termine de animarse la celda actual para avanzar
                yield return StartCoroutine(siguiente.AnimarLlenado(tiempoLlenadoPorCelda));
                actual = siguiente;

                if (actual.tipo == Celda.TipoCelda.Objetivo)
                {
                    Debug.Log("¡GANASTE! La energía llegó al objetivo.");
                    juegoTerminado = true;
                }
            }
            else
            {
                Debug.Log("¡CORTOCIRCUITO! El flujo se detuvo.");
                juegoTerminado = true;
            }
        }
    }

    Celda ObtenerFuente()
    {
        foreach (Celda celda in grilla)
        {
            if (celda.tipo == Celda.TipoCelda.Fuente)
                return celda;
        }
        return null;
    }

    Celda ObtenerVecino(int x, int y, Direccion dir)
    {
        switch (dir)
        {
            case Direccion.Arriba: y += 1; break;
            case Direccion.Abajo: y -= 1; break;
            case Direccion.Izquierda: x -= 1; break;
            case Direccion.Derecha: x += 1; break;
        }

        if (x >= 0 && x < ancho && y >= 0 && y < alto)
            return grilla[x, y];

        return null;
    }

    Direccion Opuesta(Direccion dir)
    {
        switch (dir)
        {
            case Direccion.Arriba: return Direccion.Abajo;
            case Direccion.Abajo: return Direccion.Arriba;
            case Direccion.Izquierda: return Direccion.Derecha;
            case Direccion.Derecha: return Direccion.Izquierda;
        }
        return Direccion.Arriba;
    }
}