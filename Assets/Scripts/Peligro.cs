using UnityEngine;
public class Peligro : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D otro)
    {
        Herir(otro);
    }
    void OnTriggerStay2D(Collider2D otro)
    {
        Herir(otro);
    }
    void Herir(Collider2D otro)
    {
        if (otro.TryGetComponent<PlayerHealth>(out var vida))
            vida.RecibirDano();
    }
}
