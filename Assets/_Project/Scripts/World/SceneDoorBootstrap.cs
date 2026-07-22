using UnityEngine;

/// <summary>
/// Spawns scene doors merged onto building facades (not free-standing pillars).
/// Exit/return doors are larger, lit, and easy to spot.
/// </summary>
public class SceneDoorBootstrap : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoBootstrap()
    {
        string scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (scene == "MainMenu" || scene == "Leaderboard") return;
        if (Object.FindAnyObjectByType<SceneDoor>() != null) return;
        if (Object.FindAnyObjectByType<SceneDoorBootstrap>() != null) return;

        var go = new GameObject("SceneDoorBootstrap");
        go.AddComponent<SceneDoorBootstrap>();
    }

    void Start()
    {
        string scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        if (scene == "campusyard")
            SpawnCampusDoors();
        else if (scene == "classroom")
            SpawnStandaloneReturn("campusyard", "KELUAR · Campus Yard");
        else if (scene == "MainScene")
            SpawnStandaloneReturn("campusyard", "KELUAR · Campus Yard");
    }

    void SpawnCampusDoors()
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

    void SpawnStandaloneReturn(string target, string label)
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

        float facadeWidth = absX > absZ ? b.size.z : b.size.x;
        float doorScale = Mathf.Clamp(facadeWidth / 5.5f, 1.0f, 1.85f);

        var doorGo = new GameObject(doorName);
        doorGo.transform.SetParent(building, true);
        doorGo.transform.position = surface;
        doorGo.transform.rotation = Quaternion.LookRotation(faceNormal, Vector3.up);

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
        trigger.center = new Vector3(0f, 1.4f, 0.7f);
        trigger.size = large
            ? new Vector3(2.8f, 3.2f, 2.4f)
            : new Vector3(2.2f, 2.8f, 1.8f);

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
        light.intensity = exitStyle ? 2.2f : 1.6f;
        light.range = exitStyle ? 10f : 8f;
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
            rend.material.color = color;
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

    static Vector3 FindPlayerPos()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) return player.transform.position;

        var fps = Object.FindAnyObjectByType<FPSController>();
        if (fps != null) return fps.transform.position;

        return Vector3.zero;
    }
}
