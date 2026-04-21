using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

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

    [Header("Mecánica Nivel 3+ (Bifurcación)")]
    [Tooltip("Activa esto SOLO en los niveles donde quieras la tubería en T y 2 salidas")]
    public bool usarBifurcacion = false;
    public Vector2Int posicionObjetivo2 = new Vector2Int(5, 0);

    [Header("Configuración del Flujo")]
    public float tiempoLlenadoNormal = 1.0f; 
    public float tiempoLlenadoRapido = 0.1f; 
    [HideInInspector] public float tiempoLlenadoActual; 
    
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

    [Header("Progreso y Navegación")]
    public int numeroDeEsteNivel = 1;
    public string nombreSiguienteEscena;

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
        posicionObjetivo2.x = Mathf.Clamp(posicionObjetivo2.x, 0, ancho - 1);
        posicionObjetivo2.y = Mathf.Clamp(posicionObjetivo2.y, 0, alto - 1);

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

        if (usarBifurcacion) GenerarCaminosSegurosBifurcados();
        else GenerarCaminoSeguroClasico();

        grilla[posicionFuente.x, posicionFuente.y].tipo = Celda.TipoCelda.Fuente;
        grilla[posicionFuente.x, posicionFuente.y].ActualizarVisual();

        grilla[posicionObjetivo.x, posicionObjetivo.y].tipo = Celda.TipoCelda.Objetivo;
        grilla[posicionObjetivo.x, posicionObjetivo.y].ActualizarVisual();

        if (usarBifurcacion)
        {
            grilla[posicionObjetivo2.x, posicionObjetivo2.y].tipo = Celda.TipoCelda.Objetivo;
            grilla[posicionObjetivo2.x, posicionObjetivo2.y].ActualizarVisual();
        }
    }

    IEnumerator RutinaFlujo()
    {
        List<Celda> frentesDeAgua = new List<Celda>();
        Celda fuente = ObtenerFuente();
        if (fuente == null) yield break;
        
        frentesDeAgua.Add(fuente);
        int objetivosNecesarios = usarBifurcacion ? 2 : 1;
        int objetivosAlcanzados = 0;

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

        yield return StartCoroutine(fuente.AnimarLlenado(tiempoLlenadoActual, Direccion.Ninguna));

        while (!juegoTerminado)
        {
            List<Celda> siguientesCeldas = new List<Celda>();
            List<Direccion> direccionesEntrada = new List<Direccion>();

            foreach (Celda actual in frentesDeAgua)
            {
                if (actual.tipo == Celda.TipoCelda.Objetivo) continue;

                foreach (Direccion dir in System.Enum.GetValues(typeof(Direccion)))
                {
                    if (dir == Direccion.Ninguna) continue; 
                    if (actual.TieneConexion(dir))
                    {
                        Celda vecino = ObtenerVecino(actual.x, actual.y, dir);
                        if (vecino != null && !vecino.estaActiva && vecino.TieneConexion(Opuesta(dir)))
                        {
                            siguientesCeldas.Add(vecino);
                            direccionesEntrada.Add(Opuesta(dir));
                        }
                    }
                }
            }

            if (siguientesCeldas.Count > 0)
            {
                List<Coroutine> animacionesEnCurso = new List<Coroutine>();
                for (int i = 0; i < siguientesCeldas.Count; i++)
                {
                    animacionesEnCurso.Add(StartCoroutine(siguientesCeldas[i].AnimarLlenado(tiempoLlenadoActual, direccionesEntrada[i])));
                }
                foreach (Coroutine anim in animacionesEnCurso) yield return anim;

                frentesDeAgua = siguientesCeldas;

                foreach (Celda celdaLlena in frentesDeAgua)
                {
                    if (celdaLlena.tipo == Celda.TipoCelda.Objetivo) objetivosAlcanzados++;
                }

                if (objetivosAlcanzados >= objetivosNecesarios) TerminarJuego(true);
            }
            else 
            {
                TerminarJuego(false);
            }
        }
    }

    void TerminarJuego(bool victoria)
    {
        juegoTerminado = true;
        if (botonAcelerar != null) botonAcelerar.SetActive(false);
        StartCoroutine(AnimarAparicionPopup(victoria));
    }

    public void RegistrarMovimiento()
    {
        if (!juegoTerminado) { movimientosActuales++; ActualizarTextoMovimientos(); }
    }

    void ActualizarTextoMovimientos()
    {
        if (textoMovimientos != null)
        {
            if (movimientosActuales <= maxMovimientos3Estrellas) textoMovimientos.text = $"MOVIMIENTOS: {movimientosActuales} / {maxMovimientos3Estrellas}";
            else if (movimientosActuales <= maxMovimientos2Estrellas) textoMovimientos.text = $"MOVIMIENTOS: {movimientosActuales} / {maxMovimientos2Estrellas}";
            else textoMovimientos.text = $"MOVIMIENTOS: {movimientosActuales}"; 
        }
    }

    public void AcelerarFlujo()
    {
        flujoAcelerado = true;
        tiempoLlenadoActual = tiempoLlenadoRapido;
        if (botonAcelerar != null) botonAcelerar.SetActive(false); 
    }
    
    public void IniciarFlujo() { juegoTerminado = false; flujoAcelerado = false; StartCoroutine(RutinaFlujo()); }
    public void ReiniciarNivel() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    public void VolverAlSelector() => SceneManager.LoadScene("ModoLogicoNiveles");
    
    public void CargarSiguienteNivel() 
    {
        if (!string.IsNullOrEmpty(nombreSiguienteEscena)) SceneManager.LoadScene(nombreSiguienteEscena);
        else SceneManager.LoadScene("ModoLogicoNiveles"); 
    }

    Celda ObtenerFuente()
    {
        foreach (Celda celda in grilla) if (celda != null && celda.tipo == Celda.TipoCelda.Fuente) return celda;
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

    IEnumerator AnimarAparicionPopup(bool victoria)
    {
        if (panelPopup == null) yield break; 
        if (botonSiguienteNivel != null) botonSiguienteNivel.SetActive(victoria);

        if (victoria)
        {
            int estrellasGanadas = (movimientosActuales <= maxMovimientos3Estrellas) ? 3 : (movimientosActuales <= maxMovimientos2Estrellas) ? 2 : 1;
            if (estrellasPopup != null)
            {
                for (int i = 0; i < estrellasPopup.Length; i++)
                {
                    if (estrellasPopup[i] != null) estrellasPopup[i].color = (i < estrellasGanadas) ? colorEstrellaGanada : colorEstrellaPerdida;
                }
            }

            int proximoNivel = numeroDeEsteNivel + 1; 
            PlayerPrefs.SetInt("MaxNivelDesbloqueado", Mathf.Max(PlayerPrefs.GetInt("MaxNivelDesbloqueado", 1), proximoNivel));
            
            string nombreNivel = SceneManager.GetActiveScene().name;
            int estrellasViejas = PlayerPrefs.GetInt("Estrellas_" + nombreNivel, 0);
            if (estrellasGanadas > estrellasViejas) PlayerPrefs.SetInt("Estrellas_" + nombreNivel, estrellasGanadas);
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

    // ==============================================================================
    // 🔥 NUEVA INTELIGENCIA ARTIFICIAL (PATHFINDING) PARA CAMINOS SIEMPRE POSIBLES 🔥
    // ==============================================================================

    void GenerarCaminoSeguroClasico()
    {
        List<Vector2Int> camino = EncontrarCaminoBFS(posicionFuente, posicionObjetivo, new HashSet<Vector2Int>());
        if (camino != null) AsignarFormasRuta(camino);
    }

    void GenerarCaminosSegurosBifurcados()
    {
        int intentos = 0;
        bool exito = false;

        // El código intentará generar un camino sin cruces hasta 100 veces por milisegundo
        while (!exito && intentos < 100)
        {
            intentos++;
            HashSet<Vector2Int> ocupadas = new HashSet<Vector2Int>();
            
            // 1. Elegir un punto central de bifurcación al azar
            Vector2Int split = new Vector2Int(Random.Range(1, ancho - 1), Random.Range(1, alto - 1));
            if (split == posicionFuente || split == posicionObjetivo || split == posicionObjetivo2) continue;

            // 2. Camino 1: Fuente a Tubería T
            List<Vector2Int> camino1 = EncontrarCaminoBFS(posicionFuente, split, ocupadas);
            if (camino1 == null) continue;
            foreach (var p in camino1) ocupadas.Add(p); // Bloqueamos estas casillas
            ocupadas.Remove(split); // Liberamos la T para que los otros caminos puedan salir de ahí

            // 3. Camino 2: Tubería T a Objetivo 1
            List<Vector2Int> camino2 = EncontrarCaminoBFS(split, posicionObjetivo, ocupadas);
            if (camino2 == null) continue;
            foreach (var p in camino2) ocupadas.Add(p);
            ocupadas.Remove(split);

            // 4. Camino 3: Tubería T a Objetivo 2
            List<Vector2Int> camino3 = EncontrarCaminoBFS(split, posicionObjetivo2, ocupadas);
            if (camino3 == null) continue;

            // Si llegamos a esta línea, ¡tenemos 3 caminos perfectos que no se cruzan!
            AsignarFormasRuta(camino1);
            AsignarFormasRuta(camino2);
            AsignarFormasRuta(camino3);

            // Plantar la única Tubería T en el punto de corte
            grilla[split.x, split.y].tipo = Celda.TipoCelda.Bifurcacion_SinArriba;
            grilla[split.x, split.y].AplicarSnapYActualizarTipo();
            
            exito = true;
        }

        if (!exito)
        {
            Debug.LogWarning("No se pudo hacer bifurcación sin chocar. Creando camino simple por seguridad.");
            GenerarCaminoSeguroClasico(); // Salvavidas por si el tablero es demasiado pequeño
        }
    }

    // El algoritmo de búsqueda en anchura que evita que los caminos choquen
    List<Vector2Int> EncontrarCaminoBFS(Vector2Int inicio, Vector2Int fin, HashSet<Vector2Int> ocupadas)
    {
        Queue<List<Vector2Int>> cola = new Queue<List<Vector2Int>>();
        HashSet<Vector2Int> visitados = new HashSet<Vector2Int>(ocupadas);

        cola.Enqueue(new List<Vector2Int> { inicio });
        visitados.Add(inicio);

        Vector2Int[] direcciones = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        while (cola.Count > 0)
        {
            List<Vector2Int> actual = cola.Dequeue();
            Vector2Int pos = actual[actual.Count - 1];

            if (pos == fin) return actual;

            // Barajamos las direcciones para que los caminos sean serpenteantes y divertidos
            for (int i = 0; i < direcciones.Length; i++) {
                Vector2Int temp = direcciones[i];
                int randomIndex = Random.Range(i, direcciones.Length);
                direcciones[i] = direcciones[randomIndex];
                direcciones[randomIndex] = temp;
            }

            foreach (Vector2Int dir in direcciones)
            {
                Vector2Int nuevaPos = pos + dir;
                if (nuevaPos.x >= 0 && nuevaPos.x < ancho && nuevaPos.y >= 0 && nuevaPos.y < alto)
                {
                    if (!visitados.Contains(nuevaPos))
                    {
                        visitados.Add(nuevaPos);
                        List<Vector2Int> nuevoCamino = new List<Vector2Int>(actual);
                        nuevoCamino.Add(nuevaPos);
                        cola.Enqueue(nuevoCamino);
                    }
                }
            }
        }
        return null; // Retorna null si es imposible llegar sin chocar
    }

    void AsignarFormasRuta(List<Vector2Int> camino)
    {
        for (int i = 1; i < camino.Count - 1; i++)
        {
            Vector2Int prev = camino[i - 1];
            Vector2Int next = camino[i + 1];
            Vector2Int current = camino[i];
            bool esLineaRecta = (prev.x == next.x) || (prev.y == next.y);

            if (esLineaRecta) grilla[current.x, current.y].tipo = Celda.TipoCelda.RectaHorizontal;
            else grilla[current.x, current.y].tipo = Celda.TipoCelda.CurvaArribaDer;

            grilla[current.x, current.y].AplicarSnapYActualizarTipo();
        }
    }
}