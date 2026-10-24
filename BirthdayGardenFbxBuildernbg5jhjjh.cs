// Put this file in:  Assets/Editor/BirthdayGardenFbxBuilder.cs
// Then use the menu:  Tools > Birthday Garden (FBX) > Build Environment
//
// Requirements: the .fbx files from birthday_garden_FBX.zip (Models_FBX/) are imported into your
// project, anywhere under Assets/. The script finds them by file name - no setup needed.
//
// Ground is built here too: procedural grass is regenerated automatically; lawn lamps reuse the
// environment's existing lantern model and lights. No BirthdayGardenGround folder or ground_*.png files are needed.
// Walkway + groundcover: the stone path and the flower/grass groundcover elsewhere on the lawn now
// use the Ground_Props_12_FBX pack (Stepping_Stone_Large/Small, Stepping_Stones_Cluster,
// Flower_Bed_Wide, Flower_Bush_Pink, Flower_Bush_Pink_White, Flower_Bush_With_Rock, Grass_Flower_Tuft).
// Import those .fbx files anywhere under Assets/ as well - they're found by name the same way.
//
// Sign texts: the builder also writes the text from the reference image onto the models with
// TextMeshPro ("Party Area ->, Games, Memories, Messages, Surprises", the glowing "Happy Birthday",
// and "Good Friends Make Better Days" on the screen). Needs TMP Essentials once:
//   Window > TextMeshPro > Import TMP Essential Resources
// Menu "Add Sign Texts" re-applies the texts to objects already in your scene.
// Colliders: larger static props use their real meshes; small decorations are skipped.
// For an existing garden, use Tools > Birthday Garden (FBX) > Update Mesh Colliders.
//
// Photos: every light string (walkway path + plaza gallery) now has polaroid photos pegged on it.
// Density / size: PhotoGap, PathPhotoSize, GalleryPhotoSize, LaneRowPhotoSize in the photos section.

#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine.Video;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_RENDER_PIPELINE_URP
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
#endif

public static class BirthdayGardenFbxBuilder
{
    const string RootName = "BirthdayGarden";
    const string OutDir = "Assets/BirthdayGarden";

    // Light offsets are in model space (FBX is not mirrored on import, so MX = 1).
    // If the lantern-post light ends up on the wrong side of the post, change MX to -1f.
    const float MX = 1f;

    // Added to every model's yaw. If the WHOLE pack comes in facing the wrong way
    // (e.g. cake presents / signs on the back), set this to 180f.
    const float FrontYaw = 0f;
    const float LightBoost = 1.5f;
    const float PhotoStringX = 2.2f;
    const float ClearLampX = 3.05f;
    const float ClearLampZOffset = 0.65f;
    const float ImportedStringLift = 0.8f;
    const float ImportedStringZOffset = 0.9f;
    static readonly float[] PathLampStations = { -30f, -22f, -14f, -6f };
    static readonly Color WarmBulbHdr = new Color(3.2f, 1.45f, 0.12f, 1f);
    static readonly Color WarmLampColor = new Color(1f, 0.68f, 0.18f, 1f);
    // Only centre-walkway string bulbs use this near-white HDR value. It sits
    // just over the existing Bloom threshold for a small, restrained halo.
    static readonly Color CentreWalkwayWhiteHdr = new Color(1.20f, 1.20f, 1.20f, 1f);
    const string CentreWalkwayMaterialName = "GlowBulbWalkwaySoftWhite";

    // World-space metres. A whole model is measured so its thin legs/supports
    // stay solid, while separate small props do not get new colliders.
    const float ColliderMinObjectSize = 0.75f;
    const float ColliderMinObjectWidth = 0.15f;
    const float ColliderMinPostHeight = 1.2f;
    const float ColliderMinPostWidth = 0.08f;
    const float ColliderMinMeshSize = 0.20f;

    enum Col { None, Box, Capsule, Arch }

    struct LightDef
    {
        public Vector3 p; public string c; public float i, r;
        public LightDef(float x, float y, float z, string c, float i, float r) { p = new Vector3(x, y, z); this.c = c; this.i = i; this.r = r; }
    }

    // Suggested point lights (same data as manifest.json), in model-local space.
    static readonly Dictionary<string, LightDef[]> Lights = new Dictionary<string, LightDef[]>
    {
        { "campfire_lounge",     new[] { new LightDef(0, 0.9f, 0, "#ff8a30", 2.5f, 10f) } },
        { "cake_table",          new[] { new LightDef(0, 1.8f, 0, "#ffb860", 1.2f, 5f) } },
        { "fairy_tree",          new[] { new LightDef(0, 3.4f, 0, "#ffc27a", 1.6f, 10f) } },
        { "arch_happy_birthday", new[] { new LightDef(0, 2.35f, 0.8f, "#ffb870", 1.0f, 5f), new LightDef(0, 3.0f, 0.6f, "#ffc27a", 0.7f, 6f) } },
        { "arch_curtains",       new[] { new LightDef(0, 3.0f, 0.6f, "#ffc27a", 0.8f, 6f) } },
        { "screen_frame",        new[] { new LightDef(0, 1.7f, 1.0f, "#b79bff", 1.2f, 7f) } },
        { "lantern_post",        new[] { new LightDef(-0.62f, 2.4f, 0, "#ffa84a", 1.2f, 6f) } },
        { "lounge_cushions",     new[] { new LightDef(0.1f, 0.6f, -0.1f, "#ffa84a", 0.8f, 4f) } },
        { "picnic_table",        new[] { new LightDef(0, 1.0f, 0.1f, "#ffa84a", 1.0f, 4f) } },
        { "stone_block_ivy",     new[] { new LightDef(-0.35f, 1.4f, -0.1f, "#ffa84a", 0.8f, 4f) } },
        { "lantern",             new[] { new LightDef(0, 0.17f, 0, "#ffa84a", 0.8f, 4f) } },
        { "string_lights",       new[] { new LightDef(0, 2.2f, 0.2f, "#ffc27a", 0.6f, 5f) } },
    };

    static Dictionary<string, GameObject> cache;
    static Transform root;
    static List<Vector3> taken; // x = world x, y = world z, z = radius

    // ------------------------------------------------------------------ menu
    [MenuItem("Tools/Birthday Garden (FBX)/Build Environment")]
    public static void Build()
    {
        Clear();
        cache = new Dictionary<string, GameObject>();
        taken = new List<Vector3>();
        var rootGo = new GameObject(RootName);
        Undo.RegisterCreatedObjectUndo(rootGo, "Build Birthday Garden");
        root = rootGo.transform;

        MakeGround();
        SetupAtmosphere();
        Layout();
        AddTexts();
        BuildPhotoDecor();
        Scatter();
        ScatterGroundCover();
        BuildGroundLamps();
        MakeBulbsGlow();
        MakeFireflies();
        SetupCamera();
        SetupPostProcessing();
        ApplyWarmBulbsAndPhotoClearance();
        ApplyMeshColliders(root, false);
        SetupLightReveal();

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("Birthday Garden built. Press Play, or save the scene.");
    }

    [MenuItem("Tools/Birthday Garden (FBX)/Clear Environment")]
    public static void Clear()
    {
        var old = GameObject.Find(RootName);
        if (old != null) Object.DestroyImmediate(old);
        var oldGround = GameObject.Find("BirthdayGardenGround");
        if (oldGround != null) Object.DestroyImmediate(oldGround);
    }

    // Connect the finished scene to the runtime 6 / 8 / 10 second sequence.
    // Also available as a menu action for gardens that are already built.
    [MenuItem("Tools/Birthday Garden (FBX)/Setup Light Reveal")]
    public static void SetupLightReveal()
    {
        var garden = GameObject.Find(RootName);
        if (garden == null)
        {
            Debug.LogWarning("[BirthdayGarden] Build the environment before setting up its light reveal.");
            return;
        }
        var reveal = garden.GetComponent<GardenLightReveal>();
        if (reveal == null) reveal = Undo.AddComponent<GardenLightReveal>(garden);
        Undo.RecordObject(reveal, "Setup garden light reveal");
        foreach (var other in garden.GetComponentsInChildren<GardenLightReveal>(true))
            if (other != reveal) Undo.DestroyObjectImmediate(other);
        reveal.enabled = true;
        reveal.treeRevealSeconds = 6f;
        reveal.groundLanternDelaySeconds = 2f;
        reveal.remainingLightsDelaySeconds = 2f;
        reveal.lights = new List<GardenLightReveal.LightEntry>();
        reveal.bulbs = new List<GardenLightReveal.BulbEntry>();
        reveal.glows = new List<GardenLightReveal.GlowEntry>();

        var registeredLights = new HashSet<Light>();
        foreach (var light in garden.GetComponentsInChildren<Light>(true))
        {
            registeredLights.Add(light);
            reveal.lights.Add(new GardenLightReveal.LightEntry { light = light, z = light.transform.position.z });
        }
        // SetupAtmosphere may reuse a directional light outside the garden hierarchy.
        foreach (var light in Object.FindObjectsByType<Light>(FindObjectsSortMode.InstanceID))
        {
            if (light.type != LightType.Directional) continue;
            if (registeredLights.Add(light))
                reveal.lights.Add(new GardenLightReveal.LightEntry
                {
                    light = light, z = light.transform.position.z, group = GardenLightReveal.RevealGroup.Other
                });
            break;
        }

        var wholeObjects = new HashSet<GameObject>();
        foreach (var t in garden.GetComponentsInChildren<Transform>(true))
        {
            bool neonText = IsBirthdayNeonText(t);
            if (t.name != "Bulb" && t.name != "LanternPool" && t.name != "Fireflies" && !neonText) continue;
            wholeObjects.Add(t.gameObject);
            reveal.bulbs.Add(new GardenLightReveal.BulbEntry
            {
                bulb = t.gameObject, z = t.position.z,
                group = neonText || t.name == "Fireflies" ? GardenLightReveal.RevealGroup.Other : GardenLightReveal.RevealGroup.Auto
            });
        }

        foreach (var renderer in garden.GetComponentsInChildren<Renderer>(true))
        {
            if (wholeObjects.Contains(renderer.gameObject)) continue;
            var materials = renderer.sharedMaterials;
            for (int i = 0; i < materials.Length; i++)
            {
                var material = materials[i];
                if (material == null || !IsGardenGlowMaterial(material.name)) continue;
                // MakeBulbsGlow uses unlit colour materials, so their base colour is the visible glow.
                var property = material.HasProperty("_BaseColor") ? "_BaseColor" :
                               material.HasProperty("_Color") ? "_Color" : null;
                if (property == null) continue;
                reveal.glows.Add(new GardenLightReveal.GlowEntry
                {
                    renderer = renderer, materialIndex = i, onColor = material.GetColor(property),
                    z = renderer.bounds.center.z
                });
            }
        }

        EditorUtility.SetDirty(reveal);
        EditorSceneManager.MarkSceneDirty(garden.scene);
        Debug.Log($"[BirthdayGarden] Light reveal connected: {reveal.lights.Count} lights, {reveal.bulbs.Count} bulb/effect objects, {reveal.glows.Count} glow material slots. Press Play: trees at 6s, ground lanterns at 8s, all lights at 10s.");
    }

    static bool IsGardenGlowMaterial(string name)
    {
        name = name.ToLowerInvariant();
        return name.StartsWith("glowbulb") || name.StartsWith("glowlantern") ||
               name.StartsWith("glowflame") || name.StartsWith("glowember") ||
               name.StartsWith("partybulbglow");
    }

    static bool IsBirthdayNeonText(Transform text)
    {
        if (text.name != "BDayText") return false;
        for (var parent = text.parent; parent != null; parent = parent.parent)
            if (parent.name.StartsWith("arch_happy_birthday")) return true;
        return false;
    }

    // ------------------------------------------------------------------ layout
    // Coordinates: you spawn at (0,0,-36) and walk +Z along the stone path,
    // through the gateway arch, into the party plaza. The stage + cake are at the far end.
    static void Layout()
    {
        // path: individual stepping stones + lantern-lit flower border (Ground_Props_12_FBX pack)
        BuildWalkway();

        // lantern posts + string lights lining the path
        foreach (float z in PathLampStations)
        {
            Spawn("lantern_post", new Vector3(-ClearLampX, 0, z + ClearLampZOffset), 0, 1f, Col.Capsule, 0.3f, 0.12f, 2.9f);
            Spawn("lantern_post", new Vector3(ClearLampX, 0, z + ClearLampZOffset), 180, 1f, Col.Capsule, 0.3f, 0.12f, 2.9f);
        }
        foreach (float z in new[] { -22f, -6f }) Spawn("string_lights", new Vector3(0, ImportedStringLift, z + ImportedStringZOffset), 0, 1f, Col.None, 0f);

        // signposts (arrows point to +Z, toward the party)
        Spawn("signpost", new Vector3(-2.8f, 0, -33f), 90, 1f, Col.Capsule, 0.6f, 0.2f, 3.5f);
        Spawn("signpost", new Vector3(-3.2f, 0, 8f), 90, 1f, Col.Capsule, 0.6f, 0.2f, 3.5f);
        Spawn("cat_sign", new Vector3(5.5f, 0, -12f), 200, 1f, Col.Box, 1f);

        // gateway
        Spawn("arch_curtains", new Vector3(0, 0, 3.5f), 180, 1f, Col.Arch, 2f);

        // stage
        Spawn("arch_happy_birthday", new Vector3(0, 0, 31f), 180, 1f, Col.Arch, 2f);
        Spawn("cake_table", new Vector3(0, 0, 26.5f), 180, 1f, Col.Capsule, 1.8f, 1.2f, 1.6f);
        var screen = Spawn("screen_frame", ScreenPos, ScreenYaw, 1f, Col.Box, 3f);

        if (screen != null)
        {
            screen.transform.localScale = new Vector3(1f, 1.8f, 1f);
        }
        foreach (float x in new[] { -3.5f, 3.5f }) Spawn("balloons", new Vector3(x, 0, 24.2f), 0, 1f, Col.None, 0.5f);
        Spawn("balloons", new Vector3(-8f, 0, 18f), 0, 1f, Col.None, 0.5f);
        Spawn("balloons", new Vector3(8f, 0, 18f), 0, 1f, Col.None, 0.5f);

        EnsureStageGarland();   // the small polaroid garland by the screen (photos are filled in by BuildPhotoDecor)

        // lounge areas
        Spawn("campfire_lounge", new Vector3(-15f, 0, 14f), 0, 1f, Col.Capsule, 3f, 0.95f, 1.0f);
        Spawn("lounge_cushions", new Vector3(15f, 0, 13f), 0, 1f, Col.None, 3f);
        Spawn("picnic_table", new Vector3(14f, 0, 23f), 0, 1f, Col.Box, 2.5f);
        Spawn("bench", new Vector3(-8f, 0, 20f), 90, 1f, Col.Box, 1.5f);
        Spawn("bench", new Vector3(8f, 0, 9f), -90, 1f, Col.Box, 1.5f);
        Spawn("backpack", new Vector3(-11.5f, 0, 10.5f), 40, 1f, Col.None, 0.5f);
        Spawn("stone_block_ivy", new Vector3(14f, 0, 4f), 160, 1f, Col.Box, 1.8f);
        Spawn("stone_block_ivy", new Vector3(-14f, 0, 4f), 20, 1f, Col.Box, 1.8f);

        // flower pots
        foreach (var p in new[] { new Vector2(-2.5f, 29f), new Vector2(2.5f, 29f), new Vector2(-5f, 5f), new Vector2(5f, 5f), new Vector2(-10f, 7f), new Vector2(10f, 7f) })
            Spawn("flower_pot", new Vector3(p.x, 0, p.y), 0, 1f, Col.None, 0.5f);

        // hand-placed big trees
        foreach (var p in new[] { new Vector2(-22f, 22f), new Vector2(22f, 20f), new Vector2(-20f, 0f), new Vector2(20f, 0f),
                                  new Vector2(-10f, 38f), new Vector2(10f, 38f), new Vector2(0f, 42f) })
            Spawn("fairy_tree", new Vector3(p.x, 0, p.y), Random.Range(0f, 360f), 1f, Col.Capsule, 2.5f, 0.45f, 3f);
    }

    // ------------------------------------------------------------------ walkway (Ground_Props_12_FBX pack)
    // Builds the stone footpath itself: staggered large stepping stones down the middle, small stones
    // filling the gaps, a loose cluster here and there, and a low flower/grass border on both edges -
    // matching the lantern-lit stone path reference photo. Runs the same corridor as before: x in
    // roughly [-1.3, 1.3], z from -34 (entrance) to 0 (gateway).
    const float WalkwayZStart = -34f, WalkwayZEnd = 0f;

    static void BuildWalkway()
    {
        var rng = new System.Random(42);

        // staggered large stepping stones down the centre of the path
        bool left = true;
        for (float z = WalkwayZStart; z < WalkwayZEnd; z += 0.55f + (float)rng.NextDouble() * 0.25f)
        {
            float x = (left ? -0.35f : 0.35f) + (float)(rng.NextDouble() * 0.3 - 0.15);
            float yaw = (float)(rng.NextDouble() * 360);
            float scale = 0.9f + (float)rng.NextDouble() * 0.3f;
            Spawn("Stepping_Stone_Large", new Vector3(x, 0, z), yaw, scale, Col.None, 0.5f);
            left = !left;
        }

        // small stones filling the gaps between the large ones
        for (float z = WalkwayZStart; z < WalkwayZEnd; z += 0.7f)
        {
            if (rng.NextDouble() < 0.5) continue;
            float x = (float)(rng.NextDouble() * 1.6 - 0.8);
            Spawn("Stepping_Stone_Small", new Vector3(x, 0, z), (float)(rng.NextDouble() * 360), 0.8f + (float)rng.NextDouble() * 0.3f, Col.None, 0.3f);
        }

        // a few loose stone clusters at intervals along the path
        foreach (float z in new[] { -28f, -16f, -4f })
            Spawn("Stepping_Stones_Cluster", new Vector3((float)(rng.NextDouble() * 1.0 - 0.5), 0, z), (float)(rng.NextDouble() * 360), 1f, Col.None, 0.6f);

        // low flowers and grass tufts lining both edges of the path
        for (float z = WalkwayZStart; z < WalkwayZEnd; z += 0.9f)
            foreach (float sx in new[] { -1.1f, 1.1f })
            {
                if (rng.NextDouble() < 0.35) continue;
                float x = sx + (float)(rng.NextDouble() * 0.3 - 0.15);
                if (rng.NextDouble() < 0.6)
                    Spawn("Grass_Flower_Tuft", new Vector3(x, 0, z), (float)(rng.NextDouble() * 360), 0.8f + (float)rng.NextDouble() * 0.4f, Col.None, 0.2f);
                else
                    Spawn("Flower_Bed_Wide", new Vector3(x, 0, z), (float)(rng.NextDouble() * 360), 0.7f + (float)rng.NextDouble() * 0.3f, Col.None, 0.3f);
            }
    }

    // low groundcover (dirt patches of grass + pink flowers) scattered across the rest of the lawn,
    // away from the walkway corridor and the plaza - matching the flower/dirt ground reference photo.
    static void ScatterGroundCover()
    {
        var rng = new System.Random(777);
        System.Func<double, double, float> R = (a, b) => (float)(a + rng.NextDouble() * (b - a));
        string[] beds = { "Flower_Bed_Wide", "Flower_Bush_Pink", "Flower_Bush_Pink_White", "Flower_Bush_With_Rock" };
        for (int n = 0, tries = 0; n < 140 && tries < 1400; tries++)
        {
            float x = R(-45, 45), z = R(-38, 46);
            if (Mathf.Abs(x) < 2.0f && z > WalkwayZStart - 1f && z < WalkwayZEnd + 1f) continue; // keep the walkway clear
            if (!Free(x, z, 0.6f)) continue;
            if (rng.NextDouble() < 0.55)
                Spawn("Grass_Flower_Tuft", new Vector3(x, 0, z), R(0, 360), R(0.8, 1.3), Col.None, 0.3f);
            else
                Spawn(beds[rng.Next(beds.Length)], new Vector3(x, 0, z), R(0, 360), R(0.8, 1.2), Col.None, 0.5f);
            n++;
        }
    }

    // random bushes, rocks and a ring of fairy-light trees
    static void Scatter()
    {
        var rng = new System.Random(1234);
        System.Func<double, double, float> R = (a, b) => (float)(a + rng.NextDouble() * (b - a));

        // perimeter trees
        for (int i = 0; i < 24; i++)
        {
            float a = i / 24f * Mathf.PI * 2f + R(-0.1, 0.1);
            float x = Mathf.Cos(a) * 42f + R(-3, 3), z = 4f + Mathf.Sin(a) * 44f + R(-3, 3);
            if (Mathf.Abs(x) < 5f && z < -30f) continue; // keep the entrance open
            Spawn("fairy_tree", new Vector3(x, 0, z), R(0, 360), R(0.9, 1.25), Col.Capsule, 2.5f, 0.45f, 3f);
        }

        string[] bushes = { "bush_flowers_large", "bush_flowers_medium", "bush_flowers_small", "bush_flowers_medium", "bush_flowers_small" };
        for (int n = 0, tries = 0; n < 60 && tries < 600; tries++)
        {
            float x = R(-40, 40), z = R(-38, 46);
            if (!Free(x, z, 1.5f)) continue;
            Spawn(bushes[rng.Next(bushes.Length)], new Vector3(x, 0, z), R(0, 360), R(0.9, 1.3), Col.None, 1.2f);
            n++;
        }
        string[] rocks = { "rocks_pair", "rocks_cluster" };
        for (int n = 0, tries = 0; n < 26 && tries < 600; tries++)
        {
            float x = R(-40, 40), z = R(-38, 46);
            if (!Free(x, z, 1.2f)) continue;
            Spawn(rocks[rng.Next(rocks.Length)], new Vector3(x, 0, z), R(0, 360), R(0.9, 1.5), Col.Box, 1.0f);
            n++;
        }
    }

    static bool Free(float x, float z, float margin)
    {
        if (Mathf.Abs(x) < 4.5f && z < 5f) return false;                        // path corridor
        if (Vector2.Distance(new Vector2(x, z), new Vector2(0, 16)) < 7.5f) return false; // plaza walking area
        foreach (var t in taken)
            if (Vector2.Distance(new Vector2(x, z), new Vector2(t.x, t.y)) < t.z + margin) return false;
        return true;
    }

    // ------------------------------------------------------------------ spawning
    static GameObject Find(string name)
    {
        if (cache.TryGetValue(name, out var cached)) return cached;
        GameObject found = null;
        foreach (var guid in AssetDatabase.FindAssets(name + " t:GameObject"))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (Path.GetFileNameWithoutExtension(path) == name) { found = AssetDatabase.LoadAssetAtPath<GameObject>(path); break; }
        }
        if (found == null) Debug.LogWarning($"[BirthdayGarden] model '{name}' not found - did you import {name}.glb?");
        cache[name] = found;
        return found;
    }

    static GameObject Spawn(string name, Vector3 pos, float yaw = 0, float scale = 1f, Col col = Col.None, float radius = 0.5f,
                            float capR = 0.5f, float capH = 2f)
    {
        var prefab = Find(name);
        if (prefab == null) return null;
        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, root);
        go.name = name;
        go.transform.SetPositionAndRotation(pos, Quaternion.Euler(0, yaw + FrontYaw, 0));
        go.transform.localScale = Vector3.one * scale;

        if (Lights.TryGetValue(name, out var defs))
            foreach (var d in defs)
            {
                var lg = new GameObject("Light_" + name);
                lg.transform.SetParent(go.transform, false);
                lg.transform.localPosition = new Vector3(MX * d.p.x, d.p.y, d.p.z);
                var l = lg.AddComponent<Light>();
                l.type = LightType.Point;
                l.shadows = LightShadows.None;
                l.range = d.r * scale;
                l.intensity = d.i * LightBoost;
                if (ColorUtility.TryParseHtmlString(d.c, out var c)) l.color = c;
            }

        // Collision is added after the layout is finished, using the actual
        // child meshes. Keep the old layout arguments for placement compatibility.

        foreach (var t in go.GetComponentsInChildren<Transform>())
            GameObjectUtility.SetStaticEditorFlags(t.gameObject, StaticEditorFlags.BatchingStatic | StaticEditorFlags.OccludeeStatic);

        taken.Add(new Vector3(pos.x, pos.z, radius * scale));
        return go;
    }

    // ------------------------------------------------------- mesh collision
    [MenuItem("Tools/Birthday Garden (FBX)/Update Mesh Colliders")]
    public static void UpdateMeshColliders()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogWarning("[BirthdayGarden] Stop Play mode before updating colliders.");
            return;
        }

        var scene = SceneManager.GetActiveScene();
        Transform garden = null;
        foreach (var sceneRoot in scene.GetRootGameObjects())
            if (sceneRoot.name == RootName) { garden = sceneRoot.transform; break; }
        if (garden == null)
        {
            Debug.LogWarning("[BirthdayGarden] No BirthdayGarden root in the active scene.");
            return;
        }

        Undo.IncrementCurrentGroup();
        int undoGroup = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Update Birthday Garden Mesh Colliders");
        try
        {
            ApplyMeshColliders(garden, true);
            EditorSceneManager.MarkSceneDirty(scene);
        }
        finally { Undo.CollapseUndoOperations(undoGroup); }
    }

    sealed class CollisionModel
    {
        public readonly List<MeshFilter> meshes = new List<MeshFilter>();
        public readonly List<Collider> oldSolids = new List<Collider>();
        public Bounds bounds;
        public bool hasBounds;
    }

    static void ApplyMeshColliders(Transform garden, bool recordUndo)
    {
        var models = new Dictionary<Transform, CollisionModel>();
        int added = 0, removed = 0, retained = 0, skipped = 0, failed = 0;
        foreach (var filter in garden.GetComponentsInChildren<MeshFilter>(true))
        {
            if (filter.sharedMesh == null || filter.GetComponent<MeshRenderer>() == null ||
                IgnoreCollisionDecoration(filter.transform, garden)) { skipped++; continue; }
            Transform owner = CollisionOwner(filter.transform, garden);
            if (!models.TryGetValue(owner, out var model))
                models.Add(owner, model = new CollisionModel());
            model.meshes.Add(filter);
            Bounds bounds = CollisionWorldBounds(filter);
            if (!model.hasBounds) { model.bounds = bounds; model.hasBounds = true; }
            else model.bounds.Encapsulate(bounds);
        }

        foreach (var collider in garden.GetComponentsInChildren<Collider>(true))
        {
            if (collider.isTrigger || !(collider is BoxCollider || collider is CapsuleCollider || collider is SphereCollider))
                continue;
            Transform owner = CollisionOwner(collider.transform, garden);
            if (models.TryGetValue(owner, out var model)) model.oldSolids.Add(collider);
        }

        foreach (var entry in models)
        {
            Transform owner = entry.Key;
            CollisionModel model = entry.Value;
            // Static concave meshes must not replace character/dynamic or animated physics.
            if (!LargeCollisionObject(model.bounds.size) || ProtectedCollisionModel(owner, garden))
            { skipped += model.meshes.Count; continue; }

            bool complete = true;
            int coveredMeshes = 0;
            foreach (var filter in model.meshes)
            {
                if (ProtectedCollisionBranch(filter.transform, garden)) { skipped++; continue; }
                Vector3 size = CollisionWorldBounds(filter).size;
                // Ignore separate tiny bulb/peg meshes even inside a larger model.
                if (Mathf.Max(size.x, Mathf.Max(size.y, size.z)) < ColliderMinMeshSize)
                { skipped++; continue; }
                Mesh mesh = filter.sharedMesh;
                if (!CollisionHasTriangles(mesh) || Mathf.Abs(filter.transform.localToWorldMatrix.determinant) < 0.000001f)
                { complete = false; failed++; continue; }

                var existing = filter.GetComponents<MeshCollider>();
                MeshCollider target = null;
                foreach (var candidate in existing)
                    if (!candidate.isTrigger && candidate.sharedMesh == mesh) { target = candidate; break; }
                Collider source = CollisionSource(filter.transform, owner, model, garden);
                if (target != null && !target.convex && source == null) { retained++; coveredMeshes++; continue; }

                bool created = target == null;
                try
                {
                    if (created)
                        target = recordUndo ? Undo.AddComponent<MeshCollider>(filter.gameObject) : filter.gameObject.AddComponent<MeshCollider>();
                    else if (recordUndo) Undo.RecordObject(target, "Use Actual Garden Mesh");

                    if (source != null)
                    {
                        // Apply these to reused meshes too: a disabled old mesh must
                        // not replace an enabled box and leave the model non-solid.
                        target.sharedMaterial = source.sharedMaterial;
                        target.contactOffset = source.contactOffset;
                        target.enabled = source.enabled || (!created && target.enabled);
                    }
                    target.convex = false; // Keep archways, gaps and spaces between table legs open.
                    if (target.sharedMesh != mesh) target.sharedMesh = mesh;
                    if (PrefabUtility.IsPartOfPrefabInstance(target))
                        PrefabUtility.RecordPrefabInstancePropertyModifications(target);
                    if (created) added++; else retained++;
                    coveredMeshes++;
                }
                catch (System.Exception error)
                {
                    complete = false; failed++;
                    if (created && target != null)
                    {
                        if (recordUndo) Undo.DestroyObjectImmediate(target); else Object.DestroyImmediate(target);
                    }
                    Debug.LogWarning("[BirthdayGarden] Could not create mesh collision on " + filter.name + ": " + error.Message, filter);
                }
            }

            // Do not discard the original protection when a model has unsupported geometry.
            if (!complete || coveredMeshes == 0) continue;
            foreach (var old in model.oldSolids)
            {
                if (old == null || ProtectedCollisionBranch(old.transform, garden)) continue;
                if (recordUndo) Undo.DestroyObjectImmediate(old); else Object.DestroyImmediate(old);
                removed++;
            }
        }
        Debug.Log($"[BirthdayGarden] Mesh colliders: {added} added, {retained} reused, {removed} old box/capsule/sphere colliders replaced. " +
            $"{skipped} small/decorative/protected meshes skipped, {failed} unsupported meshes. Save the scene.");
    }

    static Collider CollisionSource(Transform part, Transform owner, CollisionModel model, Transform garden)
    {
        Collider disabled = null;
        for (Transform current = part; current != null; current = current.parent)
        {
            foreach (var old in model.oldSolids)
            {
                if (old.transform != current || ProtectedCollisionBranch(current, garden)) continue;
                if (old.enabled) return old;
                if (disabled == null) disabled = old;
            }
            if (current == owner) break;
        }
        // Some imported models put their old collider on a separate child.
        foreach (var old in model.oldSolids)
            if (old.enabled && !ProtectedCollisionBranch(old.transform, garden)) return old;
        return disabled;
    }

    static Transform CollisionOwner(Transform part, Transform garden)
    {
        var prefabRoot = PrefabUtility.GetOutermostPrefabInstanceRoot(part.gameObject);
        if (prefabRoot != null && prefabRoot.transform != garden && prefabRoot.transform.IsChildOf(garden))
            return prefabRoot.transform;
        Transform top = part;
        for (Transform current = part; current != null && current != garden; current = current.parent)
        {
            if (current.name == "PhotoWall" || current.name == "PhotoStand" || current.name == "GarlandPole") return current;
            top = current;
        }
        // These are containers of independent props, not one large physical object.
        return top.name == "PhotoDecor" || top.name == "GroundLamps" ? part : top;
    }

    static bool LargeCollisionObject(Vector3 size)
    {
        float longest = Mathf.Max(size.x, Mathf.Max(size.y, size.z));
        float middle = Mathf.Max(Mathf.Min(size.x, size.y), Mathf.Min(Mathf.Max(size.x, size.y), size.z));
        return (longest >= ColliderMinObjectSize && middle >= ColliderMinObjectWidth) ||
            (size.y >= ColliderMinPostHeight && Mathf.Max(size.x, size.z) >= ColliderMinPostWidth);
    }

    static Bounds CollisionWorldBounds(MeshFilter filter)
    {
        Bounds local = filter.sharedMesh.bounds;
        var matrix = filter.transform.localToWorldMatrix;
        Vector3 x = matrix.MultiplyVector(new Vector3(local.extents.x, 0, 0));
        Vector3 y = matrix.MultiplyVector(new Vector3(0, local.extents.y, 0));
        Vector3 z = matrix.MultiplyVector(new Vector3(0, 0, local.extents.z));
        return new Bounds(matrix.MultiplyPoint3x4(local.center), new Vector3(
            Mathf.Abs(x.x) + Mathf.Abs(y.x) + Mathf.Abs(z.x),
            Mathf.Abs(x.y) + Mathf.Abs(y.y) + Mathf.Abs(z.y),
            Mathf.Abs(x.z) + Mathf.Abs(y.z) + Mathf.Abs(z.z)) * 2f);
    }

    static bool CollisionHasTriangles(Mesh mesh)
    {
        if (mesh.vertexCount < 3) return false;
        for (int sub = 0; sub < mesh.subMeshCount; sub++)
            if (mesh.GetTopology(sub) == MeshTopology.Triangles && mesh.GetIndexCount(sub) >= 3) return true;
        return false;
    }

    static bool IgnoreCollisionDecoration(Transform part, Transform garden)
    {
        for (Transform current = part; current != null && current != garden; current = current.parent)
        {
            string name = current.name.ToLowerInvariant();
            if (current.GetComponent<TMP_Text>() != null || current.GetComponent<ParticleSystem>() != null ||
                name.Contains("bulb") || name.Contains("photoquad") || name.Contains("lanternpool") ||
                name.Contains("firework") || name.Contains("firefl") || name.Contains("flame") ||
                name.Contains("glow") || name.Contains("spark") || name == "cord" ||
                name.Contains("string_lights") || name.Contains("grass_flower_tuft")) return true;
        }
        return false;
    }

    static bool ProtectedCollisionModel(Transform owner, Transform garden)
    {
        return ProtectedCollisionBranch(owner, garden) || owner.GetComponentInChildren<Rigidbody>(true) != null ||
            owner.GetComponentInChildren<ArticulationBody>(true) != null || owner.GetComponentInChildren<CharacterController>(true) != null ||
            owner.GetComponentInChildren<Animator>(true) != null || owner.GetComponentInChildren<Animation>(true) != null ||
            owner.GetComponentInChildren<SkinnedMeshRenderer>(true) != null;
    }

    static bool ProtectedCollisionBranch(Transform part, Transform garden)
    {
        for (Transform current = part; current != null; current = current.parent)
        {
            if (current.GetComponent<Rigidbody>() != null || current.GetComponent<ArticulationBody>() != null ||
                current.GetComponent<CharacterController>() != null || current.GetComponent<Animator>() != null ||
                current.GetComponent<Animation>() != null || current.GetComponent<SkinnedMeshRenderer>() != null) return true;
            foreach (var collider in current.GetComponents<Collider>()) if (collider.isTrigger) return true;
            if (current == garden) break;
        }
        return false;
    }


    // ------------------------------------------------------------------ sign texts (TextMeshPro)
    // Set to true to write "Good Friends Make Better Days" on the cinema screen again.
    static readonly bool ScreenShowText = false;

    static readonly Color Cream = new Color(0.96f, 0.89f, 0.78f);
    static readonly Color Neon = new Color(1f, 0.82f, 0.55f);

    [MenuItem("Tools/Birthday Garden (FBX)/Add Sign Texts")]
    public static void AddTexts()
    {
        if (TMP_Settings.instance == null || TMP_Settings.defaultFontAsset == null)
        {
            Debug.LogError("[BirthdayGarden] TextMeshPro has no default font. Run Window > TextMeshPro > Import TMP Essential Resources, then try again.");
            return;
        }
        string arrow = Glyph('\u2192', "->"), heart = Glyph('\u2665', "<3");
        int n = 0;
        foreach (var tr in Object.FindObjectsByType<Transform>(FindObjectsSortMode.InstanceID))
        {
            if (tr.name.StartsWith("signpost")) n += DoSignpost(tr, arrow);
            else if (tr.name.StartsWith("arch_happy_birthday")) n += DoNeonSign(tr, heart);
            else if (tr.name.StartsWith("screen_frame")) n += DoScreen(tr, heart);
        }
        if (n == 0) Debug.LogWarning("[BirthdayGarden] No sign/arch/screen objects found in the scene to add text to.");
        else Debug.Log($"[BirthdayGarden] Added {n} text labels.");
    }

    static string Glyph(char c, string fallback)
    {
        var f = TMP_Settings.defaultFontAsset;
        return (f != null && f.HasCharacter(c)) ? c.ToString() : fallback;
    }

    static void ClearTexts(Transform t)
    {
        var old = new List<GameObject>();
        foreach (var c in t.GetComponentsInChildren<Transform>(true)) if (c.name == "BDayText") old.Add(c.gameObject);
        foreach (var g in old) Object.DestroyImmediate(g);
    }

    static List<Renderer> FindQuads(Transform t, string pattern)
    {
        var res = new List<Renderer>();
        foreach (var r in t.GetComponentsInChildren<Renderer>(true))
            if (Regex.IsMatch(r.name, pattern)) res.Add(r);
        return res;
    }

    // Reads a flat quad's centre, outward normal and size, whatever way the FBX importer rotated it.
    static bool QuadInfo(Renderer r, out Vector3 c, out Vector3 nrm, out float w, out float h)
    {
        c = nrm = Vector3.zero; w = h = 0;
        var mf = r.GetComponent<MeshFilter>();
        if (mf == null || mf.sharedMesh == null) return false;
        var m = mf.sharedMesh; var t = r.transform; var b = m.bounds;
        var ln = m.normals.Length > 0 ? m.normals[0] : Vector3.forward;
        nrm = t.TransformDirection(ln).normalized;
        c = t.TransformPoint(b.center);
        var s = Vector3.Scale(b.size, t.lossyScale);
        int thin = 0;
        for (int i = 1; i < 3; i++) if (s[i] < s[thin]) thin = i;
        int vert = -1; float best = -1f;
        for (int i = 0; i < 3; i++)
        {
            if (i == thin) continue;
            var axis = i == 0 ? Vector3.right : i == 1 ? Vector3.up : Vector3.forward;
            float sc = Mathf.Abs(Vector3.Dot(t.TransformDirection(axis), Vector3.up));
            if (sc > best) { best = sc; vert = i; }
        }
        int hor = 3 - thin - vert;
        w = s[hor]; h = s[vert];
        return true;
    }

    // cx, cy = offset from the quad centre as a fraction of width / height; wf, hf = box size as a fraction
    static void Label(Transform parent, Renderer quad, string txt, float cx, float cy, float wf, float hf,
                      Color col, TextAlignmentOptions al, FontStyles st)
    {
        if (!QuadInfo(quad, out var c, out var nrm, out var w, out var h)) return;
        var fwd = -nrm;
        var right = Vector3.Cross(Vector3.up, fwd).normalized;
        var go = new GameObject("BDayText");
        go.transform.SetParent(parent, true);
        go.transform.position = c + right * (cx * w) + Vector3.up * (cy * h) + nrm * 0.012f;
        go.transform.rotation = Quaternion.LookRotation(fwd, Vector3.up);
        var t = go.AddComponent<TextMeshPro>();
        t.text = txt; t.color = col; t.alignment = al; t.fontStyle = st;
        t.enableAutoSizing = true; t.fontSizeMin = 0.1f; t.fontSizeMax = 50f;
        t.textWrappingMode = TextWrappingModes.NoWrap; t.overflowMode = TextOverflowModes.Truncate;
        t.rectTransform.sizeDelta = new Vector2(wf * w, hf * h);
    }

    static int DoSignpost(Transform root_, string arrow)
    {
        ClearTexts(root_);
        string[] labels = { "Party Area", "Games", "Memories", "Messages", "Surprises" };
        var board = Mat("SignBoard", new Color(0.47f, 0.26f, 0.15f), "Universal Render Pipeline/Lit", "Standard");
        int n = 0;
        foreach (var q in FindQuads(root_, @"sign_\d"))
        {
            int idx = int.Parse(Regex.Match(q.name, @"sign_(\d)").Groups[1].Value);
            if (idx >= labels.Length) continue;
            q.sharedMaterial = board; // plain wood behind the text
            Label(root_, q, labels[idx], -0.06f, 0f, 0.62f, 0.62f, Cream, TextAlignmentOptions.MidlineLeft, FontStyles.Normal);
            Label(root_, q, arrow, 0.40f, 0f, 0.14f, 0.55f, Cream, TextAlignmentOptions.Midline, FontStyles.Bold);
            n += 2;
        }
        return n;
    }

    static int DoNeonSign(Transform root_, string heart)
    {
        ClearTexts(root_);
        var board = Mat("NeonBoard", new Color(0.26f, 0.14f, 0.08f), "Universal Render Pipeline/Lit", "Standard");
        int n = 0;
        foreach (var q in FindQuads(root_, @"sign_front"))
        {
            q.sharedMaterial = board;
            Label(root_, q, "Happy", -0.12f, 0.2f, 0.55f, 0.46f, Neon, TextAlignmentOptions.MidlineLeft, FontStyles.Italic);
            Label(root_, q, "Birthday", 0.0f, -0.2f, 0.82f, 0.52f, Neon, TextAlignmentOptions.Midline, FontStyles.Italic);
            Label(root_, q, heart, 0.38f, 0.26f, 0.14f, 0.28f, Neon, TextAlignmentOptions.Midline, FontStyles.Normal);
            n += 3;
        }
        return n;
    }

    static int DoScreen(Transform root_, string heart)
    {
        ClearTexts(root_);
        var bg = Mat("ScreenNight", Color.white, "Universal Render Pipeline/Unlit", "Unlit/Texture");
        var tex = MakeScreenBackground();
        bg.mainTexture = tex;
        if (bg.HasProperty("_BaseMap")) bg.SetTexture("_BaseMap", tex);
        EditorUtility.SetDirty(bg);
        int n = 0;
        foreach (var q in FindQuads(root_, @"^screen"))
        {
            if (q.name.ToLower().Contains("frame")) continue;
            q.sharedMaterial = bg;
            if (ScreenShowText)
                Label(root_, q, "Good Friends\nMake\nBetter Days " + heart, -0.2f, 0.12f, 0.5f, 0.6f,
                      new Color(0.93f, 0.87f, 0.98f), TextAlignmentOptions.MidlineLeft, FontStyles.Italic);
            n++;
        }
        return n;
    }


    // ------------------------------------------------------------------ video on the cinema screen
    [MenuItem("Tools/Birthday Garden (FBX)/Setup Screen Video (select a video in Project first)")]
    public static void SetupScreenVideo()
    {
        var clip = Selection.activeObject as VideoClip;
        if (clip == null)
        {
            Debug.LogError("[BirthdayGarden] Click your video file (.mp4) in the Project window first, then run this menu item again.");
            return;
        }
        int n = 0;
        foreach (var tr in Object.FindObjectsByType<Transform>(FindObjectsSortMode.InstanceID))
        {
            if (!tr.name.StartsWith("screen_frame")) continue;
            ClearTexts(tr); // removes the text labels on the screen
            foreach (var q in FindQuads(tr, @"^screen"))
            {
                if (q.name.ToLower().Contains("frame")) continue;
                var vp = q.GetComponent<VideoPlayer>();
                if (vp == null) vp = q.gameObject.AddComponent<VideoPlayer>();
                var audio = q.GetComponent<AudioSource>();
                if (audio == null) audio = q.gameObject.AddComponent<AudioSource>();

                vp.source = VideoSource.VideoClip;
                vp.clip = clip;
                vp.playOnAwake = true;
                vp.isLooping = true;
                vp.renderMode = VideoRenderMode.MaterialOverride;
                vp.targetMaterialRenderer = q;
                vp.targetMaterialProperty = (q.sharedMaterial != null && q.sharedMaterial.HasProperty("_BaseMap")) ? "_BaseMap" : "_MainTex";
                vp.audioOutputMode = VideoAudioOutputMode.AudioSource;
                vp.SetTargetAudioSource(0, audio);

                audio.playOnAwake = false;
                audio.spatialBlend = 1f;      // louder when you walk up to the screen
                audio.minDistance = 4f;
                audio.maxDistance = 35f;
                EditorUtility.SetDirty(q.gameObject);
                n++;
            }
        }
        if (n == 0) Debug.LogWarning("[BirthdayGarden] No cinema screen found in the scene. Build the environment first.");
        else
        {
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("[BirthdayGarden] Video set up on the screen. Press Play.");
        }
    }

    // night-sky gradient + mountains + stars + tiny town lights (the picture behind the screen text)
    static Texture2D MakeScreenBackground()
    {
        EnsureFolder();
        const int W = 512, H = 288;
        var tex = new Texture2D(W, H, TextureFormat.RGBA32, false);
        var rng = new System.Random(5);
        var top = new Color(0.14f, 0.1f, 0.36f); var mid = new Color(0.55f, 0.35f, 0.7f); var low = new Color(0.98f, 0.62f, 0.5f);
        for (int y = 0; y < H; y++)
        {
            float t = y / (float)(H - 1); // 0 bottom .. 1 top
            var sky = t > 0.45f ? Color.Lerp(mid, top, (t - 0.45f) / 0.55f) : Color.Lerp(low, mid, t / 0.45f);
            for (int x = 0; x < W; x++) tex.SetPixel(x, y, sky);
        }
        Color[] mtn = { new Color(0.32f, 0.22f, 0.5f), new Color(0.22f, 0.15f, 0.38f), new Color(0.14f, 0.1f, 0.28f) };
        for (int k = 0; k < 3; k++)
        {
            float baseY = H * (0.34f - 0.09f * k), ph = k * 1.7f;
            for (int x = 0; x < W; x++)
            {
                float u = x / (float)W;
                float ym = baseY + H * 0.2f * (0.6f + 0.4f * Mathf.Sin(u * 5f + ph)) * Mathf.Pow(0.5f + 0.5f * Mathf.Sin(u * 11f + ph * 2f), 1.3f);
                for (int y = 0; y < Mathf.Min(H, (int)ym); y++) tex.SetPixel(x, y, mtn[k]);
            }
        }
        for (int i = 0; i < 90; i++) tex.SetPixel(rng.Next(W), (int)(H * (0.55 + 0.45 * rng.NextDouble())) % H, new Color(1f, 0.96f, 0.85f));
        for (int i = 0; i < 40; i++) tex.SetPixel(rng.Next(W), rng.Next(2, 30), new Color(1f, 0.75f, 0.4f));
        tex.Apply();
        var path = OutDir + "/ScreenNight.png";
        File.WriteAllBytes(path, tex.EncodeToPNG());
        AssetDatabase.ImportAsset(path);
        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }


    // ------------------------------------------------------------------ photos: garlands + photo wall
    // Put your framed_photo_*.png files anywhere in Assets/. Every texture whose name starts with
    // "framed_photo" is used, cycled over all garlands and the wall.
    static readonly Vector3 ScreenPos = new Vector3(-13f, 0, 28f);
    const float ScreenYaw = 195f;

    // photos pegged on the light strings
    const float PhotoGap = 1.1f;            // metres of string per photo (smaller = more photos)
    const float PathPhotoSize = 0.55f;      // photos on the walkway strings
    const float GalleryPhotoSize = 0.45f;   // photos on the plaza strings beside the lane
    const float LaneRowPhotoSize = 0.36f;   // photos on the strings you walk under (kept small so they stay above head height)

    static int PhotoCount(float len) { return Mathf.Max(1, Mathf.RoundToInt(len / PhotoGap)); }

    // the polaroid card inside the 1024x1024 png (transparent margin around it is cropped away)
    const float CropU0 = 86f / 1024f, CropV0 = 85f / 1024f, CropSize = 853f / 1024f;

    static List<Material> photoMats, photoOrder;
    static int photoCursor;
    static Mesh bulbMesh;
    static Material bulbMat, ropeMat, pegMat;
    static Material[] plankMats;
    static List<Material> picMats;      // picture-only crop (for the polaroid models)
    static int picCursor;
    // the picture area inside the polaroid card (cropped to the garland polaroid's aspect ratio)
    const float PicU0 = 166f / 1024f, PicV0 = 240f / 1024f, PicW = 691f / 1024f, PicH = 646f / 1024f;

    [MenuItem("Tools/Birthday Garden (FBX)/Add Photo Garlands and Wall")]
    public static void AddPhotos()
    {
        var r = GameObject.Find(RootName);
        if (r == null) r = new GameObject(RootName);
        root = r.transform;
        if (cache == null) cache = new Dictionary<string, GameObject>();
        if (taken == null) taken = new List<Vector3>();
        BuildPhotoDecor();
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
    }

    static void BuildPhotoDecor()
    {
        var old = GameObject.Find("PhotoDecor");
        if (old != null) Object.DestroyImmediate(old);
        if (!LoadPhotoMats()) return;

        bulbMesh = BulbMesh();
        // unlit + HDR colour: every bulb is bright on its own and picks up the Bloom glow
        bulbMat = Mat("PartyBulbGlow", WarmBulbHdr, "Universal Render Pipeline/Unlit", "Unlit/Color");
        ropeMat = Mat("PartyRope", new Color(0.22f, 0.15f, 0.09f), "Universal Render Pipeline/Unlit", "Sprites/Default");
        pegMat = Mat("PartyPeg", new Color(0.7f, 0.5f, 0.3f), "Universal Render Pipeline/Lit", "Standard");
        plankMats = new[]
        {
            Mat("WallPlankA", new Color(0.30f, 0.18f, 0.10f), "Universal Render Pipeline/Lit", "Standard"),
            Mat("WallPlankB", new Color(0.24f, 0.14f, 0.08f), "Universal Render Pipeline/Lit", "Standard"),
            Mat("WallPlankC", new Color(0.35f, 0.21f, 0.12f), "Universal Render Pipeline/Lit", "Standard"),
        };

        var decor = new GameObject("PhotoDecor");
        decor.transform.SetParent(root, false);
        var d = decor.transform;

        // the small polaroid garland by the screen: put photos on its empty polaroids
        EnsureStageGarland();
        FillGarlandModels();

        // 1) walkway path: light strings WITH photos - across the path ...
        float[] postZ = { -30f, -22f, -14f, -6f };
        foreach (float z in postZ)
        {
            var a = new Vector3(-2.2f, 2.9f, z); var b = new Vector3(2.2f, 2.9f, z);
            Garland(d, a, b, 0.45f, PhotoCount((b - a).magnitude), PathPhotoSize, 0);
        }
        // ... and along both sides, post to post
        for (int i = 0; i < postZ.Length - 1; i++)
            foreach (float x in new[] { -2.2f, 2.2f })
            {
                var a = new Vector3(x, 3.0f, postZ[i]); var b = new Vector3(x, 3.0f, postZ[i + 1]);
                Garland(d, a, b, 0.6f, PhotoCount((b - a).magnitude), PathPhotoSize, 0);
            }

        // 2) photo gallery in the middle of the plaza
        BuildGallery(d);

        // 3) photo wall next to the cinema screen
        Vector3 f = Quaternion.Euler(0, ScreenYaw, 0) * Vector3.forward;
        Vector3 viewerRight = Vector3.Cross(Vector3.up, -f).normalized;
        BuildPhotoWall(d, ScreenPos + viewerRight * 6.4f, ScreenYaw);
        Debug.Log("[BirthdayGarden] Photos added (" + photoMats.Count + " photos in rotation).");
    }

    // the original small garland by the screen + its two poles (only created if missing)
    static void EnsureStageGarland()
    {
        if (root == null) return;
        if (root.Find("photo_garland") == null)
            Spawn("photo_garland", new Vector3(12f, 0, 27f), 180, 1f, Col.None, 2.5f);
        if (root.Find("GarlandPole") == null)
        {
            var wood = Mat("PartyWood", new Color(0.3f, 0.17f, 0.09f), "Universal Render Pipeline/Lit", "Standard");
            foreach (float dx in new[] { -2.15f, 2.15f })
            {
                var pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pole.name = "GarlandPole";
                pole.transform.SetParent(root);
                pole.transform.position = new Vector3(12f + dx, 1.4f, 27f);
                pole.transform.localScale = new Vector3(0.14f, 1.4f, 0.14f);
                pole.GetComponent<Renderer>().sharedMaterial = wood;
            }
        }
    }

    // swaps the empty blue polaroid pictures of every photo_garland in the scene for your photos
    static void FillGarlandModels()
    {
        int n = 0;
        foreach (var tr in Object.FindObjectsByType<Transform>(FindObjectsSortMode.InstanceID))
        {
            if (!tr.name.StartsWith("photo_garland")) continue;
            foreach (var r in tr.GetComponentsInChildren<Renderer>(true))
            {
                if (!Regex.IsMatch(r.name, @"^photo_\d")) continue;
                r.sharedMaterial = NextPic();
                EditorUtility.SetDirty(r);
                n++;
            }
        }
        if (n == 0) Debug.LogWarning("[BirthdayGarden] No polaroid pictures found on the photo_garland model.");
    }

    static bool LoadPhotoMats()
    {
        photoMats = new List<Material>();
        picMats = new List<Material>();
        var paths = new List<string>();
        foreach (var g in AssetDatabase.FindAssets("framed_photo t:Texture2D"))
        {
            var p = AssetDatabase.GUIDToAssetPath(g);
            if (Path.GetFileNameWithoutExtension(p).StartsWith("framed_photo")) paths.Add(p);
        }
        paths.Sort();
        foreach (var p in paths)
        {
            var ti = AssetImporter.GetAtPath(p) as TextureImporter;
            if (ti != null && (ti.wrapMode != TextureWrapMode.Clamp || !ti.alphaIsTransparency))
            {
                ti.wrapMode = TextureWrapMode.Clamp;
                ti.alphaIsTransparency = true;
                ti.SaveAndReimport();
            }
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(p);
            if (tex == null) continue;
            var m = Mat("Photo_" + tex.name, new Color(0.92f, 0.92f, 0.92f), "Universal Render Pipeline/Unlit", "Unlit/Texture");
            var sc = new Vector2(CropSize, CropSize); var off = new Vector2(CropU0, CropV0);
            m.mainTexture = tex; m.mainTextureScale = sc; m.mainTextureOffset = off;
            if (m.HasProperty("_BaseMap"))
            {
                m.SetTexture("_BaseMap", tex); m.SetTextureScale("_BaseMap", sc); m.SetTextureOffset("_BaseMap", off);
            }
            EditorUtility.SetDirty(m);
            photoMats.Add(m);

            var pm = Mat("PhotoPic_" + tex.name, new Color(0.92f, 0.92f, 0.92f), "Universal Render Pipeline/Unlit", "Unlit/Texture");
            var psc = new Vector2(PicW, PicH); var poff = new Vector2(PicU0, PicV0);
            pm.mainTexture = tex; pm.mainTextureScale = psc; pm.mainTextureOffset = poff;
            if (pm.HasProperty("_BaseMap"))
            {
                pm.SetTexture("_BaseMap", tex); pm.SetTextureScale("_BaseMap", psc); pm.SetTextureOffset("_BaseMap", poff);
            }
            EditorUtility.SetDirty(pm);
            picMats.Add(pm);
        }
        if (photoMats.Count == 0)
        {
            Debug.LogError("[BirthdayGarden] No textures named framed_photo_*.png found in Assets/. Drag your photos into the project first.");
            return false;
        }
        // shuffled order, so the same photo isn't always next to itself
        var rng = new System.Random(77);
        photoOrder = new List<Material>(photoMats);
        for (int i = photoOrder.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            var t = photoOrder[i]; photoOrder[i] = photoOrder[j]; photoOrder[j] = t;
        }
        var rng2 = new System.Random(5);
        for (int i = picMats.Count - 1; i > 0; i--)
        {
            int j = rng2.Next(i + 1);
            var t2 = picMats[i]; picMats[i] = picMats[j]; picMats[j] = t2;
        }
        photoCursor = 0; picCursor = 0;
        return true;
    }

    static Material NextPhoto() { return photoOrder[photoCursor++ % photoOrder.Count]; }
    static Material NextPic() { return picMats[picCursor++ % picMats.Count]; }

    static Mesh BulbMesh()
    {
        EnsureFolder();
        var path = OutDir + "/LowSphere.asset";
        var m = AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if (m != null) return m;
        const int seg = 10, rings = 6;
        var v = new List<Vector3>(); var n = new List<Vector3>(); var t = new List<int>();
        for (int r = 0; r <= rings; r++)
        {
            float ph = Mathf.PI * r / rings;
            for (int s = 0; s <= seg; s++)
            {
                float th = 2f * Mathf.PI * s / seg;
                var p = new Vector3(Mathf.Sin(ph) * Mathf.Cos(th), Mathf.Cos(ph), Mathf.Sin(ph) * Mathf.Sin(th));
                v.Add(p * 0.5f); n.Add(p);
            }
        }
        for (int r = 0; r < rings; r++)
            for (int s = 0; s < seg; s++)
            {
                int a = r * (seg + 1) + s, b = a + seg + 1;
                t.Add(a); t.Add(a + 1); t.Add(b);
                t.Add(a + 1); t.Add(b + 1); t.Add(b);
            }
        m = new Mesh { name = "LowSphere" };
        m.SetVertices(v); m.SetNormals(n); m.SetTriangles(t, 0);
        m.RecalculateBounds();
        AssetDatabase.CreateAsset(m, path);
        return m;
    }

    static GameObject Bulb(Transform parent, Vector3 worldPos, float size)
    {
        var go = new GameObject("Bulb");
        go.transform.SetParent(parent, true);
        go.transform.position = worldPos;
        go.transform.localScale = Vector3.one * size;
        go.AddComponent<MeshFilter>().sharedMesh = bulbMesh;
        var mr = go.AddComponent<MeshRenderer>();
        mr.sharedMaterial = bulbMat;
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        return go;
    }

    static GameObject QuadObj(Transform parent, Material m, Vector3 worldPos, Quaternion rot, float size)
    {
        var go = new GameObject("PhotoQuad");
        go.transform.SetParent(parent, true);
        go.transform.SetPositionAndRotation(worldPos, rot);
        go.transform.localScale = new Vector3(size, size, 1f);
        go.AddComponent<MeshFilter>().sharedMesh = Resources.GetBuiltinResource<Mesh>("Quad.fbx");
        var mr = go.AddComponent<MeshRenderer>();
        mr.sharedMaterial = m;
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        return go;
    }

    static GameObject CubeObj(Transform parent, Vector3 localPos, Vector3 localScale, Material m, bool collider = false)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        if (!collider) Object.DestroyImmediate(go.GetComponent<Collider>());
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.transform.localScale = localScale;
        go.GetComponent<Renderer>().sharedMaterial = m;
        return go;
    }

    static void Cord(Transform parent, Vector3 top, Vector3 bottom)
    {
        var go = new GameObject("Cord");
        go.transform.SetParent(parent, true);
        var lr = go.AddComponent<LineRenderer>();
        lr.positionCount = 2; lr.SetPosition(0, top); lr.SetPosition(1, bottom);
        lr.useWorldSpace = true; lr.startWidth = lr.endWidth = 0.008f;
        lr.sharedMaterial = ropeMat;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    }

    // A sagging rope with a long series of small glowing bulbs, pegged polaroids (double-sided) and one soft
    // point light in the middle. With photos > 0 the bigger hanging bulbs go in the GAPS between the photos
    // (photos - 1 of them); with photos == 0 it hangs `hanging` bulbs spread along the rope.
    static void Garland(Transform parent, Vector3 a, Vector3 b, float sag, int photos, float photoSize, int hanging)
    {
        var g = new GameObject("Garland");
        g.transform.SetParent(parent, false);
        System.Func<float, Vector3> P = t => Vector3.Lerp(a, b, t) - Vector3.up * (sag * 4f * t * (1f - t));
        float len = (b - a).magnitude;

        int n = Mathf.Max(10, (int)(len / 0.2f));
        var lr = g.AddComponent<LineRenderer>();
        lr.positionCount = n + 1;
        for (int i = 0; i <= n; i++) lr.SetPosition(i, P(i / (float)n));
        lr.useWorldSpace = true; lr.startWidth = lr.endWidth = 0.014f; lr.numCapVertices = 2;
        lr.sharedMaterial = ropeMat;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        int nb = Mathf.Max(4, (int)(len / 0.3f));            // small bulb every ~30 cm
        for (int i = 0; i <= nb; i++) Bulb(g.transform, P(i / (float)nb) - Vector3.up * 0.04f, 0.07f);

        var flat = new Vector3(b.x - a.x, 0, b.z - a.z).normalized;
        var nrm = Vector3.Cross(Vector3.up, flat).normalized;
        var rng = new System.Random((int)(a.x * 31 + a.z * 17 + b.x * 7) + 5);

        int hang = photos > 0 ? photos - 1 : hanging;         // bulbs on cords
        for (int i = 0; i < hang; i++)
        {
            float t = photos > 0 ? (i + 1f) / photos : (i + 1f) / (hang + 1f);
            var top = P(t);
            float L = 0.16f + (float)rng.NextDouble() * 0.08f;
            Vector3 bulbPosition = top - Vector3.up * (L + 0.05f) + nrm * 0.22f;
            Cord(g.transform, top, bulbPosition + Vector3.up * 0.05f);
            Bulb(g.transform, bulbPosition, 0.10f);
        }

        for (int i = 0; i < photos; i++)
        {
            var top = P((i + 0.5f) / photos);
            var m = NextPhoto();
            float roll = (float)(rng.NextDouble() * 16 - 8);
            var face = Quaternion.LookRotation(-nrm, Vector3.up);          // quad front (-Z) faces +nrm
            var back = Quaternion.LookRotation(nrm, Vector3.up);
            var rollQ = Quaternion.AngleAxis(roll, nrm);
            var center = top + rollQ * (Vector3.down * (photoSize * 0.5f + 0.03f));
            QuadObj(g.transform, m, center + nrm * 0.003f, rollQ * face, photoSize);
            QuadObj(g.transform, m, center - nrm * 0.003f, rollQ * back, photoSize);
            var peg = CubeObj(g.transform, Vector3.zero, new Vector3(0.035f, 0.09f, 0.03f), pegMat);
            peg.transform.SetParent(g.transform, true);
            peg.transform.SetPositionAndRotation(top - Vector3.up * 0.015f, rollQ * face);
        }

        if (len > 2f)                                          // soft warm light under the string
        {
            var lg = new GameObject("GarlandLight");
            lg.transform.SetParent(g.transform, true);
            lg.transform.position = P(0.5f) - Vector3.up * 0.5f;
            var l = lg.AddComponent<Light>();
            l.type = LightType.Point; l.shadows = LightShadows.None;
            l.color = WarmLampColor; l.intensity = 1.35f; l.range = Mathf.Clamp(len * 0.7f, 3f, 6f);
        }
    }

    // ---- photo gallery: free-standing photo posts with light strings overhead, spaced so you can walk between
    static void BuildGallery(Transform parent)
    {
        var g = new GameObject("PhotoGallery");
        g.transform.SetParent(parent, false);
        var rng = new System.Random(31);
        const float H = 2.6f;
        float[] innerZ = { 9f, 13f, 17f, 21f };
        float[] outerZ = { 11f, 15f };
        foreach (float sx in new[] { -1f, 1f })
        {
            foreach (float z in innerZ) PhotoStand(g.transform, new Vector3(sx * 3.6f, 0, z), sx > 0 ? -90f : 90f, rng);
            foreach (float z in outerZ) PhotoStand(g.transform, new Vector3(sx * 7f, 0, z), sx > 0 ? -90f : 90f, rng);
            for (int i = 0; i < innerZ.Length - 1; i++)
            {
                float len = innerZ[i + 1] - innerZ[i];
                Garland(g.transform, new Vector3(sx * 3.6f, H, innerZ[i]), new Vector3(sx * 3.6f, H, innerZ[i + 1]), 0.4f, PhotoCount(len), GalleryPhotoSize, 0);
            }
            Garland(g.transform, new Vector3(sx * 7f, H, outerZ[0]), new Vector3(sx * 7f, H, outerZ[1]), 0.4f, PhotoCount(outerZ[1] - outerZ[0]), GalleryPhotoSize, 0);
        }
        // light strings across the middle lane (high enough to walk under) - small photos so they stay above head height
        foreach (float z in new[] { 13f, 17f })
            Garland(g.transform, new Vector3(-3.6f, H, z), new Vector3(3.6f, H, z), 0.45f, PhotoCount(7.2f), LaneRowPhotoSize, 0);
    }

    static void PhotoStand(Transform parent, Vector3 pos, float yaw, System.Random rng)
    {
        var wood = Mat("PartyWood", new Color(0.3f, 0.17f, 0.09f), "Universal Render Pipeline/Lit", "Standard");
        var s = new GameObject("PhotoStand");
        s.transform.SetParent(parent, false);
        s.transform.SetPositionAndRotation(pos, Quaternion.Euler(0, yaw + (float)(rng.NextDouble() * 20 - 10), 0));
        var t = s.transform;

        var pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pole.transform.SetParent(t, false);
        pole.transform.localPosition = new Vector3(0, 1.3f, 0);
        pole.transform.localScale = new Vector3(0.12f, 1.3f, 0.12f);
        pole.GetComponent<Renderer>().sharedMaterial = wood;
        CubeObj(t, new Vector3(0, 0.05f, 0), new Vector3(0.45f, 0.1f, 0.45f), wood);
        CubeObj(t, new Vector3(0, 2.25f, 0), new Vector3(1.1f, 0.05f, 0.05f), wood);       // arm
        Bulb(t, t.TransformPoint(new Vector3(0, 2.66f, 0)), 0.1f);

        const float size = 0.5f;   // small photos
        foreach (float sx in new[] { -1f, 1f })
        {
            float roll = (float)(rng.NextDouble() * 12 - 6);
            float cy = 2.2f - 0.2f - size * 0.5f;
            Vector3 c = new Vector3(sx * 0.4f, cy, 0);
            var m = NextPhoto();
            QuadObj(t, m, t.TransformPoint(c + new Vector3(0, 0, 0.004f)), t.rotation * Quaternion.Euler(0, 180f, roll), size);
            QuadObj(t, m, t.TransformPoint(c - new Vector3(0, 0, 0.004f)), t.rotation * Quaternion.Euler(0, 0f, -roll), size);
            Vector3 top = new Vector3(sx * 0.4f, cy + size * 0.5f, 0);
            Cord(t, t.TransformPoint(new Vector3(sx * 0.4f, 2.22f, 0)), t.TransformPoint(top));
            var peg = CubeObj(t, top, new Vector3(0.035f, 0.08f, 0.03f), pegMat);
            peg.transform.localRotation = Quaternion.Euler(0, 0, roll);
            Bulb(t, t.TransformPoint(new Vector3(sx * 0.55f, 2.22f, 0)), 0.07f);
        }
        taken.Add(new Vector3(pos.x, pos.z, 1.0f));
    }

    // ---- make every bulb / lantern glass / flame of the imported models bright (unlit HDR + Bloom)
    [MenuItem("Tools/Birthday Garden (FBX)/Make Lights Glow")]
    public static void MakeBulbsGlow()
    {
        var rootGo = GameObject.Find(RootName);
        if (rootGo == null) { Debug.LogWarning("[BirthdayGarden] Build the environment first."); return; }
        int n = 0;
        foreach (var r in rootGo.GetComponentsInChildren<Renderer>(true))
        {
            var ms = r.sharedMaterials; bool changed = false;
            for (int i = 0; i < ms.Length; i++)
            {
                if (ms[i] == null) continue;
                string nm = ms[i].name; Material rep = null;
                string lower = nm.ToLowerInvariant();
                if (lower.StartsWith(CentreWalkwayMaterialName.ToLowerInvariant())) rep = GlowMat(CentreWalkwayMaterialName, CentreWalkwayWhiteHdr);
                else if (lower.StartsWith("bulb") || lower.StartsWith("glowbulb")) rep = GlowMat("GlowBulb", WarmBulbHdr);
                else if (lower.StartsWith("partybulbglow")) rep = GlowMat("PartyBulbGlow", WarmBulbHdr);
                else if (lower.StartsWith("lantern_glow") || lower.StartsWith("glowlantern")) rep = GlowMat("GlowLantern", new Color(2.8f, 1.15f, 0.09f));
                else if (nm.StartsWith("flame_in") || nm == "flame") rep = GlowMat("GlowFlameIn", new Color(4f, 3.4f, 1.6f));
                else if (nm.StartsWith("flame_mid")) rep = GlowMat("GlowFlameMid", new Color(4f, 2.2f, 0.5f));
                else if (nm.StartsWith("flame_out")) rep = GlowMat("GlowFlameOut", new Color(3.2f, 1.0f, 0.2f));
                else if (nm.StartsWith("ember")) rep = GlowMat("GlowEmber", new Color(2.5f, 0.9f, 0.2f));
                if (rep != null && ms[i] != rep) { ms[i] = rep; changed = true; n++; }
            }
            if (changed) { r.sharedMaterials = ms; EditorUtility.SetDirty(r); }
        }
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("[BirthdayGarden] Made " + n + " light materials glow.");
    }

    static Material GlowMat(string name, Color hdr)
    {
        return Mat(name, hdr, "Universal Render Pipeline/Unlit", "Unlit/Color");
    }

    [MenuItem("Tools/Birthday Garden (FBX)/Warm Bulbs and Clear Photo Views")]
    public static void ApplyWarmBulbsAndPhotoClearance()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogWarning("[BirthdayGarden] Stop Play before adjusting the bulbs and lamps.");
            return;
        }
        var garden = GameObject.Find(RootName);
        if (garden == null) { Debug.LogWarning("[BirthdayGarden] Open the existing garden scene first."); return; }
        root = garden.transform;
        MakeBulbsGlow();
        int moved = 0;
        var ties = root.Find("PhotoLightSupportTies");
        if (ties == null)
        {
            var tiesGo = new GameObject("PhotoLightSupportTies");
            Undo.RegisterCreatedObjectUndo(tiesGo, "Support photo ropes");
            tiesGo.transform.SetParent(root, false);
            ties = tiesGo.transform;
        }
        var supportMaterial = Mat("PartyRope", new Color(0.22f, 0.15f, 0.09f), "Universal Render Pipeline/Unlit", "Sprites/Default");
        foreach (Transform item in root)
        {
            if (item.name.StartsWith("lantern_post"))
            {
                float station = ClosestPathLampStation(item.position.z);
                if (Mathf.Abs(item.position.z - station) > 1.6f) continue;
                float side = item.position.x < 0f ? -1f : 1f;
                Undo.RecordObject(item, "Move lamp clear of photos");
                var p = item.position;
                p.x = side * ClearLampX;
                p.z = station + ClearLampZOffset;
                item.position = p;
                string tieName = "PhotoRopeSupport_" + (side < 0f ? "Left_" : "Right_") + Mathf.RoundToInt(-station);
                var tie = ties.Find(tieName);
                if (tie == null)
                {
                    var tieGo = new GameObject(tieName);
                    Undo.RegisterCreatedObjectUndo(tieGo, "Support photo rope");
                    tieGo.transform.SetParent(ties, false);
                    tie = tieGo.transform;
                }
                var rope = tie.GetComponent<LineRenderer>();
                if (rope == null) rope = Undo.AddComponent<LineRenderer>(tie.gameObject);
                Undo.RecordObject(rope, "Adjust photo rope support");
                rope.positionCount = 2;
                rope.useWorldSpace = true;
                rope.SetPosition(0, item.position + Vector3.up * 2.95f);
                rope.SetPosition(1, new Vector3(side * PhotoStringX, 2.9f, station));
                rope.startWidth = rope.endWidth = 0.012f;
                rope.sharedMaterial = supportMaterial;
                rope.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                moved++;
            }
            else if (item.name.StartsWith("string_lights"))
            {
                float station = ClosestPathLampStation(item.position.z);
                if (Mathf.Abs(item.position.z - station) > 1.6f) continue;
                Undo.RecordObject(item, "Raise string lamps clear of photos");
                item.position = new Vector3(item.position.x, ImportedStringLift, station + ImportedStringZOffset);
                moved++;
            }
        }
        foreach (var light in garden.GetComponentsInChildren<Light>(true))
        {
            if (light.type == LightType.Directional || !IsWarmDecorLight(light.transform)) continue;
            Undo.RecordObject(light, "Warm golden garden lights");
            light.useColorTemperature = false;
            light.color = WarmLampColor;
            light.intensity = Mathf.Max(light.intensity, HasNamedAncestor(light.transform, "tree") ? 2.8f : 1.35f);
            EditorUtility.SetDirty(light);
        }
        AdjustHangingPhotoBulbs(garden);
        ConfigureWarmBloom(garden);
        ApplyCentreWalkwaySoftWhite();
        EditorSceneManager.MarkSceneDirty(garden.scene);
        SceneView.RepaintAll();
        Debug.Log("[BirthdayGarden] Warm tree/lamp lights and soft-white centre strings applied; " + moved + " lamp groups moved clear of photos. Save the scene and press Play.");
    }

    [MenuItem("Tools/Birthday Garden (FBX)/Centre Walkway Bulbs - Soft White")]
    public static void ApplyCentreWalkwaySoftWhite()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogWarning("[BirthdayGarden] Stop Play before changing the centre walkway bulbs.");
            return;
        }
        var garden = GameObject.Find(RootName);
        if (garden == null) { Debug.LogWarning("[BirthdayGarden] Open the existing garden scene first."); return; }

        // Never recolor the shared gold materials: trees and lanterns use them.
        // A separate asset changes only the bulb slots in the selected strings.
        var white = GlowMat(CentreWalkwayMaterialName, CentreWalkwayWhiteHdr);
        var groups = CentreWalkwayStringGroups(garden.transform);
        int bulbSlots = 0;
        foreach (var group in groups)
        {
            foreach (var renderer in group.GetComponentsInChildren<Renderer>(true))
            {
                var materials = renderer.sharedMaterials;
                bool changed = false;
                for (int slot = 0; slot < materials.Length; slot++)
                {
                    var material = materials[slot];
                    if (material == null) continue;
                    string name = material.name.ToLowerInvariant();
                    bool bulb = renderer.name == "Bulb" || name.StartsWith("bulb") ||
                                name.StartsWith("glowbulb") || name.StartsWith("partybulbglow");
                    if (!bulb) continue;
                    materials[slot] = white;
                    changed = true;
                    bulbSlots++;
                }
                if (!changed) continue;
                Undo.RecordObject(renderer, "Soft-white centre walkway bulbs");
                renderer.sharedMaterials = materials;
                EditorUtility.SetDirty(renderer);
            }
            foreach (var light in group.GetComponentsInChildren<Light>(true))
            {
                if (light.type == LightType.Directional) continue;
                Undo.RecordObject(light, "Gentle white walkway illumination");
                light.useColorTemperature = false;
                light.color = Color.white;
                light.intensity = 0.45f;
                EditorUtility.SetDirty(light);
            }
        }
        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(garden.scene);
        SceneView.RepaintAll();
        Debug.Log("[BirthdayGarden] Centre walkway only: " + bulbSlots + " bulb slots are white with subtle glow. Tree lights, lamps, global Bloom and positions are unchanged.");
    }

    static List<Transform> CentreWalkwayStringGroups(Transform garden)
    {
        var groups = new List<Transform>();
        foreach (Transform child in garden)
            if (child.name.StartsWith("string_lights") && Mathf.Abs(child.position.x) < 0.5f && child.position.z < 0f)
                groups.Add(child);

        // Centre strings cross the path. Side photo strings have constant X;
        // gallery strings live in a separate hierarchy or at positive Z.
        var photoDecor = garden.Find("PhotoDecor");
        if (photoDecor == null) return groups;
        foreach (Transform child in photoDecor)
        {
            if (child.name != "Garland") continue;
            var rope = child.GetComponent<LineRenderer>();
            if (rope == null || rope.positionCount < 2) continue;
            Vector3 a = rope.GetPosition(0), b = rope.GetPosition(rope.positionCount - 1);
            if (!rope.useWorldSpace) { a = rope.transform.TransformPoint(a); b = rope.transform.TransformPoint(b); }
            bool acrossPath = Mathf.Min(a.x, b.x) < -1f && Mathf.Max(a.x, b.x) > 1f &&
                              Mathf.Abs(a.z - b.z) < 0.1f && (a.z + b.z) * 0.5f < 0f;
            if (acrossPath) groups.Add(child);
        }
        return groups;
    }

    static float ClosestPathLampStation(float z)
    {
        float best = PathLampStations[0];
        foreach (float station in PathLampStations)
            if (Mathf.Abs(z - station) < Mathf.Abs(z - best)) best = station;
        return best;
    }

    static bool HasNamedAncestor(Transform item, string token)
    {
        for (var current = item; current != null; current = current.parent)
            if (current.name.ToLowerInvariant().Contains(token)) return true;
        return false;
    }

    static bool IsWarmDecorLight(Transform item)
    {
        return HasNamedAncestor(item, "tree") || HasNamedAncestor(item, "garland") ||
               HasNamedAncestor(item, "string_light") || HasNamedAncestor(item, "lantern");
    }

    static void AdjustHangingPhotoBulbs(GameObject garden)
    {
        foreach (var item in garden.GetComponentsInChildren<Transform>(true))
        {
            if (item.name != "Bulb" || item.parent == null || item.parent.name != "Garland" || item.localScale.x < 0.095f) continue;
            var rope = item.parent.GetComponent<LineRenderer>();
            if (rope == null || rope.positionCount < 2) continue;
            Vector3 a = rope.GetPosition(0), b = rope.GetPosition(rope.positionCount - 1);
            Vector3 flat = b - a; flat.y = 0f;
            if (flat.sqrMagnitude < 0.01f) continue;
            float t = Mathf.Clamp01(Vector3.Dot(item.position - a, flat) / flat.sqrMagnitude);
            float index = t * (rope.positionCount - 1);
            int first = Mathf.FloorToInt(index);
            Vector3 top = Vector3.Lerp(rope.GetPosition(first), rope.GetPosition(Mathf.Min(first + 1, rope.positionCount - 1)), index - first);
            Vector3 normal = Vector3.Cross(Vector3.up, flat.normalized).normalized;
            Vector3 oldPosition = item.position;
            Vector3 target = top - Vector3.up * 0.25f + normal * 0.22f;
            foreach (var cord in item.parent.GetComponentsInChildren<LineRenderer>(true))
            {
                if (cord.name != "Cord" || cord.positionCount != 2 || Vector3.Distance(cord.GetPosition(1), oldPosition) > 0.16f) continue;
                Undo.RecordObject(cord, "Move hanging bulb cord");
                cord.SetPosition(0, top);
                cord.SetPosition(1, target + Vector3.up * 0.05f);
            }
            Undo.RecordObject(item, "Move bulb clear of photo plane");
            item.position = target;
            item.localScale = Vector3.one * 0.10f;
        }
    }

    // Reflection keeps this editor action usable in both Built-in and URP
    // projects without requiring a manually configured preprocessor symbol.
    static void ConfigureWarmBloom(GameObject garden)
    {
        System.Type volumeType = null, bloomType = null, profileType = null, cameraDataType = null;
        foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
        {
            volumeType = volumeType ?? assembly.GetType("UnityEngine.Rendering.Volume");
            profileType = profileType ?? assembly.GetType("UnityEngine.Rendering.VolumeProfile");
            bloomType = bloomType ?? assembly.GetType("UnityEngine.Rendering.Universal.Bloom");
            cameraDataType = cameraDataType ?? assembly.GetType("UnityEngine.Rendering.Universal.UniversalAdditionalCameraData");
        }
        var pipeline = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline;
        if (pipeline == null || !pipeline.GetType().Name.Contains("Universal") || volumeType == null || bloomType == null || profileType == null) return;
        var profileField = volumeType.GetField("sharedProfile");
        var componentsField = profileType.GetField("components");
        if (profileField == null || componentsField == null) return;
        Component volume = null;
        foreach (var component in garden.GetComponentsInChildren(volumeType, true))
            if (component.gameObject.name == "GardenPostProcessing") { volume = component; break; }
        if (volume == null)
        {
            var go = new GameObject("GardenPostProcessing");
            Undo.RegisterCreatedObjectUndo(go, "Add garden bloom");
            go.transform.SetParent(garden.transform, false);
            volume = Undo.AddComponent(go, volumeType);
        }
        Undo.RecordObject(volume, "Warm garden bloom");
        volumeType.GetField("isGlobal")?.SetValue(volume, true);
        var profile = profileField.GetValue(volume) as ScriptableObject;
        if (profile == null)
        {
            EnsureFolder();
            profile = ScriptableObject.CreateInstance(profileType);
            AssetDatabase.CreateAsset(profile, AssetDatabase.GenerateUniqueAssetPath(OutDir + "/GardenWarmGlowProfile.asset"));
            profileField.SetValue(volume, profile);
        }
        Object bloom = null;
        var components = componentsField.GetValue(profile) as System.Collections.IEnumerable;
        if (components != null) foreach (var component in components)
            if (bloomType.IsInstanceOfType(component)) { bloom = component as Object; break; }
        if (bloom == null)
        {
            var add = profileType.GetMethod("Add", new[] { typeof(System.Type), typeof(bool) });
            if (add != null) bloom = add.Invoke(profile, new object[] { bloomType, false }) as Object;
        }
        if (bloom == null) return;
        if (AssetDatabase.Contains(profile) && !AssetDatabase.Contains(bloom)) AssetDatabase.AddObjectToAsset(bloom, profile);
        Undo.RecordObject(bloom, "Golden bulb bloom");
        bloomType.GetField("active")?.SetValue(bloom, true);
        SetVolumeParameter(bloom, "intensity", 1.6f);
        SetVolumeParameter(bloom, "threshold", 1.05f);
        SetVolumeParameter(bloom, "scatter", 0.45f);
        EditorUtility.SetDirty(bloom);
        EditorUtility.SetDirty(profile);
        EditorUtility.SetDirty(volume);
        AssetDatabase.SaveAssets();
        foreach (var camera in garden.scene.GetRootGameObjects())
            foreach (var view in camera.GetComponentsInChildren<Camera>(true))
            {
                Undo.RecordObject(view, "HDR garden glow");
                view.allowHDR = true;
                if (cameraDataType == null) continue;
                var data = view.GetComponent(cameraDataType) ?? Undo.AddComponent(view.gameObject, cameraDataType);
                Undo.RecordObject(data, "Enable garden post processing");
                cameraDataType.GetProperty("renderPostProcessing")?.SetValue(data, true);
                EditorUtility.SetDirty(data);
            }
    }

    static void SetVolumeParameter(Object component, string fieldName, float value)
    {
        var parameter = component.GetType().GetField(fieldName)?.GetValue(component);
        if (parameter == null) return;
        parameter.GetType().GetProperty("overrideState")?.SetValue(parameter, true);
        parameter.GetType().GetProperty("value")?.SetValue(parameter, value);
    }

    // Rustic plank wall with 13 pegged polaroids, a fairy-light string on top, lanterns and flower pots.
    // Local +Z of the wall is its front.
    static void BuildPhotoWall(Transform parent, Vector3 pos, float yaw)
    {
        var w = new GameObject("PhotoWall");
        w.transform.SetParent(parent, false);
        w.transform.SetPositionAndRotation(pos, Quaternion.Euler(0, yaw, 0));
        var t = w.transform;
        const float W = 4.8f, H = 2.9f, Y0 = 0.3f;

        for (int i = 0; i < 10; i++)
            CubeObj(t, new Vector3(-W / 2f + 0.24f + i * (W / 10f), Y0 + H / 2f, 0), new Vector3(W / 10f - 0.01f, H, 0.08f), plankMats[i % 3]);
        CubeObj(t, new Vector3(0, Y0 + H + 0.08f, 0.02f), new Vector3(W + 0.5f, 0.18f, 0.2f), plankMats[1]);
        CubeObj(t, new Vector3(0, Y0 - 0.06f, 0.02f), new Vector3(W + 0.5f, 0.16f, 0.2f), plankMats[1]);
        foreach (float sx in new[] { -1f, 1f })
            CubeObj(t, new Vector3(sx * (W / 2f + 0.15f), (Y0 + H + 0.3f) / 2f, 0.02f), new Vector3(0.22f, Y0 + H + 0.3f, 0.24f), plankMats[0]);
        // The final collision pass follows each plank/post instead of filling the whole wall with a box.

        // photos: 4 + 5 + 4
        float[] rowY = { Y0 + H * 0.82f, Y0 + H * 0.5f, Y0 + H * 0.18f };
        float[][] rowX = { new[] { -1.8f, -0.6f, 0.6f, 1.8f }, new[] { -2.0f, -1.0f, 0f, 1.0f, 2.0f }, new[] { -1.8f, -0.6f, 0.6f, 1.8f } };
        float[] size = { 0.9f, 0.8f, 0.9f };
        var rng = new System.Random(12);
        for (int r = 0; r < 3; r++)
            for (int c = 0; c < rowX[r].Length; c++)
            {
                float roll = (float)(rng.NextDouble() * 10 - 5);
                var m = NextPhoto();
                var q = QuadObj(t, m, Vector3.zero, Quaternion.identity, size[r]);
                q.transform.localPosition = new Vector3(rowX[r][c], rowY[r], 0.06f);
                q.transform.localRotation = Quaternion.Euler(0, 180f, roll);   // quad front -> wall front (+Z)
                var peg = CubeObj(t, new Vector3(rowX[r][c], rowY[r] + size[r] * 0.5f + 0.01f, 0.07f), new Vector3(0.04f, 0.1f, 0.03f), pegMat);
                peg.transform.localRotation = Quaternion.Euler(0, 0, roll);
            }

        // fairy lights along the top beam + both posts
        Garland(t, t.TransformPoint(new Vector3(-W / 2f - 0.15f, Y0 + H + 0.45f, 0.18f)),
                   t.TransformPoint(new Vector3(W / 2f + 0.15f, Y0 + H + 0.45f, 0.18f)), 0.35f, 0, 0.5f, 0);

        // lanterns + flower pots in front
        foreach (float sx in new[] { -1f, 1f })
        {
            Vector3 lp = t.TransformPoint(new Vector3(sx * 2.1f, 0, 0.9f));
            Spawn("lantern", lp, yaw, 1f, Col.None, 0.4f);
            Vector3 fp = t.TransformPoint(new Vector3(sx * 1.5f, 0, 1.0f));
            Spawn("flower_pot", fp, yaw, 1f, Col.None, 0.5f);
        }
        taken.Add(new Vector3(pos.x, pos.z, 3.6f));
    }

    // ------------------------------------------------------------------ ground, sky, light
    static void MakeGround()
    {
        var g = GameObject.CreatePrimitive(PrimitiveType.Plane);
        g.name = "Ground";
        g.transform.SetParent(root);
        g.transform.position = new Vector3(0, 0, 4);
        g.transform.localScale = new Vector3(10f, 1f, 10f); // 100 x 100 m
        var texture = MakeGroundGrassTexture();
        var m = Mat("GardenGround", Color.white, "Universal Render Pipeline/Lit", "Standard");
        m.mainTexture = texture;
        if (m.HasProperty("_BaseMap")) m.SetTexture("_BaseMap", texture);
        if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", 0f);
        if (m.HasProperty("_Glossiness")) m.SetFloat("_Glossiness", 0f);
        if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", 0f);
        m.EnableKeyword("_EMISSION");
        if (m.HasProperty("_EmissionMap")) m.SetTexture("_EmissionMap", texture);
        if (m.HasProperty("_EmissionColor")) m.SetColor("_EmissionColor", new Color(0.22f, 0.22f, 0.22f, 1f));
        m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
        SetGroundTiling(m, 100f / 4.5f, 100f / 4.5f);
        g.GetComponent<Renderer>().sharedMaterial = m;
        EditorUtility.SetDirty(m);
        AssetDatabase.SaveAssets();
        GameObjectUtility.SetStaticEditorFlags(g, StaticEditorFlags.BatchingStatic | StaticEditorFlags.OccludeeStatic);
    }

    static void SetGroundTiling(Material m, float tx, float ty)
    {
        var s = new Vector2(tx, ty);
        m.mainTextureScale = s;
        if (m.HasProperty("_BaseMap")) m.SetTextureScale("_BaseMap", s);
        if (m.HasProperty("_EmissionMap")) m.SetTextureScale("_EmissionMap", s);
    }

    static Texture2D MakeGroundGrassTexture()
    {
        EnsureFolder();
        const int S = 512;
        var rng = new System.Random(2024);
        int[] periods = { 4, 8, 16, 32 };
        float[] weights = { 0.40f, 0.30f, 0.20f, 0.10f };
        var lats = new float[periods.Length][];
        for (int i = 0; i < periods.Length; i++) lats[i] = GroundNoiseLattice(periods[i], rng);
        var warmLat = GroundNoiseLattice(6, rng);

        var dark = new Color(0.10f, 0.18f, 0.04f);
        var mid = new Color(0.20f, 0.30f, 0.06f);
        var light = new Color(0.33f, 0.43f, 0.08f);
        var warm = new Color(0.42f, 0.43f, 0.09f);

        var col = new Color[S * S];
        for (int y = 0; y < S; y++)
            for (int x = 0; x < S; x++)
            {
                float u = x / (float)S, v = y / (float)S, t = 0f;
                for (int k = 0; k < periods.Length; k++) t += weights[k] * GroundNoise(u, v, periods[k], lats[k]);
                Color c = t < 0.5f ? Color.Lerp(dark, mid, t * 2f) : Color.Lerp(mid, light, (t - 0.5f) * 2f);
                float w = GroundNoise(u, v, 6, warmLat);
                if (w > 0.62f) c = Color.Lerp(c, warm, Mathf.Clamp01((w - 0.62f) * 2.5f) * 0.55f);
                col[y * S + x] = c;
            }

        for (int i = 0; i < 7000; i++)
        {
            float x = (float)rng.NextDouble() * S, y = (float)rng.NextDouble() * S;
            float ang = (-90f + ((float)rng.NextDouble() - 0.5f) * 70f) * Mathf.Deg2Rad;
            int len = 4 + rng.Next(7);
            double pick = rng.NextDouble();
            Color tint = pick < 0.15 ? warm : (pick < 0.6 ? light : dark);
            for (int s = 0; s < len; s++)
            {
                int px = ((int)(x + Mathf.Cos(ang) * s) % S + S) % S;
                int py = ((int)(y + Mathf.Sin(ang) * s) % S + S) % S;
                col[py * S + px] = Color.Lerp(col[py * S + px], tint, 0.5f);
            }
        }

        var px32 = new Color32[S * S];
        for (int i = 0; i < px32.Length; i++) px32[i] = col[i];
        var tex = new Texture2D(S, S, TextureFormat.RGBA32, false);
        tex.SetPixels32(px32); tex.Apply();
        var path = OutDir + "/GardenGrass.png";
        File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        var ti = AssetImporter.GetAtPath(path) as TextureImporter;
        if (ti != null)
        {
            ti.wrapMode = TextureWrapMode.Repeat; ti.anisoLevel = 8; ti.mipmapEnabled = true; ti.alphaIsTransparency = false;
            ti.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }

    static float[] GroundNoiseLattice(int period, System.Random rng)
    {
        var a = new float[period * period];
        for (int i = 0; i < a.Length; i++) a[i] = (float)rng.NextDouble();
        return a;
    }

    static float GroundNoise(float u, float v, int period, float[] lat)
    {
        float gx = u * period, gy = v * period;
        int x0 = Mathf.FloorToInt(gx), y0 = Mathf.FloorToInt(gy);
        float fx = gx - x0, fy = gy - y0;
        fx = fx * fx * (3f - 2f * fx); fy = fy * fy * (3f - 2f * fy);
        int xa = ((x0 % period) + period) % period, xb = (xa + 1) % period;
        int ya = ((y0 % period) + period) % period, yb = (ya + 1) % period;
        float a = lat[ya * period + xa], b = lat[ya * period + xb], c = lat[yb * period + xa], d = lat[yb * period + xb];
        return Mathf.Lerp(Mathf.Lerp(a, b, fx), Mathf.Lerp(c, d, fx), fy);
    }

    // Ground lighting uses the same lantern prefab, LightDef and glow pass as the environment.
    // The deleted ground pack's textured cards and texture-analysis code are not required.
    static void BuildGroundLamps()
    {
        if (Find("lantern") == null) return;
        var holder = new GameObject("GroundLamps").transform;
        holder.SetParent(root, false);
        var rng = new System.Random(777);
        var spots = new List<Vector2>();

        for (float z = -32f; z < 2f; z += 5.2f)
            foreach (float side in new[] { -1f, 1f })
            {
                float x = side * (3.3f + (float)rng.NextDouble() * 0.8f);
                float lz = z + ((float)rng.NextDouble() - 0.5f) * 1.6f;
                if (GroundLampSpotFree(x, lz, 0.6f)) spots.Add(new Vector2(x, lz));
            }

        for (int tries = 0, n = 0; n < 22 && tries < 4000; tries++)
        {
            float x = Mathf.Lerp(-46f, 46f, (float)rng.NextDouble());
            float z = Mathf.Lerp(-42f, 50f, (float)rng.NextDouble());
            if (!GroundLampSpotFree(x, z, 1f) || GroundLampsTooClose(spots, x, z, 7.5f)) continue;
            spots.Add(new Vector2(x, z));
            n++;
        }

        var pool = MakeGroundPoolMaterial(MakeGroundPoolTexture());
        foreach (var spot in spots)
        {
            float yaw = (float)rng.NextDouble() * 360f;
            float scale = 0.9f + (float)rng.NextDouble() * 0.25f;
            var lamp = Spawn("lantern", new Vector3(spot.x, 0.02f, spot.y), yaw, scale, Col.None, 0.4f);
            if (lamp == null) continue;
            lamp.transform.SetParent(holder, true);
            PlaceGroundProp(lamp, spot.x, 0.02f, spot.y);

            var glow = GameObject.CreatePrimitive(PrimitiveType.Quad);
            glow.name = "LanternPool";
            Object.DestroyImmediate(glow.GetComponent<Collider>());
            glow.transform.SetParent(holder, false);
            glow.transform.position = new Vector3(spot.x, 0.07f, spot.y);
            glow.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            glow.transform.localScale = new Vector3(3.4f, 3.4f, 1f);
            var renderer = glow.GetComponent<Renderer>();
            renderer.sharedMaterial = pool;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }
        AssetDatabase.SaveAssets();
    }

    static bool GroundLampSpotFree(float x, float z, float margin)
    {
        if (Mathf.Abs(x) < 1.9f && z > -40f && z < 4f) return false;
        if (Vector2.Distance(new Vector2(x, z), new Vector2(0, 16)) < 7.5f) return false;
        foreach (var t in taken)
            if (Vector2.Distance(new Vector2(x, z), new Vector2(t.x, t.y)) < t.z + margin) return false;
        return true;
    }

    static Texture2D MakeGroundPoolTexture()
    {
        const int S = 128;
        var px = new Color32[S * S];
        for (int y = 0; y < S; y++)
            for (int x = 0; x < S; x++)
            {
                float dx = (x + 0.5f) / S * 2f - 1f, dy = (y + 0.5f) / S * 2f - 1f;
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                float a = Mathf.Pow(Mathf.Clamp01(1f - d), 2.2f);
                px[y * S + x] = new Color32(255, 200, 120, (byte)(a * 255f));
            }
        var t = new Texture2D(S, S, TextureFormat.RGBA32, false);
        t.SetPixels32(px); t.Apply();
        var path = OutDir + "/GroundLanternPool.png";
        File.WriteAllBytes(path, t.EncodeToPNG());
        Object.DestroyImmediate(t);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        var ti = AssetImporter.GetAtPath(path) as TextureImporter;
        if (ti != null) { ti.wrapMode = TextureWrapMode.Clamp; ti.alphaIsTransparency = true; ti.SaveAndReimport(); }
        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }

    static Material MakeGroundPoolMaterial(Texture2D tex)
    {
        var path = OutDir + "/GroundLanternPool.mat";
        var urp = Shader.Find("Universal Render Pipeline/Unlit");
        Shader sh = urp;
        if (sh == null) sh = Shader.Find("Legacy Shaders/Particles/Additive");
        if (sh == null) sh = Shader.Find("Sprites/Default");
        var m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (m == null) { m = new Material(sh); AssetDatabase.CreateAsset(m, path); }
        m.shader = sh;
        if (urp != null)
        {
            m.SetTexture("_BaseMap", tex);
            m.SetColor("_BaseColor", new Color(1.6f, 0.9f, 0.35f, 0.55f));
            m.SetFloat("_Surface", 1f); m.SetFloat("_Blend", 2f);                 // transparent, additive
            m.SetFloat("_SrcBlend", 5f); m.SetFloat("_DstBlend", 1f);              // SrcAlpha, One
            if (m.HasProperty("_SrcBlendAlpha")) m.SetFloat("_SrcBlendAlpha", 1f);
            if (m.HasProperty("_DstBlendAlpha")) m.SetFloat("_DstBlendAlpha", 1f);
            m.SetFloat("_ZWrite", 0f); m.SetFloat("_Cull", 0f);
            m.SetOverrideTag("RenderType", "Transparent");
            m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            m.SetShaderPassEnabled("ShadowCaster", false);
        }
        else
        {
            m.mainTexture = tex;
            if (m.HasProperty("_TintColor")) m.SetColor("_TintColor", new Color(0.8f, 0.5f, 0.2f, 0.5f));
        }
        m.renderQueue = 3000;
        EditorUtility.SetDirty(m);
        return m;
    }

    static Bounds GroundWorldBounds(GameObject go)
    {
        var rs = go.GetComponentsInChildren<Renderer>();
        if (rs.Length == 0) return new Bounds(go.transform.position, Vector3.zero);
        var b = rs[0].bounds;
        for (int i = 1; i < rs.Length; i++) b.Encapsulate(rs[i].bounds);
        return b;
    }

    static void PlaceGroundProp(GameObject go, float x, float y, float z)
    {
        var b = GroundWorldBounds(go);
        go.transform.position += new Vector3(x - b.center.x, y - b.min.y, z - b.center.z);
    }

    static bool GroundLampsTooClose(List<Vector2> pts, float x, float z, float dist)
    {
        var p = new Vector2(x, z);
        foreach (var q in pts) if (Vector2.Distance(p, q) < dist) return true;
        return false;
    }

    static void SetupAtmosphere()
    {
        RenderSettings.skybox = null;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.22f, 0.2f, 0.38f);
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = new Color(0.07f, 0.05f, 0.15f);
        RenderSettings.fogDensity = 0.012f;

        Light moon = null;
        foreach (var l in Object.FindObjectsByType<Light>(FindObjectsSortMode.InstanceID))
            if (l.type == LightType.Directional) { moon = l; break; }
        if (moon == null)
        {
            var go = new GameObject("MoonLight");
            go.transform.SetParent(root);
            moon = go.AddComponent<Light>();
            moon.type = LightType.Directional;
        }
        moon.color = new Color(0.55f, 0.62f, 1f);
        moon.intensity = 0.7f;
        moon.shadows = LightShadows.Soft;
        moon.transform.rotation = Quaternion.Euler(48, -30, 0);
    }

    static void SetupCamera()
    {
        var spawn = new GameObject("PlayerSpawn");
        spawn.transform.SetParent(root);
        spawn.transform.position = new Vector3(0, 1.0f, -36f);
        var cam = Camera.main;
        if (cam == null) return;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.07f, 0.05f, 0.15f);
        cam.transform.SetPositionAndRotation(new Vector3(0, 1.7f, -36f), Quaternion.Euler(8, 0, 0));
    }

    // ------------------------------------------------------------------ fireflies
    static void MakeFireflies()
    {
        var go = new GameObject("Fireflies");
        go.transform.SetParent(root);
        go.transform.position = new Vector3(0, 2f, 4f);
        var ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.loop = true; main.startLifetime = 9f; main.startSpeed = 0.15f; main.startSize = 0.09f;
        main.maxParticles = 250; main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startColor = new Color(1f, 0.85f, 0.35f);
        var em = ps.emission; em.rateOverTime = 28f;
        var sh = ps.shape; sh.shapeType = ParticleSystemShapeType.Box; sh.scale = new Vector3(70, 3.5f, 80);
        var noise = ps.noise; noise.enabled = true; noise.strength = 0.6f; noise.frequency = 0.25f;
        var rend = go.GetComponent<ParticleSystemRenderer>();
        var m = Mat("GardenFirefly", new Color(1f, 0.85f, 0.35f), "Universal Render Pipeline/Particles/Unlit", "Particles/Standard Unlit", "Sprites/Default");
        rend.sharedMaterial = m;
    }

    // ------------------------------------------------------------------ bloom (URP only)
    static void SetupPostProcessing()
    {
#if UNITY_RENDER_PIPELINE_URP
        EnsureFolder();
        var path = OutDir + "/GardenVolumeProfile.asset";
        AssetDatabase.DeleteAsset(path);
        var profile = ScriptableObject.CreateInstance<VolumeProfile>();
        AssetDatabase.CreateAsset(profile, path);
        var bloom = profile.Add<Bloom>(true);
        bloom.intensity.Override(1.0f); bloom.threshold.Override(1.0f); bloom.scatter.Override(0.45f);
        var vig = profile.Add<Vignette>(true);
        vig.intensity.Override(0.3f);
        EditorUtility.SetDirty(profile);
        AssetDatabase.SaveAssets();

        var v = new GameObject("GardenPostProcessing");
        v.transform.SetParent(root);
        var vol = v.AddComponent<Volume>();
        vol.isGlobal = true;
        vol.sharedProfile = profile;

        if (Camera.main != null)
            Camera.main.GetUniversalAdditionalCameraData().renderPostProcessing = true;
#else
        Debug.Log("[BirthdayGarden] URP not detected - add a Bloom post-processing effect yourself so the lights glow.");
#endif
    }

    // ------------------------------------------------------------------ helpers
    static void EnsureFolder()
    {
        if (!AssetDatabase.IsValidFolder(OutDir)) AssetDatabase.CreateFolder("Assets", "BirthdayGarden");
    }

    static Material Mat(string name, Color c, params string[] shaderNames)
    {
        EnsureFolder();
        var path = $"{OutDir}/{name}.mat";
        var m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (m == null)
        {
            Shader s = null;
            foreach (var n in shaderNames) { s = Shader.Find(n); if (s != null) break; }
            if (s == null) s = Shader.Find("Standard");
            m = new Material(s);
            AssetDatabase.CreateAsset(m, path);
        }
        m.color = c;
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
        EditorUtility.SetDirty(m);
        return m;
    }
}
#endif
