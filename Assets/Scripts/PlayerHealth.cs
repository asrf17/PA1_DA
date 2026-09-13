using System;
using UnityEngine;
public class PlayerHealth : MonoBehaviour
{
    public int Vidas { get; private set; } = 3;
    public event Action Cambio;
    public event Action Dano;
    float inmuneHasta;
    public bool Inmune => Time.time < inmuneHasta;
    public void RecibirDano(bool caida = false)
    {
        if (Vidas <= 0 || !GameManager.Instancia.Jugando || (!caida && Inmune))
            return;
        Vidas--;
        inmuneHasta = Time.time + 1.4f;
        Dano?.Invoke();
        AudioManager.Instancia?.Reproducir(AudioManager.Sonido.Dano);
        Cambio?.Invoke();
        if (Vidas <= 0)
        {
            GameManager.Instancia.Perder();
            return;
        }
        GetComponent<PlayerController>().Reaparecer();
        GameManager.Instancia.MostrarMensaje("Cuidado. Regresas al último punto de control.");
    }
}
