using UnityEngine;
public class Moneda : MonoBehaviour
{
    public ParticleSystem destello;
    bool recogida;
    void OnTriggerEnter2D(Collider2D otro)
    {
        if (recogida || !otro.TryGetComponent<PlayerController>(out _) || !GameManager.Instancia.Jugando)
            return;
        recogida = true;
        GameManager.Instancia.SumarMoneda();
        AudioManager.Instancia?.Reproducir(AudioManager.Sonido.Moneda);
        if (destello != null)
        {
            destello.transform.SetParent(null);
            destello.Play();
            Destroy(destello.gameObject, 2);
        }
        Destroy(gameObject);
    }
}
