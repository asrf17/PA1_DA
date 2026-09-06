using UnityEngine;

public class Meta : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D otro)
    {
        if (otro.TryGetComponent(out PlayerController jugador))
        {
            GameManager.Instancia.Ganar();
        }
    }
}

