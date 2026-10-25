using System.IO;
using UnityEditor;
using UnityEditor.Rendering.Universal.ShaderGUI;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Script de automatización (solo editor) que migra el proyecto del Built-in Render Pipeline
/// al Universal Render Pipeline (URP): crea el Render Pipeline Asset y su Renderer Data,
/// los asigna en Graphics Settings, y convierte los materiales existentes (creados con el
/// shader "Standard") al shader "Universal Render Pipeline/Lit". Se ejecuta una sola vez vía
/// -executeMethod URPMigrationSetup.Migrate.
/// </summary>
public static class URPMigrationSetup
{
    private const string SettingsFolder = "Assets/Settings";
    private const string RendererDataPath = SettingsFolder + "/URP-Renderer.asset";
    private const string PipelineAssetPath = SettingsFolder + "/URP-Asset.asset";

    public static void Migrate()
    {
        Debug.Log("[URPMigrationSetup] Iniciando migración a URP...");

        if (!AssetDatabase.IsValidFolder(SettingsFolder))
        {
            AssetDatabase.CreateFolder("Assets", "Settings");
        }

        UniversalRenderPipelineAsset pipelineAsset = CreateOrLoadPipelineAsset();
        GraphicsSettings.defaultRenderPipeline = pipelineAsset;
        QualitySettings.renderPipeline = null; // hereda el pipeline global en todos los niveles de calidad

        // URP calcula la iluminación asumiendo espacio de color Linear; en Gamma (el que traía
        // el proyecto desde la Tarea 1, heredado del template por defecto) la escena se ve casi
        // negra porque la luz direccional e indirecta no se interpretan correctamente.
        if (PlayerSettings.colorSpace != ColorSpace.Linear)
        {
            PlayerSettings.colorSpace = ColorSpace.Linear;
            Debug.Log("[URPMigrationSetup] Espacio de color cambiado de Gamma a Linear (requerido por URP).");
        }

        ConvertMaterial("Assets/Materials/PelotaMat.mat");
        ConvertMaterial("Assets/Materials/PlataformaMat.mat");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[URPMigrationSetup] Migración a URP completada con éxito. Pipeline asignado: " +
                  AssetDatabase.GetAssetPath(GraphicsSettings.defaultRenderPipeline));
    }

    private static UniversalRenderPipelineAsset CreateOrLoadPipelineAsset()
    {
        var existingAsset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(PipelineAssetPath);
        if (existingAsset != null)
        {
            return existingAsset;
        }

        var rendererData = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(RendererDataPath);
        if (rendererData == null)
        {
            rendererData = ScriptableObject.CreateInstance<UniversalRendererData>();
            AssetDatabase.CreateAsset(rendererData, RendererDataPath);
        }

        var pipelineAsset = UniversalRenderPipelineAsset.Create(rendererData);
        AssetDatabase.CreateAsset(pipelineAsset, PipelineAssetPath);

        return pipelineAsset;
    }

    private static void ConvertMaterial(string path)
    {
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            Debug.LogWarning("[URPMigrationSetup] No se encontró el material: " + path);
            return;
        }

        Color baseColor = material.HasProperty("_Color") ? material.GetColor("_Color") : Color.white;
        float smoothness = material.HasProperty("_Glossiness") ? material.GetFloat("_Glossiness") : 0.15f;

        Shader urpLitShader = Shader.Find("Universal Render Pipeline/Lit");
        if (urpLitShader == null)
        {
            Debug.LogError("[URPMigrationSetup] No se encontró el shader 'Universal Render Pipeline/Lit'.");
            return;
        }

        material.shader = urpLitShader;
        material.SetColor("_BaseColor", baseColor);
        if (material.HasProperty("_Smoothness"))
        {
            material.SetFloat("_Smoothness", smoothness);
        }

        // Asignar el shader a mano no dispara el ShaderGUI de URP, así que las keywords de
        // superficie (Opaque/Transparent, blend, ZWrite) quedan sin inicializar y el material
        // se renderiza mal (se ve apagado/gris). Replicamos lo que hace el Inspector al cambiar
        // de shader, usando las mismas utilidades públicas que usa el editor de URP.
        BaseShaderGUI.SetupMaterialBlendMode(material);
        BaseShaderGUI.SetMaterialKeywords(material, LitGUI.SetMaterialKeywords);

        EditorUtility.SetDirty(material);
        Debug.Log("[URPMigrationSetup] Material convertido a URP/Lit: " + path);
    }
}
