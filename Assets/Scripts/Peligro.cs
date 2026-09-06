using UnityEngine;

public class Peligro : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D otro)
    {
        if (otro.TryGetComponent(out PlayerController jugador))
        {
            jugador.Reaparecer();
            GameManager.Instancia.MostrarMensaje("Cuidado con las púas");
        }
    }
}

