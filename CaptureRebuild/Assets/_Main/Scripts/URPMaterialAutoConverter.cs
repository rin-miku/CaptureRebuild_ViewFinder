using UnityEditor;
using UnityEngine;

public class URPMaterialAutoConverter : MonoBehaviour
{
    [MenuItem("Tools/Convert Standard Materials to URP Lit (with textures)")]
    public static void ConvertStandardToURPLit()
    {
        string[] guids = AssetDatabase.FindAssets("t:Material");
        int count = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);

            if (mat.shader.name != "Standard") continue;

            Texture baseMap = mat.GetTexture("_MainTex");
            Texture normalMap = mat.GetTexture("_BumpMap");
            Texture metallicMap = mat.GetTexture("_MetallicGlossMap");

            Color color = mat.HasProperty("_Color") ? mat.GetColor("_Color") : Color.white;

            mat.shader = Shader.Find("Universal Render Pipeline/Lit");

            if (baseMap != null)
            {
                mat.SetTexture("_BaseMap", baseMap);
                mat.SetColor("_BaseColor", color);
            }

            if (normalMap != null)
            {
                mat.SetTexture("_BumpMap", normalMap);
                mat.EnableKeyword("_NORMALMAP");
            }

            if (metallicMap != null)
            {
                mat.SetTexture("_MetallicGlossMap", metallicMap);
                mat.SetFloat("_Metallic", 1f);
                mat.EnableKeyword("_METALLICSPECGLOSSMAP");
            }

            Debug.Log($"Converted: {mat.name}");
            count++;
        }

        Debug.Log($"已处理 {count} 个 Standard 材质");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
