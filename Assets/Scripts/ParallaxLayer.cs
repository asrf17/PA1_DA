using UnityEngine;
public class ParallaxLayer : MonoBehaviour
{
    public Transform camara; public Vector2 factor = new Vector2(.8f, .5f);
    Vector3 inicio, camInicio;
    void Start()
    {
        inicio = transform.position;
        camInicio = camara.position;
    }
    void LateUpdate()
    {
        Vector3 d = camara.position - camInicio;
        transform.position = inicio + new Vector3(d.x * factor.x, d.y * factor.y, 0);
    }
}
