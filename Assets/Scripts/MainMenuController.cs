using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

/// <summary>
/// Controla la navegación desde el menú principal hacia la escena de juego.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [FormerlySerializedAs("gameSceneName")]
    [SerializeField] private string nombreEscenaJuego = "Game";

    public void JugarPartida()
    {
        SceneManager.LoadScene(nombreEscenaJuego);
    }

    public void SalirDelJuego()
    {
        Application.Quit();
    }
}
