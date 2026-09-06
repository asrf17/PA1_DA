using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float velocidad = 6f;
    [SerializeField] private float fuerzaSalto = 11f;

    [Header("Suelo")]
    [SerializeField] private Transform detectorSuelo;
    [SerializeField] private LayerMask capaSuelo;

    private Rigidbody2D cuerpo;
    private float horizontal;
    private bool saltoSolicitado;
    private bool enSuelo;
    private Vector3 puntoInicio;

    private void Awake()
    {
        cuerpo = GetComponent<Rigidbody2D>();
        puntoInicio = transform.position;
    }

    private void Update()
    {
        horizontal = Input.GetAxisRaw("Horizontal");

        if (horizontal != 0f)
        {
            GetComponent<SpriteRenderer>().flipX = horizontal < 0f;
        }

        if (Input.GetButtonDown("Jump"))
        {
            saltoSolicitado = true;
        }
    }

    private void FixedUpdate()
    {
        enSuelo = Physics2D.OverlapCircle(detectorSuelo.position, 0.12f, capaSuelo);
        cuerpo.linearVelocity = new Vector2(horizontal * velocidad, cuerpo.linearVelocity.y);

        if (saltoSolicitado && enSuelo)
        {
            cuerpo.linearVelocity = new Vector2(cuerpo.linearVelocity.x, fuerzaSalto);
        }

        saltoSolicitado = false;
    }

    public void Reaparecer()
    {
        transform.position = puntoInicio;
        cuerpo.linearVelocity = Vector2.zero;
    }
}

