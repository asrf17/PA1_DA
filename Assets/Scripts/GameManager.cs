using System;
using UnityEngine;
using UnityEngine.SceneManagement;

// Singleton de la partida. Publica eventos; no conoce el Canvas ni al jugador.
[DefaultExecutionOrder(-100)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instancia
    {
        get; private set;
    }
    public enum Estado
    {
        Inicio, Jugando, Pausa, Victoria, Derrota
    }
    public Estado EstadoActual { get; private set; } = Estado.Inicio;
    public int Monedas
    {
        get; private set;
    }
    public float Tiempo
    {
        get; private set;
    }
    public string Mensaje { get; private set; } = "Recoge 10 monedas y alcanza el faro.";
    [SerializeField] public int totalMonedas = 24;
    [SerializeField] public int monedasNecesarias = 10;
    public event Action Cambio;
    float proximoTick;
    public bool Jugando => EstadoActual == Estado.Jugando;
    void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }
        Instancia = this;
        Time.timeScale = 0;
    }
    void OnDestroy()
    {
        if (Instancia == this)
        {
            Instancia = null;
            Time.timeScale = 1;
        }
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Jugando)
                CambiarEstado(Estado.Pausa);
            else if (EstadoActual == Estado.Pausa)
                Continuar();
        }
        if (Input.GetKeyDown(KeyCode.Return) && EstadoActual == Estado.Inicio)
            Continuar();
        if (Input.GetKeyDown(KeyCode.R) && EstadoActual != Estado.Inicio)
            Reiniciar();
        if (!Jugando)
            return;
        Tiempo += Time.deltaTime;
        if (Tiempo >= proximoTick)
        {
            proximoTick = Tiempo + .1f;
            Cambio?.Invoke();
        }
    }
    public void Continuar()
    {
        CambiarEstado(Estado.Jugando);
    }
    public void Pausar()
    {
        if (Jugando)
            CambiarEstado(Estado.Pausa);
    }
    public void Reiniciar()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void SumarMoneda()
    {
        if (!Jugando)
            return;
        Monedas++;
        Mensaje = Monedas >= monedasNecesarias ? "El faro está listo. ¡Llega a la meta!" : "Busca monedas por las plataformas.";
        Cambio?.Invoke();
    }
    public void MostrarMensaje(string mensaje)
    {
        Mensaje = mensaje;
        Cambio?.Invoke();
    }
    public void Ganar()
    {
        if (!Jugando)
            return;
        if (Monedas < monedasNecesarias)
        {
            MostrarMensaje("Necesitas " + (monedasNecesarias - Monedas) + " monedas más para activar el faro.");
            return;
        }
        CambiarEstado(Estado.Victoria);
        AudioManager.Instancia?.Reproducir(AudioManager.Sonido.Victoria);
    }
    public void Perder()
    {
        CambiarEstado(Estado.Derrota);
    }
    void CambiarEstado(Estado estado)
    {
        EstadoActual = estado;
        Time.timeScale = Jugando ? 1 : 0;
        Cambio?.Invoke();
    }
}
