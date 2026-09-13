using UnityEngine;
public class Meta : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D otro)
    {
        if (otro.TryGetComponent<PlayerController>(out _))
            GameManager.Instancia.Ganar();
    }
}
