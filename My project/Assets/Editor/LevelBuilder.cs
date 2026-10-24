using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Adds "Labs > Build Full Level" to the menu bar. It rebuilds SampleScene as one level that
// uses everything from Labs 2-6, left to right:
//   Start area   - tree + mushroom checkpoint, bouncy pad (Lab 4), breakable bricks (Lab 5)
//   Lake         - deadly water with a sparkle particle system (Labs 4-5)
//   Cliff        - second checkpoint, walking slime between two Wall pillars (Lab 6)
//   Trapdoor     - hinge joint over a water pit (Lab 5)
//   Elevator     - rising platform up to a high ledge (Lab 5), ghost spawn point (Lab 6)
//   End          - third checkpoint, moving saw that follows the player (Lab 6)
// Running it again throws away the old "Level" group and builds a fresh one.
public static class LevelBuilder
{
    const string RootName = "Level";
    const string ScenePath = "Assets/Scenes/SampleScene.unity";
    const string Art = "Assets/Level Art/";
    const int GroundLayer = 6;

    static Transform root;
    static string foreground;

    [MenuItem("Labs/Build Full Level")]
    static void Build()
    {
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("Build Full Level", "Stop Play mode first (press the Play button again), then run Labs > Build Full Level. Changes made in Play mode are thrown away.", "OK");
            return;
        }

        move player = Object.FindObjectOfType<move>();
        if (player == null && EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            // A fresh checkout opens on an empty Untitled scene, so load the level first
            EditorSceneManager.OpenScene(ScenePath);
            player = Object.FindObjectOfType<move>();
        }
        if (player == null)
        {
            EditorUtility.DisplayDialog("Build Full Level", "Couldn't find the player (the object with the 'move' script) in " + ScenePath + ".", "OK");
            return;
        }

        AddTag("Enemy");
        AddTag("Wall");
        foreground = AddSortingLayer("Foreground") ? "Foreground" : "charecter";

        // Clear out the old level pieces
        foreach (string old in new[] { RootName, "Lab 6 Enemies", "Platform (1)", "sky" })
        {
            GameObject go = GameObject.Find(old);
            if (go != null)
                Undo.DestroyObjectImmediate(go);
        }

        root = new GameObject(RootName).transform;
        Undo.RegisterCreatedObjectUndo(root.gameObject, "Build Full Level");

        PhysicsMaterial2D friction = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>("Assets/New Physics Material 2D.physicsMaterial2D");
        PhysicsMaterial2D bouncy = BouncyMaterial();

        SetUpPlayer(player);
        SetUpCamera(player.transform);
        if (Object.FindObjectOfType<LevelManager>() == null)
            Undo.RegisterCreatedObjectUndo(new GameObject("Level Manager", typeof(LevelManager)), "Build Full Level");

        Transform ground = Group("Ground");
        Transform decor = Group("Decoration");
        Transform hazards = Group("Hazards");
        Transform enemies = Group("Enemies");

        // ---------- Start area: ground top at y = -3 ----------
        Block(ground, "Start Ground", "GrassBlock", -20f, 6f, -3f, -10f, friction);
        Wall(ground, "Level Start Wall", -12.5f, 1f, false);

        Sprite[] trees = AssetDatabase.LoadAllAssetsAtPath("Assets/trees and green objects.png")
            .OfType<Sprite>().OrderByDescending(s => s.rect.width * s.rect.height).ToArray();
        if (trees.Length > 0)
            Decor(decor, "Tree", trees[0], -3f, -3f, 5f, -50);
        if (trees.Length > 1)
        {
            SpriteRenderer island = Decor(decor, "Floating Island (background)", trees[1], 3f, 2f, 3f, -60);
            island.color = new Color(0.8f, 0.9f, 0.9f);
        }

        GameObject checkpointPrefab = CheckpointPrefab();
        PlaceCheckpoint(checkpointPrefab, "Checkpoint 1 (Tree)", -1.5f, -3f);

        // Lab 4 #11: bouncy platform
        GameObject pad = Block(ground, "Bouncy Pad", "GoldPlank", -7.5f, -5.5f, -3f + 0.3125f, -3f, bouncy);
        pad.GetComponent<SpriteRenderer>().sortingOrder = 1;

        // Lab 5 #17: bricks that break when hit from below
        Sprite exploded = Load(Art + "BrickExploded.png");
        for (int i = 0; i < 3; i++)
        {
            GameObject brick = MakeSprite(ground, "Brick " + (i + 1), Art + "Brick.png", new Vector2(1.5f + i, -0.3f), "floor", 1);
            brick.layer = GroundLayer;
            brick.AddComponent<BoxCollider2D>();
            brick.AddComponent<BrickBreak>().explodedBlock = exploded;
        }

        // ---------- Lake (Labs 4-5): deadly water with sparkles ----------
        Lake(hazards, "Lake", 6f, 10f, -3.3f, true);

        // ---------- Cliff: ground top at y = -2 ----------
        Block(ground, "Cliff", "CliffBlock", 10f, 22f, -2f, -10f, friction);
        PlaceCheckpoint(checkpointPrefab, "Checkpoint 2 (Cliff Edge)", 11f, -2f);

        // Lab 6 #20: slime walks between two pillars tagged Wall
        GameObject pillarA = Block(ground, "Wall Pillar 1", "StoneBlock", 13f, 14f, -1f, -2f, null);
        GameObject pillarB = Block(ground, "Wall Pillar 2", "StoneBlock", 20f, 21f, -1f, -2f, null);
        pillarA.tag = "Wall";
        pillarB.tag = "Wall";
        Slime(enemies, 17f, -2f);

        // ---------- Trapdoor (Lab 5 #16) over a water pit ----------
        Lake(hazards, "Trapdoor Pit", 22f, 26f, -5f, false);
        Trapdoor(ground, 22f, -2f);

        // ---------- Elevator up to the high ledge ----------
        Block(ground, "Ground 3", "GrassBlock", 26f, 31f, -2f, -10f, friction);
        Block(ground, "Elevator Well Floor", "GrassBlock", 31f, 33f, -5f, -10f, friction);
        RisingPlatform(ground, 31.2f, 32.8f, -4.85f, 1.35f);
        Block(ground, "High Ledge", "GrassBlock", 33f, 45f, 1.5f, -0.5f, friction);
        Block(ground, "Lower Path and End", "GrassBlock", 33f, 70f, -2f, -10f, friction);

        // Lab 6 #21: ghost spawns at the end of the ledge when the player passes the trigger
        GhostSpawner(enemies, 35.5f, 46f);

        // ---------- End ----------
        PlaceCheckpoint(checkpointPrefab, "Checkpoint 3 (End)", 47f, -2f);
        GameObject endWall = Block(ground, "End Wall", "StoneBlock", 60f, 61f, 6f, -2f, null);
        endWall.tag = "Wall";

        // Lab 6 #22: saw that follows the player
        Saw(enemies, player.transform, 57f, -0.5f);

        // Bushes and flowers along the way
        string b = "Assets/Bushes & Flowers/";
        (string sprite, float x, float top)[] plants =
        {
            ("Bush 1/Bush 1 - GREEN.png", -10.5f, -3f), ("Flower 11/Flower 11 - RED.png", -9f, -3f),
            ("Bush 2/Bush 2 - PINK.png", -0.2f, -3f), ("Flower 12/Flower 12 - YELLOW.png", 4.3f, -3f),
            ("Bush 1/Bush 1 - RED.png", 12.2f, -2f), ("Flower 10/Flower 10 - PINK.png", 15.5f, -2f),
            ("Bush 1 (No Flowers)/Bush 1 (no flowers) - GREEN.png", 18.5f, -2f), ("Flower 11/Flower 11 - YELLOW.png", 27.5f, -2f),
            ("Bush 2/Bush 2 - ORANGE.png", 29.5f, -2f), ("Bush 1/Bush 1 - TEAL.png", 37f, 1.5f),
            ("Flower 10/Flower 10 - PURPLE.png", 41f, 1.5f), ("Flower 12/Flower 12 - PINK.png", 39f, -2f),
            ("Bush 1/Bush 1 - WARM GREEN.png", 49.5f, -2f), ("Flower 11/Flower 11 - PINK.png", 52f, -2f),
            ("Bush 2/Bush 2 - RED.png", 54.5f, -2f),
        };
        foreach (var p in plants)
        {
            Sprite s = Load(b + p.sprite);
            if (s != null)
                Decor(decor, s.name, s, p.x, p.top - 0.05f, 1f, -30);
        }

        EditorSceneManager.MarkSceneDirty(root.gameObject.scene);
        EditorSceneManager.SaveScene(root.gameObject.scene);
        Selection.activeGameObject = root.gameObject;
        Debug.Log("Build Full Level: level built and SampleScene saved.");
    }

    // Lab 2 + Lab 6 #18: the player keeps its movement/animation setup and gets stats
    static void SetUpPlayer(move player)
    {
        Undo.RecordObject(player.gameObject, "Build Full Level");
        Undo.RecordObject(player.transform, "Build Full Level");
        player.gameObject.tag = "Player";
        player.transform.SetParent(null);
        player.transform.position = new Vector3(-9f, -2.4f, 0f);
        if (player.GetComponent<PlayerStats>() == null)
            Undo.AddComponent<PlayerStats>(player.gameObject);
    }

    // Lab 2 camera size, Lab 4 #12 follow limits, Lab 4 #13 zoom, plus a sky that moves with it
    static void SetUpCamera(Transform player)
    {
        Camera cam = Camera.main;
        if (cam == null)
            return;
        Undo.RecordObject(cam, "Build Full Level");
        cam.orthographic = true;
        cam.orthographicSize = 4f;

        camerafollow follow = cam.GetComponent<camerafollow>();
        if (follow == null)
            follow = Undo.AddComponent<camerafollow>(cam.gameObject);
        Undo.RecordObject(follow, "Build Full Level");
        follow.target = player;
        follow.cameraSpeed = 6f;
        follow.minX = -5f;
        follow.maxX = 53.5f;
        follow.minY = -1f;
        follow.maxY = 3f;
        cam.transform.position = new Vector3(follow.minX, follow.minY, -10f);

        if (cam.GetComponent<CameraZoom>() == null)
            Undo.AddComponent<CameraZoom>(cam.gameObject);

        foreach (string old in new[] { "Sky", "Clouds" })
        {
            Transform t = cam.transform.Find(old);
            if (t != null)
                Undo.DestroyObjectImmediate(t.gameObject);
        }
        GameObject sky = MakeSprite(cam.transform, "Sky", "Assets/sky.png", Vector2.zero, "Default", -100);
        sky.transform.localPosition = new Vector3(0f, 0f, 10f);
        sky.transform.localScale = new Vector3(22f, 4.6f, 1f);
        GameObject clouds = MakeSprite(cam.transform, "Clouds", "Assets/s_clouds.png", Vector2.zero, "Default", -90);
        clouds.transform.localPosition = new Vector3(0f, -3.6f, 10f);
        clouds.transform.localScale = new Vector3(6f, 3f, 1f);
        Undo.RegisterCreatedObjectUndo(sky, "Build Full Level");
        Undo.RegisterCreatedObjectUndo(clouds, "Build Full Level");
    }

    // Lab 5 #15: mushroom with a flag child that holds the trigger + Checkpoint script, saved as a prefab
    static GameObject CheckpointPrefab()
    {
        GameObject mushroom = MakeSprite(root, "Mushroom Checkpoint", "Assets/MUSHROOM .png", Vector2.zero, "charecter", -1);
        mushroom.transform.localScale = new Vector3(6.25f, 6.25f, 1f);

        GameObject flag = MakeSprite(mushroom.transform, "Checkpoint", Art + "Flag.png", Vector2.zero, "charecter", -2);
        flag.transform.localScale = new Vector3(0.16f, 0.16f, 1f);
        flag.transform.localPosition = new Vector3(0.12f, 0.1f, 0f);
        CircleCollider2D area = flag.AddComponent<CircleCollider2D>();
        area.isTrigger = true;
        area.radius = 0.8f;
        flag.AddComponent<checkpoint>();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(mushroom, "Assets/Prefabs/Checkpoint.prefab");
        Object.DestroyImmediate(mushroom);
        return prefab;
    }

    static void PlaceCheckpoint(GameObject prefab, string name, float x, float groundTop)
    {
        GameObject cp = (GameObject)PrefabUtility.InstantiatePrefab(prefab, root);
        cp.name = name;
        float half = cp.GetComponent<SpriteRenderer>().bounds.extents.y;
        cp.transform.position = new Vector3(x, groundTop + half, 0f);
    }

    static void Lake(Transform parent, string name, float x0, float x1, float waterTop, bool sparkles)
    {
        float width = x1 - x0;
        GameObject lake = MakeSprite(parent, name, "Assets/s_water_img.png", Vector2.zero, foreground, 5);
        SpriteRenderer sr = lake.GetComponent<SpriteRenderer>();
        lake.transform.localScale = new Vector3(width / sr.sprite.bounds.size.x, 1f, 1f);
        float height = sr.bounds.size.y;
        lake.transform.position = new Vector2((x0 + x1) / 2f, waterTop - height / 2f);

        GameObject deep = MakeSprite(parent, name + " (deep)", Art + "DeepWater.png", Vector2.zero, foreground, 4);
        SpriteRenderer deepSr = deep.GetComponent<SpriteRenderer>();
        deepSr.drawMode = SpriteDrawMode.Tiled;
        float deepTop = waterTop - height + 0.05f;
        deepSr.size = new Vector2(width, deepTop + 10f);
        deep.transform.position = new Vector2((x0 + x1) / 2f, (deepTop - 10f) / 2f);

        // Lab 5 #15: touching the water sends the player back to the last checkpoint
        BoxCollider2D trigger = lake.AddComponent<BoxCollider2D>();
        trigger.isTrigger = true;
        lake.AddComponent<DeadlyDrop>();

        if (!sparkles)
            return;

        // Lab 4 #14: built-in particle system sparkling on the water
        GameObject fx = new GameObject("Water Sparkles");
        fx.transform.SetParent(parent);
        fx.transform.position = new Vector3((x0 + x1) / 2f, waterTop - 0.5f, 0f);
        ParticleSystem ps = fx.AddComponent<ParticleSystem>();
        ParticleSystem.MainModule main = ps.main;
        main.loop = true;
        main.prewarm = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(1f, 2f);
        main.startSpeed = 0f;
        main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.3f);
        main.maxParticles = 200;
        ParticleSystem.EmissionModule emission = ps.emission;
        emission.rateOverTime = 20f;
        ParticleSystem.ShapeModule shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(width - 0.4f, 0.8f, 0.1f);
        ParticleSystem.ColorOverLifetimeModule col = ps.colorOverLifetime;
        col.enabled = true;
        Gradient g = new Gradient();
        Color lightBlue = new Color(0.8f, 0.95f, 1f);
        g.SetKeys(new[] { new GradientColorKey(lightBlue, 0f), new GradientColorKey(Color.white, 1f) },
                  new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.5f), new GradientAlphaKey(0f, 1f) });
        col.color = g;
        ParticleSystem.SizeOverLifetimeModule size = ps.sizeOverLifetime;
        size.enabled = true;
        size.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new Keyframe(0f, 0.3f), new Keyframe(0.5f, 1f), new Keyframe(1f, 0.3f)));
        ParticleSystemRenderer psr = fx.GetComponent<ParticleSystemRenderer>();
        psr.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Particles/Center_starten.mat");
        psr.sortingLayerName = foreground;
        psr.sortingOrder = 6;
    }

    // Lab 5 #16: kinematic plank on the left, hinged trapdoor on the right.
    // The motor holds the trapdoor up against its weight; the player's weight pushes it open.
    static void Trapdoor(Transform parent, float x, float top)
    {
        const float thick = 0.3125f;
        GameObject left = Block(parent, "Trapdoor Left Platform", "WoodPlank", x, x + 1f, top, top - thick, null);
        left.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;

        GameObject door = Block(parent, "Trapdoor", "WoodPlank", x + 1.02f, x + 3f, top, top - thick, null);
        Rigidbody2D body = door.AddComponent<Rigidbody2D>();
        body.mass = 1f;
        HingeJoint2D hinge = door.AddComponent<HingeJoint2D>();
        hinge.connectedBody = left.GetComponent<Rigidbody2D>();
        hinge.anchor = new Vector2(-0.99f, 0f);
        hinge.enableCollision = true;
        hinge.useLimits = true;
        hinge.limits = new JointAngleLimits2D { min = -90f, max = 0f };
        hinge.useMotor = true;
        hinge.motor = new JointMotor2D { motorSpeed = 100f, maxMotorTorque = 15f };
    }

    // Lab 5 optional: platform moving between Point A and Point B on the y axis
    static void RisingPlatform(Transform parent, float x0, float x1, float lowY, float highY)
    {
        float x = (x0 + x1) / 2f;
        GameObject platform = Block(parent, "Rising Platform", "WoodPlank", x0, x1, lowY + 0.15625f, lowY - 0.15625f, null);
        platform.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
        GameObject a = new GameObject("Point A");
        a.transform.SetParent(parent);
        a.transform.position = new Vector2(x, lowY);
        GameObject bPoint = new GameObject("Point B");
        bPoint.transform.SetParent(parent);
        bPoint.transform.position = new Vector2(x, highY);
        RisingPlatform rp = platform.AddComponent<RisingPlatform>();
        rp.PointA = a.transform;
        rp.PointB = bPoint.transform;
        rp.speed = 2f;
    }

    static void Slime(Transform parent, float x, float groundTop)
    {
        GameObject slime = MakeSprite(parent, "Slime", Art + "Slime.png", Vector2.zero, "charecter", 2);
        slime.transform.position = new Vector2(x, groundTop + slime.GetComponent<SpriteRenderer>().bounds.extents.y + 0.02f);
        slime.tag = "Enemy";
        slime.AddComponent<Rigidbody2D>().freezeRotation = true;
        BoxCollider2D body = slime.AddComponent<BoxCollider2D>();
        BoxCollider2D head = slime.AddComponent<BoxCollider2D>();
        head.isTrigger = true;
        head.size = new Vector2(body.size.x * 1.4f, body.size.y * 0.6f);
        head.offset = new Vector2(0f, body.size.y * 0.2f);
        SlimeEnemy enemy = slime.AddComponent<SlimeEnemy>();
        enemy.maxSpeed = 2;
        enemy.damage = 1;
    }

    static void GhostSpawner(Transform parent, float triggerX, float spawnX)
    {
        GameObject spawnPoint = new GameObject("Ghost Spawn Point");
        spawnPoint.transform.SetParent(parent);
        // flies left at the height of a player standing on the high ledge
        spawnPoint.transform.position = new Vector2(spawnX, 2f);

        GameObject trigger = new GameObject("Trigger Point");
        trigger.transform.SetParent(spawnPoint.transform);
        trigger.transform.position = new Vector2(triggerX, 1f);
        BoxCollider2D area = trigger.AddComponent<BoxCollider2D>();
        area.isTrigger = true;
        area.size = new Vector2(1f, 7f);
        area.offset = new Vector2(-0.5f, 0f);
        SpawnGhost spawner = trigger.AddComponent<SpawnGhost>();
        spawner.enemy = AssetDatabase.LoadAssetAtPath<Transform>("Assets/Prefabs/Spawned Ghost.prefab");
        spawner.spawnPoint = spawnPoint.transform;
    }

    static void Saw(Transform parent, Transform player, float x, float y)
    {
        GameObject saw = MakeSprite(parent, "Rotating Saw", "Assets/Saw.png", new Vector2(x, y), "charecter", 3);
        saw.tag = "Enemy";
        CircleCollider2D area = saw.AddComponent<CircleCollider2D>();
        area.isTrigger = true;
        area.radius = 0.28f;
        MovingSaw moving = saw.AddComponent<MovingSaw>();
        moving.maxSpeed = 2;
        moving.damage = 1;
        moving.player = player;
    }

    // A solid tiled block on the Ground layer spanning x0..x1 and bottom..top
    static GameObject Block(Transform parent, string name, string sprite, float x0, float x1, float top, float bottom, PhysicsMaterial2D material)
    {
        GameObject go = MakeSprite(parent, name, Art + sprite + ".png", new Vector2((x0 + x1) / 2f, (top + bottom) / 2f), "floor", 0);
        go.layer = GroundLayer;
        SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.size = new Vector2(x1 - x0, top - bottom);
        BoxCollider2D box = go.AddComponent<BoxCollider2D>();
        box.size = sr.size;
        box.sharedMaterial = material;
        return go;
    }

    static void Wall(Transform parent, string name, float x, float y, bool tagged)
    {
        GameObject wall = new GameObject(name);
        wall.transform.SetParent(parent);
        wall.transform.position = new Vector2(x, y);
        wall.AddComponent<BoxCollider2D>().size = new Vector2(1f, 20f);
        if (tagged)
            wall.tag = "Wall";
    }

    static SpriteRenderer Decor(Transform parent, string name, Sprite sprite, float x, float groundTop, float scale, int order)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent);
        go.transform.localScale = new Vector3(scale, scale, 1f);
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = order;
        // sit the bottom of the sprite on the ground
        go.transform.position = new Vector2(x, groundTop - sprite.bounds.min.y * scale);
        return sr;
    }

    static GameObject MakeSprite(Transform parent, string name, string path, Vector2 position, string layer, int order)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent);
        go.transform.position = position;
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Load(path);
        sr.sortingLayerName = layer;
        sr.sortingOrder = order;
        return go;
    }

    static Transform Group(string name)
    {
        Transform t = new GameObject(name).transform;
        t.SetParent(root);
        return t;
    }

    static Sprite Load(string path)
    {
        Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (s == null)
            Debug.LogWarning("Build Full Level: couldn't load sprite " + path);
        return s;
    }

    static PhysicsMaterial2D BouncyMaterial()
    {
        const string path = "Assets/Bouncy Material.physicsMaterial2D";
        PhysicsMaterial2D m = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(path);
        if (m == null)
        {
            m = new PhysicsMaterial2D("Bouncy Material") { friction = 0.4f, bounciness = 0.9f };
            AssetDatabase.CreateAsset(m, path);
        }
        return m;
    }

    static SerializedObject TagManager()
    {
        return new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
    }

    static void AddTag(string tag)
    {
        SerializedObject tagManager = TagManager();
        SerializedProperty tags = tagManager.FindProperty("tags");
        for (int i = 0; i < tags.arraySize; i++)
        {
            if (tags.GetArrayElementAtIndex(i).stringValue == tag)
                return;
        }
        tags.InsertArrayElementAtIndex(tags.arraySize);
        tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
        tagManager.ApplyModifiedProperties();
    }

    // Lab 4/5 put the water on a Foreground sorting layer in front of the player
    static bool AddSortingLayer(string name)
    {
        if (SortingLayer.layers.Any(l => l.name == name))
            return true;
        SerializedObject tagManager = TagManager();
        SerializedProperty layers = tagManager.FindProperty("m_SortingLayers");
        layers.InsertArrayElementAtIndex(layers.arraySize);
        SerializedProperty layer = layers.GetArrayElementAtIndex(layers.arraySize - 1);
        layer.FindPropertyRelative("name").stringValue = name;
        layer.FindPropertyRelative("uniqueID").intValue = Random.Range(1, int.MaxValue);
        layer.FindPropertyRelative("locked").boolValue = false;
        tagManager.ApplyModifiedProperties();
        return SortingLayer.layers.Any(l => l.name == name);
    }
}
