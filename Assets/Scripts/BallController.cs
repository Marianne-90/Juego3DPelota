using UnityEngine;

/// <summary>
/// Mueve a la pelota en los ejes X y Z a partir del arrastre del dedo (o del mouse en el editor).
/// El eje Y queda a cargo de la física (gravedad + rebote mediante el Physic Material).
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class BallController : MonoBehaviour
{
    [Header("Sensibilidad del arrastre")]
    [Tooltip("Qué tan rápido responde la pelota al arrastre del dedo/mouse.")]
    [SerializeField] private float dragSensitivity = 0.02f;

    [Tooltip("Velocidad horizontal máxima permitida (unidades/segundo).")]
    [SerializeField] private float maxHorizontalSpeed = 6f;

    [Header("Límites del escenario")]
    [Tooltip("Distancia máxima permitida desde el centro de la plataforma en X/Z.")]
    [SerializeField] private float horizontalBounds = 4f;

    private Rigidbody _rigidbody;
    private Vector2 _lastInputPosition;
    private bool _isDragging;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        ReadInput();
    }

    private void ReadInput()
    {
        // Móvil: un dedo tocando la pantalla.
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    _lastInputPosition = touch.position;
                    _isDragging = true;
                    break;
                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    if (_isDragging)
                    {
                        Vector2 delta = touch.position - _lastInputPosition;
                        ApplyDrag(delta);
                        _lastInputPosition = touch.position;
                    }
                    break;
                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    _isDragging = false;
                    break;
            }

            return;
        }

        // Editor / PC: botón izquierdo del mouse, para poder probar sin dispositivo Android.
        if (Input.GetMouseButtonDown(0))
        {
            _lastInputPosition = Input.mousePosition;
            _isDragging = true;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            _isDragging = false;
        }
        else if (_isDragging && Input.GetMouseButton(0))
        {
            Vector2 currentPosition = Input.mousePosition;
            Vector2 delta = currentPosition - _lastInputPosition;
            ApplyDrag(delta);
            _lastInputPosition = currentPosition;
        }
    }

    private void ApplyDrag(Vector2 screenDelta)
    {
        // La X de pantalla mueve al personaje en X del mundo;
        // la Y de pantalla (arriba/abajo del dedo) mueve al personaje en Z del mundo.
        Vector3 worldDelta = new Vector3(screenDelta.x, 0f, screenDelta.y) * dragSensitivity;
        Vector3 targetVelocity = worldDelta / Time.deltaTime;

        targetVelocity.x = Mathf.Clamp(targetVelocity.x, -maxHorizontalSpeed, maxHorizontalSpeed);
        targetVelocity.z = Mathf.Clamp(targetVelocity.z, -maxHorizontalSpeed, maxHorizontalSpeed);

        _rigidbody.linearVelocity = new Vector3(targetVelocity.x, _rigidbody.linearVelocity.y, targetVelocity.z);
    }

    private void LateUpdate()
    {
        // Mantiene a la pelota dentro de los límites de la plataforma en X/Z.
        Vector3 position = transform.position;
        position.x = Mathf.Clamp(position.x, -horizontalBounds, horizontalBounds);
        position.z = Mathf.Clamp(position.z, -horizontalBounds, horizontalBounds);
        transform.position = position;
    }
}
