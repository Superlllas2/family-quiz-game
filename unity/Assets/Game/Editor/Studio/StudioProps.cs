using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace FamilyQuiz.EditorTools.Studio
{
    /// <summary>
    /// The bought-in props of the studio (tools/blender/build_props.py; Poly Haven CC0 and Sketchfab CC BY, license logs
    /// in tools/blender/assets): one FBX per prop in Assets/Game/Art/Models/Props, textures in Art/Textures/Props/&lt;Prop&gt;,
    /// and Props.json describing each material slot. This builds their URP Lit materials and puts a copy of a prop at
    /// every Prop_&lt;Name&gt;_&lt;n&gt; marker of Studio.fbx (build_studio.py, prop()).
    /// </summary>
    public static class StudioProps
    {
        public const string ModelFolder = "Assets/Game/Art/Models/Props";
        public const string TextureFolder = "Assets/Game/Art/Textures/Props";
        public const string MaterialFolder = "Assets/Game/Art/Materials/Props";
        private const string Lit = "Universal Render Pipeline/Lit";

        /// <summary>Props whose longest side is below this are probe lit: too small to be worth lightmap texels.</summary>
        public const float ProbeLitBelow = 0.45f;

        [Serializable]
        public sealed class MaterialEntry
        {
            public string slot;
            public string source;
            public float[] baseColor = { 1, 1, 1, 1 };
            public float metallic;
            public float smoothness = 0.4f;
            public string alpha = "opaque";
            public float cutoff = 0.5f;
            public float normalScale = 1f;
            public float[] emission = { 0, 0, 0 };
            public string @base;
            public string normal;
            public string mask;
            public string emit;
        }

        [Serializable]
        public sealed class PropEntry
        {
            public string name;
            public float[] size;
            public int triangles;
            public MaterialEntry[] materials;
        }

        [Serializable]
        private sealed class Manifest
        {
            public PropEntry[] props;
        }

        /// <summary>The props of Props.json by name (empty when build_props.py has not run).</summary>
        public static Dictionary<string, PropEntry> Load()
        {
            string path = ModelFolder + "/Props.json";
            if (!File.Exists(path)) return new Dictionary<string, PropEntry>();
            var manifest = JsonUtility.FromJson<Manifest>(File.ReadAllText(path));
            return (manifest.props ?? new PropEntry[0]).ToDictionary(p => p.name);
        }

        /// <summary>
        /// Materials of the props that are lit from inside: a part of the source material's name, how bright (times the
        /// look's practical colour), and whether the base map doubles as the emission map (a flame's texture, a fabric
        /// shade's weave). Their emission also lights the bake.
        /// </summary>
        private static readonly (string part, float intensity, bool fromBase)[] Glows =
        {
            ("flame", 9f, true),                        // candle and oil lamp flames
            ("lamps_classic_chandelier", 2.6f, false),  // the chandelier's little fabric shades
            ("lampara_shade", 2.4f, true),              // the floor lamps' shades (build_props.py, SPLITS)
            ("lightbulb_01_glass", 3.2f, false),        // festoon bulbs
            ("oil_lamp_glass", 0.9f, true),             // the oil lamps' chimneys, lit by the flame inside
        };

        /// <summary>Import settings for the prop textures and models, then the materials, keyed by slot name.</summary>
        public static Dictionary<string, Material> CreateMaterials(Dictionary<string, PropEntry> props, Color practical)
        {
            AssetDatabase.Refresh();
            Directory.CreateDirectory(MaterialFolder);
            var shader = Shader.Find(Lit);
            var result = new Dictionary<string, Material>();
            foreach (var prop in props.Values)
            foreach (var entry in prop.materials ?? new MaterialEntry[0])
            {
                string folder = TextureFolder + "/" + prop.name + "/";
                var baseMap = Texture(folder, entry.@base, TextureImporterType.Default, true);
                var normal = Texture(folder, entry.normal, TextureImporterType.NormalMap, false);
                var mask = Texture(folder, entry.mask, TextureImporterType.Default, false);
                var emit = Texture(folder, entry.emit, TextureImporterType.Default, true);

                string path = MaterialFolder + "/" + entry.slot + ".mat";
                var m = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (m == null)
                {
                    m = new Material(shader) { name = entry.slot };
                    AssetDatabase.CreateAsset(m, path);
                }

                m.shader = shader;
                var c = entry.baseColor;
                m.SetColor("_BaseColor", c.Length >= 4 ? new Color(c[0], c[1], c[2], c[3]) : Color.white);
                m.SetTexture("_BaseMap", baseMap);
                m.SetTextureScale("_BaseMap", Vector2.one);
                m.SetTextureOffset("_BaseMap", Vector2.zero);

                m.SetTexture("_BumpMap", normal);
                m.SetFloat("_BumpScale", entry.normalScale);
                Keyword(m, "_NORMALMAP", normal != null);

                // URP Lit: smoothness = mask alpha x _Smoothness, metallic = mask red, occlusion = mask green.
                m.SetTexture("_MetallicGlossMap", mask);
                m.SetFloat("_SmoothnessTextureChannel", 0f);
                m.SetFloat("_Smoothness", mask != null ? 1f : Mathf.Clamp01(entry.smoothness));
                m.SetFloat("_Metallic", mask != null ? 1f : Mathf.Clamp01(entry.metallic));
                Keyword(m, "_METALLICSPECGLOSSMAP", mask != null);
                m.SetTexture("_OcclusionMap", mask);
                m.SetFloat("_OcclusionStrength", 1f);
                Keyword(m, "_OCCLUSIONMAP", mask != null);

                var e = entry.emission ?? new float[3];
                var emission = e.Length >= 3 ? new Color(e[0], e[1], e[2]) : Color.black;
                string source = (entry.source ?? "").ToLowerInvariant();
                foreach (var glow in Glows)
                {
                    if (!source.Contains(glow.part)) continue;
                    emission = practical * glow.intensity;
                    if (glow.fromBase) emit = baseMap;
                }

                bool emits = emission.maxColorComponent > 0.001f;
                m.SetColor("_EmissionColor", emits ? emission : Color.black);
                m.SetTexture("_EmissionMap", emit);
                Keyword(m, "_EMISSION", emits);
                m.globalIlluminationFlags = emits ? MaterialGlobalIlluminationFlags.BakedEmissive : MaterialGlobalIlluminationFlags.EmissiveIsBlack;

                bool cut = entry.alpha == "mask";
                bool blend = entry.alpha == "blend";
                m.SetFloat("_AlphaClip", cut ? 1f : 0f);
                m.SetFloat("_Cutoff", entry.cutoff);
                Keyword(m, "_ALPHATEST_ON", cut);
                m.SetFloat("_Surface", blend ? 1f : 0f);
                m.SetFloat("_Blend", 0f);
                m.SetFloat("_BlendModePreserveSpecular", 0f);
                m.SetFloat("_SrcBlend", blend ? 5f : 1f);
                m.SetFloat("_DstBlend", blend ? 10f : 0f);
                m.SetFloat("_SrcBlendAlpha", 1f);
                m.SetFloat("_DstBlendAlpha", blend ? 10f : 0f);
                m.SetFloat("_ZWrite", blend ? 0f : 1f);
                Keyword(m, "_SURFACE_TYPE_TRANSPARENT", blend);
                m.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                m.SetOverrideTag("RenderType", blend ? "Transparent" : cut ? "TransparentCutout" : "Opaque");
                m.renderQueue = blend ? (int)RenderQueue.Transparent : cut ? (int)RenderQueue.AlphaTest : -1;
                // Leaves and other cut outs are thin cards: both sides.
                m.SetFloat("_Cull", cut ? 0f : 2f);
                m.doubleSidedGI = cut;
                EditorUtility.SetDirty(m);
                result[entry.slot] = m;
            }

            AssetDatabase.SaveAssets();
            return result;
        }

        private static void Keyword(Material m, string keyword, bool on)
        {
            if (on) m.EnableKeyword(keyword);
            else m.DisableKeyword(keyword);
        }

        private static Texture2D Texture(string folder, string file, TextureImporterType type, bool srgb)
        {
            if (string.IsNullOrEmpty(file)) return null;
            string path = folder + file;
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                Debug.LogWarning("Studio: missing prop texture " + path);
                return null;
            }

            bool alpha = file.EndsWith(".png") && file.Contains("_base");
            if (importer.textureType != type || importer.sRGBTexture != srgb || importer.mipmapEnabled == false
                || importer.anisoLevel != 8 || importer.textureCompression != TextureImporterCompression.CompressedHQ
                || importer.alphaIsTransparency != alpha)
            {
                importer.textureType = type;
                importer.sRGBTexture = srgb;
                importer.mipmapEnabled = true;
                importer.anisoLevel = 8;
                importer.filterMode = FilterMode.Trilinear;
                importer.wrapMode = TextureWrapMode.Repeat;
                importer.maxTextureSize = 2048;
                importer.textureCompression = TextureImporterCompression.CompressedHQ;
                importer.alphaIsTransparency = alpha;
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        /// <summary>
        /// A copy of each prop at its Prop_&lt;Name&gt;_&lt;n&gt; marker, under parent: the marker's position, its design
        /// frame (x along its wall, z towards the wall; the prop's front faces -z) and its scale. Static and lightmapped,
        /// or probe lit when small. Returns the placed renderers.
        /// </summary>
        public static List<Renderer> Place(Transform parent, Dictionary<string, Transform> markers, Dictionary<string, PropEntry> props,
            Dictionary<string, Material> materials, Func<Transform, Quaternion> designFrame)
        {
            var placed = new List<Renderer>();
            var root = new GameObject("Props").transform;
            root.SetParent(parent, false);
            foreach (var pair in markers.Where(p => p.Key.StartsWith("Prop_")).OrderBy(p => p.Key))
            {
                string rest = pair.Key.Substring(5);
                int cut = rest.LastIndexOf('_');
                string name = cut > 0 ? rest.Substring(0, cut) : rest;
                var model = AssetDatabase.LoadAssetAtPath<GameObject>(ModelFolder + "/" + name + ".fbx");
                if (model == null || !props.TryGetValue(name, out var entry))
                {
                    Debug.LogWarning("Studio: no prop model for marker " + pair.Key);
                    continue;
                }

                var go = (GameObject)PrefabUtility.InstantiatePrefab(model, root);
                go.name = rest;
                go.transform.SetPositionAndRotation(pair.Value.position, designFrame(pair.Value));
                // The FBX axis change may flip a marker's scale sign; the prop only wants its size.
                var s = pair.Value.lossyScale;
                var scale = new Vector3(Mathf.Abs(s.x), Mathf.Abs(s.y), Mathf.Abs(s.z));
                go.transform.localScale = scale;
                float longest = (entry.size ?? new float[] { 1 }).Max() * scale.x;
                foreach (var r in go.GetComponentsInChildren<MeshRenderer>())
                {
                    var mats = r.sharedMaterials;
                    for (int i = 0; i < mats.Length; i++)
                    {
                        string slot = mats[i] != null ? mats[i].name : "";
                        if (materials.TryGetValue(slot, out var m)) mats[i] = m;
                        else Debug.LogWarning("Studio: no prop material " + slot + " on " + go.name);
                    }

                    r.sharedMaterials = mats;
                    r.shadowCastingMode = ShadowCastingMode.On;
                    GameObjectUtility.SetStaticEditorFlags(r.gameObject, StaticEditorFlags.ContributeGI | StaticEditorFlags.BatchingStatic
                                                                         | StaticEditorFlags.ReflectionProbeStatic | StaticEditorFlags.OccludeeStatic);
                    r.receiveGI = longest < ProbeLitBelow ? ReceiveGI.LightProbes : ReceiveGI.Lightmaps;
                    placed.Add(r);
                }
            }

            return placed;
        }

        /// <summary>Removes material assets left over from slots that no longer exist.</summary>
        public static void Prune(Dictionary<string, Material> materials)
        {
            if (!Directory.Exists(MaterialFolder)) return;
            foreach (var file in Directory.GetFiles(MaterialFolder, "*.mat"))
            {
                string name = Path.GetFileNameWithoutExtension(file);
                if (!materials.ContainsKey(name)) AssetDatabase.DeleteAsset(file.Replace('\\', '/'));
            }
        }
    }
}
