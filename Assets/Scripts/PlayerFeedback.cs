using UnityEngine;
public class PlayerFeedback : MonoBehaviour
{
    public ParticleSystem polvo, impacto;
    PlayerController jugador; PlayerHealth vida;
    void Awake()
    {
        jugador = GetComponent<PlayerController>();
        vida = GetComponent<PlayerHealth>();
    }
    void OnEnable()
    {
        jugador.Salto += Saltar;
        jugador.Aterrizaje += Aterrizar;
        vida.Dano += Golpe;
    }
    void OnDisable()
    {
        jugador.Salto -= Saltar;
        jugador.Aterrizaje -= Aterrizar;
        vida.Dano -= Golpe;
    }
    void Saltar()
    {
        polvo.Emit(12);
        AudioManager.Instancia?.Reproducir(AudioManager.Sonido.Salto);
    }
    void Aterrizar()
    {
        polvo.Emit(8);
    }
    void Golpe()
    {
        impacto.Emit(22);
    }
}
