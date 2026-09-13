using UnityEngine;

namespace PA1Legacy
{
public class PlataformaMovil : MonoBehaviour
{
    [SerializeField] private float distancia = 3f;
    [SerializeField] private float velocidad = 1.5f;

    private Vector3 inicio;

    private void Start()
    {
        inicio = transform.position;
    }

    private void Update()
    {
        transform.position = inicio + Vector3.right * Mathf.PingPong(Time.time * velocidad, distancia);
    }
}


}
