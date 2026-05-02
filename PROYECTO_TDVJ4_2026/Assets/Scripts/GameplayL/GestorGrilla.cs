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
    public bool usarBifurcacion = false;
    public Vector2Int posicionObjetivo2 = new Vector2Int(5, 0);
    public List<Vector2Int> lugaresPermitidosParaLaT;

    [Header("Nivel 5+ (Doble Bifurcación)")]
    public bool usarDobleBifurcacion = false;
    public Vector2Int posicionT1 = new Vector2Int(1, 0);
    public Vector2Int posicionT2 = new Vector2Int(2, 2);

    [Header("Nivel 6 (Válvula + Tubería Giratoria)")]
    public bool usarValvulaYGiratoria = false;
    public Vector2Int posicionValvula = new Vector2Int(2, 1);
    public Vector2Int posicionTuberiaGiratoria = new Vector2Int(3, 1);
    public Celda.TipoCelda tipoInicialTuberiaGiratoria = Celda.TipoCelda.RectaHorizontal;
    public float tiempoEntreGirosTuberia = 1.5f;
    public bool valvulaIniciaAbierta = false;

    [Header("Nivel 6 - Restricciones de Fila Giratoria")]
    public bool controlarFilaDeTuberiaGiratoria = true;
    public List<int> columnasConTuberiaEnFilaGiratoria = new List<int>();

    [Header("Dificultad Procedural")]
    public int intentosMaximosGeneracion = 80;
    [Range(0f, 1f)] public float probabilidadEnganoVisual = 0.15f;
    [Range(0f, 1f)] public float probabilidadCaminoFalso = 0.25f;
    public bool agregarCaminosFalsos = true;

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
    public GameObject animacionTutorial;

    [Header("Estrellas UI (Popup)")]
    public Image[] estrellasPopup;
    public Color colorEstrellaGanada = Color.cyan;
    public Color colorEstrellaPerdida = new Color(0.2f, 0.2f, 0.2f, 1f);

    [Header("Progreso y Navegación")]
    public int numeroDeEsteNivel = 1;
    public string nombreSiguienteEscena;

    private Celda[,] grilla;
    private HashSet<Vector2Int> celdasProtegidas = new HashSet<Vector2Int>();
    private List<List<Vector2Int>> rutasSolucion = new List<List<Vector2Int>>();

    private bool UsaDosObjetivos => usarBifurcacion || usarDobleBifurcacion;

    private readonly Direccion[] direccionesFlujo =
    {
        Direccion.Arriba,
        Direccion.Abajo,
        Direccion.Izquierda,
        Direccion.Derecha
    };

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
        PrepararTableroSeguro();
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
                GameObject celdaObj = Instantiate(prefabCelda, posicion, Quaternion.identity, transform);
                celdaObj.transform.localScale = Vector3.one * tamañoCelda;

                Celda celda = celdaObj.GetComponent<Celda>();
                celda.x = x;
                celda.y = y;
                celda.tipo = Celda.TipoCelda.Vacia;
                grilla[x, y] = celda;
            }
        }
    }

    void PrepararTableroSeguro()
    {
        NormalizarPosiciones();

        bool generado = false;
        for (int intento = 0; intento < intentosMaximosGeneracion; intento++)
        {
            LimpiarTableroAleatorio();

            if (GenerarSolucionSegunMecanicas())
            {
                ColocarFuenteYObjetivos();

                if (agregarCaminosFalsos && Random.value < probabilidadCaminoFalso)
                    AgregarCaminoFalsoSeguro();

                DesordenarTuberias();

                // Aplica el corte de la fila giratoria después de desordenar.
                LimpiarFilaGiratoriaNoPermitida();
                EvitarConexionesExternasALaRuta();

                if (ValidarRutasSolucion())
                {
                    generado = true;
                    break;
                }
            }
        }

        if (!generado)
        {
            Debug.LogWarning("No se pudo generar tablero válido con caminos falsos. Reintentando sin caminos falsos ni engaños visuales.");
            generado = GenerarRespaldoSeguro();
        }

        if (!generado)
        {
            Debug.LogError("No se pudo construir un tablero resoluble. Revisa posiciones de fuente/objetivos/T/válvula/giratoria y columnas permitidas.");
        }
    }

    void NormalizarPosiciones()
    {
        posicionFuente = ClampVector2Int(posicionFuente);
        posicionObjetivo = ClampVector2Int(posicionObjetivo);
        posicionObjetivo2 = ClampVector2Int(posicionObjetivo2);
        posicionT1 = ClampVector2Int(posicionT1);
        posicionT2 = ClampVector2Int(posicionT2);
        posicionValvula = ClampVector2Int(posicionValvula);
        posicionTuberiaGiratoria = ClampVector2Int(posicionTuberiaGiratoria);

        if (usarValvulaYGiratoria)
            ValidarYCorregirConfiguracionNivel6();
    }

    void LimpiarTableroAleatorio()
    {
        celdasProtegidas.Clear();
        rutasSolucion.Clear();

        for (int x = 0; x < ancho; x++)
        {
            for (int y = 0; y < alto; y++)
            {
                Celda celda = grilla[x, y];
                if (celda == null) continue;

                float r = Random.value;
                if (r < 0.35f) celda.tipo = Celda.TipoCelda.CurvaArribaDer;
                else if (r < 0.70f) celda.tipo = Celda.TipoCelda.CurvaDerAbajo;
                else if (r < 0.85f) celda.tipo = Celda.TipoCelda.RectaHorizontal;
                else celda.tipo = Celda.TipoCelda.RectaVertical;

                celda.giraAutomaticamente = false;
                celda.valvulaAbierta = true;
                celda.valvulaVertical = false;
                celda.estaActiva = false;
                celda.tiempoEntreGiros = tiempoEntreGirosTuberia;
                celda.ActualizarVisual();
            }
        }
    }

    bool GenerarSolucionSegunMecanicas()
    {
        if (usarValvulaYGiratoria && usarBifurcacion)
            return GenerarCaminoSeguroValvulaYGiratoriaConBifurcacion();

        if (usarValvulaYGiratoria)
            return GenerarCaminoSeguroValvulaYGiratoria();

        if (usarDobleBifurcacion)
            return GenerarCaminoSeguroDobleBifurcacion();

        if (usarBifurcacion)
            return GenerarCaminosSegurosBifurcados();

        return GenerarCaminoSeguroClasico();
    }

    void ColocarFuenteYObjetivos()
    {
        Celda fuente = grilla[posicionFuente.x, posicionFuente.y];
        fuente.tipo = Celda.TipoCelda.Fuente;
        fuente.giraAutomaticamente = false;
        fuente.ActualizarVisual();

        Celda objetivo = grilla[posicionObjetivo.x, posicionObjetivo.y];
        objetivo.tipo = Celda.TipoCelda.Objetivo;
        objetivo.giraAutomaticamente = false;
        objetivo.ActualizarVisual();

        if (UsaDosObjetivos)
        {
            Celda objetivo2 = grilla[posicionObjetivo2.x, posicionObjetivo2.y];
            objetivo2.tipo = Celda.TipoCelda.Objetivo;
            objetivo2.giraAutomaticamente = false;
            objetivo2.ActualizarVisual();
        }
    }

    IEnumerator RutinaFlujo()
    {
        List<Celda> frentesDeAgua = new List<Celda>();

        if (!DentroDeGrilla(posicionFuente))
        {
            Debug.LogWarning("La posición de la fuente está fuera de la grilla.");
            yield break;
        }

        Celda fuente = grilla[posicionFuente.x, posicionFuente.y];
        if (fuente == null) yield break;

        frentesDeAgua.Add(fuente);
        int objetivosNecesarios = UsaDosObjetivos ? 2 : 1;
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

                foreach (Direccion dir in direccionesFlujo)
                {
                    if (!actual.TieneConexion(dir)) continue;

                    Celda vecino = ObtenerVecino(actual.x, actual.y, dir);
                    if (vecino != null && !vecino.estaActiva && vecino.TieneConexion(Opuesta(dir)))
                    {
                        siguientesCeldas.Add(vecino);
                        direccionesEntrada.Add(Opuesta(dir));
                    }
                }
            }

            if (siguientesCeldas.Count > 0)
            {
                List<Coroutine> animacionesEnCurso = new List<Coroutine>();
                for (int i = 0; i < siguientesCeldas.Count; i++)
                    animacionesEnCurso.Add(StartCoroutine(siguientesCeldas[i].AnimarLlenado(tiempoLlenadoActual, direccionesEntrada[i])));

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
                if (HayValvulaOGiratoriaBloqueando(frentesDeAgua)) yield return null;
                else TerminarJuego(false);
            }
        }
    }

    bool HayValvulaOGiratoriaBloqueando(List<Celda> frentesDeAgua)
    {
        foreach (Celda actual in frentesDeAgua)
        {
            if (actual.tipo == Celda.TipoCelda.Objetivo) continue;

            foreach (Direccion dir in direccionesFlujo)
            {
                if (!actual.TieneConexion(dir)) continue;

                Celda vecino = ObtenerVecino(actual.x, actual.y, dir);
                if (vecino == null || vecino.estaActiva) continue;

                if (vecino.tipo == Celda.TipoCelda.ValvulaHorizontal && !vecino.valvulaAbierta)
                    return true;

                if (vecino.giraAutomaticamente && !vecino.TieneConexion(Opuesta(dir)))
                    return true;
            }
        }

        return false;
    }

    void TerminarJuego(bool victoria)
    {
        juegoTerminado = true;
        if (botonAcelerar != null) botonAcelerar.SetActive(false);
        StartCoroutine(AnimarAparicionPopup(victoria));
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
            if (movimientosActuales <= maxMovimientos3Estrellas)
                textoMovimientos.text = $"MOVIMIENTOS: {movimientosActuales} / {maxMovimientos3Estrellas}";
            else if (movimientosActuales <= maxMovimientos2Estrellas)
                textoMovimientos.text = $"MOVIMIENTOS: {movimientosActuales} / {maxMovimientos2Estrellas}";
            else
                textoMovimientos.text = $"MOVIMIENTOS: {movimientosActuales}";
        }
    }

    public void AcelerarFlujo()
    {
        flujoAcelerado = true;
        tiempoLlenadoActual = tiempoLlenadoRapido;
        if (botonAcelerar != null) botonAcelerar.SetActive(false);
    }

    public void IniciarFlujo()
    {
        juegoTerminado = false;
        flujoAcelerado = false;
        StartCoroutine(RutinaFlujo());
    }

    public void ReiniciarNivel() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    public void VolverAlSelector() => SceneManager.LoadScene("ModoLogicoNiveles");

    public void CargarSiguienteNivel()
    {
        if (!string.IsNullOrEmpty(nombreSiguienteEscena)) SceneManager.LoadScene(nombreSiguienteEscena);
        else SceneManager.LoadScene("ModoLogicoNiveles");
    }

    bool DentroDeGrilla(Vector2Int posicion)
    {
        return posicion.x >= 0 && posicion.x < ancho && posicion.y >= 0 && posicion.y < alto;
    }

    Vector2Int ClampVector2Int(Vector2Int posicion)
    {
        posicion.x = Mathf.Clamp(posicion.x, 0, ancho - 1);
        posicion.y = Mathf.Clamp(posicion.y, 0, alto - 1);
        return posicion;
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

        return Direccion.Ninguna;
    }

    IEnumerator AnimarAparicionPopup(bool victoria)
    {
        if (panelPopup == null) yield break;
        if (botonSiguienteNivel != null) botonSiguienteNivel.SetActive(victoria);

        if (victoria)
        {
            int estrellasGanadas = (movimientosActuales <= maxMovimientos3Estrellas) ? 3 :
                                   (movimientosActuales <= maxMovimientos2Estrellas) ? 2 : 1;

            if (estrellasPopup != null)
            {
                for (int i = 0; i < estrellasPopup.Length; i++)
                {
                    if (estrellasPopup[i] != null)
                        estrellasPopup[i].color = (i < estrellasGanadas) ? colorEstrellaGanada : colorEstrellaPerdida;
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

    bool GenerarCaminoSeguroClasico()
    {
        List<Vector2Int> camino = EncontrarCaminoDesdeFuenteHasta(posicionObjetivo, new HashSet<Vector2Int>());
        if (camino == null) return false;

        AsignarFormasRuta(camino, true, false);
        rutasSolucion.Add(camino);
        return true;
    }

    bool GenerarCaminoSeguroValvulaYGiratoria()
    {
        ValidarYCorregirConfiguracionNivel6();

        HashSet<Vector2Int> ocupadas = CrearBloqueosFilaGiratoria();
        ocupadas.Add(posicionObjetivo);
        ocupadas.Add(posicionValvula);
        ocupadas.Add(posicionTuberiaGiratoria);

        ocupadas.Remove(posicionValvula);
        List<Vector2Int> tramoFuenteAValvula = EncontrarCaminoDesdeFuenteHasta(posicionValvula, ocupadas);
        if (tramoFuenteAValvula == null) return false;
        foreach (Vector2Int p in tramoFuenteAValvula) ocupadas.Add(p);

        ocupadas.Remove(posicionValvula);
        ocupadas.Remove(posicionTuberiaGiratoria);
        List<Vector2Int> tramoValvulaAGiratoria = EncontrarCaminoBFS(posicionValvula, posicionTuberiaGiratoria, ocupadas);
        if (tramoValvulaAGiratoria == null) return false;
        foreach (Vector2Int p in tramoValvulaAGiratoria) ocupadas.Add(p);

        ocupadas.Remove(posicionTuberiaGiratoria);
        ocupadas.Remove(posicionObjetivo);
        List<Vector2Int> tramoGiratoriaAFuego = EncontrarCaminoBFS(posicionTuberiaGiratoria, posicionObjetivo, ocupadas);
        if (tramoGiratoriaAFuego == null) return false;

        List<Vector2Int> rutaCompleta = UnirRutas(tramoFuenteAValvula, tramoValvulaAGiratoria, tramoGiratoriaAFuego);
        if (!ConfigurarEspecialesSegunRuta(rutaCompleta)) return false;

        AsignarFormasRuta(tramoFuenteAValvula, true, false);
        AsignarFormasRuta(tramoValvulaAGiratoria, true, false);
        AsignarFormasRuta(tramoGiratoriaAFuego, true, false);
        rutasSolucion.Add(rutaCompleta);
        return true;
    }

    bool GenerarCaminoSeguroValvulaYGiratoriaConBifurcacion()
    {
        ValidarYCorregirConfiguracionNivel6();

        Vector2Int split = ElegirSplitParaBifurcacion();
        if (split == posicionFuente || split == posicionObjetivo || split == posicionObjetivo2 || split == posicionValvula || split == posicionTuberiaGiratoria)
            return false;

        HashSet<Vector2Int> ocupadas = CrearBloqueosFilaGiratoria();
        ocupadas.Add(posicionObjetivo);
        ocupadas.Add(posicionObjetivo2);
        ocupadas.Add(split);
        ocupadas.Add(posicionValvula);
        ocupadas.Add(posicionTuberiaGiratoria);

        ocupadas.Remove(posicionValvula);
        List<Vector2Int> tramoFuenteAValvula = EncontrarCaminoDesdeFuenteHasta(posicionValvula, ocupadas);
        if (tramoFuenteAValvula == null) return false;
        foreach (Vector2Int p in tramoFuenteAValvula) ocupadas.Add(p);

        ocupadas.Remove(posicionValvula);
        ocupadas.Remove(posicionTuberiaGiratoria);
        List<Vector2Int> tramoValvulaAGiratoria = EncontrarCaminoBFS(posicionValvula, posicionTuberiaGiratoria, ocupadas);
        if (tramoValvulaAGiratoria == null) return false;
        foreach (Vector2Int p in tramoValvulaAGiratoria) ocupadas.Add(p);

        ocupadas.Remove(posicionTuberiaGiratoria);
        ocupadas.Remove(split);
        List<Vector2Int> tramoGiratoriaASplit = EncontrarCaminoBFS(posicionTuberiaGiratoria, split, ocupadas);
        if (tramoGiratoriaASplit == null) return false;
        foreach (Vector2Int p in tramoGiratoriaASplit) ocupadas.Add(p);

        ocupadas.Remove(split);
        ocupadas.Remove(posicionObjetivo);
        ocupadas.Add(posicionObjetivo2);
        List<Vector2Int> rama1 = EncontrarCaminoBFS(split, posicionObjetivo, ocupadas);
        if (rama1 == null) return false;
        foreach (Vector2Int p in rama1) ocupadas.Add(p);

        ocupadas.Remove(split);
        ocupadas.Remove(posicionObjetivo2);
        List<Vector2Int> rama2 = EncontrarCaminoBFS(split, posicionObjetivo2, ocupadas);
        if (rama2 == null) return false;

        List<Vector2Int> rutaHastaSplit = UnirRutas(tramoFuenteAValvula, tramoValvulaAGiratoria, tramoGiratoriaASplit);
        if (!ConfigurarEspecialesSegunRuta(rutaHastaSplit)) return false;

        AsignarFormasRuta(tramoFuenteAValvula, true, false);
        AsignarFormasRuta(tramoValvulaAGiratoria, true, false);
        AsignarFormasRuta(tramoGiratoriaASplit, true, false);
        AsignarFormasRuta(rama1, true, false);
        AsignarFormasRuta(rama2, true, false);

        SetearBifurcacionSegunConexiones(split, new List<Vector2Int>
        {
            ObtenerVecinoEnCamino(tramoGiratoriaASplit, split),
            ObtenerVecinoEnCamino(rama1, split),
            ObtenerVecinoEnCamino(rama2, split)
        });

        rutasSolucion.Add(UnirRutas(rutaHastaSplit, rama1));
        rutasSolucion.Add(UnirRutas(rutaHastaSplit, rama2));
        return true;
    }

    bool ConfigurarEspecialesSegunRuta(List<Vector2Int> ruta)
    {
        if (!ConfigurarValvulaSegunRuta(ruta)) return false;
        if (!ConfigurarGiratoriaSegunRuta(ruta)) return false;
        return true;
    }

    bool ConfigurarValvulaSegunRuta(List<Vector2Int> ruta)
    {
        int index = ruta.IndexOf(posicionValvula);
        if (index <= 0 || index >= ruta.Count - 1) return false;

        Direccion entrada = DireccionEntre(posicionValvula, ruta[index - 1]);
        Direccion salida = DireccionEntre(posicionValvula, ruta[index + 1]);

        bool horizontal = (entrada == Direccion.Izquierda && salida == Direccion.Derecha) ||
                          (entrada == Direccion.Derecha && salida == Direccion.Izquierda);
        bool vertical = (entrada == Direccion.Arriba && salida == Direccion.Abajo) ||
                        (entrada == Direccion.Abajo && salida == Direccion.Arriba);

        if (!horizontal && !vertical) return false;

        Celda valvula = grilla[posicionValvula.x, posicionValvula.y];
        valvula.tipo = Celda.TipoCelda.ValvulaHorizontal;
        valvula.valvulaVertical = vertical;
        valvula.valvulaAbierta = valvulaIniciaAbierta;
        valvula.giraAutomaticamente = false;
        valvula.ActualizarVisual();
        celdasProtegidas.Add(posicionValvula);
        return true;
    }

    bool ConfigurarGiratoriaSegunRuta(List<Vector2Int> ruta)
    {
        int index = ruta.IndexOf(posicionTuberiaGiratoria);
        if (index <= 0 || index >= ruta.Count - 1) return false;

        Celda.TipoCelda tipoNecesario = TipoParaConexion(ruta[index - 1], posicionTuberiaGiratoria, ruta[index + 1]);
        if (tipoNecesario == Celda.TipoCelda.Vacia) return false;

        Celda giratoria = grilla[posicionTuberiaGiratoria.x, posicionTuberiaGiratoria.y];
        giratoria.tipo = tipoNecesario;
        giratoria.giraAutomaticamente = true;
        giratoria.tiempoEntreGiros = tiempoEntreGirosTuberia;
        giratoria.ActualizarVisual();
        celdasProtegidas.Add(posicionTuberiaGiratoria);
        return true;
    }

    Vector2Int ElegirSplitParaBifurcacion()
    {
        if (lugaresPermitidosParaLaT != null && lugaresPermitidosParaLaT.Count > 0)
            return ClampVector2Int(lugaresPermitidosParaLaT[Random.Range(0, lugaresPermitidosParaLaT.Count)]);

        return ClampVector2Int(posicionT2);
    }

    bool GenerarCaminoSeguroDobleBifurcacion()
    {
        HashSet<Vector2Int> ocupadas = new HashSet<Vector2Int> { posicionObjetivo, posicionObjetivo2 };

        List<Vector2Int> tramoFuenteAT1 = EncontrarCaminoDesdeFuenteHasta(posicionT1, ocupadas);
        if (tramoFuenteAT1 == null) return false;
        foreach (Vector2Int p in tramoFuenteAT1) ocupadas.Add(p);
        ocupadas.Remove(posicionT1);

        List<Vector2Int> tramoT1AT2 = EncontrarCaminoBFS(posicionT1, posicionT2, ocupadas);
        if (tramoT1AT2 == null) return false;
        foreach (Vector2Int p in tramoT1AT2) ocupadas.Add(p);
        ocupadas.Remove(posicionT2);

        ocupadas.Remove(posicionObjetivo);
        ocupadas.Add(posicionObjetivo2);
        List<Vector2Int> rama1 = EncontrarCaminoBFS(posicionT2, posicionObjetivo, ocupadas);
        if (rama1 == null) return false;
        foreach (Vector2Int p in rama1) ocupadas.Add(p);

        ocupadas.Remove(posicionT2);
        ocupadas.Remove(posicionObjetivo2);
        List<Vector2Int> rama2 = EncontrarCaminoBFS(posicionT2, posicionObjetivo2, ocupadas);
        if (rama2 == null) return false;

        AsignarFormasRuta(tramoFuenteAT1, true, false);
        AsignarFormasRuta(tramoT1AT2, true, false);
        AsignarFormasRuta(rama1, true, false);
        AsignarFormasRuta(rama2, true, false);

        SetearBifurcacionSegunConexiones(posicionT1, new List<Vector2Int>
        {
            ObtenerVecinoEnCamino(tramoFuenteAT1, posicionT1),
            ObtenerVecinoEnCamino(tramoT1AT2, posicionT1)
        });

        SetearBifurcacionSegunConexiones(posicionT2, new List<Vector2Int>
        {
            ObtenerVecinoEnCamino(tramoT1AT2, posicionT2),
            ObtenerVecinoEnCamino(rama1, posicionT2),
            ObtenerVecinoEnCamino(rama2, posicionT2)
        });

        rutasSolucion.Add(UnirRutas(tramoFuenteAT1, tramoT1AT2, rama1));
        rutasSolucion.Add(UnirRutas(tramoFuenteAT1, tramoT1AT2, rama2));
        return true;
    }

    bool GenerarCaminosSegurosBifurcados()
    {
        List<Vector2Int> puntos = new List<Vector2Int>();
        if (lugaresPermitidosParaLaT != null && lugaresPermitidosParaLaT.Count > 0)
            puntos.AddRange(lugaresPermitidosParaLaT);
        else
        {
            for (int x = 1; x < ancho - 1; x++)
            {
                for (int y = 1; y < alto - 1; y++)
                    puntos.Add(new Vector2Int(x, y));
            }
        }

        MezclarLista(puntos);

        foreach (Vector2Int puntoOriginal in puntos)
        {
            Vector2Int split = ClampVector2Int(puntoOriginal);
            if (split == posicionFuente || split == posicionObjetivo || split == posicionObjetivo2) continue;

            HashSet<Vector2Int> ocupadas = new HashSet<Vector2Int> { posicionObjetivo, posicionObjetivo2 };

            List<Vector2Int> tronco = EncontrarCaminoDesdeFuenteHasta(split, ocupadas);
            if (tronco == null) continue;

            ocupadas.Remove(posicionObjetivo);
            ocupadas.Remove(posicionObjetivo2);
            foreach (Vector2Int p in tronco) ocupadas.Add(p);
            ocupadas.Remove(split);

            ocupadas.Add(posicionObjetivo2);
            List<Vector2Int> rama1 = EncontrarCaminoBFS(split, posicionObjetivo, ocupadas);
            if (rama1 == null) continue;

            ocupadas.Remove(posicionObjetivo2);
            foreach (Vector2Int p in rama1) ocupadas.Add(p);
            ocupadas.Remove(split);

            List<Vector2Int> rama2 = EncontrarCaminoBFS(split, posicionObjetivo2, ocupadas);
            if (rama2 == null) continue;

            AsignarFormasRuta(tronco, true, false);
            AsignarFormasRuta(rama1, true, false);
            AsignarFormasRuta(rama2, true, false);

            SetearBifurcacionSegunConexiones(split, new List<Vector2Int>
            {
                ObtenerVecinoEnCamino(tronco, split),
                ObtenerVecinoEnCamino(rama1, split),
                ObtenerVecinoEnCamino(rama2, split)
            });

            rutasSolucion.Add(UnirRutas(tronco, rama1));
            rutasSolucion.Add(UnirRutas(tronco, rama2));
            return true;
        }

        return false;
    }

    List<Vector2Int> EncontrarCaminoDesdeFuenteHasta(Vector2Int destino, HashSet<Vector2Int> ocupadas)
    {
        Vector2Int primerPaso = posicionFuente + Vector2Int.right;

        if (!DentroDeGrilla(primerPaso)) return null;
        if (ocupadas.Contains(primerPaso)) return null;

        HashSet<Vector2Int> bloqueos = new HashSet<Vector2Int>(ocupadas);
        bloqueos.Add(posicionFuente);

        List<Vector2Int> caminoDesdePrimerPaso = EncontrarCaminoBFS(primerPaso, destino, bloqueos);
        if (caminoDesdePrimerPaso == null) return null;

        List<Vector2Int> caminoCompleto = new List<Vector2Int> { posicionFuente };
        caminoCompleto.AddRange(caminoDesdePrimerPaso);
        return caminoCompleto;
    }

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

            MezclarArray(direcciones);

            foreach (Vector2Int dir in direcciones)
            {
                Vector2Int nuevaPos = pos + dir;
                if (!DentroDeGrilla(nuevaPos)) continue;
                if (visitados.Contains(nuevaPos)) continue;

                visitados.Add(nuevaPos);
                List<Vector2Int> nuevoCamino = new List<Vector2Int>(actual) { nuevaPos };
                cola.Enqueue(nuevoCamino);
            }
        }

        return null;
    }

    bool AsignarFormasRuta(List<Vector2Int> camino, bool proteger, bool evitarProtegidas)
    {
        if (camino == null || camino.Count < 2) return false;

        if (evitarProtegidas)
        {
            foreach (Vector2Int p in camino)
            {
                if (celdasProtegidas.Contains(p)) return false;
            }
        }

        if (proteger)
        {
            foreach (Vector2Int p in camino)
                celdasProtegidas.Add(p);
        }

        for (int i = 1; i < camino.Count - 1; i++)
        {
            Vector2Int prev = camino[i - 1];
            Vector2Int current = camino[i];
            Vector2Int next = camino[i + 1];

            if (current == posicionFuente || current == posicionObjetivo || current == posicionObjetivo2 || current == posicionValvula || current == posicionTuberiaGiratoria)
                continue;

            Celda.TipoCelda tipoNecesario = TipoParaConexion(prev, current, next);
            if (tipoNecesario == Celda.TipoCelda.Vacia) return false;

            Celda celda = grilla[current.x, current.y];
            celda.tipo = tipoNecesario;
            celda.ActualizarVisual();
        }

        return true;
    }

    Celda.TipoCelda TipoParaConexion(Vector2Int prev, Vector2Int current, Vector2Int next)
    {
        Vector2Int entrada = prev - current;
        Vector2Int salida = next - current;

        if ((entrada == Vector2Int.left && salida == Vector2Int.right) ||
            (entrada == Vector2Int.right && salida == Vector2Int.left))
            return Celda.TipoCelda.RectaHorizontal;

        if ((entrada == Vector2Int.up && salida == Vector2Int.down) ||
            (entrada == Vector2Int.down && salida == Vector2Int.up))
            return Celda.TipoCelda.RectaVertical;

        if ((entrada == Vector2Int.up && salida == Vector2Int.right) ||
            (entrada == Vector2Int.right && salida == Vector2Int.up))
            return Celda.TipoCelda.CurvaArribaDer;

        if ((entrada == Vector2Int.right && salida == Vector2Int.down) ||
            (entrada == Vector2Int.down && salida == Vector2Int.right))
            return Celda.TipoCelda.CurvaDerAbajo;

        if ((entrada == Vector2Int.down && salida == Vector2Int.left) ||
            (entrada == Vector2Int.left && salida == Vector2Int.down))
            return Celda.TipoCelda.CurvaAbajoIzq;

        if ((entrada == Vector2Int.left && salida == Vector2Int.up) ||
            (entrada == Vector2Int.up && salida == Vector2Int.left))
            return Celda.TipoCelda.CurvaIzqArriba;

        return Celda.TipoCelda.Vacia;
    }

    void DesordenarTuberias()
    {
        for (int x = 0; x < ancho; x++)
        {
            for (int y = 0; y < alto; y++)
            {
                Celda celda = grilla[x, y];
                if (celda == null) continue;
                if (celda.tipo == Celda.TipoCelda.Fuente) continue;
                if (celda.tipo == Celda.TipoCelda.Objetivo) continue;
                if (celda.tipo == Celda.TipoCelda.Vacia) continue;
                if (celda.tipo == Celda.TipoCelda.ValvulaHorizontal) continue;
                if (celda.giraAutomaticamente) continue;

                Vector2Int pos = new Vector2Int(x, y);
                bool protegida = celdasProtegidas.Contains(pos);

                int giros = Random.Range(1, 4);
                celda.visualTuberia.Rotate(0, 0, giros * 90f);
                celda.AplicarSnapYActualizarTipo();

                if (!protegida && Random.value < probabilidadEnganoVisual)
                {
                    celda.tipo = (Random.value < 0.5f) ? Celda.TipoCelda.RectaVertical : Celda.TipoCelda.CurvaDerAbajo;
                    celda.ActualizarVisual();
                }
            }
        }
    }

    void AgregarCaminoFalsoSeguro()
    {
        for (int intento = 0; intento < 10; intento++)
        {
            Vector2Int inicio = new Vector2Int(Random.Range(0, ancho), Random.Range(0, alto));
            Vector2Int fin = new Vector2Int(Random.Range(0, ancho), Random.Range(0, alto));
            if (inicio == fin) continue;

            List<Vector2Int> falso = EncontrarCaminoBFS(inicio, fin, new HashSet<Vector2Int>());
            if (falso == null || falso.Count < 4) continue;
            if (AsignarFormasRuta(falso, false, true)) break;
        }
    }

    bool ValidarRutasSolucion()
    {
        if (rutasSolucion.Count == 0) return false;

        Dictionary<Vector2Int, HashSet<Direccion>> conexionesRequeridas = CalcularConexionesRequeridasDeLaSolucion();
        if (conexionesRequeridas.Count == 0) return false;

        foreach (KeyValuePair<Vector2Int, HashSet<Direccion>> item in conexionesRequeridas)
        {
            Vector2Int pos = item.Key;
            if (!DentroDeGrilla(pos)) return false;
            if (!PiezaPuedeResolverConexiones(grilla[pos.x, pos.y], item.Value)) return false;
        }

        return true;
    }

    Dictionary<Vector2Int, HashSet<Direccion>> CalcularConexionesRequeridasDeLaSolucion()
    {
        Dictionary<Vector2Int, HashSet<Direccion>> conexiones = new Dictionary<Vector2Int, HashSet<Direccion>>();

        foreach (List<Vector2Int> ruta in rutasSolucion)
        {
            if (ruta == null || ruta.Count < 2) continue;

            for (int i = 0; i < ruta.Count - 1; i++)
            {
                Vector2Int actual = ruta[i];
                Vector2Int siguiente = ruta[i + 1];
                Direccion dir = DireccionEntre(actual, siguiente);
                if (dir == Direccion.Ninguna) continue;

                AgregarConexionRequerida(conexiones, actual, dir);
                AgregarConexionRequerida(conexiones, siguiente, Opuesta(dir));
            }
        }

        return conexiones;
    }

    void AgregarConexionRequerida(Dictionary<Vector2Int, HashSet<Direccion>> conexiones, Vector2Int pos, Direccion dir)
    {
        if (dir == Direccion.Ninguna) return;
        if (!conexiones.ContainsKey(pos)) conexiones[pos] = new HashSet<Direccion>();
        conexiones[pos].Add(dir);
    }

    bool PiezaPuedeResolverConexiones(Celda celda, HashSet<Direccion> dirs)
    {
        if (celda == null || dirs == null || dirs.Count == 0) return false;

        if (celda.tipo == Celda.TipoCelda.Fuente)
            return dirs.Count == 1 && dirs.Contains(Direccion.Derecha);

        if (celda.tipo == Celda.TipoCelda.Objetivo)
            return dirs.Count >= 1;

        if (celda.tipo == Celda.TipoCelda.Vacia)
            return false;

        if (celda.tipo == Celda.TipoCelda.ValvulaHorizontal)
        {
            if (dirs.Count != 2) return false;
            if (celda.valvulaVertical)
                return dirs.Contains(Direccion.Arriba) && dirs.Contains(Direccion.Abajo);
            return dirs.Contains(Direccion.Izquierda) && dirs.Contains(Direccion.Derecha);
        }

        bool esRecta = celda.tipo == Celda.TipoCelda.RectaHorizontal || celda.tipo == Celda.TipoCelda.RectaVertical;
        bool esCurva = celda.tipo == Celda.TipoCelda.CurvaArribaDer ||
                       celda.tipo == Celda.TipoCelda.CurvaDerAbajo ||
                       celda.tipo == Celda.TipoCelda.CurvaAbajoIzq ||
                       celda.tipo == Celda.TipoCelda.CurvaIzqArriba;
        bool esBifurcacion = celda.tipo == Celda.TipoCelda.Bifurcacion_SinArriba ||
                             celda.tipo == Celda.TipoCelda.Bifurcacion_SinDer ||
                             celda.tipo == Celda.TipoCelda.Bifurcacion_SinAbajo ||
                             celda.tipo == Celda.TipoCelda.Bifurcacion_SinIzq;

        if (celda.giraAutomaticamente && dirs.Count == 2)
            return esRecta || esCurva;

        if (dirs.Count == 2)
        {
            bool parOpuesto = (dirs.Contains(Direccion.Izquierda) && dirs.Contains(Direccion.Derecha)) ||
                              (dirs.Contains(Direccion.Arriba) && dirs.Contains(Direccion.Abajo));
            bool parEsquina = !parOpuesto;

            if (esRecta) return parOpuesto;
            if (esCurva) return parEsquina;
            if (esBifurcacion) return true;
        }

        if (dirs.Count == 3)
            return esBifurcacion;

        return false;
    }

    bool GenerarRespaldoSeguro()
    {
        float probEnganoOriginal = probabilidadEnganoVisual;
        bool agregarFalsosOriginal = agregarCaminosFalsos;

        probabilidadEnganoVisual = 0f;
        agregarCaminosFalsos = false;

        bool generado = false;
        int intentosRespaldo = Mathf.Max(intentosMaximosGeneracion * 3, 120);

        for (int intento = 0; intento < intentosRespaldo; intento++)
        {
            LimpiarTableroAleatorio();

            if (!GenerarSolucionSegunMecanicas()) continue;

            ColocarFuenteYObjetivos();
            DesordenarTuberias();
            LimpiarFilaGiratoriaNoPermitida();
            EvitarConexionesExternasALaRuta();

            if (ValidarRutasSolucion())
            {
                generado = true;
                break;
            }
        }

        probabilidadEnganoVisual = probEnganoOriginal;
        agregarCaminosFalsos = agregarFalsosOriginal;
        return generado;
    }

    void EvitarConexionesExternasALaRuta()
    {
        if (celdasProtegidas == null || celdasProtegidas.Count == 0) return;

        HashSet<Vector2Int> posicionesRevisadas = new HashSet<Vector2Int>();

        foreach (Vector2Int posRuta in celdasProtegidas)
        {
            foreach (Direccion dir in direccionesFlujo)
            {
                Celda vecino = ObtenerVecino(posRuta.x, posRuta.y, dir);
                if (vecino == null) continue;

                Vector2Int posVecino = new Vector2Int(vecino.x, vecino.y);
                if (celdasProtegidas.Contains(posVecino)) continue;
                if (posicionesRevisadas.Contains(posVecino)) continue;

                posicionesRevisadas.Add(posVecino);
                QuitarConexionHacia(vecino, Opuesta(dir));
            }
        }
    }

    void QuitarConexionHacia(Celda celda, Direccion direccionProhibida)
    {
        if (celda == null) return;
        if (celda.tipo == Celda.TipoCelda.Fuente || celda.tipo == Celda.TipoCelda.Objetivo || celda.tipo == Celda.TipoCelda.ValvulaHorizontal) return;
        if (celda.giraAutomaticamente) return;

        for (int i = 0; i < 4; i++)
        {
            if (!celda.TieneConexion(direccionProhibida)) return;
            celda.visualTuberia.Rotate(0, 0, 90f);
            celda.AplicarSnapYActualizarTipo();
        }

        if (celda.TieneConexion(direccionProhibida))
        {
            celda.tipo = Celda.TipoCelda.Vacia;
            celda.ActualizarVisual();
        }
    }

    void ValidarYCorregirConfiguracionNivel6()
    {
        if (!usarValvulaYGiratoria) return;

        posicionValvula = ClampVector2Int(posicionValvula);
        posicionTuberiaGiratoria = ClampVector2Int(posicionTuberiaGiratoria);

        if (!columnasConTuberiaEnFilaGiratoria.Contains(posicionTuberiaGiratoria.x))
            columnasConTuberiaEnFilaGiratoria.Add(posicionTuberiaGiratoria.x);

        for (int i = columnasConTuberiaEnFilaGiratoria.Count - 1; i >= 0; i--)
        {
            int columna = columnasConTuberiaEnFilaGiratoria[i];
            if (columna < 0 || columna >= ancho)
                columnasConTuberiaEnFilaGiratoria.RemoveAt(i);
        }
    }

    bool CeldaPermitidaEnFilaGiratoria(Vector2Int posicion)
    {
        if (!usarValvulaYGiratoria || !controlarFilaDeTuberiaGiratoria) return true;
        if (posicion.y != posicionTuberiaGiratoria.y) return true;
        if (posicion == posicionTuberiaGiratoria) return true;
        return columnasConTuberiaEnFilaGiratoria.Contains(posicion.x);
    }

    HashSet<Vector2Int> CrearBloqueosFilaGiratoria()
    {
        HashSet<Vector2Int> bloqueos = new HashSet<Vector2Int>();
        if (!usarValvulaYGiratoria || !controlarFilaDeTuberiaGiratoria) return bloqueos;

        int filaGiratoria = posicionTuberiaGiratoria.y;
        for (int x = 0; x < ancho; x++)
        {
            Vector2Int posicion = new Vector2Int(x, filaGiratoria);
            if (!CeldaPermitidaEnFilaGiratoria(posicion)) bloqueos.Add(posicion);
        }

        return bloqueos;
    }

    void LimpiarFilaGiratoriaNoPermitida()
    {
        if (!usarValvulaYGiratoria || !controlarFilaDeTuberiaGiratoria) return;

        int filaGiratoria = posicionTuberiaGiratoria.y;
        for (int x = 0; x < ancho; x++)
        {
            Vector2Int posicion = new Vector2Int(x, filaGiratoria);
            if (CeldaPermitidaEnFilaGiratoria(posicion)) continue;

            Celda celda = grilla[x, filaGiratoria];
            if (celda == null) continue;

            celda.tipo = Celda.TipoCelda.Vacia;
            celda.giraAutomaticamente = false;
            celda.valvulaAbierta = true;
            celda.ActualizarVisual();
        }
    }

    Vector2Int ObtenerVecinoEnCamino(List<Vector2Int> camino, Vector2Int centro)
    {
        for (int i = 0; i < camino.Count; i++)
        {
            if (camino[i] != centro) continue;
            if (i > 0) return camino[i - 1];
            if (i < camino.Count - 1) return camino[i + 1];
        }

        return centro;
    }

    void SetearBifurcacionSegunConexiones(Vector2Int posicionT, List<Vector2Int> vecinosNecesarios)
    {
        HashSet<Direccion> direccionesNecesarias = new HashSet<Direccion>();
        List<Vector2Int> vecinosValidos = new List<Vector2Int>();

        foreach (Vector2Int vecino in vecinosNecesarios)
        {
            if (vecino == posicionT) continue;

            Direccion dir = DireccionEntre(posicionT, vecino);
            if (dir == Direccion.Ninguna) continue;

            direccionesNecesarias.Add(dir);
            vecinosValidos.Add(vecino);
        }

        Celda celdaT = grilla[posicionT.x, posicionT.y];

        // Si solo necesita dos conexiones, NO fuerzo una T: eso agregaba una salida extra
        // que podía provocar caminos muertos o piezas "de más".
        if (direccionesNecesarias.Count == 2 && vecinosValidos.Count >= 2)
        {
            Celda.TipoCelda tipoNormal = TipoParaConexion(vecinosValidos[0], posicionT, vecinosValidos[1]);
            if (tipoNormal != Celda.TipoCelda.Vacia)
                celdaT.tipo = tipoNormal;
        }
        else
        {
            if (!direccionesNecesarias.Contains(Direccion.Arriba)) celdaT.tipo = Celda.TipoCelda.Bifurcacion_SinArriba;
            else if (!direccionesNecesarias.Contains(Direccion.Derecha)) celdaT.tipo = Celda.TipoCelda.Bifurcacion_SinDer;
            else if (!direccionesNecesarias.Contains(Direccion.Abajo)) celdaT.tipo = Celda.TipoCelda.Bifurcacion_SinAbajo;
            else celdaT.tipo = Celda.TipoCelda.Bifurcacion_SinIzq;
        }

        celdaT.giraAutomaticamente = false;
        celdaT.ActualizarVisual();
        celdasProtegidas.Add(posicionT);
    }

    Direccion DireccionEntre(Vector2Int desde, Vector2Int hacia)
    {
        Vector2Int delta = hacia - desde;
        if (delta == Vector2Int.up) return Direccion.Arriba;
        if (delta == Vector2Int.down) return Direccion.Abajo;
        if (delta == Vector2Int.left) return Direccion.Izquierda;
        if (delta == Vector2Int.right) return Direccion.Derecha;
        return Direccion.Ninguna;
    }

    List<Vector2Int> UnirRutas(params List<Vector2Int>[] rutas)
    {
        List<Vector2Int> resultado = new List<Vector2Int>();
        foreach (List<Vector2Int> ruta in rutas)
        {
            if (ruta == null) continue;
            for (int i = 0; i < ruta.Count; i++)
            {
                if (resultado.Count > 0 && resultado[resultado.Count - 1] == ruta[i]) continue;
                resultado.Add(ruta[i]);
            }
        }
        return resultado;
    }

    void MezclarLista(List<Vector2Int> lista)
    {
        for (int i = 0; i < lista.Count; i++)
        {
            Vector2Int temp = lista[i];
            int randomIndex = Random.Range(i, lista.Count);
            lista[i] = lista[randomIndex];
            lista[randomIndex] = temp;
        }
    }

    void MezclarArray(Vector2Int[] array)
    {
        for (int i = 0; i < array.Length; i++)
        {
            Vector2Int temp = array[i];
            int randomIndex = Random.Range(i, array.Length);
            array[i] = array[randomIndex];
            array[randomIndex] = temp;
        }
    }

}
