using UnityEngine;
using UnityEngine.UI;
// Observer: solo escucha a la partida y a la vida, sin consultas constantes a la escena.
public class GameHUD : MonoBehaviour
{
    public PlayerHealth vida;
    public Text monedas, vidas, tiempo, mensaje, titulo, descripcion, botonTexto, sonidoTexto;
    public GameObject panel;
    public Button principal, reiniciar, pausa, sonido;
    GameManager partida;
    void Start()
    {
        partida = GameManager.Instancia;
        partida.Cambio += Refrescar;
        vida.Cambio += Refrescar;
        principal.onClick.AddListener(Principal);
        reiniciar.onClick.AddListener(partida.Reiniciar);
        pausa.onClick.AddListener(partida.Pausar);
        sonido.onClick.AddListener(() => { AudioManager.Instancia.AlternarSonido(); sonidoTexto.text = AudioManager.Instancia.Silenciado ? "SONIDO: NO" : "SONIDO: SÍ"; });
        Refrescar();
    }
    void OnDestroy()
    {
        if (partida != null)
            partida.Cambio -= Refrescar;
        if (vida != null)
            vida.Cambio -= Refrescar;
    }
    void Principal()
    {
        if (partida.EstadoActual == GameManager.Estado.Victoria || partida.EstadoActual == GameManager.Estado.Derrota)
            partida.Reiniciar();
        else
            partida.Continuar();
    }
    void Refrescar()
    {
        monedas.text = "MONEDAS  " + partida.Monedas.ToString("00") + " / " + partida.totalMonedas;
        int restantes = Mathf.Clamp(vida.Vidas, 0, 3);
        vidas.text = "VIDAS  " + new string('●', restantes) + new string('○', 3 - restantes);
        tiempo.text = "TIEMPO  " + System.TimeSpan.FromSeconds(partida.Tiempo).ToString(@"mm\:ss");
        mensaje.text = partida.Mensaje;
        panel.SetActive(!partida.Jugando);
        pausa.interactable = partida.Jugando;
        reiniciar.gameObject.SetActive(partida.EstadoActual == GameManager.Estado.Pausa);
        switch (partida.EstadoActual)
        {
            case GameManager.Estado.Inicio:
                titulo.text = "BERIE\nRUMBO AL FARO";
                descripcion.text = "Una aventura en la costa\n\nReúne 10 monedas y llega al faro.\nSalta las púas y activa los puntos de control.\n\nA / D o ← →   Mover\nESPACIO / W / ↑   Saltar\nESC   Pausa     ·     R   Reiniciar";
                botonTexto.text = "COMENZAR  ›";
                break;
            case GameManager.Estado.Pausa:
                titulo.text = "UN RESPIRO";
                descripcion.text = "Tu aventura te espera.\n\nMonedas: " + partida.Monedas + " / " + partida.totalMonedas;
                botonTexto.text = "CONTINUAR  ›";
                break;
            case GameManager.Estado.Victoria:
                titulo.text = "¡FARO ACTIVADO!";
                descripcion.text = "Has completado la travesía.\n\nMonedas: " + partida.Monedas + " / " + partida.totalMonedas + "\nTiempo: " + System.TimeSpan.FromSeconds(partida.Tiempo).ToString(@"mm\:ss") + "\n\n" + (partida.Monedas == partida.totalMonedas ? "¡Exploración perfecta!" : "¿Puedes encontrar todas las monedas?");
                botonTexto.text = "VOLVER A JUGAR  ›";
                break;
            case GameManager.Estado.Derrota:
                titulo.text = "OTRA OPORTUNIDAD";
                descripcion.text = "Se acabaron las vidas.\n\nObserva los saltos y busca las plataformas.\n¡Berie todavía puede llegar al faro!";
                botonTexto.text = "INTENTAR DE NUEVO  ›";
                break;
        }
    }
}
