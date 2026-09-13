using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Unity.Cinemachine;
using Object = UnityEngine.Object;

public static class PA2LevelBuilder
{
    const string Root = "Assets/PA2";
    const string Source = "Assets/Sprites/Berie's_Adventure_Seaside_Asset_Pack_Free/PNG/";
    static Material spriteMaterial, particleMaterial;
    static Font font;
    static Transform world, decor, items;
    static int coinCount;
    static Color Ink = new Color(.04f, .13f, .20f), Cream = new Color(1, .96f, .84f), Teal = new Color(.02f, .51f, .53f);

    [MenuItem("PA2/Construir nivel costero")]
    public static void Build()
    {
        if (EditorApplication.isPlaying)
            throw new InvalidOperationException("Detén Play antes de construir.");
        // Conserva cualquier edición del PA1 antes de crear la escena de continuación.
        EditorSceneManager.SaveOpenScenes();
        Directory.CreateDirectory(Root + "/Materials");
        AssetDatabase.Refresh();
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        spriteMaterial = AssetDatabase.LoadAssetAtPath<Material>(Root + "/Materials/PixelUnlit.mat");
        if (spriteMaterial == null)
        {
            spriteMaterial = new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default"));
            AssetDatabase.CreateAsset(spriteMaterial, Root + "/Materials/PixelUnlit.mat");
        }
        particleMaterial = AssetDatabase.LoadAssetAtPath<Material>(Root + "/Materials/Particles.mat");
        if (particleMaterial == null)
        {
            particleMaterial = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            AssetDatabase.CreateAsset(particleMaterial, Root + "/Materials/Particles.mat");
        }
        PrepareSprites();
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        world = new GameObject("01 · ESCENARIO / Tilemaps").transform;
        decor = new GameObject("02 · COSTA / Decoración").transform;
        items = new GameObject("03 · INTERACCIONES").transform;
        var systems = new GameObject("04 · SISTEMAS").transform;
        var gm = new GameObject("GameManager · Singleton").AddComponent<GameManager>();
        gm.transform.SetParent(systems);
        var am = new GameObject("AudioManager · Música y efectos").AddComponent<AudioManager>();
        am.transform.SetParent(systems);
        am.musica = am.gameObject.AddComponent<AudioSource>();
        am.efectos = am.gameObject.AddComponent<AudioSource>();
        am.musica.clip = AssetDatabase.LoadAssetAtPath<AudioClip>(Root + "/Audio/CostaLoop.wav");
        am.musica.loop = true;
        am.musica.playOnAwake = true;
        am.musica.volume = .55f;
        am.efectos.playOnAwake = false;
        am.efectos.volume = .7f;
        am.clips = new[] { "Salto", "Moneda", "Dano", "Control", "Victoria" }.Select(n => AssetDatabase.LoadAssetAtPath<AudioClip>(Root + "/Audio/" + n + ".wav")).ToArray();
        var camera = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener)).GetComponent<Camera>();
        camera.tag = "MainCamera";
        camera.orthographic = true;
        camera.orthographicSize = 5.5f;
        camera.transform.position = new Vector3(6, 3, -10);
        camera.backgroundColor = new Color(.15f, .5f, 1);
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.gameObject.AddComponent<CinemachineBrain>();
        MakeBackground(camera.transform);
        var grid = new GameObject("Grid · 16 px = 1 unidad", typeof(Grid));
        grid.transform.SetParent(world);
        var ground = Map("Suelo y acantilados", grid.transform, true, 0);
        Block(ground, -4, 15, -5, -1);
        Block(ground, 19, 38, -5, -1);
        Block(ground, 42, 62, -5, -1);
        Block(ground, 66, 87, -5, -1);
        var platforms = Map("Plataformas elevadas", grid.transform, true, 1);
        Platform(platforms, 6, 9, 1);
        Platform(platforms, 22, 25, 1);
        Platform(platforms, 29, 32, 2);
        Platform(platforms, 45, 48, 1);
        Platform(platforms, 52, 55, 2);
        Platform(platforms, 69, 72, 1);
        Platform(platforms, 76, 79, 2);
        // Paredes laterales invisibles fuera de la zona visible.
        Wall("Límite izquierdo", -4.4f);
        Wall("Límite derecho", 88.4f);
        var moving = new GameObject("Plataforma móvil · PA1", typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(PlataformaMovil));
        moving.transform.SetParent(world);
        moving.transform.position = new Vector3(35, 1.7f);
        moving.layer = 8;
        var body = moving.GetComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        moving.GetComponent<BoxCollider2D>().size = new Vector2(2.8f, .3f);
        moving.GetComponent<PlataformaMovil>().distancia = 5;
        for (int i = 0; i < 3; i++)
            SpriteObject("Arena móvil", TileSprite(i == 0 ? "PlatformLeft" : i == 2 ? "PlatformRight" : "Platform"), moving.transform, new Vector3(i - 1, 0), 1, 2);
        var player = MakePlayer();
        var cm = new GameObject("Cinemachine · Seguimiento 2D").AddComponent<CinemachineCamera>();
        cm.Follow = player.transform;
        cm.Lens.OrthographicSize = 5.5f;
        cm.Lens.ModeOverride = LensSettings.OverrideModes.Orthographic;
        cm.transform.position = camera.transform.position;
        var composer = cm.gameObject.AddComponent<CinemachinePositionComposer>();
        composer.CameraDistance = 10;
        composer.TargetOffset = new Vector3(1.4f, 1.4f, 0);
        composer.Damping = new Vector3(.35f, .5f, 0);
        composer.Composition.ScreenPosition = new Vector2(-.15f, -.1f);
        composer.Composition.DeadZone.Enabled = true;
        composer.Composition.DeadZone.Size = new Vector2(.16f, .12f);
        var bounds = new GameObject("Confiner · Límites del mapa", typeof(PolygonCollider2D));
        bounds.transform.SetParent(world);
        var polygon = bounds.GetComponent<PolygonCollider2D>();
        polygon.isTrigger = true;
        polygon.points = new[] { new Vector2(-4, -5), new Vector2(-4, 13), new Vector2(88, 13), new Vector2(88, -5) };
        var confiner = cm.gameObject.AddComponent<CinemachineConfiner2D>();
        confiner.BoundingShape2D = polygon;
        confiner.Damping = .15f;
        var warp = cm.gameObject.AddComponent<CameraRespawn>();
        warp.jugador = player;
        warp.camara = cm;
        coinCount = 0;
        foreach (var v in new[] { new Vector2(3, .9f), new Vector2(5, .9f), new Vector2(7, 2.9f), new Vector2(9, 2.9f), new Vector2(14, .9f), new Vector2(20, .9f), new Vector2(23, 2.9f), new Vector2(25, 2.9f), new Vector2(29, 3.9f), new Vector2(31, 3.9f), new Vector2(34, .9f), new Vector2(37, 2.7f), new Vector2(43, .9f), new Vector2(46, 2.9f), new Vector2(48, 2.9f), new Vector2(53, 3.9f), new Vector2(55, 3.9f), new Vector2(59, .9f), new Vector2(61, .9f), new Vector2(67, .9f), new Vector2(70, 2.9f), new Vector2(72, 2.9f), new Vector2(77, 3.9f), new Vector2(79, 3.9f) })
            Coin(v);
        gm.totalMonedas = coinCount;
        foreach (float x in new[] { 11.5f, 12.4f, 27, 28, 50, 51, 57.5f, 74, 75 })
            Spike(x);
        MakeCheckpoint(21);
        MakeCheckpoint(44);
        MakeCheckpoint(68);
        // Barriles físicos del PA1: reaccionan al empuje del personaje.
        foreach (float x in new[] { 4.2f, 30.5f })
        {
            var barrel = SpriteObject("Barril empujable", Sprite("object_barrel_light_idle.png"), items, new Vector3(x, .55f), 1.2f, 3);
            barrel.gameObject.layer = 8;
            barrel.gameObject.AddComponent<BoxCollider2D>().size = new Vector2(.75f, .8f);
            var rb = barrel.gameObject.AddComponent<Rigidbody2D>();
            rb.mass = 2;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.gravityScale = 2.6f;
        }
        Decorate();
        MakeGoal(84);
        MakeHUD(player.GetComponent<PlayerHealth>(), camera);
        var es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene("Assets/Scenes/PA2_Costa.unity", true) };
        PlayerSettings.productName = "Berie - Rumbo al faro";
        PlayerSettings.defaultScreenWidth = 1280;
        PlayerSettings.defaultScreenHeight = 720;
        PlayerSettings.runInBackground = true;
        foreach (var map in Object.FindObjectsByType<Tilemap>(FindObjectsSortMode.None))
            map.RefreshAllTiles();
        foreach (var tc in Object.FindObjectsByType<TilemapCollider2D>(FindObjectsSortMode.None))
            tc.ProcessTilemapChanges();
        foreach (var cc in Object.FindObjectsByType<CompositeCollider2D>(FindObjectsSortMode.None))
        {
            cc.geometryType = CompositeCollider2D.GeometryType.Polygons;
            cc.GenerateGeometry();
            if (cc.pathCount == 0)
                throw new InvalidOperationException("Colisión vacía: " + cc.name);
        }
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/PA2_Costa.unity");
        AssetDatabase.SaveAssets();
        Selection.activeGameObject = player.gameObject;
        SceneView.lastActiveSceneView?.Frame(new Bounds(new Vector3(8, 2), new Vector3(24, 14, 1)), false);
        Debug.Log("PA2 construido: Tilemaps, Cinemachine, Animator, 24 monedas, HUD, audio y partículas.");
    }
    static void PrepareSprites()
    {
        string[] stems = { "character_berie_idle_", "character_berie_run_", "character_berie_jump_", "character_berie_fall_", "collectibles_coin_gold_", "collectibles_treasure_diamond_" };
        foreach (string file in Directory.GetFiles(Source, "*.png"))
        {
            string n = Path.GetFileName(file);
            if (stems.Any(n.StartsWith) || n == "background.png" || n.StartsWith("vegetation_tree_palm") || n.StartsWith("vegetatation_grass_small") || n == "trap_spike_1.png" || n == "object_barrel_light_idle.png")
                Import(file.Replace('\\', '/'), 32, n.StartsWith("character_") ? new Vector2(.5f, 0) : new Vector2(.5f, .5f));
        }
        foreach (string file in Directory.GetFiles(Root + "/Art/Terrain", "*.png"))
            Import(file.Replace('\\', '/'), 16, new Vector2(.5f, .5f));
    }
    static void Import(string path, int ppu, Vector2 pivot)
    {
        var ti = (TextureImporter)AssetImporter.GetAtPath(path);
        if (ti == null)
            return;
        ti.textureType = TextureImporterType.Sprite;
        ti.spriteImportMode = SpriteImportMode.Single;
        ti.spritePixelsPerUnit = ppu;
        ti.filterMode = FilterMode.Point;
        ti.textureCompression = TextureImporterCompression.Uncompressed;
        ti.mipmapEnabled = false;
        ti.alphaIsTransparency = true;
        var settings = new TextureImporterSettings();
        ti.ReadTextureSettings(settings);
        settings.spriteAlignment = (int)SpriteAlignment.Custom;
        settings.spritePivot = pivot;
        ti.SetTextureSettings(settings);
        ti.SaveAndReimport();
    }
    static Sprite Sprite(string name) => AssetDatabase.LoadAssetAtPath<Sprite>(Source + name);
    static Sprite TileSprite(string name) => AssetDatabase.LoadAssetAtPath<Sprite>(Root + "/Art/Terrain/" + name + ".png");
    static Sprite[] Frames(string stem) => Directory.GetFiles(Source, stem + "*.png").Where(f => int.TryParse(Path.GetFileNameWithoutExtension(f).Substring(stem.Length), out _)).OrderBy(f => int.Parse(Path.GetFileNameWithoutExtension(f).Substring(stem.Length))).Select(f => AssetDatabase.LoadAssetAtPath<Sprite>(f.Replace('\\', '/'))).ToArray();
    static SpriteRenderer SpriteObject(string name, Sprite sprite, Transform parent, Vector3 pos, float scale, int order)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent);
        go.transform.localPosition = pos;
        go.transform.localScale = Vector3.one * scale;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = order;
        sr.sharedMaterial = spriteMaterial;
        return sr;
    }
    static Tilemap Map(string name, Transform parent, bool collision, int order)
    {
        var go = new GameObject(name, typeof(Tilemap), typeof(TilemapRenderer));
        go.transform.SetParent(parent, false);
        var renderer = go.GetComponent<TilemapRenderer>();
        renderer.sharedMaterial = spriteMaterial;
        renderer.sortingOrder = order;
        if (collision)
        {
            go.layer = 8;
            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Static;
            var tc = go.AddComponent<TilemapCollider2D>();
            tc.compositeOperation = Collider2D.CompositeOperation.Merge;
            go.AddComponent<CompositeCollider2D>();
        }
        return go.GetComponent<Tilemap>();
    }
    static Tile Tile(string n)
    {
        string path = Root + "/Tiles/" + n + ".asset";
        var t = AssetDatabase.LoadAssetAtPath<Tile>(path);
        if (t != null)
            return t;
        t = ScriptableObject.CreateInstance<Tile>();
        t.sprite = TileSprite(n);
        t.colliderType = UnityEngine.Tilemaps.Tile.ColliderType.Grid;
        AssetDatabase.CreateAsset(t, path);
        return t;
    }
    static void Block(Tilemap map, int left, int right, int bottom, int top)
    {
        for (int x = left; x <= right; x++)
            for (int y = bottom; y <= top; y++)
            {
                string n = y == top ? (x == left ? "TopLeft" : x == right ? "TopRight" : "Top") : y == bottom ? (x == left ? "BottomLeft" : x == right ? "BottomRight" : "Bottom") : (x == left ? "Left" : x == right ? "Right" : "Fill");
                map.SetTile(new Vector3Int(x, y, 0), Tile(n));
            }
    }
    static void Platform(Tilemap map, int l, int r, int y)
    {
        for (int x = l; x <= r; x++)
            map.SetTile(new Vector3Int(x, y, 0), Tile(x == l ? "PlatformLeft" : x == r ? "PlatformRight" : "Platform"));
        var underside = Map("Borde decorativo " + l, map.transform.parent, false, -1);
        for (int x = l; x <= r; x++)
            underside.SetTile(new Vector3Int(x, y - 1, 0), Tile(x == l ? "PlatformBottomLeft" : x == r ? "PlatformBottomRight" : "PlatformBottom"));
    }
    static void Wall(string name, float x)
    {
        var go = new GameObject(name, typeof(BoxCollider2D));
        go.transform.SetParent(world);
        go.layer = 8;
        go.transform.position = new Vector3(x, 4);
        go.GetComponent<BoxCollider2D>().size = new Vector2(.5f, 18);
    }
    static PlayerController MakePlayer()
    {
        var go = new GameObject("Berie · Jugador PA1", typeof(Rigidbody2D), typeof(CapsuleCollider2D));
        go.tag = "Player";
        go.transform.position = new Vector3(1, .05f);
        var rb = go.GetComponent<Rigidbody2D>();
        rb.gravityScale = 2.6f;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        var col = go.GetComponent<CapsuleCollider2D>();
        col.size = new Vector2(.72f, 1.3f);
        col.offset = new Vector2(0, .65f);
        var mat = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(Root + "/Materials/Player.physicsMaterial2D");
        if (mat == null)
        {
            mat = new PhysicsMaterial2D("Sin fricción") { friction = 0, bounciness = 0 };
            AssetDatabase.CreateAsset(mat, Root + "/Materials/Player.physicsMaterial2D");
        }
        col.sharedMaterial = mat;
        var pc = go.AddComponent<PlayerController>();
        go.AddComponent<PlayerHealth>();
        var sr = SpriteObject("Visual · Animator", Frames("character_berie_idle_")[0], go.transform, Vector3.zero, 1.25f, 5);
        var animator = sr.gameObject.AddComponent<Animator>();
        animator.runtimeAnimatorController = MakeAnimator();
        var anim = go.AddComponent<PlayerAnimation>();
        anim.animator = animator;
        anim.imagen = sr;
        var fx = go.AddComponent<PlayerFeedback>();
        fx.polvo = Particles("Polvo al saltar", go.transform, Vector3.zero, new Color(1, .88f, .6f), false);
        fx.impacto = Particles("Impacto", go.transform, Vector3.up * .7f, new Color(1, .35f, .24f), false);
        return pc;
    }
    static AnimatorController MakeAnimator()
    {
        string path = Root + "/Animations/Berie.controller";
        var existing = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
        if (existing != null)
            return existing;
        var ac = AnimatorController.CreateAnimatorControllerAtPath(path);
        foreach (var s in new[] { "EnSuelo", "Corriendo", "Cayendo" })
            ac.AddParameter(s, AnimatorControllerParameterType.Bool);
        var sm = ac.layers[0].stateMachine;
        var idle = State(ac, sm, "Idle", "character_berie_idle_", 8, new Vector3(220, 80));
        var run = State(ac, sm, "Run", "character_berie_run_", 12, new Vector3(450, 80));
        var jump = State(ac, sm, "Jump", "character_berie_jump_", 10, new Vector3(220, 240));
        var fall = State(ac, sm, "Fall", "character_berie_fall_", 8, new Vector3(450, 240));
        sm.defaultState = idle;
        Transition(sm, idle, new[] { ("EnSuelo", true), ("Corriendo", false) });
        Transition(sm, run, new[] { ("EnSuelo", true), ("Corriendo", true) });
        Transition(sm, jump, new[] { ("EnSuelo", false), ("Cayendo", false) });
        Transition(sm, fall, new[] { ("EnSuelo", false), ("Cayendo", true) });
        return ac;
    }
    static AnimatorState State(AnimatorController ac, AnimatorStateMachine sm, string name, string stem, float fps, Vector3 pos)
    {
        var clip = new AnimationClip { name = name, frameRate = fps };
        var frames = Frames(stem);
        var keys = new ObjectReferenceKeyframe[frames.Length + 1];
        for (int i = 0; i < keys.Length; i++)
            keys[i] = new ObjectReferenceKeyframe { time = i / fps, value = frames[i % frames.Length] };
        AnimationUtility.SetObjectReferenceCurve(clip, new EditorCurveBinding { path = "", type = typeof(SpriteRenderer), propertyName = "m_Sprite" }, keys);
        var set = AnimationUtility.GetAnimationClipSettings(clip);
        set.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(clip, set);
        AssetDatabase.CreateAsset(clip, Root + "/Animations/" + name + ".anim");
        var st = sm.AddState(name, pos);
        st.motion = clip;
        return st;
    }
    static void Transition(AnimatorStateMachine sm, AnimatorState dest, (string, bool)[] conditions)
    {
        var t = sm.AddAnyStateTransition(dest);
        t.hasExitTime = false;
        t.duration = 0;
        t.canTransitionToSelf = false;
        foreach (var c in conditions)
            t.AddCondition(c.Item2 ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0, c.Item1);
    }
    static ParticleSystem Particles(string name, Transform parent, Vector3 position, Color color, bool burst)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent);
        go.transform.localPosition = position;
        var ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var main = ps.main;
        main.playOnAwake = false;
        main.loop = false;
        main.duration = .5f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(.25f, .55f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(.6f, 2);
        main.startSize = new ParticleSystem.MinMaxCurve(.05f, .13f);
        main.startColor = color;
        main.gravityModifier = .3f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = 0;
        if (burst)
            emission.SetBursts(new[] { new ParticleSystem.Burst(0, 16) });
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = .18f;
        var r = go.GetComponent<ParticleSystemRenderer>();
        r.sharedMaterial = particleMaterial;
        r.sortingOrder = 10;
        return ps;
    }
    static void Coin(Vector2 p)
    {
        var sr = SpriteObject("Moneda " + (++coinCount).ToString("00"), Frames("collectibles_coin_gold_")[0], items, p, 1.2f, 4);
        var go = sr.gameObject;
        go.AddComponent<CircleCollider2D>().isTrigger = true;
        go.GetComponent<CircleCollider2D>().radius = .3f;
        var loop = go.AddComponent<SpriteLoop>();
        loop.cuadros = Frames("collectibles_coin_gold_");
        go.AddComponent<Moneda>().destello = Particles("Destello de moneda", go.transform, Vector3.zero, new Color(1, .75f, .12f), true);
    }
    static void Spike(float x)
    {
        var sr = SpriteObject("Púas", Sprite("trap_spike_1.png"), items, new Vector3(x, .35f), 1.1f, 3);
        var c = sr.gameObject.AddComponent<BoxCollider2D>();
        c.size = new Vector2(.65f, .38f);
        c.offset = new Vector2(0, -.05f);
        c.isTrigger = true;
        sr.gameObject.AddComponent<Peligro>();
    }
    static void MakeCheckpoint(float x)
    {
        var go = new GameObject("Punto de control " + x);
        go.transform.SetParent(items);
        go.transform.position = new Vector3(x, 0);
        var c = go.AddComponent<BoxCollider2D>();
        c.size = new Vector2(1, 2);
        c.offset = Vector2.up;
        c.isTrigger = true;
        var sr = SpriteObject("Cristal", Frames("collectibles_treasure_diamond_")[0], go.transform, new Vector3(0, 1.5f), 1.8f, 4);
        sr.color = new Color(.65f, .75f, 1);
        var loop = sr.gameObject.AddComponent<SpriteLoop>();
        loop.cuadros = Frames("collectibles_treasure_diamond_");
        var cp = go.AddComponent<Checkpoint>();
        cp.indicador = sr;
        cp.reaparicion = new GameObject("Reaparición segura").transform;
        cp.reaparicion.SetParent(go.transform);
        cp.reaparicion.localPosition = new Vector3(0, .06f);
        Label("CONTROL", new Vector3(x, 2.3f), .12f, Cream);
    }
    static void MakeBackground(Transform camera)
    {
        var layer = new GameObject("Fondo costero · Parallax");
        layer.transform.SetParent(decor);
        layer.transform.position = new Vector3(6, 3, 0);
        var par = layer.AddComponent<ParallaxLayer>();
        par.camara = camera;
        par.factor = new Vector2(.88f, .75f);
        for (int i = -2; i <= 4; i++)
            SpriteObject("Mar y nubes " + i, Sprite("background.png"), layer.transform, new Vector3(i * 23.46667f, 0, 0), 2.346667f, -30);
    }
    static void Decorate()
    {
        foreach (float x in new[] { -1f, 8, 23, 33, 46, 54, 70, 81, 86 })
        {
            var sr = SpriteObject("Palmera", Sprite("vegetation_tree_palm1.png"), decor, new Vector3(x, 2.15f), 1.45f, -4);
            if ((int)x % 2 == 0)
                sr.flipX = true;
        }
        foreach (float x in new[] { 2f, 5, 10, 20, 26, 32, 35, 44, 49, 56, 60, 67, 73, 80, 85 })
            SpriteObject("Vegetación", Sprite("vegetatation_grass_small1.png"), decor, new Vector3(x, .35f), 1.1f, -2);
        Label("01  PLAYA DEL DESPERTAR", new Vector3(5, 4.4f), .15f, Cream);
        Label("02  SALTOS DE MAREA", new Vector3(31, 5.5f), .15f, Cream);
        Label("03  RUMBO AL FARO", new Vector3(70, 5.4f), .15f, Cream);
        Label("MOVER  A / D      SALTAR  ESPACIO", new Vector3(5, -2), .115f, Ink);
    }
    static void Label(string text, Vector3 p, float size, Color color)
    {
        var go = new GameObject(text);
        go.transform.SetParent(decor);
        go.transform.position = p;
        var tm = go.AddComponent<TextMesh>();
        tm.text = text;
        tm.font = font;
        tm.fontSize = 64;
        tm.characterSize = size * .32f;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.color = color;
        var r = go.GetComponent<MeshRenderer>();
        r.sharedMaterial = font.material;
        r.sortingOrder = 4;
    }
    static void MakeGoal(float x)
    {
        var go = new GameObject("Faro · Meta");
        go.transform.SetParent(items);
        go.transform.position = new Vector3(x, 0);
        var c = go.AddComponent<BoxCollider2D>();
        c.isTrigger = true;
        c.size = new Vector2(1.8f, 3);
        c.offset = new Vector2(0, 1.5f);
        go.AddComponent<Meta>();
        // Arquitectura del faro con los tiles de arena, rematada con el tesoro luminoso.
        var grid = new GameObject("Faro Grid", typeof(Grid));
        grid.transform.SetParent(go.transform);
        grid.transform.localPosition = new Vector3(-1, 0);
        var map = Map("Torre de arena", grid.transform, false, 2);
        Block(map, 0, 1, 0, 3);
        var crystal = SpriteObject("Luz del faro", Frames("collectibles_treasure_diamond_")[0], go.transform, new Vector3(0, 4.6f), 3, 4);
        crystal.gameObject.AddComponent<SpriteLoop>().cuadros = Frames("collectibles_treasure_diamond_");
        Label("META · 10 MONEDAS", new Vector3(x, 5.8f), .14f, Cream);
    }
    static RectTransform Rect(string name, Transform parent, Vector2 min, Vector2 max, Vector2 offsetMin, Vector2 offsetMax)
    {
        var rt = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = min;
        rt.anchorMax = max;
        rt.offsetMin = offsetMin;
        rt.offsetMax = offsetMax;
        return rt;
    }
    static Image Panel(string name, Transform parent, Vector2 min, Vector2 max, Vector2 lo, Vector2 hi, Color color)
    {
        var rt = Rect(name, parent, min, max, lo, hi);
        var im = rt.gameObject.AddComponent<Image>();
        im.color = color;
        return im;
    }
    static Text Text(string name, Transform parent, string value, int size, Vector2 min, Vector2 max, Vector2 lo, Vector2 hi, TextAnchor align, Color color)
    {
        var rt = Rect(name, parent, min, max, lo, hi);
        var t = rt.gameObject.AddComponent<Text>();
        t.font = font;
        t.text = value;
        t.fontSize = size;
        t.color = color;
        t.alignment = align;
        t.raycastTarget = false;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Truncate;
        return t;
    }
    static Button Button(string name, Transform parent, string value, Vector2 min, Vector2 max, Vector2 lo, Vector2 hi, Color color, out Text label)
    {
        var im = Panel(name, parent, min, max, lo, hi, color);
        var b = im.gameObject.AddComponent<Button>();
        b.targetGraphic = im;
        var colors = b.colors;
        colors.highlightedColor = new Color(.8f, 1, 1);
        colors.pressedColor = new Color(.6f, .8f, .8f);
        b.colors = colors;
        label = Text("Etiqueta", im.transform, value, 19, Vector2.zero, Vector2.one, new Vector2(8, 0), new Vector2(-8, 0), TextAnchor.MiddleCenter, Cream);
        return b;
    }
    static void MakeHUD(PlayerHealth health, Camera camera)
    {
        var go = new GameObject("05 · UI / Canvas responsivo", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = camera;
        canvas.planeDistance = 1;
        canvas.sortingOrder = 100;
        var scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.matchWidthOrHeight = .5f;
        var hud = go.AddComponent<GameHUD>();
        hud.vida = health;
        var top = Panel("HUD superior", go.transform, new Vector2(0, 1), Vector2.one, new Vector2(24, -82), new Vector2(-24, -22), new Color(.03f, .12f, .2f, .94f));
        hud.monedas = Text("Monedas", top.transform, "", 21, Vector2.zero, new Vector2(.34f, 1), new Vector2(22, 0), Vector2.zero, TextAnchor.MiddleLeft, Cream);
        hud.vidas = Text("Vidas", top.transform, "", 21, new Vector2(.34f, 0), new Vector2(.60f, 1), Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, new Color(1, .67f, .44f));
        hud.tiempo = Text("Tiempo", top.transform, "", 21, new Vector2(.61f, 0), new Vector2(.83f, 1), Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, Cream);
        hud.pausa = Button("Pausa", top.transform, "II  PAUSA", new Vector2(.84f, 0), Vector2.one, new Vector2(4, 9), new Vector2(-10, -9), Teal, out _);
        var bottom = Panel("Objetivo", go.transform, Vector2.zero, new Vector2(1, 0), new Vector2(24, 20), new Vector2(-24, 66), new Color(.03f, .12f, .2f, .90f));
        hud.mensaje = Text("Mensaje", bottom.transform, "", 19, Vector2.zero, Vector2.one, new Vector2(18, 0), new Vector2(-18, 0), TextAnchor.MiddleCenter, Cream);
        var shade = Panel("Pantalla de estado", go.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(.02f, .08f, .13f, .66f));
        hud.panel = shade.gameObject;
        var card = Panel("Panel central", shade.transform, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(-280, -296), new Vector2(280, 296), new Color(.035f, .14f, .22f, .98f));
        Text("Marca", card.transform, "PA2  /  AVENTURA COSTERA", 15, new Vector2(0, 1), Vector2.one, new Vector2(20, -53), new Vector2(-20, -25), TextAnchor.MiddleCenter, new Color(.4f, .88f, .84f));
        hud.titulo = Text("Título", card.transform, "", 38, new Vector2(0, 1), Vector2.one, new Vector2(22, -163), new Vector2(-22, -62), TextAnchor.MiddleCenter, Cream);
        hud.titulo.fontStyle = FontStyle.Bold;
        hud.descripcion = Text("Descripción", card.transform, "", 20, new Vector2(0, 0), Vector2.one, new Vector2(30, 162), new Vector2(-30, -175), TextAnchor.MiddleCenter, Cream);
        hud.principal = Button("Acción principal", card.transform, "", new Vector2(0, 0), new Vector2(1, 0), new Vector2(44, 94), new Vector2(-44, 146), Teal, out hud.botonTexto);
        hud.reiniciar = Button("Reiniciar", card.transform, "REINICIAR", new Vector2(0, 0), new Vector2(.5f, 0), new Vector2(44, 40), new Vector2(-7, 79), new Color(.18f, .28f, .35f), out _);
        hud.sonido = Button("Sonido", card.transform, "SONIDO: SÍ", new Vector2(.5f, 0), new Vector2(1, 0), new Vector2(7, 40), new Vector2(-44, 79), new Color(.18f, .28f, .35f), out hud.sonidoTexto);
        Text("Créditos", card.transform, "Arte: Crusenho  ·  Berie's Adventure Seaside Asset Pack", 12, Vector2.zero, new Vector2(1, 0), new Vector2(12, 9), new Vector2(-12, 31), TextAnchor.MiddleCenter, new Color(.55f, .72f, .78f));
    }
}
