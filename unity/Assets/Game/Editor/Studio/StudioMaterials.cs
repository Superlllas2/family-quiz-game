using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace FamilyQuiz.EditorTools.Studio
{
    /// <summary>
    /// URP Lit materials for every material slot of Studio.fbx (matched by name, M_*), written to
    /// Assets/Game/Art/Materials. Physically based and matte where it should be (docs/21_staging.md, "Look"): wood,
    /// fabric and the worn floor are never glossy; only metal, ceramic and the drum shells shine.
    /// </summary>
    public static class StudioMaterials
    {
        public const string Folder = "Assets/Game/Art/Materials";
        private const string Lit = "Universal Render Pipeline/Lit";

        private sealed class Spec
        {
            public Color Color = Color.white;
            public string Texture;
            public Vector2 Tiling = Vector2.one;
            public Vector2 Offset = Vector2.zero;
            public string Normal;
            public float NormalScale = 1f;
            public float Smoothness = 0.3f;
            public float Metallic;
            public Color Emission = Color.black;
            public string EmissionMap;
            public bool TwoSided;
            public bool BakedEmission = true;
        }

        private static Spec Tex(string tex, float metresPerTile, float smooth, string normal = null, Color? color = null)
        {
            return new Spec
            {
                Texture = tex, Tiling = Vector2.one / metresPerTile, Smoothness = smooth, Normal = normal,
                Color = color ?? Color.white
            };
        }

        private static Spec Plain(Color color, float smooth, float metallic = 0f)
        {
            return new Spec { Color = color, Smoothness = smooth, Metallic = metallic };
        }

        private static Spec Glow(Color emission, Color? color = null, bool twoSided = false, float smooth = 0.3f)
        {
            return new Spec { Color = color ?? Color.black, Emission = emission, TwoSided = twoSided, Smoothness = smooth };
        }

        /// <summary>Maps a texture once over a rectangle of the set (design x and y in metres, see build_studio.py).</summary>
        private static Spec Once(string tex, float x0, float x1, float y0, float y1, float smooth)
        {
            float w = x1 - x0, h = y1 - y0;
            return new Spec { Texture = tex, Tiling = new Vector2(1 / w, 1 / h), Offset = new Vector2(-x0 / w, -y0 / h), Smoothness = smooth };
        }

        private static Dictionary<string, Spec> Specs()
        {
            var warmLight = new Color(1f, 0.56f, 0.26f);
            return new Dictionary<string, Spec>
            {
                ["M_FloorWood"] = Tex("FloorWood", 2f, 0.4f, "FloorWood_N"),
                ["M_WoodPanel"] = Tex("WoodPanel", 1.2f, 0.32f, "WoodPanel_N"),
                ["M_WoodDark"] = Tex("WoodDark", 1f, 0.34f, "WoodDark_N"),
                ["M_WoodWarm"] = Tex("WoodWarm", 0.7f, 0.42f, "WoodWarm_N"),
                ["M_StageFloor"] = Tex("WoodDark", 1.5f, 0.28f, "WoodDark_N", new Color(0.7f, 0.7f, 0.7f)),
                ["M_FabricSofa"] = Tex("Fabric", 0.25f, 0.12f, "Fabric_N", new Color(0.19f, 0.29f, 0.24f)),
                ["M_FabricChair"] = Tex("Fabric", 0.25f, 0.12f, "Fabric_N", new Color(0.40f, 0.25f, 0.16f)),
                ["M_Velvet"] = Tex("Velvet", 0.6f, 0.4f, null, new Color(0.08f, 0.32f, 0.23f)),
                ["M_Board"] = Tex("Board", 1f, 0.08f),
                ["M_BackCloth"] = Plain(new Color(0.025f, 0.025f, 0.03f), 0.05f),
                ["M_Rug"] = Once("Rug", -9.0f, -0.9f, 2.1f, 4.9f, 0.08f),
                ["M_Rug2"] = Once("Rug2", -0.2f, 2.7f, 2.2f, 4.6f, 0.08f),
                ["M_MetalBrass"] = Plain(new Color(0.78f, 0.58f, 0.3f), 0.62f, 1f),
                ["M_MetalBlack"] = Plain(new Color(0.04f, 0.04f, 0.045f), 0.45f, 0.6f),
                ["M_Chrome"] = Plain(new Color(0.8f, 0.8f, 0.82f), 0.85f, 1f),
                ["M_Truss"] = Plain(new Color(0.05f, 0.05f, 0.055f), 0.35f, 0.5f),
                ["M_Ceramic"] = Plain(new Color(0.85f, 0.82f, 0.76f), 0.7f),
                ["M_Paper"] = Plain(new Color(0.9f, 0.88f, 0.82f), 0.1f),
                ["M_Leather"] = Plain(new Color(0.18f, 0.1f, 0.07f), 0.45f),
                ["M_Pot"] = Plain(new Color(0.55f, 0.3f, 0.2f), 0.3f),
                ["M_Soil"] = Plain(new Color(0.1f, 0.07f, 0.05f), 0.05f),
                ["M_Stem"] = Plain(new Color(0.2f, 0.25f, 0.12f), 0.2f),
                ["M_Leaf"] = new Spec { Texture = "Leaf", Tiling = Vector2.one / 0.3f, Smoothness = 0.35f, TwoSided = true },
                ["M_BookA"] = Plain(new Color(0.40f, 0.09f, 0.08f), 0.3f),
                ["M_BookB"] = Plain(new Color(0.10f, 0.14f, 0.30f), 0.3f),
                ["M_BookC"] = Plain(new Color(0.62f, 0.46f, 0.15f), 0.3f),
                ["M_BookD"] = Plain(new Color(0.13f, 0.30f, 0.18f), 0.3f),
                ["M_BookE"] = Plain(new Color(0.75f, 0.68f, 0.52f), 0.3f),
                ["M_DrumShell"] = Plain(new Color(0.45f, 0.06f, 0.06f), 0.75f, 0.3f),
                ["M_DrumHead"] = Plain(new Color(0.88f, 0.86f, 0.8f), 0.3f),
                ["M_Cymbal"] = Plain(new Color(0.85f, 0.65f, 0.3f), 0.7f, 1f),
                ["M_KeysWhite"] = Plain(new Color(0.92f, 0.92f, 0.9f), 0.5f),
                ["M_Amp"] = Plain(new Color(0.06f, 0.06f, 0.06f), 0.3f),
                ["M_AmpCloth"] = Plain(new Color(0.12f, 0.11f, 0.1f), 0.05f),
                ["M_GlowStrip"] = Glow(warmLight * 1.6f),
                ["M_Bulb"] = Glow(new Color(1f, 0.78f, 0.52f) * 10f, Color.white),
                ["M_LampShade"] = Glow(new Color(1f, 0.66f, 0.36f) * 1.4f, new Color(0.9f, 0.8f, 0.62f), true, 0.2f),
                ["M_LampShadeGreen"] = Glow(new Color(0.12f, 0.45f, 0.24f) * 0.7f, new Color(0.08f, 0.3f, 0.16f), true, 0.9f),
                ["M_City"] = new Spec
                {
                    Color = Color.black, Smoothness = 0f, EmissionMap = "City", Emission = Color.white * 3f,
                    Tiling = new Vector2(1 / 34f, 1 / 17f), Offset = new Vector2(26 / 34f, 2 / 17f)
                }
            };
        }

        /// <summary>Creates or updates every material and returns them by name.</summary>
        public static Dictionary<string, Material> CreateAll()
        {
            Directory.CreateDirectory(Folder);
            var shader = Shader.Find(Lit);
            var result = new Dictionary<string, Material>();
            foreach (var pair in Specs())
            {
                string path = Folder + "/" + pair.Key + ".mat";
                var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (mat == null)
                {
                    mat = new Material(shader) { name = pair.Key };
                    AssetDatabase.CreateAsset(mat, path);
                }

                Apply(mat, pair.Value);
                EditorUtility.SetDirty(mat);
                result[pair.Key] = mat;
            }

            AssetDatabase.SaveAssets();
            return result;
        }

        private static Texture2D Load(string name)
        {
            return name == null ? null : AssetDatabase.LoadAssetAtPath<Texture2D>(StudioTextures.Folder + "/" + name + ".png");
        }

        private static void Apply(Material m, Spec s)
        {
            m.shader = Shader.Find(Lit);
            m.SetColor("_BaseColor", s.Color);
            m.SetTexture("_BaseMap", Load(s.Texture));
            m.SetTextureScale("_BaseMap", s.Tiling);
            m.SetTextureOffset("_BaseMap", s.Offset);
            m.SetFloat("_Smoothness", s.Smoothness);
            m.SetFloat("_Metallic", s.Metallic);
            var normal = Load(s.Normal);
            m.SetTexture("_BumpMap", normal);
            m.SetFloat("_BumpScale", s.NormalScale);
            if (normal != null) m.EnableKeyword("_NORMALMAP");
            else m.DisableKeyword("_NORMALMAP");

            bool emits = s.Emission.maxColorComponent > 0f;
            m.SetColor("_EmissionColor", s.Emission);
            m.SetTexture("_EmissionMap", Load(s.EmissionMap));
            if (emits) m.EnableKeyword("_EMISSION");
            else m.DisableKeyword("_EMISSION");
            m.globalIlluminationFlags = !emits
                ? MaterialGlobalIlluminationFlags.EmissiveIsBlack
                : s.BakedEmission ? MaterialGlobalIlluminationFlags.BakedEmissive : MaterialGlobalIlluminationFlags.None;

            m.SetFloat("_Cull", s.TwoSided ? 0f : 2f);
            m.doubleSidedGI = s.TwoSided;
        }
    }
}
