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
    ///       [-look warm|retro|modern] [-bake] [-textures] [-capture outDir] -logFile build.log
    /// </summary>
    public static class StudioBuilder
    {
        public const string ScenePath = "Assets/Scenes/Studio.unity";
        public const string ModelPath = "Assets/Game/Art/Models/Studio.fbx";
        public const string LooksFolder = "Assets/Game/Art/Looks";

        private static readonly string[] ProbeLitPieces =
        {
            "HostShelfBooks", "LeftShelfBooks", "DeskProps", "DrumKit", "Keyboard", "Amps", "PlantBig", "PlantStage",
            "PlantShelf1", "PlantShelf2", "PlantBand", "Pendants", "FloorLamp", "SideTable", "Sconces", "Truss",
            "SofaLegs", "ArmchairLegs", "HostChairs", "StageLamp1", "StageLamp2", "ProsceniumTrim", "QuestionBoardFrame"
        };

        public static void Run()
        {
            try
            {
                var args = Environment.GetCommandLineArgs();
                string look = Arg(args, "-look") ?? StudioLooks.WarmLateNight;
                bool bake = args.Contains("-bake");
                if (args.Contains("-textures") || !File.Exists(StudioTextures.Folder + "/City.png")) StudioTextures.GenerateAll();
                Build(StudioLooks.Get(look), bake);
                string capture = Arg(args, "-capture");
                if (capture != null) StudioCapture.CaptureAll(capture, look);
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
            var materials = StudioMaterials.CreateAll();
            materials["M_City"].SetColor("_EmissionColor", Color.white * look.CityEmission);
            materials["M_GlowStrip"].SetColor("_EmissionColor", look.Strip * 0.6f);
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
            rig.CurtainLeft = Find(markers, "Dyn_CurtainL");
            rig.CurtainRight = Find(markers, "Dyn_CurtainR");
            for (int i = 0; i < 8; i++) rig.Seats[i] = Find(markers, "Seat_" + (i + 1));
            rig.HostKonstantin = Find(markers, "Host_konstantin");
            rig.HostIlja = Find(markers, "Host_ilja");

            var lights = new GameObject("Lights").transform;
            lights.SetParent(root.transform);
            AddLights(look, markers, lights, rig);
            AddProbes(root.transform);
            AddCameraAndShots(root.transform, rig);
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
                bool city = go.name == "CityBackdrop";
                renderer.shadowCastingMode = city ? ShadowCastingMode.Off : ShadowCastingMode.On;
                if (dynamic || city)
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
                    scale.floatValue = go.name == "Floor" || go.name == "BackWall" || go.name == "SideWalls" ? 0.6f : 1f;
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
                    case "Shelf":
                        Point(parent, pair.Key, at, p, 2.5f * look.PracticalScale, 2f, 0.05f);
                        break;
                    case "Sconce":
                        Point(parent, pair.Key, at, p, 6f * look.PracticalScale, 4f, 0.08f);
                        break;
                    case "DeskStrip":
                        Area(parent, pair.Key, at, Quaternion.LookRotation(new Vector3(0, -0.55f, -1f)), new Vector2(2.5f, 0.06f),
                            look.Strip, 6f, 3f);
                        break;
                    case "Window":
                        Area(parent, pair.Key, at + new Vector3(0, 0, -0.1f), Quaternion.LookRotation(new Vector3(0.1f, -0.25f, -1f)),
                            new Vector2(7.6f, 3.6f), look.Window, look.WindowIntensity, 12f);
                        break;
                    case "StageLamp":
                    {
                        var target = Find(markers, "LT_Board").position;
                        var l = Spot(parent, pair.Key, at, target, look.StageLamp, look.StageLampIntensity, 12f, 34f, LightmapBakeType.Realtime);
                        l.shadows = LightShadows.None;
                        var lamps = (rig.StageLamps ?? new Light[0]).ToList();
                        lamps.Add(l);
                        rig.StageLamps = lamps.ToArray();
                        break;
                    }
                }
            }

            // Soft fill from the audience side, like the big soft lights of a TV studio.
            Area(parent, "Fill", new Vector3(-1.5f, 5.2f, -7.5f), Quaternion.LookRotation(new Vector3(0, -0.35f, 1f)),
                new Vector2(16f, 4f), look.Fill, look.FillIntensity, 25f);

            // Uplights grazing the back wall between the window, the shelves and the stage.
            foreach (float x in new[] { -8.95f, -0.62f, 2.6f, 8.65f, 12.1f })
            {
                var l = Spot(parent, "Uplight", new Vector3(x, 0.25f, 6.2f), new Vector3(x, 5.5f, 6.55f), look.Accent, look.AccentIntensity, 6f, 55f,
                    LightmapBakeType.Baked);
                l.shadowRadius = 0.05f;
            }

            // A dim light over the empty band riser.
            Spot(parent, "BandTop", new Vector3(10.3f, 6.6f, 3.2f), new Vector3(10.3f, 0.4f, 4.2f), look.Practical, 25f, 10f, 70f, LightmapBakeType.Baked);

            // A soft realtime wash on the curtain (it moves, so it cannot be baked).
            Spot(parent, "CurtainWash", new Vector3(5.5f, 7.0f, 0.2f), new Vector3(5.5f, 2.4f, 4.3f), look.StageLamp, 80f, 12f, 56f,
                LightmapBakeType.Realtime).shadows = LightShadows.None;

            // Realtime keys: the guests and hosts move, so their light and shadows are realtime (0015).
            rig.GuestKey = Spot(parent, "GuestKey", new Vector3(-4.0f, 6.2f, -2.2f), new Vector3(-4.6f, 0.7f, 3.7f), look.Key,
                look.GuestKeyIntensity, 14f, 62f, LightmapBakeType.Mixed);
            rig.GuestKey.shadows = LightShadows.Soft;
            rig.HostKey = Spot(parent, "HostKey", new Vector3(1.0f, 5.0f, 0.2f), new Vector3(1.25f, 1.0f, 3.9f), look.Key,
                look.HostKeyIntensity, 10f, 48f, LightmapBakeType.Mixed);
            rig.HostKey.shadows = LightShadows.Soft;
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

        private static void AddProbes(Transform parent)
        {
            var go = new GameObject("LightProbes");
            go.transform.SetParent(parent, false);
            var group = go.AddComponent<LightProbeGroup>();
            var points = new List<Vector3>();
            for (float x = -9.5f; x <= 3.01f; x += 1f)
            for (float z = 2.0f; z <= 4.81f; z += 0.9f)
            foreach (float y in new[] { 0.45f, 1.1f, 1.9f })
                points.Add(new Vector3(x, y, z));
            for (float x = 3.5f; x <= 7.51f; x += 1f)
            foreach (float z in new[] { 4.3f, 5.3f })
            foreach (float y in new[] { 0.9f, 2.1f, 3.4f })
                points.Add(new Vector3(x, y, z));
            for (float x = -9f; x <= 9.01f; x += 3f)
            foreach (float z in new[] { -4f, 0f })
                points.Add(new Vector3(x, 1.2f, z));
            group.probePositions = points.ToArray();

            var probe = new GameObject("ReflectionProbe").AddComponent<ReflectionProbe>();
            probe.transform.SetParent(parent, false);
            probe.transform.position = new Vector3(0f, 2.5f, 2.5f);
            probe.mode = ReflectionProbeMode.Baked;
            probe.size = new Vector3(25f, 7f, 10f);
            probe.boxProjection = true;
            probe.resolution = 256;
        }

        private static void AddCameraAndShots(Transform parent, StudioRig rig)
        {
            var cameraGo = new GameObject("StudioCamera");
            cameraGo.transform.SetParent(parent, false);
            var camera = cameraGo.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 60f;
            camera.allowHDR = true;
            camera.allowMSAA = false;
            var data = cameraGo.AddComponent<UniversalAdditionalCameraData>();
            data.renderPostProcessing = true;
            data.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
            data.antialiasingQuality = AntialiasingQuality.High;
            rig.Camera = camera;

            var shots = new GameObject("Shots").transform;
            shots.SetParent(parent, false);
            rig.WideShot = Shot(shots, "Wide", new Vector3(-3.1f, 2.5f, -6.9f), new Vector3(-3.1f, 1.05f, 3.6f));
            rig.SeatsShot = Shot(shots, "Seats", new Vector3(-4.6f, 1.45f, -5.6f), new Vector3(-4.6f, 1.45f, 3.6f));
            rig.EstablishStart = Shot(shots, "EstablishStart", new Vector3(0.8f, 7.4f, -11.5f), new Vector3(1.2f, 1.6f, 3.5f));
            rig.EstablishEnd = Shot(shots, "EstablishEnd", new Vector3(0.2f, 4.0f, -9.8f), new Vector3(0.6f, 1.4f, 3.5f));
            rig.HostSoloKonstantin = Shot(shots, "HostSoloKonstantin", new Vector3(0.55f, 1.62f, 0.5f), new Vector3(0.7f, 1.32f, 3.85f));
            rig.HostSoloIlja = Shot(shots, "HostSoloIlja", new Vector3(1.95f, 1.62f, 0.5f), new Vector3(1.8f, 1.32f, 3.85f));
            rig.CurtainShot = Shot(shots, "Curtain", new Vector3(5.5f, 2.3f, -4.0f), new Vector3(5.5f, 2.55f, 4.0f));
            rig.StageShot = Shot(shots, "Stage", new Vector3(5.5f, 2.075f, -2.0f), new Vector3(5.5f, 2.075f, 5.9f));
            rig.StagePushIn = Shot(shots, "StagePushIn", new Vector3(5.5f, 2.075f, -1.65f), new Vector3(5.5f, 2.075f, 5.9f));
            camera.transform.SetPositionAndRotation(rig.WideShot.position, rig.WideShot.rotation);
            camera.fieldOfView = rig.WideFieldOfView;
        }

        private static Transform Shot(Transform parent, string name, Vector3 at, Vector3 lookAt)
        {
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
            settings.lightmapResolution = 14f;
            settings.lightmapPadding = 3;
            settings.lightmapMaxSize = 2048;
            settings.directSampleCount = 32;
            settings.indirectSampleCount = 256;
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
