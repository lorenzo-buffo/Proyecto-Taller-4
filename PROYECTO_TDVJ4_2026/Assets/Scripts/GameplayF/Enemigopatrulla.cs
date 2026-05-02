using UnityEngine;

public class EnemigoPatrulla : MonoBehaviour
{
    [Header("Ruta de patrulla")]
    [Tooltip("Arrastrá acá GameObjects vacíos que marquen los puntos del loop")]
    public Transform[] waypoints;

    [Tooltip("Velocidad de movimiento. Enemigo 1: 2.0 (lento). Enemigo 2: 3.5 (rápido).")]
    public float velocidad = 2f;

    [Tooltip("Distancia mínima para considerar que llegó al waypoint")]
    public float distanciaUmbral = 0.1f;

    private int indiceActual = 0;

    void Update()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        Transform destino = waypoints[indiceActual];

        // Mover suavemente hacia el siguiente waypoint
        transform.position = Vector2.MoveTowards(
            transform.position,
            destino.position,
            velocidad * Time.deltaTime
        );

        // Si llegó al waypoint, pasar al siguiente en el loop
        if (Vector2.Distance(transform.position, destino.position) < distanciaUmbral)
        {
            indiceActual = (indiceActual + 1) % waypoints.Length;
        }
    }

    // Destruye las gotas que toca — igual que DestructorGotas pero móvil
    void OnTriggerEnter2D(Collider2D otro)
    {
        if (otro.CompareTag("Gota"))
        {
            EmisorCaudal.gotasActivas--;
            EmisorCaudal.gotasDestruidas++;
            Destroy(otro.gameObject);
        }
    }
}