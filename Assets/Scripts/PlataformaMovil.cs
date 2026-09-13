using UnityEngine;
[RequireComponent(typeof(Rigidbody2D))]
public class PlataformaMovil : MonoBehaviour
{
    [SerializeField] public float distancia = 3f;
    [SerializeField] public float velocidad = 1.3f;
    Rigidbody2D cuerpo; Vector2 inicio;
    void Awake()
    {
        cuerpo = GetComponent<Rigidbody2D>();
        cuerpo.bodyType = RigidbodyType2D.Kinematic;
        inicio = cuerpo.position;
    }
    void FixedUpdate()
    {
        Vector2 destino = inicio + Vector2.right * ((1 - Mathf.Cos(Time.fixedTime * velocidad)) * .5f * distancia);
        Vector2 desplazamiento = destino - cuerpo.position;
        cuerpo.MovePosition(destino);
        foreach (var p in pasajeros)
            if (p != null)
                p.position += desplazamiento;
    }
    readonly System.Collections.Generic.HashSet<Rigidbody2D> pasajeros = new System.Collections.Generic.HashSet<Rigidbody2D>();
    void OnCollisionEnter2D(Collision2D c)
    {
        if (c.rigidbody != null && c.gameObject.GetComponent<PlayerController>() != null && c.transform.position.y > transform.position.y + .1f)
            pasajeros.Add(c.rigidbody);
    }
    void OnCollisionExit2D(Collision2D c)
    {
        if (c.rigidbody != null)
            pasajeros.Remove(c.rigidbody);
    }
}
