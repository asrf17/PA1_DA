using UnityEngine;

// Este script necesita físicas y una forma de colisión para que el personaje pueda moverse y chocar con el escenario.
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    // Estos valores se pueden ajustar desde el Inspector para probar cómo se siente el personaje.
    [SerializeField] private float velocidad = 6f;
    [SerializeField] private float fuerzaSalto = 11f;

    [Header("Suelo")]
    // Este objeto pequeño se coloca debajo de los pies para saber si el personaje está pisando una plataforma.
    [SerializeField] private Transform detectorSuelo;
    // Solo los objetos que estén en esta capa cuentan como suelo.
    [SerializeField] private LayerMask capaSuelo;

    // Guardamos el Rigidbody2D para poder mover al personaje usando las físicas de Unity.
    private Rigidbody2D cuerpo;
    // Aquí se guarda si el jugador está presionando izquierda, derecha o ninguna tecla.
    private float horizontal;
    // Se activa al presionar saltar y se usa después en la parte de físicas.
    private bool saltoSolicitado;
    // Indica si el detector de suelo está tocando una plataforma.
    private bool enSuelo;
    // Recordamos dónde inició el personaje para poder devolverlo allí cuando toca un peligro.
    private Vector3 puntoInicio;

    private void Awake()
    {
        // Buscamos el Rigidbody2D que ya tiene este mismo personaje.
        cuerpo = GetComponent<Rigidbody2D>();
        // Guardamos la posición inicial una sola vez, al comenzar el juego.
        puntoInicio = transform.position;
    }

    private void Update()
    {
        // Leemos las teclas de movimiento. Da -1 para izquierda, 1 para derecha y 0 si no se presiona nada.
        horizontal = Input.GetAxisRaw("Horizontal");

        if (horizontal != 0f)
        {
            // Volteamos la imagen para que el personaje mire hacia donde está caminando.
            GetComponent<SpriteRenderer>().flipX = horizontal < 0f;
        }

        if (Input.GetButtonDown("Jump"))
        {
            // Aquí solo guardamos que el jugador quiere saltar; el salto se aplica en FixedUpdate.
            saltoSolicitado = true;
        }
    }

    private void FixedUpdate()
    {
        // Revisamos un círculo pequeño debajo de los pies. Si toca la capa de suelo, el personaje puede saltar.
        enSuelo = Physics2D.OverlapCircle(detectorSuelo.position, 0.12f, capaSuelo);

        // Movemos al personaje en horizontal y dejamos que la gravedad siga controlando la velocidad vertical.
        cuerpo.linearVelocity = new Vector2(horizontal * velocidad, cuerpo.linearVelocity.y);

        if (saltoSolicitado && enSuelo)
        {
            // Solo saltamos si se presionó la tecla y además el personaje está tocando el suelo.
            cuerpo.linearVelocity = new Vector2(cuerpo.linearVelocity.x, fuerzaSalto);
        }

        // Reiniciamos la solicitud para que no se repita el salto en los siguientes cuadros.
        saltoSolicitado = false;
    }

    public void Reaparecer()
    {
        // Llevamos al personaje al punto donde comenzó y detenemos cualquier movimiento que tenía.
        transform.position = puntoInicio;
        cuerpo.linearVelocity = Vector2.zero;
    }
}
