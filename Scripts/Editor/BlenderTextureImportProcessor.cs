using UnityEngine;
using UnityEditor;
using System.IO;

public class BlenderTextureImportProcessor : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        string filename = Path.GetFileName(assetPath).ToLower();
        string filenameWithoutExt = Path.GetFileNameWithoutExtension(assetPath).ToLower();

        TextureImporter importer = (TextureImporter)assetImporter;

        if (filenameWithoutExt.Contains("_albedo") ||
            filenameWithoutExt.Contains("_basecolor") ||
            filenameWithoutExt.Contains("_diffuse"))
        {
            Debug.Log($"[Blender Baker] Импорт Albedo: {filename}");

            importer.textureType = TextureImporterType.Default;
            importer.sRGBTexture = true;
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.mipmapEnabled = true;
            importer.filterMode = FilterMode.Point;  // Point filtering для pixel art стиля
            importer.textureCompression = TextureImporterCompression.CompressedHQ;

            SetPlatformTextureSettings(importer, "Standalone", 2048, TextureImporterFormat.DXT5);
            SetPlatformTextureSettings(importer, "Android", 1024, TextureImporterFormat.ASTC_6x6);
            SetPlatformTextureSettings(importer, "iPhone", 1024, TextureImporterFormat.ASTC_6x6);
        }

        else if (filenameWithoutExt.Contains("_normal") ||
                 filenameWithoutExt.Contains("_norm"))
        {
            Debug.Log($"[Blender Baker] Импорт Normal Map: {filename}");

            importer.textureType = TextureImporterType.NormalMap;
            importer.sRGBTexture = false;  // Linear space
            importer.mipmapEnabled = true;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;

            TextureImporterSettings settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.normalMapFilter = TextureImporterNormalFilter.Standard;
            importer.SetTextureSettings(settings);

            SetPlatformTextureSettings(importer, "Standalone", 2048, TextureImporterFormat.BC5);
            SetPlatformTextureSettings(importer, "Android", 1024, TextureImporterFormat.ASTC_5x5);
            SetPlatformTextureSettings(importer, "iPhone", 1024, TextureImporterFormat.ASTC_5x5);
        }

        else if (filenameWithoutExt.Contains("_mrao") ||
                 filenameWithoutExt.Contains("_mask"))
        {
            Debug.Log($"[Blender Baker] Импорт MRAO Map: {filename}");
            Debug.Log($"  → R = Metallic | G = Roughness | B = AO");

            importer.textureType = TextureImporterType.Default;
            importer.sRGBTexture = false;
            importer.alphaSource = TextureImporterAlphaSource.None;
            importer.mipmapEnabled = true;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;

            SetPlatformTextureSettings(importer, "Standalone", 2048, TextureImporterFormat.DXT5);
            SetPlatformTextureSettings(importer, "Android", 1024, TextureImporterFormat.ASTC_6x6);
            SetPlatformTextureSettings(importer, "iPhone", 1024, TextureImporterFormat.ASTC_6x6);
        }

        else if (filenameWithoutExt.Contains("_metallic") ||
                 filenameWithoutExt.Contains("_metal"))
        {
            Debug.Log($"[Blender Baker] Импорт Metallic: {filename}");

            importer.textureType = TextureImporterType.Default;
            importer.sRGBTexture = false;  // Linear space
            importer.alphaSource = TextureImporterAlphaSource.None;
            importer.mipmapEnabled = true;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;

            SetPlatformTextureSettings(importer, "Standalone", 2048, TextureImporterFormat.BC4);
        }

        else if (filenameWithoutExt.Contains("_roughness") ||
                 filenameWithoutExt.Contains("_rough"))
        {
            Debug.Log($"[Blender Baker] Импорт Roughness: {filename}");

            importer.textureType = TextureImporterType.Default;
            importer.sRGBTexture = false;  // Linear space
            importer.alphaSource = TextureImporterAlphaSource.None;
            importer.mipmapEnabled = true;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;

            SetPlatformTextureSettings(importer, "Standalone", 2048, TextureImporterFormat.BC4);
        }

        else if (filenameWithoutExt.Contains("_ao") ||
                 filenameWithoutExt.Contains("_occlusion"))
        {
            Debug.Log($"[Blender Baker] Импорт AO: {filename}");

            importer.textureType = TextureImporterType.Default;
            importer.sRGBTexture = false;  // Linear space
            importer.alphaSource = TextureImporterAlphaSource.None;
            importer.mipmapEnabled = true;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;

            SetPlatformTextureSettings(importer, "Standalone", 2048, TextureImporterFormat.BC4);
        }

        else if (filenameWithoutExt.Contains("_emission") ||
                 filenameWithoutExt.Contains("_emissive"))
        {
            Debug.Log($"[Blender Baker] Импорт Emission: {filename}");

            importer.textureType = TextureImporterType.Default;
            importer.sRGBTexture = true;  // sRGB для цветов
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.mipmapEnabled = true;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;

            SetPlatformTextureSettings(importer, "Standalone", 2048, TextureImporterFormat.DXT5);
        }
    }

    void SetPlatformTextureSettings(TextureImporter importer, string platform, int maxSize, TextureImporterFormat format)
    {
        TextureImporterPlatformSettings platformSettings = new TextureImporterPlatformSettings();
        platformSettings.name = platform;
        platformSettings.overridden = true;
        platformSettings.maxTextureSize = maxSize;
        platformSettings.format = format;
        platformSettings.textureCompression = TextureImporterCompression.CompressedHQ;

        importer.SetPlatformTextureSettings(platformSettings);
    }

    void OnPostprocessTexture(Texture2D texture)
    {
        string filename = Path.GetFileNameWithoutExtension(assetPath).ToLower();

        if (filename.Contains("_mrao"))
        {
            Debug.Log($"✓ MRAO карта готова к использованию: {texture.name}");
        }
    }
}

public class BlenderBakerUtilities
{
    [MenuItem("Tools/Blender Baker/Fix MRAO Texture Settings")]
    static void FixMRAOTextureSettings()
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture2D MRAO");

        int fixedTextures = 0;
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

            if (importer != null && importer.sRGBTexture)
            {
                Debug.LogWarning($"Исправляю MRAO текстуру: {path}");
                importer.sRGBTexture = false;  // Отключаем sRGB
                importer.SaveAndReimport();
                fixedTextures++;
            }
        }

        if (fixedTextures > 0)
        {
            Debug.Log($"✓ Исправлено {fixedTextures} MRAO текстур");
        }
        else
        {
            Debug.Log("✓ Все MRAO текстуры уже настроены корректно");
        }
    }

    [MenuItem("Tools/Blender Baker/Fix Albedo Filter Mode")]
    static void FixAlbedoFilterMode()
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture2D");

        int fixedTextures = 0;
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string filename = Path.GetFileNameWithoutExtension(path).ToLower();

            if (filename.Contains("_albedo") || filename.Contains("_basecolor") || filename.Contains("_diffuse"))
            {
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

                if (importer != null && importer.filterMode != FilterMode.Point)
                {
                    Debug.LogWarning($"Исправляю Filter Mode для Albedo: {path}");
                    importer.filterMode = FilterMode.Point;
                    importer.SaveAndReimport();
                    fixedTextures++;
                }
            }
        }

        if (fixedTextures > 0)
        {
            Debug.Log($"✓ Исправлено {fixedTextures} Albedo текстур (установлен Point filter)");
        }
        else
        {
            Debug.Log("✓ Все Albedo текстуры уже имеют Point filter mode");
        }
    }

    [MenuItem("Tools/Blender Baker/Create Material from Selected Textures")]
    static void CreateMaterialFromTextures()
    {
        Object[] selected = Selection.objects;

        Texture2D albedo = null;
        Texture2D normal = null;
        Texture2D mrao = null;
        Texture2D metallic = null;
        Texture2D roughness = null;
        Texture2D ao = null;
        Texture2D emission = null;

        foreach (Object obj in selected)
        {
            if (obj is Texture2D texture)
            {
                string name = texture.name.ToLower();

                if (name.Contains("_albedo") || name.Contains("_basecolor") || name.Contains("_diffuse"))
                    albedo = texture;
                else if (name.Contains("_normal"))
                    normal = texture;
                else if (name.Contains("_mrao"))
                    mrao = texture;
                else if (name.Contains("_metallic") || name.Contains("_metal"))
                    metallic = texture;
                else if (name.Contains("_roughness") || name.Contains("_rough"))
                    roughness = texture;
                else if (name.Contains("_ao") || name.Contains("_occlusion"))
                    ao = texture;
                else if (name.Contains("_emission") || name.Contains("_emissive"))
                    emission = texture;
            }
        }

        if (albedo == null)
        {
            Debug.LogError("❌ Не найдена Albedo текстура. Выберите текстуры и попробуйте снова.");
            return;
        }

        string materialName = albedo.name.Replace("_Albedo", "").Replace("_albedo", "");

        // Используем Standard shader для Built-in Render Pipeline
        Material mat = new Material(Shader.Find("Standard"));

        // Albedo
        mat.SetTexture("_MainTex", albedo);
        mat.SetColor("_Color", Color.white);

        // Normal Map
        if (normal != null)
        {
            mat.SetTexture("_BumpMap", normal);
            mat.EnableKeyword("_NORMALMAP");
            mat.SetFloat("_BumpScale", 1f);
        }

        // MRAO (комбинированная карта: Metallic, Roughness, AO)
        if (mrao != null)
        {
            mat.SetTexture("_MetallicGlossMap", mrao);
            mat.SetFloat("_Metallic", 1f);
            mat.SetFloat("_Glossiness", 1f);
            mat.EnableKeyword("_METALLICGLOSSMAP");

            // Для Built-in нужно установить workflow
            mat.SetFloat("_SmoothnessTextureChannel", 0); // 0 = Metallic Alpha, 1 = Albedo Alpha
            mat.SetFloat("_GlossMapScale", 1f);

            Debug.Log("→ MRAO карта назначена. R=Metallic, G=Roughness, B=AO");
        }
        else
        {
            // Если нет MRAO, используем отдельные текстуры
            if (metallic != null)
            {
                mat.SetTexture("_MetallicGlossMap", metallic);
                mat.SetFloat("_Metallic", 1f);
                mat.EnableKeyword("_METALLICGLOSSMAP");
            }
            else
            {
                mat.SetFloat("_Metallic", 0f);
            }

            if (roughness != null)
            {
                // В Standard shader roughness инвертируется в smoothness
                mat.SetFloat("_Glossiness", 0.5f);
                Debug.LogWarning("⚠ Roughness текстура найдена, но Standard shader использует Smoothness. Рекомендуется использовать MRAO.");
            }
        }

        // Ambient Occlusion
        if (ao != null)
        {
            mat.SetTexture("_OcclusionMap", ao);
            mat.SetFloat("_OcclusionStrength", 1f);
        }

        // Emission
        if (emission != null)
        {
            mat.SetTexture("_EmissionMap", emission);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", Color.white);
        }

        // Сохранение материала
        string path = AssetDatabase.GetAssetPath(albedo);
        string directory = Path.GetDirectoryName(path);
        string materialPath = Path.Combine(directory, $"{materialName}.mat");

        AssetDatabase.CreateAsset(mat, materialPath);
        AssetDatabase.SaveAssets();

        Debug.Log($"✓ Материал создан: {materialPath}");
        Debug.Log($"  Shader: Standard (Built-in Render Pipeline)");
        Selection.activeObject = mat;
    }

    [MenuItem("Tools/Blender Baker/Validate All Texture Settings")]
    static void ValidateAllTextureSettings()
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture2D");

        int totalTextures = 0;
        int albedoCount = 0;
        int normalCount = 0;
        int mraoCount = 0;
        int wrongSettings = 0;
        int wrongFilterMode = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string filename = Path.GetFileNameWithoutExtension(path).ToLower();
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

            if (importer == null) continue;

            totalTextures++;

            if (filename.Contains("_albedo") || filename.Contains("_basecolor") || filename.Contains("_diffuse"))
            {
                albedoCount++;
                if (!importer.sRGBTexture)
                {
                    Debug.LogWarning($"⚠ Albedo должна быть sRGB: {path}");
                    wrongSettings++;
                }
                if (importer.filterMode != FilterMode.Point)
                {
                    Debug.LogWarning($"⚠ Albedo должна иметь Point filter mode: {path}");
                    wrongFilterMode++;
                }
            }

            else if (filename.Contains("_normal"))
            {
                normalCount++;
                if (importer.textureType != TextureImporterType.NormalMap)
                {
                    Debug.LogWarning($"⚠ Normal Map тип не установлен: {path}");
                    wrongSettings++;
                }
            }

            else if (filename.Contains("_mrao"))
            {
                mraoCount++;
                if (importer.sRGBTexture)
                {
                    Debug.LogWarning($"⚠ MRAO не должна быть sRGB: {path}");
                    wrongSettings++;
                }
            }
        }

        Debug.Log($"═══════════════════════════════════");
        Debug.Log($"Всего текстур: {totalTextures}");
        Debug.Log($"  Albedo: {albedoCount}");
        Debug.Log($"  Normal: {normalCount}");
        Debug.Log($"  MRAO: {mraoCount}");
        Debug.Log($"═══════════════════════════════════");

        if (wrongSettings > 0 || wrongFilterMode > 0)
        {
            Debug.LogWarning($"⚠ Найдено {wrongSettings} текстур с неправильными настройками");
            Debug.LogWarning($"⚠ Найдено {wrongFilterMode} Albedo текстур без Point filter mode");
            Debug.Log("Используйте:");
            Debug.Log("  Tools → Blender Baker → Fix MRAO Texture Settings");
            Debug.Log("  Tools → Blender Baker → Fix Albedo Filter Mode");
        }
        else
        {
            Debug.Log("✓ Все текстуры настроены корректно!");
        }
    }
}

[System.Serializable]
public class BlenderMaterialTemplate
{
    public string materialName;
    public Texture2D albedoTexture;
    public Texture2D normalTexture;
    public Texture2D mraoTexture;
    public Texture2D metallicTexture;
    public Texture2D roughnessTexture;
    public Texture2D aoTexture;
    public Texture2D emissionTexture;
    public Color baseColor = Color.white;
    public float metallic = 0f;
    public float smoothness = 0.5f;
    public float normalStrength = 1f;
    public float aoStrength = 1f;
}