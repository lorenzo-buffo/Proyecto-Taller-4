using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GestorGrilla : MonoBehaviour
{
    [Header("Configuración de Grilla")]
    public int ancho = 6;
    public int alto = 6;
    public GameObject prefabCelda;
    public float tamañoCelda = 1.1f;

    // 🔥 NUEVAS VARIABLES PARA POSICIONAR INICIO Y FIN
    [Header("Posiciones Iniciales")]
    [Tooltip("Coordenadas de inicio (X, Y). Recuerda empezar en 0.")]
    public Vector2Int posicionFuente = new Vector2Int(0, 0);
    [Tooltip("Coordenadas del final (X, Y).")]
    public Vector2Int posicionObjetivo = new Vector2Int(5, 5);

    [Header("Configuración del Flujo")]
    public float tiempoLlenadoPorCelda = 1.0f; 
    private bool juegoTerminado = false;

    [Header("Interfaz de Usuario")]
    public GameObject panelPopup;
    public GameObject botonSiguienteNivel; 

    private Celda[,] grilla;

    void Start()
    {
        grilla = new Celda[ancho, alto];

        if (panelPopup != null) panelPopup.transform.localScale = Vector3.zero;

        GenerarGrilla();
        PrepararTableroVacio(); 
        
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
                Vector3 posicion = new Vector3(x * tamañoCelda - offsetX, y * tamañoCelda - offsetY, 0);
                GameObject celdaObj = Instantiate(prefabCelda, posicion, Quaternion.identity);
                Celda celda = celdaObj.GetComponent<Celda>();
                celda.x = x;
                celda.y = y;
                celda.tipo = Celda.TipoCelda.Vacia; 
                grilla[x, y] = celda;
            }
        }
    }

    void PrepararTableroVacio()
    {
        // 🔥 MODIFICADO: Usamos las coordenadas del Inspector y las limitamos por seguridad
        posicionFuente.x = Mathf.Clamp(posicionFuente.x, 0, ancho - 1);
        posicionFuente.y = Mathf.Clamp(posicionFuente.y, 0, alto - 1);
        posicionObjetivo.x = Mathf.Clamp(posicionObjetivo.x, 0, ancho - 1);
        posicionObjetivo.y = Mathf.Clamp(posicionObjetivo.y, 0, alto - 1);

        // Colocamos la Fuente en la coordenada elegida
        grilla[posicionFuente.x, posicionFuente.y].tipo = Celda.TipoCelda.Fuente;
        grilla[posicionFuente.x, posicionFuente.y].ActualizarVisual();

        // Colocamos el Objetivo en la coordenada elegida
        grilla[posicionObjetivo.x, posicionObjetivo.y].tipo = Celda.TipoCelda.Objetivo;
        grilla[posicionObjetivo.x, posicionObjetivo.y].ActualizarVisual();
    }

    public void IniciarFlujo()
    {
        juegoTerminado = false;
        StartCoroutine(RutinaFlujo());
    }

    IEnumerator RutinaFlujo()
    {
        Celda actual = ObtenerFuente();
        if (actual == null) yield break;

        yield return new WaitForSeconds(4.0f);

        yield return StartCoroutine(actual.AnimarLlenado(tiempoLlenadoPorCelda, Direccion.Ninguna));

        while (!juegoTerminado)
        {
            Celda siguiente = null;
            Direccion direccionEntrada = Direccion.Ninguna;

            foreach (Direccion dir in System.Enum.GetValues(typeof(Direccion)))
            {
                if (dir == Direccion.Ninguna) continue; 

                if (actual.TieneConexion(dir))
                {
                    Celda vecino = ObtenerVecino(actual.x, actual.y, dir);

                    if (vecino != null && !vecino.estaActiva && vecino.TieneConexion(Opuesta(dir)))
                    {
                        siguiente = vecino;
                        direccionEntrada = Opuesta(dir); 
                        break;
                    }
                }
            }

            if (siguiente != null)
            {
                yield return StartCoroutine(siguiente.AnimarLlenado(tiempoLlenadoPorCelda, direccionEntrada));
                actual = siguiente;

                if (actual.tipo == Celda.TipoCelda.Objetivo)
                {
                    Debug.Log("¡GANASTE! La energía llegó al objetivo.");
                    juegoTerminado = true;
                    
                    int indiceEscenaActual = SceneManager.GetActiveScene().buildIndex;
                    int proximoNivelADesbloquear = indiceEscenaActual + 1;
                    int maxNivelYaDesbloqueadoEnPrefs = PlayerPrefs.GetInt("MaxNivelDesbloqueado", 1);
                    
                    if (proximoNivelADesbloquear > maxNivelYaDesbloqueadoEnPrefs)
                    {
                        PlayerPrefs.SetInt("MaxNivelDesbloqueado", proximoNivelADesbloquear);
                        PlayerPrefs.Save(); 
                    }

                    StartCoroutine(AnimarAparicionPopup(true));
                }
            }
            else
            {
                Debug.Log("¡CORTOCIRCUITO! El flujo se detuvo.");
                juegoTerminado = true;
                StartCoroutine(AnimarAparicionPopup(false));
            }
        }
    }

    IEnumerator AnimarAparicionPopup(bool victoria)
    {
        if (panelPopup == null) yield break; 

        if (botonSiguienteNivel != null)
        {
            botonSiguienteNivel.SetActive(victoria);
        }

        float duracionAnimacion = 0.4f; 
        float tiempo = 0f;

        Vector3 escalaInicial = Vector3.zero;
        Vector3 escalaFinal = Vector3.one;

        while (tiempo < duracionAnimacion)
        {
            tiempo += Time.deltaTime;
            float progreso = tiempo / duracionAnimacion;
            float curvaSuave = Mathf.SmoothStep(0f, 1f, progreso);

            panelPopup.transform.localScale = Vector3.Lerp(escalaInicial, escalaFinal, curvaSuave);
            
            yield return null; 
        }

        panelPopup.transform.localScale = escalaFinal;
    }

    Celda ObtenerFuente()
    {
        foreach (Celda celda in grilla)
        {
            if (celda.tipo == Celda.TipoCelda.Fuente) return celda;
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

        if (x >= 0 && x < ancho && y >= 0 && y < alto) return grilla[x, y];
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

    public void ReiniciarNivel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void VolverAlSelector()
    {
        SceneManager.LoadScene("Menu"); 
    }

    public void CargarSiguienteNivel()
    {
        int siguienteIndice = SceneManager.GetActiveScene().buildIndex + 1;

        if (siguienteIndice < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(siguienteIndice);
        }
        else
        {
            SceneManager.LoadScene("Menu");
        }
    }
}