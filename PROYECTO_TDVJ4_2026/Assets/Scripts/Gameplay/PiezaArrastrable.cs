using UnityEngine;
using UnityEngine.EventSystems;

public class PiezaArrastrable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Configuración de la Pieza")]
    public Celda.TipoCelda tipoDePieza; 
    public float velocidadCinta = 100f; 
    public bool siendoArrastrada = false;

    [Header("Sonidos")]
    [Tooltip("Sonido al hacer clic y levantar la pieza")]
    public AudioClip clipAgarrar;
    [Tooltip("Sonido al soltar la pieza en la grilla (o fuera de ella)")]
    public AudioClip clipSoltar;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private float limiteInferior = -600f; 

    void Awake() 
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    void Update() 
    {
        if (!siendoArrastrada) 
        {
            rectTransform.anchoredPosition += Vector2.down * velocidadCinta * Time.deltaTime;

            if (rectTransform.anchoredPosition.y < limiteInferior) 
            {
                Destroy(gameObject);
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData) 
    {
        siendoArrastrada = true; 
        transform.SetParent(transform.root); 
        canvasGroup.blocksRaycasts = false; 

        // 🔥 Reproducimos el sonido de AGARRAR usando la posición de la cámara principal
        if (clipAgarrar != null)
        {
            AudioSource.PlayClipAtPoint(clipAgarrar, Camera.main.transform.position);
        }
    }

    public void OnDrag(PointerEventData eventData) 
    {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData) 
    {
        canvasGroup.blocksRaycasts = true;

        // 🔥 Reproducimos el sonido de SOLTAR antes de que la pieza se destruya
        if (clipSoltar != null)
        {
            AudioSource.PlayClipAtPoint(clipSoltar, Camera.main.transform.position);
        }

        Vector2 posicionRatonMundo = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(posicionRatonMundo, Vector2.zero);

        if (hit.collider != null) 
        {
            Celda celdaDestino = hit.collider.GetComponent<Celda>();
            
            if (celdaDestino != null && celdaDestino.tipo != Celda.TipoCelda.Fuente && celdaDestino.tipo != Celda.TipoCelda.Objetivo) 
            {
                celdaDestino.tipo = this.tipoDePieza;
                celdaDestino.ActualizarVisual();
                Destroy(gameObject);
                return;
            }
        }
        
        Destroy(gameObject);
    }
}