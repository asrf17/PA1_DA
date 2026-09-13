using UnityEngine;
public class SpriteLoop : MonoBehaviour
{
    public Sprite[] cuadros;
    public float fps = 8;
    public float flotacion = .08f;
    SpriteRenderer imagen; Vector3 inicio;
    void Awake()
    {
        imagen = GetComponent<SpriteRenderer>();
        inicio = transform.localPosition;
    }
    void Update()
    {
        if (cuadros.Length > 0)
            imagen.sprite = cuadros[(int)(Time.time * fps) % cuadros.Length];
        transform.localPosition = inicio + Vector3.up * (Mathf.Sin(Time.time * 2 + inicio.x) * flotacion);
    }
}
