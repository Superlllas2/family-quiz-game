using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FamilyQuiz.Presentation.Staging;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace FamilyQuiz.EditorTools.Studio
{
    /// <summary>
    /// Builds Assets/Scenes/Studio.unity from Studio.fbx (docs/21_staging.md, "Game night 1 version"; 0015, 0016):
    /// materials by slot name, static flags, the lights of one look, light probes, a reflection probe, the film look,
    /// the camera and its shot points, then bakes the lightmaps and adds the scene to the build.
    /// Command line (no -quit, it exits by itself):
    /// Unity -batchmode -projectPath unity -executeMethod FamilyQuiz.EditorTools.Studio.StudioBuilder.Run
    ///       [-look warm|retro|modern] [-bake] [-textures | -procTextures] [-capture outDir] -logFile build.log
    /// (-textures remakes the procedural textures and the scan masks, -procTextures only the procedural ones)
    /// </summary>
    public static class StudioBuilder
    {
        public const string ScenePath = "Assets/Scenes/Studio.unity";
        public const string ModelPath = "Assets/Game/Art/Models/Studio.fbx";
        public const string LooksFolder = "Assets/Game/Art/Looks";

        private static readonly string[] ProbeLitPieces =
        {
            "HostShelfBooks", "DeskProps", "CoffeeTableProps", "DrumKit", "Keyboard", "Amps", "Guitar", "Bass", "PlantBig", "PlantStage",
            "PlantShelf2", "PlantBand", "Pendants", "FloorLamp", "SideTable", "Sconces", "BandSconce", "Truss", "BandGear", "SideTable2",
            "SofaLegs", "ArmchairLegs", "HostChairs", "StageLamp1", "StageLamp2", "ProsceniumTrim", "QuestionBoardFrame"
        };

        public static void Run()
        {
            try
            {
                var args = Environment.GetCommandLineArgs();
                string look = Arg(args, "-look") ?? StudioLooks.WarmLateNight;
                bool bake = args.Contains("-bake");
                if (args.Contains("-textures") || args.Contains("-procTextures") || !File.Exists(StudioTextures.Folder + "/Rug3.png"))
                    StudioTextures.GenerateAll();
                if (args.Contains("-textures")) StudioScans.PrepareAll(true);
                if (args.Contains("-textures") || args.Contains("-procTextures") || !File.Exists(StudioTextures.Folder + "/CityWindows.png")) StudioTextures.CityWindows();
                // -captureOnly renders the saved scene without rebuilding it; -hide <name> hides one object first.
                if (!args.Contains("-captureOnly")) Build(StudioLooks.Get(look), bake);
                string capture = Arg(args, "-capture");
                if (capture != null) StudioCapture.CaptureAll(capture, look, Arg(args, "-hide"));
                EditorApplication.Exit(0);
            }
            catch (Exception e)
            {
                Debug.LogError("Studio build failed: " + e);
                EditorApplication.Exit(1);
            }
        }

        [MenuItem("Family Quiz/Studio/Build scene (warm, no bake)")]
        public static void BuildFromMenu()
        {
            Build(StudioLooks.Get(StudioLooks.WarmLateNight), false);
        }

        public static string Arg(string[] args, string name)
        {
            for (int i = 0; i < args.Length - 1; i++)
                if (args[i] == name) return args[i + 1];
            return null;
        }

        public static void Build(StudioLook look, bool bake)
        {
            AssetDatabase.Refresh();
            StudioScans.PrepareAll(false);
            var materials = StudioMaterials.CreateAll();
            materials["M_City"].SetColor("_EmissionColor", Color.white * look.CityEmission);
            // The near blocks a little dimmer and cooler than the skyline, so the window reads as a night city, not a
            // wall of lit windows.
            materials["M_CityBuilding"].SetColor("_EmissionColor", new Color(0.82f, 0.88f, 1f) * look.CityEmission * 0.42f);
            materials["M_CityBuildingFar"].SetColor("_EmissionColor", new Color(0.7f, 0.78f, 1f) * look.CityEmission * 0.3f);
            materials["M_GlowStrip"].SetColor("_EmissionColor", look.Strip * 0.16f);
            materials["M_Bulb"].SetColor("_EmissionColor", Color.Lerp(look.Practical, Color.white, 0.35f) * 10f);

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new GameObject("Studio");
            var rig = root.AddComponent<StudioRig>();

            var model = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
            if (model == null) throw new InvalidOperationException("No model at " + ModelPath + "; run tools/blender/build_studio.py");
            var set = (GameObject)PrefabUtility.InstantiatePrefab(model, root.transform);
            set.name = "Set";
            var markers = new Dictionary<string, Transform>();
            foreach (var t in set.GetComponentsInChildren<Transform>()) markers[t.name] = t;

            SetUpRenderers(set, materials);
            var props = StudioProps.Load();
            var propMaterials = StudioProps.CreateMaterials(props, look.Practical);
            StudioProps.Prune(propMaterials);
            StudioProps.Place(root.transform, markers, props, propMaterials, DesignFrame);
            rig.CurtainLeft = Find(markers, "Dyn_CurtainL");
            rig.CurtainRight = Find(markers, "Dyn_CurtainR");
            for (int i = 0; i < 8; i++) rig.Seats[i] = Find(markers, "Seat_" + (i + 1));
            rig.HostKonstantin = Find(markers, "Host_konstantin");
            rig.HostIlja = Find(markers, "Host_ilja");

            var lights = new GameObject("Lights").transform;
            lights.SetParent(root.transform);
            AddLights(look, markers, lights, rig);
            AddProbes(root.transform, markers);
            AddCameraAndShots(root.transform, rig, markers);
            rig.Volume = AddVolume(root.transform, look);

            RenderSettings.skybox = null;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = look.Ambient;
            RenderSettings.defaultReflectionMode = DefaultReflectionMode.Custom;
            RenderSettings.fog = false;

            Lightmapping.lightingSettings = LightingFor(look);
            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            EditorSceneManager.SaveScene(scene, ScenePath);
            AddToBuild();

            if (bake)
            {
                var started = DateTime.Now;
                Debug.Log("Studio: baking look " + look.Name + " ...");
                bool ok = Lightmapping.Bake();
                Debug.Log("Studio: bake " + (ok ? "done" : "FAILED") + " in " + (DateTime.Now - started).TotalSeconds.ToString("F0") + " s");
                EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), ScenePath);
            }

            Debug.Log("Studio: built " + ScenePath + " with look " + look.Name);
        }

        private static Transform Find(Dictionary<string, Transform> markers, string name)
        {
            if (!markers.TryGetValue(name, out var t)) throw new InvalidOperationException("Studio.fbx has no " + name);
            return t;
        }

        private static void SetUpRenderers(GameObject set, Dictionary<string, Material> materials)
        {
            foreach (var renderer in set.GetComponentsInChildren<MeshRenderer>())
            {
                var mats = renderer.sharedMaterials;
                for (int i = 0; i < mats.Length; i++)
                {
                    string name = mats[i] != null ? mats[i].name : "";
                    if (materials.TryGetValue(name, out var m)) mats[i] = m;
                    else Debug.LogWarning("Studio: no material for slot " + name + " on " + renderer.name);
                }

                renderer.sharedMaterials = mats;
                var go = renderer.gameObject;
                bool dynamic = go.name.StartsWith("Dyn_");
                bool city = go.name.StartsWith("City");
                bool glass = go.name.StartsWith("Glass");
                renderer.shadowCastingMode = city || glass ? ShadowCastingMode.Off : ShadowCastingMode.On;
                if (dynamic || city || glass)
                {
                    GameObjectUtility.SetStaticEditorFlags(go, 0);
                    renderer.lightProbeUsage = city ? LightProbeUsage.Off : LightProbeUsage.BlendProbes;
                    continue;
                }

                GameObjectUtility.SetStaticEditorFlags(go, StaticEditorFlags.ContributeGI | StaticEditorFlags.BatchingStatic
                                                           | StaticEditorFlags.ReflectionProbeStatic | StaticEditorFlags.OccludeeStatic);
                renderer.receiveGI = ProbeLitPieces.Contains(go.name) ? ReceiveGI.LightProbes : ReceiveGI.Lightmaps;
                var so = new SerializedObject(renderer);
                var scale = so.FindProperty("m_ScaleInLightmap");
                if (scale != null)
                {
                    scale.floatValue = go.name == "Ceiling" ? 0.05f : go.name == "Floor" ? 0.5f : go.name.EndsWith("Wall") ? 0.7f : 1f;
                    so.ApplyModifiedPropertiesWithoutUndo();
                }
            }
        }

        // ------------------------------------------------------------------------ lights

        private static void AddLights(StudioLook look, Dictionary<string, Transform> markers, Transform parent, StudioRig rig)
        {
            foreach (var pair in markers.Where(p => p.Key.StartsWith("LP_")).OrderBy(p => p.Key))
            {
                var at = pair.Value.position;
                string kind = pair.Key.Split('_')[1];
                var p = look.Practical;
                switch (kind)
                {
                    case "Pendant":
                        Point(parent, pair.Key, at, p, 12f * look.PracticalScale, 6f, 0.08f);
                        break;
                    case "FloorLamp":
                        Point(parent, pair.Key, at, p, 10f * look.PracticalScale, 5f, 0.15f);
                        break;
                    case "TableLamp":
                        Point(parent, pair.Key, at, p, 5f * look.PracticalScale, 3f, 0.1f);
                        break;
                    case "DeskLamp":
                        Point(parent, pair.Key, at, p, 4f * look.PracticalScale, 2.6f, 0.08f);
                        break;
                    case "Candle":
                        // A candle flame: about 1900 K, small and soft.
                        Point(parent, pair.Key, at, Color.Lerp(p, new Color(1f, 0.5f, 0.2f), 0.5f), 1.6f * look.PracticalScale, 2.4f, 0.02f);
                        break;
                    case "Festoon":
                        // One of every few bulbs of a festoon string: small warm pools along the string.
                        Point(parent, pair.Key, at, p, 1.8f * look.PracticalScale, 3f, 0.04f);
                        break;
                    case "Shelf":
                        Point(parent, pair.Key, at, p, 3.5f * look.PracticalScale, 2.2f, 0.05f);
                        break;
                    case "Sconce":
                        Point(parent, pair.Key, at, p, 6f * look.PracticalScale, 4f, 0.08f);
                        break;
                    case "DeskStrip":
                        // Directions are in the marker's frame: its zone may be turned (build_studio.py, ZONES).
                        Area(parent, pair.Key, at, DesignFrame(pair.Value) * Quaternion.LookRotation(new Vector3(0, -0.55f, -1f)),
                            new Vector2(2.5f, 0.06f), look.Strip, 6f, 3f);
                        break;
                    case "Window":
                        Area(parent, pair.Key, at + DesignFrame(pair.Value) * new Vector3(0, 0, -0.1f),
                            DesignFrame(pair.Value) * Quaternion.LookRotation(new Vector3(0.1f, -0.7f, -1f)),
                            new Vector2(7.6f, 3.6f), look.Window, look.WindowIntensity, 12f);
                        break;
                    case "StageLamp":
                    {
                        var target = Find(markers, "LT_Board").position;
                        // A wide soft cone: on the closed curtain it makes a soft pool instead of a hard oval.
                        var l = Spot(parent, pair.Key, at, target, look.StageLamp, look.StageLampIntensity, 12f, 40f, LightmapBakeType.Realtime);
                        l.innerSpotAngle = 4f;
                        l.shadows = LightShadows.None;
                        var lamps = (rig.StageLamps ?? new Light[0]).ToList();
                        lamps.Add(l);
                        rig.StageLamps = lamps.ToArray();
                        break;
                    }
                }
            }

            // The set's own light markers, LK_<name> aimed at LKT_<name>, placed per zone in build_studio.py.
            foreach (var pair in markers.Where(p => p.Key.StartsWith("LK_")).OrderBy(p => p.Key))
            {
                string name = pair.Key.Substring(3);
                var at = pair.Value.position;
                var target = Find(markers, "LKT_" + name).position;
                if (name == "Fill")
                {
                    // Soft fill from the audience side, like the big soft lights of a TV studio.
                    Area(parent, "Fill", at, Quaternion.LookRotation(target - at), new Vector2(18f, 4f), look.Fill, look.FillIntensity, 28f);
                }
                else if (name.StartsWith("Uplight"))
                {
                    // Uplights grazing the walls beside the window, the proscenium and the band.
                    Spot(parent, name, at, target, look.Accent, look.AccentIntensity, 6f, 55f, LightmapBakeType.Baked).shadowRadius = 0.05f;
                }
                else if (name.StartsWith("FloorPool"))
                {
                    // A soft warm pool on the open floor.
                    var pool = Spot(parent, name, at, target, look.Practical, 24f, 9f, 50f, LightmapBakeType.Baked);
                    pool.innerSpotAngle = 8f;
                    pool.shadowRadius = 0.3f;
                }
                else if (name == "BandTop")
                {
                    // A warm light over the band riser.
                    Spot(parent, name, at, target, look.Practical, 80f, 10f, 70f, LightmapBakeType.Baked);
                }
                else if (name == "BandFront")
                {
                    // A soft front light on the instruments, so they read from the Wide and Establish shots. Mixed
                    // (direct light in realtime, no shadows): the drums and keys are probe lit and would barely
                    // catch a baked light.
                    var front = Spot(parent, name, at, target, look.Key, 40f, 12f, 55f, LightmapBakeType.Mixed);
                    front.shadows = LightShadows.None;
                }
                else if (name.StartsWith("Rim"))
                {
                    // Realtime rims from behind and above the guests and the hosts: no shadows, so they stay cheap.
                    // A narrow cone: wider, it catches the brass pendant shades over the sofa and they glare.
                    var rim = Spot(parent, name, at, target, look.Rim, look.RimIntensity, 9f, 46f, LightmapBakeType.Realtime);
                    rim.innerSpotAngle = 20f;
                    rim.shadows = LightShadows.None;
                }
                else if (name == "CurtainWash")
                {
                    // A soft realtime wash on the curtain (it moves, so it cannot be baked).
                    var wash = Spot(parent, name, at, target, look.StageLamp, 100f, 12f, 60f, LightmapBakeType.Realtime);
                    wash.shadows = LightShadows.None;
                    wash.innerSpotAngle = 10f;
                }
                else if (name == "GuestKey")
                {
                    // Realtime keys: the guests and hosts move, so their light and shadows are realtime (0015).
                    rig.GuestKey = Spot(parent, name, at, target, look.Key, look.GuestKeyIntensity, 14f, 62f, LightmapBakeType.Mixed);
                    rig.GuestKey.shadows = LightShadows.Soft;
                }
                else if (name == "HostKey")
                {
                    rig.HostKey = Spot(parent, name, at, target, look.Key, look.HostKeyIntensity, 10f, 48f, LightmapBakeType.Mixed);
                    rig.HostKey.shadows = LightShadows.Soft;
                }
                else Debug.LogWarning("Studio: no light for marker " + pair.Key);
            }
        }

        private static Light NewLight(Transform parent, string name, LightType type, Color color, float intensity, float range,
            LightmapBakeType mode)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var light = go.AddComponent<Light>();
            light.type = type;
            light.color = color;
            light.intensity = intensity;
            light.range = range;
            light.lightmapBakeType = mode;
            light.shadows = LightShadows.Soft;
            go.AddComponent<UniversalAdditionalLightData>();
            return light;
        }

        private static Light Point(Transform parent, string name, Vector3 at, Color color, float intensity, float range, float radius)
        {
            var l = NewLight(parent, name, LightType.Point, color, intensity, range, LightmapBakeType.Baked);
            l.transform.position = at;
            l.shadowRadius = radius;
            return l;
        }

        private static Light Spot(Transform parent, string name, Vector3 at, Vector3 target, Color color, float intensity, float range,
            float angle, LightmapBakeType mode)
        {
            var l = NewLight(parent, name, LightType.Spot, color, intensity, range, mode);
            l.transform.position = at;
            l.transform.rotation = Quaternion.LookRotation(target - at);
            l.spotAngle = angle;
            l.innerSpotAngle = angle * 0.3f;
            l.shadowRadius = 0.2f;
            return l;
        }

        private static Light Area(Transform parent, string name, Vector3 at, Quaternion rotation, Vector2 size, Color color, float intensity,
            float range)
        {
            var l = NewLight(parent, name, LightType.Rectangle, color, intensity, range, LightmapBakeType.Baked);
            l.transform.position = at;
            l.transform.rotation = rotation;
            l.areaSize = size;
            return l;
        }

        // ------------------------------------------------------------------------ probes, camera, look

        /// <summary>
        /// Light probes laid out per zone in the zone's own frame (Zone_* markers from build_studio.py: x along its wall,
        /// z towards the wall), so they follow the bay when the layout changes.
        /// </summary>
        private static void AddProbes(Transform parent, Dictionary<string, Transform> markers)
        {
            var guests = Find(markers, "Zone_Guests");
            var stage = Find(markers, "Zone_Stage");
            var band = Find(markers, "Zone_Band");
            CheckZoneFrame(guests, markers);

            var go = new GameObject("LightProbes");
            go.transform.SetParent(parent, false);
            var group = go.AddComponent<LightProbeGroup>();
            var points = new List<Vector3>();
            for (float x = -9.5f; x <= 2.51f; x += 1f)
            for (float z = 2.0f; z <= 4.81f; z += 0.9f)
            foreach (float y in new[] { 0.45f, 1.1f, 1.9f })
                points.Add(ZonePoint(guests, new Vector3(x, y, z)));
            // Behind the arch, and over the apron in front of it (build_studio.py, APRON_FRONT).
            for (float x = 3.5f; x <= 7.51f; x += 1f)
            foreach (float z in new[] { 4.3f, 5.3f })
            foreach (float y in new[] { 0.9f, 2.1f, 3.4f })
                points.Add(ZonePoint(stage, new Vector3(x, y, z)));
            for (float x = 2.0f; x <= 9.61f; x += 0.95f)
            foreach (float z in new[] { 2.1f, 3.2f })
            foreach (float y in new[] { 0.9f, 1.9f, 3.0f })
                points.Add(ZonePoint(stage, new Vector3(x, y, z)));
            // The band stage and its drum riser.
            for (float x = 9.0f; x <= 13.61f; x += 1.15f)
            foreach (float z in new[] { 2.2f, 3.6f, 5.0f })
            foreach (float y in new[] { 0.9f, 1.9f })
                points.Add(ZonePoint(band, new Vector3(x, y, z)));
            // The open floor of the bay, kept clear of the walls.
            for (float x = -8f; x <= 13.01f; x += 2.5f)
            for (float z = -9f; z <= 2.51f; z += 2.5f)
            {
                var p = new Vector3(x, 1.2f, z);
                if (InZone(guests, p).z < 1.6f && InZone(stage, p).z < 1.0f && InZone(band, p).z < 1.1f)
                    points.Add(p);
            }

            group.probePositions = points.ToArray();

            var probe = new GameObject("ReflectionProbe").AddComponent<ReflectionProbe>();
            probe.transform.SetParent(parent, false);
            probe.transform.position = new Vector3(2f, 2.5f, -1.5f);
            probe.mode = ReflectionProbeMode.Baked;
            probe.size = new Vector3(30f, 7f, 20f);
            probe.boxProjection = true;
            probe.resolution = 512;
        }

        /// <summary>
        /// A marker's design frame: x along its wall, z towards the wall. Empties arrive from the FBX turned half a turn
        /// about y (their forward faces the audience), so the design frame is that rotation turned back.
        /// </summary>
        private static Quaternion DesignFrame(Transform marker)
        {
            return marker.rotation * Quaternion.Euler(0f, 180f, 0f);
        }

        private static Vector3 ZonePoint(Transform zone, Vector3 local)
        {
            return zone.position + DesignFrame(zone) * local;
        }

        private static Vector3 InZone(Transform zone, Vector3 world)
        {
            return Quaternion.Inverse(DesignFrame(zone)) * (world - zone.position);
        }

        /// <summary>The guest zone's frame must map its design points to where the model put them (CAMT_Seats).</summary>
        private static void CheckZoneFrame(Transform guests, Dictionary<string, Transform> markers)
        {
            var expected = Find(markers, "CAMT_Seats").position;
            var got = ZonePoint(guests, new Vector3(-4.6f, 1.45f, 3.6f));
            if (Vector3.Distance(expected, got) > 0.05f)
                throw new InvalidOperationException("Zone_Guests frame is off: " + got + " instead of " + expected);
        }

        private static void AddCameraAndShots(Transform parent, StudioRig rig, Dictionary<string, Transform> markers)
        {
            var cameraGo = new GameObject("StudioCamera");
            cameraGo.transform.SetParent(parent, false);
            var camera = cameraGo.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 700f;
            camera.allowHDR = true;
            camera.allowMSAA = false;
            var data = cameraGo.AddComponent<UniversalAdditionalCameraData>();
            data.renderPostProcessing = true;
            data.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
            data.antialiasingQuality = AntialiasingQuality.High;
            rig.Camera = camera;

            // Every shot is a CAM_<name> marker aimed at CAMT_<name>, placed per zone in build_studio.py.
            var shots = new GameObject("Shots").transform;
            shots.SetParent(parent, false);
            Transform Shot(string name) => ShotFrom(shots, markers, name);
            rig.WideShot = Shot("Wide");
            rig.SeatsShot = Shot("Seats");
            rig.EstablishStart = Shot("EstablishStart");
            rig.EstablishEnd = Shot("EstablishEnd");
            rig.HostSoloKonstantin = Shot("HostSoloKonstantin");
            rig.HostSoloIlja = Shot("HostSoloIlja");
            rig.CurtainShot = Shot("Curtain");
            rig.StageShot = Shot("Stage");
            rig.StagePushIn = Shot("StagePushIn");
            camera.transform.SetPositionAndRotation(rig.WideShot.position, rig.WideShot.rotation);
            camera.fieldOfView = rig.WideFieldOfView;
        }

        private static Transform ShotFrom(Transform parent, Dictionary<string, Transform> markers, string name)
        {
            var at = Find(markers, "CAM_" + name).position;
            var lookAt = Find(markers, "CAMT_" + name).position;
            var t = new GameObject("Shot_" + name).transform;
            t.SetParent(parent, false);
            t.SetPositionAndRotation(at, Quaternion.LookRotation(lookAt - at));
            return t;
        }

        private static Volume AddVolume(Transform parent, StudioLook look)
        {
            Directory.CreateDirectory(LooksFolder);
            string path = LooksFolder + "/Look_" + look.Name + ".asset";
            AssetDatabase.DeleteAsset(path);
            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, path);

            var tone = profile.Add<Tonemapping>(true);
            tone.mode.Override(TonemappingMode.ACES);
            var color = profile.Add<ColorAdjustments>(true);
            color.postExposure.Override(look.PostExposure);
            color.contrast.Override(look.Contrast);
            color.saturation.Override(look.Saturation);
            var wb = profile.Add<WhiteBalance>(true);
            wb.temperature.Override(look.Temperature);
            wb.tint.Override(look.Tint);
            var bloom = profile.Add<Bloom>(true);
            bloom.intensity.Override(look.BloomIntensity);
            bloom.threshold.Override(look.BloomThreshold);
            bloom.scatter.Override(0.72f);
            var vignette = profile.Add<Vignette>(true);
            vignette.intensity.Override(look.Vignette);
            vignette.smoothness.Override(0.45f);
            var grain = profile.Add<FilmGrain>(true);
            grain.type.Override(look.LargeGrain ? FilmGrainLookup.Medium4 : FilmGrainLookup.Thin2);
            grain.intensity.Override(look.Grain);
            grain.response.Override(0.8f);
            if (look.ChromaticAberration > 0)
                profile.Add<ChromaticAberration>(true).intensity.Override(look.ChromaticAberration);
            if (look.SplitShadows != Color.grey || look.SplitHighlights != Color.grey)
            {
                var split = profile.Add<SplitToning>(true);
                split.shadows.Override(look.SplitShadows);
                split.highlights.Override(look.SplitHighlights);
            }

            var dof = profile.Add<DepthOfField>(true);
            dof.mode.Override(DepthOfFieldMode.Off);
            dof.gaussianStart.Override(4f);
            dof.gaussianEnd.Override(9f);
            dof.gaussianMaxRadius.Override(1.2f);
            dof.highQualitySampling.Override(true);

            foreach (var component in profile.components) AssetDatabase.AddObjectToAsset(component, profile);
            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();

            var go = new GameObject("FilmLook");
            go.transform.SetParent(parent, false);
            var volume = go.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.sharedProfile = profile;
            return volume;
        }

        private static LightingSettings LightingFor(StudioLook look)
        {
            string path = LooksFolder + "/Lighting.lighting";
            var settings = AssetDatabase.LoadAssetAtPath<LightingSettings>(path);
            if (settings == null)
            {
                settings = new LightingSettings { name = "StudioLighting" };
                AssetDatabase.CreateAsset(settings, path);
            }

            settings.bakedGI = true;
            settings.realtimeGI = false;
            settings.mixedBakeMode = MixedLightingMode.IndirectOnly;
            settings.lightmapper = LightingSettings.Lightmapper.ProgressiveGPU;
            settings.lightmapResolution = 30f;
            settings.lightmapPadding = 3;
            settings.lightmapMaxSize = 4096;
            settings.directSampleCount = 64;
            settings.indirectSampleCount = 512;
            settings.environmentSampleCount = 64;
            settings.maxBounces = 3;
            settings.lightmapCompression = LightmapCompression.HighQuality;
            settings.ao = true;
            settings.aoMaxDistance = 0.6f;
            settings.aoExponentIndirect = 1.2f;
            settings.aoExponentDirect = 0.4f;
            settings.filteringMode = LightingSettings.FilterMode.Auto;
            settings.directionalityMode = LightmapsMode.CombinedDirectional;
            settings.indirectScale = 1.1f;
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
            return settings;
        }

        private static void AddToBuild()
        {
            var scenes = EditorBuildSettings.scenes.ToList();
            if (scenes.Any(s => s.path == ScenePath)) return;
            scenes.Add(new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
