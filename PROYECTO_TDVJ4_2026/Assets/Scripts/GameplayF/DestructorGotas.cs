using UnityEngine;

public class DestructorGotas : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D otro)
    {
        if (otro.CompareTag("Gota"))
        {
            EmisorCaudal.gotasActivas--; 
            
            // 🔥 Sumamos a la cuenta de gotas perdidas
            EmisorCaudal.gotasDestruidas++; 
            
            Destroy(otro.gameObject);
        }
    }
}