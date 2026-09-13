using UnityEngine;
[DefaultExecutionOrder(-90)]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instancia
    {
        get; private set;
    }
    public enum Sonido
    {
        Salto, Moneda, Dano, Control, Victoria
    }
    public AudioSource musica, efectos;
    public AudioClip[] clips;
    public bool Silenciado
    {
        get; private set;
    }
    void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }
        Instancia = this;
    }
    void OnDestroy()
    {
        if (Instancia == this)
            Instancia = null;
    }
    public void Reproducir(Sonido sonido)
    {
        if (clips.Length > (int)sonido)
            efectos.PlayOneShot(clips[(int)sonido]);
    }
    public void AlternarSonido()
    {
        Silenciado = !Silenciado;
        musica.mute = Silenciado;
        efectos.mute = Silenciado;
    }
}
