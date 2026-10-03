using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Controla la navegación desde el menú principal hacia la escena de juego.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "Game";

    public void JugarPartida()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void SalirDelJuego()
    {
        Application.Quit();
    }
}
