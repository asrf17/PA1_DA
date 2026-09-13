using UnityEngine;
using Unity.Cinemachine;
public class CameraRespawn : MonoBehaviour
{
    public PlayerController jugador; public CinemachineCamera camara;
    void OnEnable()
    {
        jugador.Reubicado += Reubicar;
    }
    void OnDisable()
    {
        jugador.Reubicado -= Reubicar;
    }
    void Reubicar(Vector3 delta)
    {
        camara.OnTargetObjectWarped(jugador.transform, delta);
        camara.PreviousStateIsValid = false;
    }
}
