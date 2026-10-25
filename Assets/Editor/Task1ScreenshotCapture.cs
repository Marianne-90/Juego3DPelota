using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Captura imágenes de las escenas para usarlas en el documento de entrega.
/// Renderiza la cámara de cada escena a una RenderTexture en memoria (sin modificar ni guardar las escenas).
/// </summary>
public static class Task1ScreenshotCapture
{
    private const int Width = 1080;
    private const int Height = 1920;
    private static readonly string OutputFolder = Path.Combine(Application.dataPath, "..", "..", "Capturas", "Tarea1");

    public static void CaptureAll()
    {
        Directory.CreateDirectory(OutputFolder);

        CaptureMainMenu();
        CaptureGameScene();

        Debug.Log("[Task1ScreenshotCapture] Capturas guardadas en: " + Path.GetFullPath(OutputFolder));
    }

    private static void CaptureMainMenu()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity", OpenSceneMode.Single);

        GameObject cameraObject = GameObject.Find("Main Camera");
        Camera camera = cameraObject != null ? cameraObject.GetComponent<Camera>() : null;

        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas != null && camera != null)
        {
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = 1f;
        }

        RenderAndSave(camera, "01_MainMenu.png");
    }

    private static void CaptureGameScene()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/Game.unity", OpenSceneMode.Single);

        GameObject cameraObject = GameObject.Find("Main Camera");
        Camera camera = cameraObject != null ? cameraObject.GetComponent<Camera>() : null;

        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas != null && camera != null)
        {
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = 1f;
        }

        RenderAndSave(camera, "02_GameScene.png");
    }

    private static void RenderAndSave(Camera camera, string fileName)
    {
        if (camera == null)
        {
            Debug.LogWarning("[Task1ScreenshotCapture] No se encontró cámara para capturar: " + fileName);
            return;
        }

        RenderTexture rt = new RenderTexture(Width, Height, 24);
        RenderTexture previousTarget = camera.targetTexture;
        RenderTexture previousActive = RenderTexture.active;

        camera.targetTexture = rt;
        camera.Render(); // primer frame tras abrir la escena: iluminación/shaders aún sin terminar de inicializarse, se descarta
        camera.Render();

        RenderTexture.active = rt;
        Texture2D screenshot = new Texture2D(Width, Height, TextureFormat.RGB24, false);
        screenshot.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
        screenshot.Apply();

        camera.targetTexture = previousTarget;
        RenderTexture.active = previousActive;
        rt.Release();
        Object.DestroyImmediate(rt);

        byte[] pngData = screenshot.EncodeToPNG();
        string path = Path.Combine(OutputFolder, fileName);
        File.WriteAllBytes(path, pngData);
        Object.DestroyImmediate(screenshot);

        Debug.Log("[Task1ScreenshotCapture] Captura guardada: " + path);
    }
}
