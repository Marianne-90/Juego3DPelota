using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Script de automatización (solo editor) que arma la escena de Tarea 1:
/// proyecto Android, menú principal, escena de juego con pelota, cámara y UI móvil.
/// Se ejecuta una sola vez vía -executeMethod Task1SceneSetup.BuildTask1.
/// </summary>
public static class Task1SceneSetup
{
    private const string ScenesFolder = "Assets/Scenes";
    private const string MainMenuScenePath = ScenesFolder + "/MainMenu.unity";
    private const string GameScenePath = ScenesFolder + "/Game.unity";
    private const string PhysicsMaterialPath = "Assets/Materials/PelotaPhysics.physicMaterial";
    // Nota: la extensión .physicMaterial se mantiene por compatibilidad de Unity aunque la clase se llame PhysicsMaterial.

    public static void BuildTask1()
    {
        Debug.Log("[Task1SceneSetup] Iniciando configuración de la Tarea 1...");

        ConfigurePlayerSettings();
        BuildGameScene();
        BuildMainMenuScene();
        ConfigureBuildSettings();
        SwitchToAndroid();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[Task1SceneSetup] Configuración de la Tarea 1 completada con éxito.");
    }

    private static void ConfigurePlayerSettings()
    {
        PlayerSettings.companyName = "Marianne";
        PlayerSettings.productName = "Juego3DPelota";

        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.marianne.juego3dpelota");
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;

        // Orientación vertical, típica de un juego tipo Helix Jump.
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;

        Debug.Log("[Task1SceneSetup] Player Settings configurados (nombre, package id, SDK, IL2CPP, ARM64).");
    }

    private static PhysicsMaterial CreateBallPhysicsMaterial()
    {
        var existing = AssetDatabase.LoadAssetAtPath<PhysicsMaterial>(PhysicsMaterialPath);
        if (existing != null) return existing;

        var material = new PhysicsMaterial("PelotaPhysics")
        {
            bounciness = 0.65f,
            dynamicFriction = 0.2f,
            staticFriction = 0.2f,
            bounceCombine = PhysicsMaterialCombine.Maximum,
            frictionCombine = PhysicsMaterialCombine.Minimum
        };

        AssetDatabase.CreateAsset(material, PhysicsMaterialPath);
        return material;
    }

    private static Material CreateBasicMaterial(string path, Color color)
    {
        var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (existing != null) return existing;

        Shader shader = Shader.Find("Standard") ?? Shader.Find("Universal Render Pipeline/Lit");
        var material = new Material(shader) { color = color };
        if (material.HasProperty("_Glossiness"))
        {
            material.SetFloat("_Glossiness", 0.15f);
        }
        if (material.HasProperty("_Smoothness"))
        {
            material.SetFloat("_Smoothness", 0.15f);
        }

        AssetDatabase.CreateAsset(material, path);
        return material;
    }

    private static Font GetBuiltinFont()
    {
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null)
        {
            font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }
        return font;
    }

    private static void BuildGameScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // ---- Plataforma base ----
        GameObject platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
        platform.name = "Plataforma";
        platform.transform.position = new Vector3(0f, -0.5f, 0f);
        platform.transform.localScale = new Vector3(10f, 1f, 10f);
        GameObjectUtility.SetStaticEditorFlags(platform, StaticEditorFlags.BatchingStatic);
        platform.GetComponent<Renderer>().sharedMaterial = CreateBasicMaterial(
            "Assets/Materials/PlataformaMat.mat", new Color(0.35f, 0.45f, 0.6f));

        // ---- Pelota (personaje principal) ----
        GameObject ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        ball.name = "Pelota";
        ball.tag = "Player";
        ball.transform.position = new Vector3(0f, 1f, 0f);
        ball.transform.localScale = Vector3.one * 0.8f;
        ball.GetComponent<Renderer>().sharedMaterial = CreateBasicMaterial(
            "Assets/Materials/PelotaMat.mat", new Color(0.95f, 0.3f, 0.25f));

        SphereCollider sphereCollider = ball.GetComponent<SphereCollider>();
        sphereCollider.material = CreateBallPhysicsMaterial();

        Rigidbody rb = ball.AddComponent<Rigidbody>();
        rb.mass = 1f;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        ball.AddComponent<BallController>();

        // ---- Cámara ----
        GameObject cameraObject = GameObject.Find("Main Camera");
        if (cameraObject == null)
        {
            cameraObject = new GameObject("Main Camera", typeof(Camera));
            cameraObject.tag = "MainCamera";
        }
        cameraObject.transform.position = new Vector3(0f, 5f, -8f);

        CameraFollow cameraFollow = cameraObject.AddComponent<CameraFollow>();
        SerializedObject serializedCamera = new SerializedObject(cameraFollow);
        serializedCamera.FindProperty("target").objectReferenceValue = ball.transform;
        serializedCamera.ApplyModifiedPropertiesWithoutUndo();

        cameraObject.transform.LookAt(ball.transform);

        // ---- UI móvil (Canvas) ----
        BuildGameUICanvas();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, GameScenePath);
        Debug.Log("[Task1SceneSetup] Escena 'Game' creada en " + GameScenePath);
    }

    private static void BuildGameUICanvas()
    {
        GameObject canvasObject = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight = 0.5f;

        EnsureEventSystem();

        // Texto de puntaje, arriba.
        GameObject scoreTextObject = new GameObject("ScoreText", typeof(Text));
        scoreTextObject.transform.SetParent(canvasObject.transform, false);
        Text scoreText = scoreTextObject.GetComponent<Text>();
        scoreText.text = "Score: 0";
        scoreText.font = GetBuiltinFont();
        scoreText.fontSize = 64;
        scoreText.alignment = TextAnchor.UpperCenter;
        scoreText.color = Color.white;

        RectTransform scoreRect = scoreTextObject.GetComponent<RectTransform>();
        scoreRect.anchorMin = new Vector2(0.5f, 1f);
        scoreRect.anchorMax = new Vector2(0.5f, 1f);
        scoreRect.pivot = new Vector2(0.5f, 1f);
        scoreRect.anchoredPosition = new Vector2(0f, -60f);
        scoreRect.sizeDelta = new Vector2(600f, 120f);
    }

    private static void BuildMainMenuScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        GameObject menuManagerObject = new GameObject("MenuManager");
        MainMenuController menuController = menuManagerObject.AddComponent<MainMenuController>();

        GameObject canvasObject = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight = 0.5f;

        EnsureEventSystem();

        // Título.
        GameObject titleObject = new GameObject("TitleText", typeof(Text));
        titleObject.transform.SetParent(canvasObject.transform, false);
        Text titleText = titleObject.GetComponent<Text>();
        titleText.text = "Juego3DPelota";
        titleText.font = GetBuiltinFont();
        titleText.fontSize = 100;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = Color.white;

        RectTransform titleRect = titleObject.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.7f);
        titleRect.anchorMax = new Vector2(0.5f, 0.7f);
        titleRect.pivot = new Vector2(0.5f, 0.5f);
        titleRect.anchoredPosition = Vector2.zero;
        titleRect.sizeDelta = new Vector2(900f, 200f);

        // Botón Jugar.
        GameObject buttonObject = new GameObject("PlayButton", typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(canvasObject.transform, false);
        Image buttonImage = buttonObject.GetComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.6f, 1f, 1f);

        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.4f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.4f);
        buttonRect.pivot = new Vector2(0.5f, 0.5f);
        buttonRect.anchoredPosition = Vector2.zero;
        buttonRect.sizeDelta = new Vector2(500f, 160f);

        GameObject buttonTextObject = new GameObject("Text", typeof(Text));
        buttonTextObject.transform.SetParent(buttonObject.transform, false);
        Text buttonText = buttonTextObject.GetComponent<Text>();
        buttonText.text = "JUGAR";
        buttonText.font = GetBuiltinFont();
        buttonText.fontSize = 56;
        buttonText.alignment = TextAnchor.MiddleCenter;
        buttonText.color = Color.white;
        RectTransform buttonTextRect = buttonTextObject.GetComponent<RectTransform>();
        buttonTextRect.anchorMin = Vector2.zero;
        buttonTextRect.anchorMax = Vector2.one;
        buttonTextRect.sizeDelta = Vector2.zero;

        Button button = buttonObject.GetComponent<Button>();
        UnityEventTools.AddPersistentListener(button.onClick, menuController.JugarPartida);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, MainMenuScenePath);
        Debug.Log("[Task1SceneSetup] Escena 'MainMenu' creada en " + MainMenuScenePath);
    }

    private static void EnsureEventSystem()
    {
        if (Object.FindAnyObjectByType<EventSystem>() != null) return;

        GameObject eventSystemObject = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
    }

    private static void ConfigureBuildSettings()
    {
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(MainMenuScenePath, true),
            new EditorBuildSettingsScene(GameScenePath, true)
        };
        Debug.Log("[Task1SceneSetup] Build Settings configurado: MainMenu (0) y Game (1).");
    }

    private static void SwitchToAndroid()
    {
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
        {
            Debug.Log("[Task1SceneSetup] Cambiando plataforma activa a Android...");
            bool switched = EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            Debug.Log("[Task1SceneSetup] Cambio de plataforma a Android: " + (switched ? "éxito" : "falló"));
        }
        else
        {
            Debug.Log("[Task1SceneSetup] La plataforma activa ya era Android.");
        }
    }
}
