using UnityEngine;

public class DestructorGotas : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D otro)
    {
        // Si lo que toca la zona es una gota, la destruye
        if (otro.CompareTag("Gota"))
        {
            Destroy(otro.gameObject);
        }
    }
}