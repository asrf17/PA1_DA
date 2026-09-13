using UnityEngine;
public class Checkpoint : MonoBehaviour
{
    public Transform reaparicion;
    public SpriteRenderer indicador;
    bool activo;
    void OnTriggerEnter2D(Collider2D otro)
    {
        if (activo || !otro.TryGetComponent<PlayerController>(out var p))
            return;
        activo = true;
        p.EstablecerControl(reaparicion.position);
        indicador.color = new Color(.3f, 1, .6f);
        AudioManager.Instancia?.Reproducir(AudioManager.Sonido.Control);
        GameManager.Instancia.MostrarMensaje("Punto de control activado.");
    }
}
