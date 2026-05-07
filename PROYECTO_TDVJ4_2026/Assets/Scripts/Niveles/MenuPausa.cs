using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuPausa : MonoBehaviour
{
    [Header("Panel de pausa")]
    [SerializeField] private GameObject panelPausa;

    [Header("Botones")]
    [SerializeField] private Button botonPausa;
    [SerializeField] private Button botonContinuar;
    [SerializeField] private Button botonReiniciar;
    [SerializeField] private Button botonMenu;

    [Header("Opciones")]
    [SerializeField] private string nombreEscenaMenu = "SelectorModos";
    [SerializeField] private bool pausarConTimeScale = true;
    [SerializeField] private bool ocultarPanelAlInicio = true;

    private bool estaPausado;

    private void Awake()
    {
        if (ocultarPanelAlInicio && panelPausa != null)
            panelPausa.SetActive(false);

        if (botonPausa != null)
            botonPausa.onClick.AddListener(AbrirPausa);

        if (botonContinuar != null)
            botonContinuar.onClick.AddListener(ContinuarJuego);

        if (botonReiniciar != null)
            botonReiniciar.onClick.AddListener(ReiniciarNivel);

        if (botonMenu != null)
            botonMenu.onClick.AddListener(IrAlMenu);
    }

    private void OnDestroy()
    {
        if (botonPausa != null)
            botonPausa.onClick.RemoveListener(AbrirPausa);

        if (botonContinuar != null)
            botonContinuar.onClick.RemoveListener(ContinuarJuego);

        if (botonReiniciar != null)
            botonReiniciar.onClick.RemoveListener(ReiniciarNivel);

        if (botonMenu != null)
            botonMenu.onClick.RemoveListener(IrAlMenu);
    }

   public void AbrirPausa()
{
    estaPausado = true;

    if (panelPausa != null)
    {
        panelPausa.SetActive(true);
        panelPausa.transform.SetAsLastSibling();
    }
    else
    {
        Debug.LogWarning("No asignaste Panel Pausa en el Inspector.");
    }

    if (botonPausa != null)
        botonPausa.gameObject.SetActive(false);

    if (pausarConTimeScale)
        Time.timeScale = 0f;
}

    public void ContinuarJuego()
    {
        estaPausado = false;

        if (panelPausa != null)
            panelPausa.SetActive(false);

        if (botonPausa != null)
            botonPausa.gameObject.SetActive(true);

        if (pausarConTimeScale)
            Time.timeScale = 1f;
    }

    public void ReiniciarNivel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void IrAlMenu()
    {
        Time.timeScale = 1f;

        if (!string.IsNullOrEmpty(nombreEscenaMenu))
        {
            SceneManager.LoadScene(nombreEscenaMenu);
        }
        else
        {
            Debug.LogWarning("No se asignó el nombre de la escena del menú en MenuPausa.");
        }
    }

    public bool EstaPausado()
    {
        return estaPausado;
    }
}
