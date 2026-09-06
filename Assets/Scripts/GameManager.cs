using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instancia { get; private set; }

    [SerializeField] private Text textoEstado;
    [SerializeField] private int totalMonedas;
    private int monedas;

    private void Awake()
    {
        Instancia = this;
        ActualizarTexto("Recoge las monedas y llega a la bandera");
    }

    public void SumarMoneda()
    {
        monedas++;
        ActualizarTexto("Monedas: " + monedas + " / " + totalMonedas);
    }

    public void MostrarMensaje(string mensaje)
    {
        ActualizarTexto(mensaje + "  Monedas: " + monedas + " / " + totalMonedas);
    }

    public void Ganar()
    {
        ActualizarTexto("¡Nivel terminado! Monedas: " + monedas + " / " + totalMonedas);
    }

    private void ActualizarTexto(string mensaje)
    {
        if (textoEstado != null)
        {
            textoEstado.text = mensaje;
        }
    }
}

