using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>
/// Calcula el puntaje en función del descenso vertical de la pelota, aplica sobre ella
/// una curva de dificultad progresiva (acelera su caída a medida que sube el puntaje) y
/// guarda el mejor puntaje de forma persistente con PlayerPrefs.
/// </summary>
public class ScoreManager : MonoBehaviour
{
    private const string ClaveMejorPuntaje = "HighScore";

    [Header("Referencias")]
    [FormerlySerializedAs("ball")]
    [SerializeField] private BallController pelota;
    [FormerlySerializedAs("scoreText")]
    [SerializeField] private Text textoPuntaje;
    [FormerlySerializedAs("bestText")]
    [SerializeField] private Text textoRecord;

    [Header("Puntaje")]
    [Tooltip("Unidades de caída necesarias para sumar 1 punto.")]
    [FormerlySerializedAs("unitsPerPoint")]
    [SerializeField] private float unidadesPorPunto = 1f;

    [Header("Dificultad progresiva")]
    [FormerlySerializedAs("baseFallSpeed")]
    [SerializeField] private float velocidadCaidaBase = 2.5f;
    [FormerlySerializedAs("maxFallSpeed")]
    [SerializeField] private float velocidadCaidaMaxima = 9f;
    [Tooltip("Cuánto aumenta la velocidad de caída por cada punto anotado.")]
    [FormerlySerializedAs("fallSpeedPerPoint")]
    [SerializeField] private float incrementoVelocidadPorPunto = 0.08f;

    [Header("Fin de la partida")]
    [Tooltip("Unidades por debajo del punto de partida a partir de las cuales se reinicia la partida.")]
    [FormerlySerializedAs("fallDistanceForGameOver")]
    [SerializeField] private float distanciaCaidaFinDePartida = 40f;

    private Vector3 _posicionInicialPelota;
    private int _puntaje;
    private int _mejorPuntaje;

    private void Start()
    {
        if (pelota == null)
        {
            pelota = FindAnyObjectByType<BallController>();
        }

        _posicionInicialPelota = pelota.transform.position;
        _mejorPuntaje = PlayerPrefs.GetInt(ClaveMejorPuntaje, 0);

        ActualizarTextoPuntaje();
        ActualizarTextoRecord();
    }

    private void Update()
    {
        if (pelota == null) return;

        float distanciaCaida = _posicionInicialPelota.y - pelota.transform.position.y;

        if (distanciaCaida >= distanciaCaidaFinDePartida)
        {
            FinalizarPartida();
            return;
        }

        int nuevoPuntaje = Mathf.Max(0, Mathf.FloorToInt(distanciaCaida / unidadesPorPunto));
        if (nuevoPuntaje != _puntaje)
        {
            _puntaje = nuevoPuntaje;
            ActualizarTextoPuntaje();
        }

        float velocidadCaida = Mathf.Min(velocidadCaidaBase + _puntaje * incrementoVelocidadPorPunto, velocidadCaidaMaxima);
        pelota.SetFallSpeed(velocidadCaida);
    }

    private void FinalizarPartida()
    {
        if (_puntaje > _mejorPuntaje)
        {
            _mejorPuntaje = _puntaje;
            PlayerPrefs.SetInt(ClaveMejorPuntaje, _mejorPuntaje);
            PlayerPrefs.Save();
            ActualizarTextoRecord();
        }

        _puntaje = 0;
        ActualizarTextoPuntaje();
        pelota.ResetTo(_posicionInicialPelota);
    }

    private void ActualizarTextoPuntaje()
    {
        if (textoPuntaje != null) textoPuntaje.text = "Puntaje: " + _puntaje;
    }

    private void ActualizarTextoRecord()
    {
        if (textoRecord != null) textoRecord.text = "Récord: " + _mejorPuntaje;
    }
}
