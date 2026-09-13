using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimiento del PA1")]
    [SerializeField] float velocidad = 6f;
    [SerializeField] float fuerzaSalto = 11f;
    [SerializeField] Transform detectorSuelo;
    [SerializeField] LayerMask capaSuelo = 256;
    public event Action Salto;
    public event Action Aterrizaje;
    public event Action<Vector3> Reubicado;
    public bool EnSuelo
    {
        get; private set;
    }
    public float VelocidadX => cuerpo.linearVelocity.x;
    public float VelocidadY => cuerpo.linearVelocity.y;
    public Vector3 PuntoControl
    {
        get; private set;
    }
    Rigidbody2D cuerpo;
    float horizontal, coyote, buffer;
    bool cortarSalto;
    readonly RaycastHit2D[] contactos = new RaycastHit2D[6];
    void Awake()
    {
        cuerpo = GetComponent<Rigidbody2D>();
        PuntoControl = transform.position;
    }
    void Update()
    {
        if (GameManager.Instancia != null && !GameManager.Instancia.Jugando)
        {
            horizontal = 0;
            buffer = 0;
            return;
        }
        horizontal = Input.GetAxisRaw("Horizontal");
        if (Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            buffer = .12f;
        if (Input.GetButtonUp("Jump") || Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.UpArrow))
            cortarSalto = true;
    }
    void FixedUpdate()
    {
        bool antes = EnSuelo;
        var filter = new ContactFilter2D { useLayerMask = true, layerMask = capaSuelo, useTriggers = false };
        int count = cuerpo.Cast(Vector2.down, filter, contactos, .09f);
        EnSuelo = false;
        for (int i = 0; i < count; i++)
            if (contactos[i].normal.y > .65f && cuerpo.linearVelocity.y <= .1f)
                EnSuelo = true;
        if (!antes && EnSuelo)
            Aterrizaje?.Invoke();
        coyote = EnSuelo ? .11f : coyote - Time.fixedDeltaTime;
        buffer -= Time.fixedDeltaTime;
        cuerpo.linearVelocity = new Vector2(horizontal * velocidad, cuerpo.linearVelocity.y);
        if (buffer > 0 && coyote > 0)
        {
            cuerpo.linearVelocity = new Vector2(cuerpo.linearVelocity.x, fuerzaSalto);
            coyote = 0;
            buffer = 0;
            EnSuelo = false;
            Salto?.Invoke();
        }
        if (cortarSalto && cuerpo.linearVelocity.y > 0)
            cuerpo.linearVelocity = new Vector2(cuerpo.linearVelocity.x, cuerpo.linearVelocity.y * .55f);
        cortarSalto = false;
        if (transform.position.y < -6)
        {
            var vida = GetComponent<PlayerHealth>();
            if (vida != null)
                vida.RecibirDano(true);
            else
                Reaparecer();
        }
    }
    public void EstablecerControl(Vector3 lugar)
    {
        PuntoControl = lugar;
    }
    public void Reaparecer()
    {
        Vector3 delta = PuntoControl - transform.position;
        cuerpo.position = PuntoControl;
        transform.position = PuntoControl;
        cuerpo.linearVelocity = Vector2.zero;
        buffer = 0;
        coyote = 0;
        Reubicado?.Invoke(delta);
    }
}
