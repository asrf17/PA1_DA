using UnityEngine;
[RequireComponent(typeof(PlayerController))]
public class PlayerAnimation : MonoBehaviour
{
    public Animator animator;
    public SpriteRenderer imagen;
    PlayerController jugador; PlayerHealth vida;
    void Awake()
    {
        jugador = GetComponent<PlayerController>();
        vida = GetComponent<PlayerHealth>();
    }
    void Update()
    {
        animator.SetBool("EnSuelo", jugador.EnSuelo);
        animator.SetBool("Corriendo", Mathf.Abs(jugador.VelocidadX) > .1f);
        animator.SetBool("Cayendo", jugador.VelocidadY < -.1f);
        if (Mathf.Abs(jugador.VelocidadX) > .1f)
            imagen.flipX = jugador.VelocidadX < 0;
        imagen.color = vida.Inmune && Mathf.Sin(Time.time * 35) > 0 ? new Color(1, .5f, .5f, .45f) : Color.white;
    }
}
