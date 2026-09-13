using UnityEngine;

namespace PA1Legacy
{
public class Moneda : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D otro)
    {
        if (otro.TryGetComponent(out PlayerController jugador))
        {
            GameManager.Instancia.SumarMoneda();
            Destroy(gameObject);
        }
    }
}


}
