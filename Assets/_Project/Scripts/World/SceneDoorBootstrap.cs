using UnityEngine;

/// <summary>
/// Spawns scene doors on facades / return doors.
/// Place on scene Systems GO or call SpawnForActiveScene from GameplaySceneSetup.
/// </summary>
public class SceneDoorBootstrap : MonoBehaviour
{
    void Start()
    {
        SpawnForActiveScene();
    }

    public static void SpawnForActiveScene()
    {
        string scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (scene == "MainMenu" || scene == "Leaderboard") return;

        // Already has doors — skip (scene-authored or previous spawn)
        if (Object.FindAnyObjectByType<SceneDoor>() != null) return;

        if (scene == "campusyard")
            SpawnCampusDoors();
        else if (scene == "classroom")
            SpawnClassroomExit("campusyard", "KELUAR · Campus Yard");
        else if (scene == "MainScene")
            SpawnStandaloneReturn("campusyard", "KELUAR · Campus Yard");
    }

    /// <summary>
    /// Exit door flush on Doorlab mesh — exact classroom door location.
    /// </summary>
    static void SpawnClassroomExit(string target, string label)
    {
        // User request: always on Doorlab
        if (TryPlaceOnDoorlab(target, label))
            return;
        if (TryPlaceInWallPintuOpening(target, label))
            return;

        Debug.LogWarning("SceneDoorBootstrap: Doorlab missing — fallback near player.");
        SpawnStandaloneReturn(target, label);
    }

    /// <summary>Place exit exactly on Doorlab transform (pos + face into room).</summary>
    static bool TryPlaceOnDoorlab(string target, string label)
    {
        Transform doorMesh = FindNamedTransform("Doorlab", "DoorLab", "doorlab");
        if (doorMesh == null) return false;

        Bounds b = GetWorldBounds(doorMesh);
        // Center of Doorlab footprint, feet on floor of mesh
        Vector3 pos = new Vector3(b.center.x, b.min.y, b.center.z);

        // Prefer Doorlab forward if it points into room; else toward room center
        Vector3 roomCenter = EstimateRoomCenter();
        Vector3 toRoom = roomCenter - pos;
        toRoom.y = 0f;

        Vector3 forward = doorMesh.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.01f)
            forward = doorMesh.right;
        forward.Normalize();

        // Pick forward vs back so door faces INTO classroom
        Vector3 inward = forward;
        if (toRoom.sqrMagnitude > 0.01f)
        {
            toRoom.Normalize();
            if (Vector3.Dot(forward, toRoom) < 0f)
                inward = -forward;
            // if mesh axes weird, fall back to room direction
            if (Mathf.Abs(Vector3.Dot(inward, toRoom)) < 0.25f)
                inward = toRoom;
        }

        // Sit flush on Doorlab face (tiny pull into room, no free-standing gap)
        pos += inward * 0.05f;

        // Match Doorlab size
        float doorW = Mathf.Max(b.size.x, b.size.z);
        float doorH = b.size.y;
        float scaleW = Mathf.Clamp(doorW / 1.35f, 0.75f, 1.6f);
        float scaleH = Mathf.Clamp(doorH / 2.4f, 0.8f, 1.5f);
        float scale = (scaleW + scaleH) * 0.5f;
        scale = Mathf.Clamp(scale, 0.85f, 1.45f);

        PlaceFlushExitDoor(target, label, pos, inward, scale, "Doorlab");
        return true;
    }

    /// <summary>Gap between wallpintu2 / wallpintu3 = doorway on south wall (z≈-18.65).</summary>
    static bool TryPlaceInWallPintuOpening(string target, string label)
    {
        Transform a = FindNamedTransform("wallpintu2", "wallpintu 2", "WallPintu2");
        Transform b = FindNamedTransform("wallpintu3", "wallpintu 3", "WallPintu3");
        if (a == null || b == null) return false;

        Bounds ba = GetWorldBounds(a);
        Bounds bb = GetWorldBounds(b);

        // Midpoint of the two wall segments = center of doorway
        Vector3 mid = (ba.center + bb.center) * 0.5f;
        float floorY = Mathf.Min(ba.min.y, bb.min.y);
        mid.y = floorY;

        Vector3 roomCenter = EstimateRoomCenter();
        Vector3 inward = roomCenter - mid;
        inward.y = 0f;
        if (inward.sqrMagnitude < 0.01f) inward = Vector3.forward;
        inward.Normalize();

        // Flush with wall face, slight pull into room
        mid += inward * 0.12f;

        // Opening width ≈ distance between wall centers minus half each extent along wall axis
        Vector3 along = bb.center - ba.center;
        along.y = 0f;
        float gap = along.magnitude;
        float scale = Mathf.Clamp(gap / 4.2f, 0.95f, 1.45f);

        PlaceFlushExitDoor(target, label, mid, inward, scale, "wallpintu gap");
        return true;
    }

    static void PlaceFlushExitDoor(
        string target, string label, Vector3 pos, Vector3 inward, float scale, string source)
    {
        var go = new GameObject("Door_Return_" + target);
        go.transform.position = pos;
        go.transform.rotation = Quaternion.LookRotation(inward, Vector3.up);

        // Natural door (not tall beacon tower) — still readable exit
        BuildFlushDoorVisual(go.transform, label, scale);
        AddTriggerAndDoor(go, target, label, false, "", 1, large: true);

        var box = go.GetComponent<BoxCollider>();
        if (box != null)
        {
            box.center = new Vector3(0f, 1.15f, 0.45f);
            box.size = new Vector3(2.0f, 2.6f, 1.4f);
        }

        Debug.Log("SceneDoorBootstrap: classroom exit (" + source + ") @ " + pos);
    }

    /// <summary>Door-sized frame flush in opening — less “pillar beacon”, more real door.</summary>
    static void BuildFlushDoorVisual(Transform parent, string label, float scale)
    {
        parent.localScale = Vector3.one * scale;

        Color frame = new Color(0.48f, 0.36f, 0.16f, 1f);
        Color panel = new Color(0.22f, 0.16f, 0.08f, 1f);
        Color gold = new Color(0.92f, 0.78f, 0.28f, 1f);

        // Standard single door proportions
        CreateBox(parent, "DoorPanel", new Vector3(0f, 1.15f, 0f), new Vector3(1.35f, 2.25f, 0.07f), panel);
        CreateBox(parent, "PostL", new Vector3(-0.72f, 1.2f, 0.02f), new Vector3(0.12f, 2.4f, 0.12f), frame);
        CreateBox(parent, "PostR", new Vector3(0.72f, 1.2f, 0.02f), new Vector3(0.12f, 2.4f, 0.12f), frame);
        CreateBox(parent, "Lintel", new Vector3(0f, 2.42f, 0.02f), new Vector3(1.6f, 0.14f, 0.14f), frame);
        CreateBox(parent, "Threshold", new Vector3(0f, 0.03f, 0.08f), new Vector3(1.55f, 0.06f, 0.28f), frame);
        CreateBox(parent, "Handle", new Vector3(0.42f, 1.05f, 0.06f), new Vector3(0.06f, 0.22f, 0.08f), gold);
        CreateBox(parent, "Accent", new Vector3(0f, 2.28f, 0.05f), new Vector3(1.25f, 0.05f, 0.06f), gold);

        // Small warm light above lintel (not tall beacon pole)
        var lightGo = new GameObject("DoorBeaconLight");
        lightGo.transform.SetParent(parent, false);
        lightGo.transform.localPosition = new Vector3(0f, 2.55f, 0.25f);
        var light = lightGo.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = gold;
        light.intensity = 1.8f;
        light.range = 6f;
        light.shadows = LightShadows.None;

        var labelGo = new GameObject("DoorLabel");
        labelGo.transform.SetParent(parent, false);
        labelGo.transform.localPosition = new Vector3(0f, 2.65f, 0.1f);
        var tm = labelGo.AddComponent<TextMesh>();
        tm.text = label.ToUpperInvariant();
        tm.fontSize = 26;
        tm.characterSize = 0.055f;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.color = gold;
        tm.fontStyle = FontStyle.Bold;
    }

    static Vector3 EstimateRoomCenter()
    {
        var floor = FindNamedTransform("floor", "Floor");
        if (floor != null)
            return GetWorldBounds(floor).center;

        var player = FindPlayerPos();
        if (player.sqrMagnitude > 0.01f) return player;

        return new Vector3(-3.5f, 6.5f, -8f); // classroom layout default
    }

    static Transform FindNamedTransform(params string[] exactOrParts)
    {
        for (int i = 0; i < exactOrParts.Length; i++)
        {
            var go = GameObject.Find(exactOrParts[i]);
            if (go != null) return go.transform;
        }

        var all = Object.FindObjectsByType<Transform>(FindObjectsInactive.Exclude);
        for (int p = 0; p < exactOrParts.Length; p++)
        {
            string key = exactOrParts[p];
            for (int i = 0; i < all.Length; i++)
            {
                if (string.Equals(all[i].name, key, System.StringComparison.OrdinalIgnoreCase))
                    return all[i];
            }
        }
        for (int p = 0; p < exactOrParts.Length; p++)
        {
            string key = exactOrParts[p];
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i].name.IndexOf(key, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    return all[i];
            }
        }
        return null;
    }

    static void SpawnCampusDoors()
    {
        Vector3 playerPos = FindPlayerPos();

        Transform mainBld = FindBuilding(
            "Building Sky_big_color01",
            "Building Sky_big",
            "Sky_big");
        if (mainBld != null)
        {
            AttachDoorToBuilding(
                mainBld,
                "Door_MainScene",
                "MainScene",
                "Main Scene",
                playerPos,
                requireUnlock: false,
                areaName: "Campus",
                requiredLevel: 1);
        }
        else
        {
            Debug.LogWarning("SceneDoorBootstrap: Main building not found.");
        }

        Transform house = FindBuilding(
            "Building_House_01_color01",
            "Building_House_01",
            "Building_House");
        if (house != null)
        {
            AttachDoorToBuilding(
                house,
                "Door_Classroom",
                "classroom",
                "Classroom",
                playerPos,
                requireUnlock: false,
                areaName: "Classroom",
                requiredLevel: 1);
        }
        else
        {
            Debug.LogWarning("SceneDoorBootstrap: House building not found.");
        }
    }

    static void SpawnStandaloneReturn(string target, string label)
    {
        Vector3 playerPos = FindPlayerPos();
        // Slightly ahead of spawn, easy reach
        Vector3 pos = playerPos + new Vector3(0f, 0f, 3.2f);
        pos.y = playerPos.y;

        var go = new GameObject("Door_Return_" + target);
        go.transform.position = pos;
        // Face player
        Vector3 face = playerPos - pos;
        face.y = 0f;
        if (face.sqrMagnitude < 0.01f) face = Vector3.back;
        go.transform.rotation = Quaternion.LookRotation(face.normalized, Vector3.up);

        BuildFacadeDoorVisual(go.transform, label, scale: 1.25f, exitStyle: true);
        AddTriggerAndDoor(go, target, label, false, "", 1, large: true);
    }

    static void AttachDoorToBuilding(
        Transform building,
        string doorName,
        string targetScene,
        string displayName,
        Vector3 playerPos,
        bool requireUnlock,
        string areaName,
        int requiredLevel)
    {
        Bounds b = GetWorldBounds(building);
        Vector3 center = b.center;
        Vector3 ext = b.extents;

        Vector3 toPlayer = playerPos - center;
        toPlayer.y = 0f;
        if (toPlayer.sqrMagnitude < 0.01f)
            toPlayer = Vector3.forward;
        toPlayer.Normalize();

        Vector3 faceNormal;
        float absX = Mathf.Abs(toPlayer.x);
        float absZ = Mathf.Abs(toPlayer.z);
        if (absX > absZ)
            faceNormal = toPlayer.x > 0f ? Vector3.right : Vector3.left;
        else
            faceNormal = toPlayer.z > 0f ? Vector3.forward : Vector3.back;

        Vector3 surface =
            center
            + new Vector3(faceNormal.x * ext.x, 0f, faceNormal.z * ext.z)
            + faceNormal * 0.12f;
        surface.y = b.min.y;
        // Pull out of facade so mesh not buried in wall
        surface += faceNormal * 0.55f;

        float facadeWidth = absX > absZ ? b.size.z : b.size.x;
        float doorScale = Mathf.Clamp(facadeWidth / 5.5f, 1.15f, 2.0f);

        // World-root door (building scale can squash children)
        var doorGo = new GameObject(doorName);
        doorGo.transform.SetParent(null, true);
        doorGo.transform.position = surface;
        doorGo.transform.rotation = Quaternion.LookRotation(faceNormal, Vector3.up);

        // Name is the stable id; tag optional (TagManager may lag)
        SafeSetTag(doorGo, targetScene);

        BuildFacadeDoorVisual(doorGo.transform, displayName, doorScale, exitStyle: false);
        AddTriggerAndDoor(doorGo, targetScene, displayName, requireUnlock, areaName, requiredLevel, large: true);

        Debug.Log(
            "SceneDoorBootstrap: " + doorName +
            " on " + building.name +
            " @ " + surface +
            " facing " + faceNormal);
    }

    static void AddTriggerAndDoor(
        GameObject go,
        string targetScene,
        string displayName,
        bool requireUnlock,
        string areaName,
        int requiredLevel,
        bool large)
    {
        var trigger = go.GetComponent<BoxCollider>();
        if (trigger == null) trigger = go.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        // Large zone in front of door — easier E interact
        trigger.center = new Vector3(0f, 1.4f, 1.1f);
        trigger.size = large
            ? new Vector3(3.4f, 3.4f, 3.2f)
            : new Vector3(2.6f, 3.0f, 2.4f);

        var door = go.GetComponent<SceneDoor>();
        if (door == null) door = go.AddComponent<SceneDoor>();
        door.Configure(targetScene, displayName, requireUnlock, areaName, requiredLevel);
    }

    static void BuildFacadeDoorVisual(Transform parent, string label, float scale, bool exitStyle)
    {
        parent.localScale = Vector3.one * scale;

        Color frame = exitStyle
            ? new Color(0.62f, 0.48f, 0.18f, 1f)
            : new Color(0.55f, 0.42f, 0.18f, 1f);
        Color panel = exitStyle
            ? new Color(0.18f, 0.22f, 0.16f, 1f)
            : new Color(0.28f, 0.18f, 0.09f, 1f);
        Color gold = new Color(0.92f, 0.78f, 0.28f, 1f);

        CreateBox(parent, "DoorPanel", new Vector3(0f, 1.25f, -0.02f), new Vector3(1.55f, 2.5f, 0.08f), panel);
        CreateBox(parent, "PostL", new Vector3(-0.85f, 1.3f, 0.02f), new Vector3(0.18f, 2.6f, 0.14f), frame);
        CreateBox(parent, "PostR", new Vector3(0.85f, 1.3f, 0.02f), new Vector3(0.18f, 2.6f, 0.14f), frame);
        CreateBox(parent, "Lintel", new Vector3(0f, 2.65f, 0.02f), new Vector3(1.95f, 0.18f, 0.16f), frame);
        CreateBox(parent, "Threshold", new Vector3(0f, 0.04f, 0.12f), new Vector3(1.9f, 0.08f, 0.45f), frame);
        CreateBox(parent, "Handle", new Vector3(0.48f, 1.15f, 0.08f), new Vector3(0.07f, 0.26f, 0.1f), gold);
        CreateBox(parent, "Accent", new Vector3(0f, 2.45f, 0.06f), new Vector3(1.5f, 0.06f, 0.08f), gold);
        CreateBox(parent, "Awning", new Vector3(0f, 2.85f, 0.25f), new Vector3(2.1f, 0.08f, 0.55f), frame);

        // Beacon pole + light so door is visible from afar
        CreateBox(parent, "BeaconPole", new Vector3(0f, 3.6f, 0.08f), new Vector3(0.08f, 1.4f, 0.08f), gold);
        CreateBox(parent, "BeaconOrb", new Vector3(0f, 4.4f, 0.08f), new Vector3(0.28f, 0.28f, 0.28f), gold);

        var lightGo = new GameObject("DoorBeaconLight");
        lightGo.transform.SetParent(parent, false);
        lightGo.transform.localPosition = new Vector3(0f, 4.4f, 0.35f);
        var light = lightGo.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = gold;
        light.intensity = exitStyle ? 3.2f : 2.6f;
        light.range = exitStyle ? 14f : 12f;
        light.shadows = LightShadows.None;

        var labelGo = new GameObject("DoorLabel");
        labelGo.transform.SetParent(parent, false);
        labelGo.transform.localPosition = new Vector3(0f, 3.25f, 0.12f);
        labelGo.transform.localRotation = Quaternion.identity;
        var tm = labelGo.AddComponent<TextMesh>();
        tm.text = label.ToUpperInvariant();
        tm.fontSize = exitStyle ? 32 : 28;
        tm.characterSize = exitStyle ? 0.085f : 0.07f;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.color = gold;
        tm.fontStyle = FontStyle.Bold;
    }

    static void CreateBox(Transform parent, string name, Vector3 localPos, Vector3 scale, Color color)
    {
        var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.name = name;
        box.transform.SetParent(parent, false);
        box.transform.localPosition = localPos;
        box.transform.localRotation = Quaternion.identity;
        box.transform.localScale = scale;

        var col = box.GetComponent<Collider>();
        if (col != null) Object.Destroy(col);

        var rend = box.GetComponent<Renderer>();
        if (rend != null)
            rend.sharedMaterial = MakeSolidMat(color);
    }

    static Material MakeSolidMat(Color color)
    {
        // URP first — built-in Standard often invisible under URP
        Shader sh = Shader.Find("Universal Render Pipeline/Unlit");
        if (sh == null) sh = Shader.Find("Universal Render Pipeline/Lit");
        if (sh == null) sh = Shader.Find("Unlit/Color");
        if (sh == null) sh = Shader.Find("Standard");
        if (sh == null) sh = Shader.Find("Sprites/Default");
        var mat = new Material(sh != null ? sh : Shader.Find("Hidden/InternalErrorShader"));
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
        if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
        if (mat.HasProperty("_EmissionColor"))
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", color * 0.35f);
        }
        return mat;
    }

    static Transform FindBuilding(params string[] nameParts)
    {
        for (int i = 0; i < nameParts.Length; i++)
        {
            var go = GameObject.Find(nameParts[i]);
            if (go != null) return go.transform;
        }

        var all = Object.FindObjectsByType<Transform>();
        for (int i = 0; i < all.Length; i++)
        {
            string n = all[i].name;
            for (int p = 0; p < nameParts.Length; p++)
            {
                if (n.IndexOf(nameParts[p], System.StringComparison.OrdinalIgnoreCase) >= 0)
                    return all[i];
            }
        }
        return null;
    }

    static Bounds GetWorldBounds(Transform t)
    {
        var rends = t.GetComponentsInChildren<Renderer>();
        if (rends.Length == 0)
            return new Bounds(t.position, new Vector3(4f, 6f, 4f));

        Bounds b = rends[0].bounds;
        for (int i = 1; i < rends.Length; i++)
            b.Encapsulate(rends[i].bounds);
        return b;
    }

    static void SafeSetTag(GameObject go, string targetScene)
    {
        if (go == null || string.IsNullOrEmpty(targetScene)) return;
        string tag = null;
        if (string.Equals(targetScene, "classroom", System.StringComparison.OrdinalIgnoreCase))
            tag = "ClassroomDoor";
        else if (string.Equals(targetScene, "MainScene", System.StringComparison.OrdinalIgnoreCase))
            tag = "MainSceneDoor";
        if (tag == null) return;

        // FindGameObjectsWithTag throws if tag not in TagManager
        try
        {
            GameObject.FindGameObjectsWithTag(tag);
            go.tag = tag;
        }
        catch (UnityException)
        {
            // Tag not defined — door still works via name / SceneDoor target
        }
    }

    static Vector3 FindPlayerPos()
    {
        GameObject player = null;
        try { player = GameObject.FindGameObjectWithTag("Player"); }
        catch (UnityException) { /* tag missing */ }
        if (player != null) return player.transform.position;

        var fps = Object.FindAnyObjectByType<FPSController>();
        if (fps != null) return fps.transform.position;

        return Vector3.zero;
    }
}
