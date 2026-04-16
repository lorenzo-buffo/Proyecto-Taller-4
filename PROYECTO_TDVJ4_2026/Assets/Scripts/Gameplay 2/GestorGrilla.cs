using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Necesario para modificar el color de las Imágenes
using TMPro; // Necesario para los textos de la UI

public class GestorGrilla : MonoBehaviour
{
    public static GestorGrilla instancia; 

    [Header("Configuración de Grilla")]
    public int ancho = 6;
    public int alto = 6;
    public GameObject prefabCelda;
    public float tamañoCelda = 1.1f;

    [Header("Posiciones Iniciales")]
    public Vector2Int posicionFuente = new Vector2Int(0, 0);
    public Vector2Int posicionObjetivo = new Vector2Int(5, 5);

    [Header("Configuración del Flujo")]
    public float tiempoLlenadoNormal = 1.0f; 
    public float tiempoLlenadoRapido = 0.1f; 
    [HideInInspector] public float tiempoLlenadoActual; 
    
    [Tooltip("Activa esto en el Nivel 1 para que el agua no salga hasta que el jugador toque la pieza")]
    public bool esNivelTutorial = false; 
    private bool juegoTerminado = false;
    private bool flujoAcelerado = false; 

    [Header("Sistema de Estrellas")]
    public int movimientosActuales = 0;
    public int maxMovimientos3Estrellas = 3;
    public int maxMovimientos2Estrellas = 6;

    [Header("Interfaz de Juego")]
    public TextMeshProUGUI textoMovimientos;
    public GameObject botonAcelerar;
    public GameObject panelPopup;
    public GameObject botonSiguienteNivel; 

    [Header("Estrellas UI (Popup)")]
    public Image[] estrellasPopup; 
    public Color colorEstrellaGanada = Color.cyan; 
    public Color colorEstrellaPerdida = new Color(0.2f, 0.2f, 0.2f, 1f); 

    private Celda[,] grilla;

    void Awake()
    {
        if (instancia == null) instancia = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        grilla = new Celda[ancho, alto];
        tiempoLlenadoActual = tiempoLlenadoNormal; 

        if (panelPopup != null) panelPopup.transform.localScale = Vector3.zero;
        if (botonAcelerar != null) botonAcelerar.SetActive(false); 

        ActualizarTextoMovimientos();
        GenerarGrilla();
        PrepararTableroRandom(); 
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

    void PrepararTableroRandom()
    {
        posicionFuente.x = Mathf.Clamp(posicionFuente.x, 0, ancho - 1);
        posicionFuente.y = Mathf.Clamp(posicionFuente.y, 0, alto - 1);
        posicionObjetivo.x = Mathf.Clamp(posicionObjetivo.x, 0, ancho - 1);
        posicionObjetivo.y = Mathf.Clamp(posicionObjetivo.y, 0, alto - 1);

        for (int x = 0; x < ancho; x++)
        {
            for (int y = 0; y < alto; y++)
            {
                grilla[x, y].tipo = (Random.value > 0.5f) ? Celda.TipoCelda.RectaHorizontal : Celda.TipoCelda.CurvaArribaDer;
                int girosAleatorios = Random.Range(0, 4);
                grilla[x, y].visualTuberia.Rotate(0, 0, girosAleatorios * 90f);
                grilla[x, y].ActualizarVisual();
            }
        }

        GenerarCaminoSeguro();

        grilla[posicionFuente.x, posicionFuente.y].tipo = Celda.TipoCelda.Fuente;
        grilla[posicionFuente.x, posicionFuente.y].ActualizarVisual();

        grilla[posicionObjetivo.x, posicionObjetivo.y].tipo = Celda.TipoCelda.Objetivo;
        grilla[posicionObjetivo.x, posicionObjetivo.y].ActualizarVisual();
    }

    public void RegistrarMovimiento()
    {
        if (!juegoTerminado)
        {
            movimientosActuales++;
            ActualizarTextoMovimientos();
        }
    }

    void ActualizarTextoMovimientos()
    {
        if (textoMovimientos != null)
        {
            // Calculamos cuál es el próximo límite a mostrar en tiempo real
            if (movimientosActuales <= maxMovimientos3Estrellas)
            {
                textoMovimientos.text = $"MOVIMIENTOS: {movimientosActuales} / {maxMovimientos3Estrellas}";
            }
            else if (movimientosActuales <= maxMovimientos2Estrellas)
            {
                textoMovimientos.text = $"MOVIMIENTOS: {movimientosActuales} / {maxMovimientos2Estrellas}";
            }
            else
            {
                textoMovimientos.text = $"MOVIMIENTOS: {movimientosActuales}"; 
            }
        }
    }

    public void AcelerarFlujo()
    {
        flujoAcelerado = true;
        tiempoLlenadoActual = tiempoLlenadoRapido;
        if (botonAcelerar != null) botonAcelerar.SetActive(false); 
        Debug.Log("¡Avance Rápido Activado!");
    }

    public void IniciarFlujo()
    {
        juegoTerminado = false;
        flujoAcelerado = false;
        StartCoroutine(RutinaFlujo());
    }

    IEnumerator RutinaFlujo()
    {
        Celda actual = ObtenerFuente();
        if (actual == null) yield break;

        // Lógica de espera interactiva
        if (esNivelTutorial)
        {
            while (Celda.seleccionada == null && !flujoAcelerado) yield return null; 
            while (Celda.seleccionada != null && !flujoAcelerado) yield return null;
            if (!flujoAcelerado) yield return new WaitForSeconds(0.5f);
        }
        else
        {
            float tiempoEspera = 4.0f;
            while (tiempoEspera > 0 && !flujoAcelerado)
            {
                tiempoEspera -= Time.deltaTime;
                yield return null; 
            }
        }

        if (botonAcelerar != null && !flujoAcelerado) botonAcelerar.SetActive(true);

        yield return StartCoroutine(actual.AnimarLlenado(tiempoLlenadoActual, Direccion.Ninguna));

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
                yield return StartCoroutine(siguiente.AnimarLlenado(tiempoLlenadoActual, direccionEntrada));
                actual = siguiente;

                if (actual.tipo == Celda.TipoCelda.Objetivo)
                {
                    juegoTerminado = true;
                    if (botonAcelerar != null) botonAcelerar.SetActive(false);
                    StartCoroutine(AnimarAparicionPopup(true));
                }
            }
            else
            {
                juegoTerminado = true;
                if (botonAcelerar != null) botonAcelerar.SetActive(false);
                StartCoroutine(AnimarAparicionPopup(false));
            }
        }
    }

    IEnumerator AnimarAparicionPopup(bool victoria)
    {
        if (panelPopup == null) yield break; 
        if (botonSiguienteNivel != null) botonSiguienteNivel.SetActive(victoria);

        if (victoria)
        {
            int estrellasGanadas = (movimientosActuales <= maxMovimientos3Estrellas) ? 3 : (movimientosActuales <= maxMovimientos2Estrellas) ? 2 : 1;
            
            // 🔥 Pintar las estrellas visuales del Popup 🔥
            if (estrellasPopup != null && estrellasPopup.Length > 0)
            {
                for (int i = 0; i < estrellasPopup.Length; i++)
                {
                    if (estrellasPopup[i] != null)
                    {
                        estrellasPopup[i].color = (i < estrellasGanadas) ? colorEstrellaGanada : colorEstrellaPerdida;
                    }
                }
            }

            int proximoNivel = SceneManager.GetActiveScene().buildIndex + 1;
            PlayerPrefs.SetInt("MaxNivelDesbloqueado", Mathf.Max(PlayerPrefs.GetInt("MaxNivelDesbloqueado", 1), proximoNivel));
            
            string nombreNivel = SceneManager.GetActiveScene().name;
            int estrellasViejas = PlayerPrefs.GetInt("Estrellas_" + nombreNivel, 0);
            if (estrellasGanadas > estrellasViejas)
            {
                PlayerPrefs.SetInt("Estrellas_" + nombreNivel, estrellasGanadas);
            }
            PlayerPrefs.Save();
        }

        float tiempo = 0f;
        while (tiempo < 0.4f)
        {
            tiempo += Time.deltaTime;
            panelPopup.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, tiempo / 0.4f);
            yield return null; 
        }
        panelPopup.transform.localScale = Vector3.one;
    }

    Celda ObtenerFuente()
    {
        foreach (Celda celda in grilla)
        {
            if (celda != null && celda.tipo == Celda.TipoCelda.Fuente) return celda;
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

    public void ReiniciarNivel() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    public void VolverAlSelector() => SceneManager.LoadScene("ModoLogicoNiveles");
    public void CargarSiguienteNivel() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);

    void GenerarCaminoSeguro()
    {
        List<Vector2Int> camino = new List<Vector2Int>();
        Vector2Int actual = posicionFuente;
        camino.Add(actual);

        while (actual != posicionObjetivo)
        {
            bool moverEnX = (actual.x != posicionObjetivo.x && actual.y != posicionObjetivo.y) ? (Random.value > 0.5f) : (actual.x != posicionObjetivo.x);
            if (moverEnX) actual.x += (posicionObjetivo.x > actual.x) ? 1 : -1;
            else actual.y += (posicionObjetivo.y > actual.y) ? 1 : -1;
            camino.Add(actual);
        }

        for (int i = 1; i < camino.Count - 1; i++)
        {
            Vector2Int prev = camino[i - 1];
            Vector2Int next = camino[i + 1];
            Vector2Int current = camino[i];

            bool esLineaRecta = (prev.x == next.x) || (prev.y == next.y);

            if (esLineaRecta)
            {
                grilla[current.x, current.y].tipo = Celda.TipoCelda.RectaHorizontal;
                if (esNivelTutorial) grilla[current.x, current.y].visualTuberia.rotation = Quaternion.Euler(0, 0, 90);
            }
            else
            {
                grilla[current.x, current.y].tipo = Celda.TipoCelda.CurvaArribaDer;
            }

            grilla[current.x, current.y].AplicarSnapYActualizarTipo();
        }
    }
}