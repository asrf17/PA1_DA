using UnityEngine;

public class CamaraFollow : MonoBehaviour
{
    [SerializeField] private Transform objetivo;

    private void LateUpdate()
    {
        if (objetivo != null)
        {
            transform.position = new Vector3(objetivo.position.x, 0f, -10f);
        }
    }
}

