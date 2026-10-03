using UnityEngine;

/// <summary>
/// Hace que la cámara siga al personaje principal.
/// Sincroniza la posición en X y en Y (altura), y mantiene una distancia
/// fija y constante en el eje Z respecto al personaje.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("Objetivo")]
    [SerializeField] private Transform target;

    [Header("Offsets")]
    [Tooltip("Altura extra de la cámara respecto al personaje (eje Y).")]
    [SerializeField] private float heightOffset = 4f;

    [Tooltip("Distancia fija y constante de la cámara respecto al personaje en el eje Z.")]
    [SerializeField] private float fixedZOffset = -8f;

    [Tooltip("Suavizado del seguimiento. 0 = instantáneo.")]
    [SerializeField] private float smoothTime = 0.15f;

    private Vector3 _velocity = Vector3.zero;

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = new Vector3(
            target.position.x,
            target.position.y + heightOffset,
            target.position.z + fixedZOffset
        );

        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref _velocity, smoothTime);
        transform.LookAt(target);
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
