using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Hace que la cámara siga al personaje principal.
/// Sincroniza la posición en X y en Y (altura), y mantiene una distancia
/// fija y constante en el eje Z respecto al personaje.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("Objetivo")]
    [FormerlySerializedAs("target")]
    [SerializeField] private Transform objetivo;

    [Header("Offsets")]
    [Tooltip("Altura extra de la cámara respecto al personaje (eje Y).")]
    [FormerlySerializedAs("heightOffset")]
    [SerializeField] private float desplazamientoAltura = 4f;

    [Tooltip("Distancia fija y constante de la cámara respecto al personaje en el eje Z.")]
    [FormerlySerializedAs("fixedZOffset")]
    [SerializeField] private float desplazamientoZFijo = -8f;

    [Tooltip("Suavizado del seguimiento. 0 = instantáneo.")]
    [FormerlySerializedAs("smoothTime")]
    [SerializeField] private float tiempoSuavizado = 0.15f;

    private Vector3 _velocidad = Vector3.zero;

    private void LateUpdate()
    {
        if (objetivo == null) return;

        Vector3 posicionDeseada = new Vector3(
            objetivo.position.x,
            objetivo.position.y + desplazamientoAltura,
            objetivo.position.z + desplazamientoZFijo
        );

        transform.position = Vector3.SmoothDamp(transform.position, posicionDeseada, ref _velocidad, tiempoSuavizado);
        transform.LookAt(objetivo);
    }

    public void SetTarget(Transform nuevoObjetivo)
    {
        objetivo = nuevoObjetivo;
    }
}
