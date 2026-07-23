using UnityEditor;
using UnityEngine;

/// <summary>
/// Clamp excess materials on MeshRenderers (Boole FBX export junk).
/// Menu + model post-process so reimport stays clean.
/// </summary>
public class MeshMaterialClampEditor : AssetPostprocessor
{
    void OnPostprocessModel(GameObject root)
    {
        if (root == null) return;
        ClampUnder(root.transform);
    }

    [MenuItem("Tools/MetaEdu/Clamp Mesh Materials (active scene)")]
    static void ClampActiveSceneMenu()
    {
        int n = MeshMaterialClamp.ClampActiveScene();
        Debug.Log("MeshMaterialClampEditor: scene clamp done (" + n + ")");
    }

    [MenuItem("Tools/MetaEdu/Clamp Mesh Materials (all school prefabs)")]
    static void ClampAllSchoolPrefabs()
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/school", "Assets/_Project/Prefabs" });
        int fixedCount = 0;
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                if (ClampUnder(root.transform))
                {
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                    fixedCount++;
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }
        AssetDatabase.SaveAssets();
        Debug.Log("MeshMaterialClampEditor: prefabs fixed " + fixedCount + "/" + guids.Length);
    }

    static bool ClampUnder(Transform t)
    {
        bool any = false;
        var filters = t.GetComponentsInChildren<MeshFilter>(true);
        for (int i = 0; i < filters.Length; i++)
        {
            var mf = filters[i];
            if (mf == null || mf.sharedMesh == null) continue;
            var r = mf.GetComponent<MeshRenderer>();
            if (r == null) continue;
            if (MeshMaterialClamp.ClampRenderer(r, mf.sharedMesh))
                any = true;
        }
        var skins = t.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        for (int i = 0; i < skins.Length; i++)
        {
            var r = skins[i];
            if (r == null || r.sharedMesh == null) continue;
            if (MeshMaterialClamp.ClampRenderer(r, r.sharedMesh))
                any = true;
        }
        return any;
    }
}
