using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GuiaTutorialNivel : MonoBehaviour
{
    [Header("Panel principal")]
    [SerializeField] private GameObject panelGuia;

    [Header("Paginas de la guia")]
    [SerializeField] private GameObject[] paginas;

    [Header("Botones opcionales")]
    [SerializeField] private Button botonAnterior;
    [SerializeField] private Button botonSiguiente;
    [SerializeField] private Button botonCerrar;

    [Header("Bloqueo general del nivel")]
    [Tooltip("Si esta activo, pausa el Time.timeScale mientras la guia esta abierta.")]
    [SerializeField] private bool pausarJuegoMientrasEsteAbierta = true;

    [Tooltip("Si esta activo, se bloquean los objetos cargados en las listas de abajo.")]
    [SerializeField] private bool bloquearObjetosMientrasEsteAbierta = true;

    [Tooltip("Arrastra aca objetos del modo logico, modo fisico, botones, joystick, controlador de agua, etc. Se desactivan mientras la guia esta abierta.")]
    [SerializeField] private GameObject[] objetosParaBloquear;

    [Tooltip("Arrastra aca scripts/componentes si queres desactivar solo el script y no todo el GameObject.")]
    [SerializeField] private Behaviour[] componentesParaBloquear;

    [Header("Opciones")]
    [Tooltip("Si esta activo, la guia se abre automaticamente al empezar la escena.")]
    [SerializeField] private bool mostrarAlInicio = true;

    [Tooltip("Si esta activo, al cerrar la guia se oculta el panel.")]
    [SerializeField] private bool ocultarAlCerrar = true;

    [Tooltip("Si esta activo, en la ultima pagina el boton Siguiente cierra la guia.")]
    [SerializeField] private bool siguienteCierraEnUltimaPagina = true;

    [Tooltip("Si esta activo, el boton Anterior se oculta en la primera pagina.")]
    [SerializeField] private bool ocultarBotonAnteriorEnPrimeraPagina = true;

    [Tooltip("Si esta activo, el boton Siguiente se oculta en la ultima pagina cuando hay boton Cerrar.")]
    [SerializeField] private bool ocultarBotonSiguienteEnUltimaPagina = false;

    private int paginaActual = 0;
    private bool guiaAbierta = false;
    private float timeScaleAnterior = 1f;

    private readonly Dictionary<GameObject, bool> estadoOriginalObjetos = new Dictionary<GameObject, bool>();
    private readonly Dictionary<Behaviour, bool> estadoOriginalComponentes = new Dictionary<Behaviour, bool>();

    private void Awake()
    {
        if (panelGuia == null)
            panelGuia = gameObject;

        ConfigurarBotones();
    }

    private void Start()
    {
        if (mostrarAlInicio)
        {
            AbrirGuia();
        }
        else
        {
            CerrarGuiaInmediato();
        }
    }

    private void OnDestroy()
    {
        if (botonAnterior != null)
            botonAnterior.onClick.RemoveListener(PaginaAnterior);

        if (botonSiguiente != null)
            botonSiguiente.onClick.RemoveListener(SiguientePagina);

        if (botonCerrar != null)
            botonCerrar.onClick.RemoveListener(CerrarGuia);
    }

    private void ConfigurarBotones()
    {
        if (botonAnterior != null)
        {
            botonAnterior.onClick.RemoveListener(PaginaAnterior);
            botonAnterior.onClick.AddListener(PaginaAnterior);
        }

        if (botonSiguiente != null)
        {
            botonSiguiente.onClick.RemoveListener(SiguientePagina);
            botonSiguiente.onClick.AddListener(SiguientePagina);
        }

        if (botonCerrar != null)
        {
            botonCerrar.onClick.RemoveListener(CerrarGuia);
            botonCerrar.onClick.AddListener(CerrarGuia);
        }
    }

    public void AbrirGuia()
    {
        guiaAbierta = true;
        paginaActual = 0;

        if (panelGuia != null)
            panelGuia.SetActive(true);

        MostrarPagina(paginaActual);
        BloquearNivel();
    }

    public void SiguientePagina()
    {
        if (!guiaAbierta)
            return;

        if (paginas == null || paginas.Length == 0)
        {
            CerrarGuia();
            return;
        }

        if (paginaActual < paginas.Length - 1)
        {
            paginaActual++;
            MostrarPagina(paginaActual);
        }
        else
        {
            if (siguienteCierraEnUltimaPagina)
                CerrarGuia();
        }
    }

    public void PaginaAnterior()
    {
        if (!guiaAbierta)
            return;

        if (paginaActual > 0)
        {
            paginaActual--;
            MostrarPagina(paginaActual);
        }
    }

    public void CerrarGuia()
    {
        if (!guiaAbierta)
            return;

        guiaAbierta = false;
        DesbloquearNivel();

        if (panelGuia != null && ocultarAlCerrar)
            panelGuia.SetActive(false);
    }

    private void CerrarGuiaInmediato()
    {
        guiaAbierta = false;

        if (panelGuia != null)
            panelGuia.SetActive(false);
    }

    private void MostrarPagina(int indice)
    {
        if (paginas != null)
        {
            for (int i = 0; i < paginas.Length; i++)
            {
                if (paginas[i] != null)
                    paginas[i].SetActive(i == indice);
            }
        }

        ActualizarBotones();
    }

    private void ActualizarBotones()
    {
        int cantidadPaginas = paginas != null ? paginas.Length : 0;
        bool hayMasDeUnaPagina = cantidadPaginas > 1;
        bool esPrimeraPagina = paginaActual <= 0;
        bool esUltimaPagina = cantidadPaginas == 0 || paginaActual >= cantidadPaginas - 1;

        if (botonAnterior != null)
        {
            bool mostrarAnterior = hayMasDeUnaPagina && (!ocultarBotonAnteriorEnPrimeraPagina || !esPrimeraPagina);
            botonAnterior.gameObject.SetActive(mostrarAnterior);
        }

        if (botonSiguiente != null)
        {
            bool mostrarSiguiente = hayMasDeUnaPagina;

            if (esUltimaPagina && ocultarBotonSiguienteEnUltimaPagina && botonCerrar != null)
                mostrarSiguiente = false;

            botonSiguiente.gameObject.SetActive(mostrarSiguiente);
        }

        if (botonCerrar != null)
            botonCerrar.gameObject.SetActive(true);
    }

    private void BloquearNivel()
    {
        if (pausarJuegoMientrasEsteAbierta)
        {
            timeScaleAnterior = Time.timeScale;
            Time.timeScale = 0f;
        }

        if (!bloquearObjetosMientrasEsteAbierta)
            return;

        estadoOriginalObjetos.Clear();
        estadoOriginalComponentes.Clear();

        if (objetosParaBloquear != null)
        {
            foreach (GameObject obj in objetosParaBloquear)
            {
                if (obj == null)
                    continue;

                // Evita apagar la guia por accidente.
                if (obj == gameObject || obj == panelGuia || obj.transform.IsChildOf(transform))
                    continue;

                estadoOriginalObjetos[obj] = obj.activeSelf;
                obj.SetActive(false);
            }
        }

        if (componentesParaBloquear != null)
        {
            foreach (Behaviour componente in componentesParaBloquear)
            {
                if (componente == null)
                    continue;

                // Evita apagar este mismo script por accidente.
                if (componente == this)
                    continue;

                estadoOriginalComponentes[componente] = componente.enabled;
                componente.enabled = false;
            }
        }
    }

    private void DesbloquearNivel()
    {
        if (pausarJuegoMientrasEsteAbierta)
            Time.timeScale = timeScaleAnterior;

        foreach (KeyValuePair<GameObject, bool> par in estadoOriginalObjetos)
        {
            if (par.Key != null)
                par.Key.SetActive(par.Value);
        }

        foreach (KeyValuePair<Behaviour, bool> par in estadoOriginalComponentes)
        {
            if (par.Key != null)
                par.Key.enabled = par.Value;
        }

        estadoOriginalObjetos.Clear();
        estadoOriginalComponentes.Clear();
    }

    public bool EstaAbierta()
    {
        return guiaAbierta;
    }
}
