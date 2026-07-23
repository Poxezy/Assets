using UnityEngine;

/// <summary>
/// School FBX Boolean exports ("Boole") often ship more materials than submeshes.
/// Unity logs: Mesh 'Boole' has more materials (N) than submeshes (1).
/// Clamp renderer material arrays to mesh.subMeshCount.
/// </summary>
public static class MeshMaterialClamp
{
    public static int ClampActiveScene()
    {
        int fixedCount = 0;
        var filters = Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include);
        for (int i = 0; i < filters.Length; i++)
        {
            var mf = filters[i];
            if (mf == null || mf.sharedMesh == null) continue;
            var r = mf.GetComponent<MeshRenderer>();
            if (r == null) continue;
            if (ClampRenderer(r, mf.sharedMesh))
                fixedCount++;
        }

        var skins = Object.FindObjectsByType<SkinnedMeshRenderer>(FindObjectsInactive.Include);
        for (int i = 0; i < skins.Length; i++)
        {
            var r = skins[i];
            if (r == null || r.sharedMesh == null) continue;
            if (ClampRenderer(r, r.sharedMesh))
                fixedCount++;
        }

        if (fixedCount > 0)
            Debug.Log("MeshMaterialClamp: fixed " + fixedCount + " renderer(s)");
        return fixedCount;
    }

    public static bool ClampRenderer(Renderer r, Mesh mesh)
    {
        if (r == null || mesh == null) return false;
        int sub = Mathf.Max(1, mesh.subMeshCount);
        var mats = r.sharedMaterials;
        if (mats == null || mats.Length <= sub) return false;

        var trimmed = new Material[sub];
        // Prefer last non-null material for each kept slot (CAD packs pad nulls then real mat).
        for (int s = 0; s < sub; s++)
        {
            Material pick = null;
            for (int m = mats.Length - 1; m >= 0; m--)
            {
                if (mats[m] != null) { pick = mats[m]; break; }
            }
            if (pick == null && s < mats.Length)
                pick = mats[s];
            trimmed[s] = pick;
        }
        r.sharedMaterials = trimmed;
        return true;
    }
}
