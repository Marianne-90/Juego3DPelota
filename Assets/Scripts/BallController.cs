using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Mueve a la pelota en los ejes X y Z a partir del arrastre del dedo (o del mouse en el editor),
/// mientras desciende de forma continua en el eje Y a una velocidad controlada por el sistema de
/// puntaje (<see cref="ScoreManager"/>), en vez de depender únicamente de la gravedad.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class BallController : MonoBehaviour
{
    [Header("Sensibilidad del arrastre")]
    [Tooltip("Qué tan rápido responde la pelota al arrastre del dedo/mouse.")]
    [FormerlySerializedAs("dragSensitivity")]
    [SerializeField] private float sensibilidadArrastre = 0.02f;

    [Tooltip("Velocidad horizontal máxima permitida (unidades/segundo).")]
    [FormerlySerializedAs("maxHorizontalSpeed")]
    [SerializeField] private float velocidadHorizontalMaxima = 6f;

    [Header("Límites del escenario")]
    [Tooltip("Distancia máxima permitida desde el centro de la plataforma en X/Z.")]
    [FormerlySerializedAs("horizontalBounds")]
    [SerializeField] private float limiteHorizontal = 4f;

    [Header("Descenso")]
    [Tooltip("Velocidad de caída inicial (unidades/segundo), antes de aplicar la curva de dificultad.")]
    [FormerlySerializedAs("initialFallSpeed")]
    [SerializeField] private float velocidadCaidaInicial = 2.5f;

    private Rigidbody _rigidbody;
    private Vector2 _lastInputPosition;
    private bool _isDragging;
    private float _currentFallSpeed;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.useGravity = false;
        _currentFallSpeed = velocidadCaidaInicial;
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
        Vector3 worldDelta = new Vector3(screenDelta.x, 0f, screenDelta.y) * sensibilidadArrastre;
        Vector3 targetVelocity = worldDelta / Time.deltaTime;

        targetVelocity.x = Mathf.Clamp(targetVelocity.x, -velocidadHorizontalMaxima, velocidadHorizontalMaxima);
        targetVelocity.z = Mathf.Clamp(targetVelocity.z, -velocidadHorizontalMaxima, velocidadHorizontalMaxima);

        _rigidbody.linearVelocity = new Vector3(targetVelocity.x, _rigidbody.linearVelocity.y, targetVelocity.z);
    }

    private void FixedUpdate()
    {
        // El descenso es continuo y controlado por código (no por gravedad), para que el
        // ScoreManager pueda acelerarlo progresivamente a través de SetFallSpeed.
        Vector3 velocity = _rigidbody.linearVelocity;
        velocity.y = -_currentFallSpeed;
        _rigidbody.linearVelocity = velocity;
    }

    private void LateUpdate()
    {
        // Mantiene a la pelota dentro de los límites de la plataforma en X/Z.
        Vector3 position = transform.position;
        position.x = Mathf.Clamp(position.x, -limiteHorizontal, limiteHorizontal);
        position.z = Mathf.Clamp(position.z, -limiteHorizontal, limiteHorizontal);
        transform.position = position;
    }

    /// <summary>Actualiza la velocidad de caída actual (curva de dificultad progresiva).</summary>
    public void SetFallSpeed(float fallSpeed)
    {
        _currentFallSpeed = fallSpeed;
    }

    /// <summary>Reubica a la pelota (fin de partida) y detiene su velocidad acumulada.</summary>
    public void ResetTo(Vector3 position)
    {
        transform.position = position;
        _rigidbody.linearVelocity = Vector3.zero;
        _currentFallSpeed = velocidadCaidaInicial;
    }
}
