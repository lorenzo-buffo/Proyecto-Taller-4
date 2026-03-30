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

    [Header("Configuración del Flujo")]
    public float tiempoLlenadoPorCelda = 1.0f; 
    private bool juegoTerminado = false;

    [Header("Interfaz de Usuario")]
    public GameObject panelPopup;
    [Tooltip("Arrastra aquí tu botón de 'Siguiente Nivel'")]
    public GameObject botonSiguienteNivel; // 🔥 NUEVO: Referencia al tercer botón

    private Celda[,] grilla;

    void Start()
    {
        grilla = new Celda[ancho, alto];

        if (panelPopup != null) panelPopup.transform.localScale = Vector3.zero;

        GenerarGrilla();
        GenerarCamino();
        RellenarCeldas();
        MezclarCeldas();
        RefrescarCeldas();

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

   void GenerarCamino()
    {
        List<Vector2Int> camino = new List<Vector2Int>();
        int x = 0;
        int y = 0;
        camino.Add(new Vector2Int(x, y));

        while (x < ancho - 1 || y < alto - 1)
        {
            bool moverDerecha = Random.value > 0.5f;

            if (moverDerecha && x < ancho - 1) x++;
            else if (y < alto - 1) y++;
            else x++; 

            camino.Add(new Vector2Int(x, y));
        }

        for (int i = 0; i < camino.Count; i++)
        {
            Vector2Int actual = camino[i];

            if (i == 0) grilla[actual.x, actual.y].tipo = Celda.TipoCelda.Fuente;
            else if (i == camino.Count - 1) grilla[actual.x, actual.y].tipo = Celda.TipoCelda.Objetivo;
            else
            {
                Vector2Int previo = camino[i - 1];
                Vector2Int siguiente = camino[i + 1];

                if (previo.x == siguiente.x || previo.y == siguiente.y)
                    grilla[actual.x, actual.y].tipo = Celda.TipoCelda.Recta;
                else
                    grilla[actual.x, actual.y].tipo = Celda.TipoCelda.Curva;
            }
        }
    }

    void RellenarCeldas()
    {
        for (int x = 0; x < ancho; x++)
        {
            for (int y = 0; y < alto; y++)
            {
                if (grilla[x, y].tipo == Celda.TipoCelda.Vacia)
                {
                    if (Random.value > 0.5f) grilla[x, y].tipo = Celda.TipoCelda.Recta;
                    else grilla[x, y].tipo = Celda.TipoCelda.Curva;
                }
            }
        }
    }

    void MezclarCeldas()
    {
        foreach (Celda celda in grilla)
        {
            int rotaciones = Random.Range(0, 4);
            for (int i = 0; i < rotaciones; i++) celda.Rotar();
        }
    }

    void RefrescarCeldas()
    {
        foreach (Celda celda in grilla) celda.Refrescar();
    }

    public void IniciarFlujo()
    {
        StartCoroutine(RutinaFlujo());
    }

    IEnumerator RutinaFlujo()
    {
        Celda actual = ObtenerFuente();
        if (actual == null) yield break;

        yield return new WaitForSeconds(2.0f);

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

                    // 🔥 Le pasamos "true" porque el jugador GANÓ
                    StartCoroutine(AnimarAparicionPopup(true));
                }
            }
            else
            {
                Debug.Log("¡CORTOCIRCUITO! El flujo se detuvo.");
                juegoTerminado = true;
                
                // 🔥 Le pasamos "false" porque el jugador PERDIÓ
                StartCoroutine(AnimarAparicionPopup(false));
            }
        }
    }

    // 🔥 MODIFICADO: Ahora recibe un bool para saber si fue victoria o derrota
    IEnumerator AnimarAparicionPopup(bool victoria)
    {
        if (panelPopup == null) yield break; 

        // Activamos o desactivamos el botón de siguiente nivel según el resultado
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

    // ==========================================
    // 🖱️ FUNCIONES DE LOS BOTONES DE LA UI
    // ==========================================

    public void ReiniciarNivel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void VolverAlSelector()
    {
        SceneManager.LoadScene("Menu"); 
    }

    // 🔥 NUEVA FUNCION: Para el botón de Siguiente Nivel
    public void CargarSiguienteNivel()
    {
        int siguienteIndice = SceneManager.GetActiveScene().buildIndex + 1;

        // Verificamos de forma segura que exista un siguiente nivel en la lista de Build Settings
        if (siguienteIndice < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(siguienteIndice);
        }
        else
        {
            // Si el jugador acaba de ganar el ÚLTIMO nivel, lo mandamos al menú
            SceneManager.LoadScene("Menu");
        }
    }
}