using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Script de automatización (solo editor) que ajusta la escena 'Game' para la Tarea 2:
/// descenso continuo de la pelota, puntaje por avance, curva de dificultad progresiva
/// y guardado del mejor puntaje con PlayerPrefs. Se ejecuta una sola vez vía
/// -executeMethod Task2SceneSetup.BuildTask2.
/// </summary>
public static class Task2SceneSetup
{
    private const string GameScenePath = "Assets/Scenes/Game.unity";

    public static void BuildTask2()
    {
        Debug.Log("[Task2SceneSetup] Iniciando configuración de la Tarea 2...");

        Scene scene = EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);

        GameObject ball = GameObject.Find("Pelota");
        GameObject platform = GameObject.Find("Plataforma");
        GameObject canvas = GameObject.Find("Canvas");
        GameObject scoreTextObject = GameObject.Find("ScoreText");

        if (ball == null || platform == null || canvas == null || scoreTextObject == null)
        {
            Debug.LogError("[Task2SceneSetup] No se encontraron todos los objetos esperados de la Tarea 1. Abortando.");
            return;
        }

        // La plataforma de prueba de la Tarea 1 ya no detiene a la pelota: a partir de la
        // Tarea 2 el descenso es continuo y controlado por código. La torre real con huecos
        // se generará de forma procedimental en la Tarea 3.
        Collider platformCollider = platform.GetComponent<Collider>();
        if (platformCollider != null)
        {
            platformCollider.enabled = false;
        }

        Text bestText = CreateBestText(canvas, scoreTextObject.GetComponent<RectTransform>());

        GameObject gameManagerObject = GameObject.Find("GameManager");
        if (gameManagerObject == null)
        {
            gameManagerObject = new GameObject("GameManager");
        }

        ScoreManager scoreManager = gameManagerObject.GetComponent<ScoreManager>();
        if (scoreManager == null)
        {
            scoreManager = gameManagerObject.AddComponent<ScoreManager>();
        }

        SerializedObject serializedManager = new SerializedObject(scoreManager);
        serializedManager.FindProperty("pelota").objectReferenceValue = ball.GetComponent<BallController>();
        serializedManager.FindProperty("textoPuntaje").objectReferenceValue = scoreTextObject.GetComponent<Text>();
        serializedManager.FindProperty("textoRecord").objectReferenceValue = bestText;
        serializedManager.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, GameScenePath);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[Task2SceneSetup] Configuración de la Tarea 2 completada con éxito.");
    }

    private static Text CreateBestText(GameObject canvas, RectTransform scoreRect)
    {
        GameObject existing = GameObject.Find("BestText");
        if (existing != null)
        {
            return existing.GetComponent<Text>();
        }

        GameObject bestTextObject = new GameObject("BestText", typeof(Text));
        bestTextObject.transform.SetParent(canvas.transform, false);

        Text bestText = bestTextObject.GetComponent<Text>();
        bestText.text = "Récord: 0";
        bestText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        bestText.fontSize = 40;
        bestText.alignment = TextAnchor.UpperCenter;
        bestText.color = new Color(1f, 1f, 1f, 0.85f);

        RectTransform bestRect = bestTextObject.GetComponent<RectTransform>();
        bestRect.anchorMin = scoreRect.anchorMin;
        bestRect.anchorMax = scoreRect.anchorMax;
        bestRect.pivot = scoreRect.pivot;
        // Justo debajo del ScoreText (que mide 120 de alto y arranca en -60).
        bestRect.anchoredPosition = new Vector2(scoreRect.anchoredPosition.x, scoreRect.anchoredPosition.y - 130f);
        bestRect.sizeDelta = new Vector2(600f, 80f);

        return bestText;
    }
}
