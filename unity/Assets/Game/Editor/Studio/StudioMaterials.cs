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

            /// <summary>A packed metallic (R) and smoothness (A) map; _Smoothness then scales it.</summary>
            public string Mask;
            /// <summary>URP detail inputs, on the mesh's own UVs (1 unit = 1 m).</summary>
            public string DetailAlbedo;
            public string DetailNormal;
            public float DetailNormalScale = 1f;
            public Vector2 DetailTiling = Vector2.one;
            /// <summary>Above 0: an alpha blended transparent surface (glass) with this alpha.</summary>
            public float GlassAlpha;
            /// <summary>Set: the Velvet shader's sheen (edges brighter by x, face on darker by y, falloff power z).</summary>
            public Vector3? Sheen;
        }

        /// <summary>
        /// A photo scanned set from StudioScans.Folder (tools/fetch_textures.mjs), at its real size; falls back to the
        /// procedural spec when the scans are not there. With grey the albedo is the set's grey copy and the tint is
        /// the colour wanted (StudioScans.GreyMean is divided out).
        /// </summary>
        private static Spec Scan(string set, float metresPerTile, float smoothScale, float normalScale, Color? tint, bool grey,
            Spec fallback)
        {
            if (!StudioScans.Has(set)) return fallback;
            var color = tint ?? Color.white;
            if (grey) color = new Color(color.r / StudioScans.GreyMean, color.g / StudioScans.GreyMean, color.b / StudioScans.GreyMean);
            return new Spec
            {
                Texture = Scanned(set + (grey ? "_grey.jpg" : "_diff.jpg")), Normal = Scanned(set + "_nor.jpg"),
                Mask = Scanned(set + "_mask.png"), Tiling = Vector2.one / metresPerTile, Smoothness = smoothScale,
                NormalScale = normalScale, Color = color
            };
        }

        private static string Scanned(string file)
        {
            return StudioScans.Folder + "/" + file;
        }

        /// <summary>Muted book cloth: the scanned leather's grain, small, on a plain colour.</summary>
        private static Spec Book(Color color)
        {
            if (!StudioScans.Has("fabric_leather_02")) return Plain(color * 1.4f, 0.3f);
            return new Spec { Color = color, Smoothness = 0.35f, Normal = Scanned("fabric_leather_02_nor.jpg"), NormalScale = 0.5f, Tiling = Vector2.one / 0.25f };
        }

        /// <summary>The procedural rug pattern with the scanned carpet's pile as a detail normal, 0.6 m a tile.</summary>
        private static Spec Carpet(Spec rug)
        {
            if (!StudioScans.Has("dirty_carpet")) return rug;
            rug.Normal = Scanned(StudioScans.FlatNormal + ".png");
            rug.DetailNormal = Scanned("dirty_carpet_nor.jpg");
            rug.DetailNormalScale = 1.2f;
            rug.DetailTiling = Vector2.one / 0.6f;
            rug.Smoothness = 0.2f;
            return rug;
        }

        /// <summary>Breaks the visible repeat of a big surface with soft blotches every few metres.</summary>
        private static Spec Varied(Spec s, float metres)
        {
            if (s.Mask == null) return s;
            s.DetailAlbedo = Scanned(StudioScans.Variation + ".png");
            s.DetailTiling = Vector2.one / metres;
            return s;
        }

        private static Spec Sheened(Spec s, Vector3 sheen)
        {
            s.Sheen = sheen;
            return s;
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
                // Photo scanned sets (smoothness is a scale on the scan's own roughness), procedural fallbacks.
                // Cozy pass (Nov 5, 2026): the tints pull the red out of the scans so the woods read brown, each a
                // different tone (honey floor, dark brown walls, near black walnut trim, a lighter walnut for the desk
                // and the tables), and every wood is matte.
                ["M_FloorWood"] = Varied(Scan("wood_floor_worn", 2f, 0.68f, 0.8f, new Color(0.58f, 0.57f, 0.6f), false,
                    Tex("FloorWood", 2f, 0.4f, "FloorWood_N")), 7f),
                ["M_WoodPanel"] = Varied(Scan("dark_wood", 2f, 0.45f, 0.6f, new Color(0.44f, 0.48f, 0.56f), false,
                    Tex("WoodPanel", 1.2f, 0.32f, "WoodPanel_N")), 5f),
                ["M_WoodDark"] = Scan("walnut_veneer", 1.8f, 0.6f, 0.6f, new Color(0.38f, 0.32f, 0.29f), false,
                    Tex("WoodDark", 1f, 0.34f, "WoodDark_N")),
                ["M_WoodWarm"] = Varied(Scan("walnut_veneer", 1.2f, 0.62f, 0.7f, new Color(0.82f, 0.68f, 0.56f), false,
                    Tex("WoodWarm", 0.7f, 0.42f, "WoodWarm_N")), 1.5f),
                ["M_StageFloor"] = Scan("dark_wooden_planks", 2f, 0.6f, 0.8f, new Color(0.8f, 0.75f, 0.72f), false,
                    Tex("WoodDark", 1.5f, 0.28f, "WoodDark_N", new Color(0.7f, 0.7f, 0.7f))),
                // Woven, matte upholstery with a soft colour drift so no cushion is perfectly uniform: the sofa a muted
                // dark green, the armchairs two browns (rust outside, chocolate inside) so they are not a matched set.
                ["M_FabricSofa"] = Varied(Scan("poly_wool_herringbone", 0.27f, 0.38f, 1f, new Color(0.19f, 0.28f, 0.23f), true,
                    Tex("Fabric", 0.25f, 0.12f, "Fabric_N", new Color(0.19f, 0.29f, 0.24f))), 0.9f),
                ["M_FabricChair"] = Varied(Scan("poly_wool_herringbone", 0.22f, 0.4f, 1f, new Color(0.44f, 0.25f, 0.15f), true,
                    Tex("Fabric", 0.25f, 0.12f, "Fabric_N", new Color(0.40f, 0.25f, 0.16f))), 0.8f),
                ["M_FabricChair2"] = Varied(Scan("poly_wool_herringbone", 0.24f, 0.4f, 1f, new Color(0.36f, 0.24f, 0.16f), true,
                    Tex("Fabric", 0.25f, 0.12f, "Fabric_N", new Color(0.28f, 0.18f, 0.12f))), 0.8f),
                // A deep, muted teal velvet curtain (D93 as amended by Ilia, Nov 5, 2026).
                ["M_Velvet"] = Sheened(Scan("velour_velvet", 0.28f, 0.5f, 0.8f, new Color(0.045f, 0.19f, 0.2f), true,
                    Tex("Velvet", 0.6f, 0.4f, null, new Color(0.05f, 0.2f, 0.21f))), new Vector3(2.2f, 0.35f, 2f)),
                // A folded wool throw on the sofa arm: oatmeal, so it reads against the green.
                ["M_Blanket"] = Varied(Scan("poly_wool_herringbone", 0.18f, 0.3f, 1.2f, new Color(0.55f, 0.48f, 0.38f), true,
                    Plain(new Color(0.55f, 0.48f, 0.38f), 0.1f)), 0.6f),
                // Throw pillows: a dusty mustard and a muted rust, woven.
                ["M_PillowA"] = Scan("poly_wool_herringbone", 0.15f, 0.35f, 1f, new Color(0.5f, 0.38f, 0.16f), true,
                    Plain(new Color(0.5f, 0.38f, 0.16f), 0.1f)),
                ["M_PillowB"] = Scan("velour_velvet", 0.2f, 0.45f, 0.8f, new Color(0.36f, 0.17f, 0.12f), true,
                    Plain(new Color(0.36f, 0.17f, 0.12f), 0.2f)),
                // The board stays plain and dark: the question overlay sits on it (D92).
                ["M_Board"] = Tex("Board", 1f, 0.08f),
                ["M_BackCloth"] = Scan("velour_velvet", 0.28f, 0.3f, 0.6f, new Color(0.025f, 0.025f, 0.03f), true,
                    Plain(new Color(0.025f, 0.025f, 0.03f), 0.05f)),
                ["M_Rug"] = Carpet(Once("Rug", -9.0f, -0.9f, 2.1f, 4.9f, 0.08f)),
                ["M_Rug2"] = Carpet(Once("Rug2", -0.2f, 2.7f, 2.2f, 4.6f, 0.08f)),
                // FLOOR_RUG in build_studio.py (world frame).
                ["M_Rug3"] = Carpet(Once("Rug3", 2.85f, 7.95f, -4.3f, -0.1f, 0.08f)),
                ["M_MetalBrass"] = Plain(new Color(0.78f, 0.58f, 0.3f), 0.62f, 1f),
                ["M_MetalBlack"] = Plain(new Color(0.04f, 0.04f, 0.045f), 0.45f, 0.6f),
                ["M_Chrome"] = Plain(new Color(0.8f, 0.8f, 0.82f), 0.85f, 1f),
                ["M_Truss"] = Plain(new Color(0.22f, 0.22f, 0.23f), 0.55f, 0.9f),   // brushed aluminium, dim under the ceiling
                ["M_Ceramic"] = Plain(new Color(0.85f, 0.82f, 0.76f), 0.7f),
                ["M_Glass"] = new Spec { Color = new Color(0.02f, 0.022f, 0.026f), Smoothness = 0.97f, GlassAlpha = 0.16f },   // dark: only reflections show
                ["M_Paper"] = Plain(new Color(0.9f, 0.88f, 0.82f), 0.1f),
                ["M_CueCard"] = Plain(new Color(0.86f, 0.82f, 0.7f), 0.08f),
                ["M_Cork"] = Plain(new Color(0.48f, 0.36f, 0.24f), 0.05f),
                ["M_MugA"] = Plain(new Color(0.16f, 0.24f, 0.3f), 0.55f),
                ["M_MugB"] = Plain(new Color(0.62f, 0.3f, 0.18f), 0.55f),
                ["M_Leather"] = Scan("brown_leather", 0.4f, 0.8f, 0.8f, new Color(0.6f, 0.45f, 0.38f), false,
                    Plain(new Color(0.18f, 0.1f, 0.07f), 0.45f)),
                ["M_Pot"] = Plain(new Color(0.55f, 0.3f, 0.2f), 0.3f),
                ["M_Soil"] = Plain(new Color(0.1f, 0.07f, 0.05f), 0.05f),
                ["M_Stem"] = Plain(new Color(0.2f, 0.25f, 0.12f), 0.2f),
                ["M_Leaf"] = new Spec { Texture = "Leaf", Tiling = Vector2.one / 0.3f, Smoothness = 0.35f, TwoSided = true },
                ["M_BookA"] = Book(new Color(0.26f, 0.07f, 0.06f)),
                ["M_BookB"] = Book(new Color(0.07f, 0.09f, 0.18f)),
                ["M_BookC"] = Book(new Color(0.36f, 0.25f, 0.11f)),
                ["M_BookD"] = Book(new Color(0.08f, 0.16f, 0.1f)),
                ["M_BookE"] = Book(new Color(0.46f, 0.4f, 0.31f)),
                // A deep wine lacquer, less saturated than a bright red kit, so the band corner sits with the set.
                ["M_DrumShell"] = Plain(new Color(0.3f, 0.08f, 0.07f), 0.62f, 0.2f),
                ["M_BandRug"] = Carpet(Plain(new Color(0.16f, 0.12f, 0.11f), 0.1f)),
                ["M_DrumHead"] = Plain(new Color(0.88f, 0.86f, 0.8f), 0.3f),
                ["M_Cymbal"] = Plain(new Color(0.85f, 0.65f, 0.3f), 0.7f, 1f),
                ["M_KeysWhite"] = Plain(new Color(0.92f, 0.92f, 0.9f), 0.5f),
                ["M_Amp"] = Plain(new Color(0.06f, 0.06f, 0.06f), 0.3f),
                ["M_AmpCloth"] = Plain(new Color(0.12f, 0.11f, 0.1f), 0.05f),
                // Church candles: ivory wax, a little waxy sheen, and their flames.
                ["M_Wax"] = new Spec { Color = new Color(0.86f, 0.8f, 0.68f), Smoothness = 0.42f, Emission = new Color(1f, 0.55f, 0.25f) * 0.05f },
                ["M_Flame"] = Glow(new Color(1f, 0.62f, 0.28f) * 14f, new Color(1f, 0.8f, 0.5f), true),
                ["M_GlowStrip"] = Glow(warmLight * 1.6f),
                // Studio floor details: rubber cables and gaffer tape spike marks.
                ["M_Cable"] = Plain(new Color(0.025f, 0.025f, 0.027f), 0.38f),
                ["M_TapeWhite"] = Plain(new Color(0.7f, 0.68f, 0.62f), 0.3f),
                ["M_TapeYellow"] = Plain(new Color(0.72f, 0.55f, 0.12f), 0.3f),
                // The light shafts (StudioBuilder swaps in the LightBeam shader and sets its colour).
                ["M_Beam"] = Plain(Color.black, 0f),
                ["M_CanLens"] = Glow(new Color(1f, 0.8f, 0.55f) * 2.2f, new Color(0.2f, 0.18f, 0.15f), false, 0.9f),
                ["M_Bulb"] = Glow(new Color(1f, 0.78f, 0.52f) * 10f, Color.white),
                ["M_LampShade"] = Glow(new Color(1f, 0.66f, 0.36f) * 1.4f, new Color(0.9f, 0.8f, 0.62f), true, 0.2f),
                // The banker's lamp shade: dark green glass, glowing only faintly from the bulb inside.
                ["M_LampShadeGreen"] = Glow(new Color(0.08f, 0.3f, 0.18f) * 0.35f, new Color(0.05f, 0.2f, 0.12f), true, 0.85f),
                ["M_City"] = new Spec
                {
                    // build_studio.py fits the backdrop's UVs to 0..1 itself.
                    Color = Color.black, Smoothness = 0f, EmissionMap = "City", Emission = Color.white * 3f
                },
                // The 3D blocks between the window and the backdrop; UVs in metres, 11.5 m x 29 m a window tile: smaller than real bays, so 130 m away they match the skyline's fine windows.
                ["M_CityBuilding"] = new Spec
                {
                    Color = new Color(0.01f, 0.011f, 0.015f), Smoothness = 0.5f, EmissionMap = "CityWindows", Emission = Color.white * 3f,
                    Tiling = new Vector2(1 / 11.5f, 1 / 29f)
                },
                ["M_CityBuildingFar"] = new Spec
                {
                    Color = new Color(0.01f, 0.011f, 0.015f), Smoothness = 0.2f, EmissionMap = "CityWindows", Emission = Color.white * 1.5f,
                    Tiling = new Vector2(1 / 11.5f, 1 / 29f), Offset = new Vector2(0.37f, 0.21f)
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
            if (name == null) return null;
            // A full path (scanned sets) or the name of a procedural texture.
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(name.Contains("/") ? name : StudioTextures.Folder + "/" + name + ".png");
            if (tex == null) Debug.LogWarning("Studio: missing texture " + name);
            return tex;
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

            // URP Lit: with a metallic map, smoothness = map alpha x _Smoothness and metallic = map red.
            var mask = Load(s.Mask);
            m.SetTexture("_MetallicGlossMap", mask);
            m.SetFloat("_SmoothnessTextureChannel", 0f);
            if (mask != null) m.EnableKeyword("_METALLICSPECGLOSSMAP");
            else m.DisableKeyword("_METALLICSPECGLOSSMAP");

            var detailAlbedo = Load(s.DetailAlbedo);
            var detailNormal = Load(s.DetailNormal);
            m.SetTexture("_DetailAlbedoMap", detailAlbedo);
            m.SetTexture("_DetailNormalMap", detailNormal);
            m.SetTextureScale("_DetailAlbedoMap", s.DetailTiling);
            m.SetFloat("_DetailAlbedoMapScale", 1f);
            m.SetFloat("_DetailNormalMapScale", s.DetailNormalScale);
            m.SetTexture("_DetailMask", null);
            m.DisableKeyword("_DETAIL_SCALED");
            if (detailAlbedo != null || detailNormal != null) m.EnableKeyword("_DETAIL_MULX2");
            else m.DisableKeyword("_DETAIL_MULX2");

            bool emits = s.Emission.maxColorComponent > 0f;
            m.SetColor("_EmissionColor", s.Emission);
            m.SetTexture("_EmissionMap", Load(s.EmissionMap));
            if (emits) m.EnableKeyword("_EMISSION");
            else m.DisableKeyword("_EMISSION");
            m.globalIlluminationFlags = !emits
                ? MaterialGlobalIlluminationFlags.EmissiveIsBlack
                : s.BakedEmission ? MaterialGlobalIlluminationFlags.BakedEmissive : MaterialGlobalIlluminationFlags.None;

            bool glass = s.GlassAlpha > 0f;
            if (glass) m.SetColor("_BaseColor", new Color(s.Color.r, s.Color.g, s.Color.b, s.GlassAlpha));
            m.SetFloat("_Surface", glass ? 1f : 0f);
            // Glass keeps its reflections at full strength while its own colour is see-through (URP's "preserve
            // specular": premultiplied alpha), so the lamps and the room show in the window at night.
            m.SetFloat("_Blend", 0f);
            m.SetFloat("_BlendModePreserveSpecular", glass ? 1f : 0f);
            m.SetFloat("_SrcBlend", 1f);                // One: premultiplied for glass, opaque otherwise
            m.SetFloat("_DstBlend", glass ? 10f : 0f);
            m.SetFloat("_SrcBlendAlpha", 1f);
            m.SetFloat("_DstBlendAlpha", glass ? 10f : 0f);
            m.SetFloat("_ZWrite", glass ? 0f : 1f);
            m.SetOverrideTag("RenderType", glass ? "Transparent" : "Opaque");
            m.renderQueue = glass ? (int)UnityEngine.Rendering.RenderQueue.Transparent : -1;
            if (glass)
            {
                m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                m.EnableKeyword("_ALPHAPREMULTIPLY_ON");
            }
            else
            {
                m.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
                m.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            }

            m.SetFloat("_Cull", s.TwoSided ? 0f : 2f);
            m.doubleSidedGI = s.TwoSided;
            if (s.Sheen.HasValue) Velvet(m, s.Sheen.Value);
        }

        /// <summary>Switches a Lit material to the Velvet shader (same properties) with this sheen.</summary>
        public static void Velvet(Material m, Vector3 sheen)
        {
            var shader = Shader.Find("FamilyQuiz/Velvet");
            if (shader == null)
            {
                Debug.LogWarning("Studio: no FamilyQuiz/Velvet shader; " + m.name + " stays Lit");
                return;
            }

            m.shader = shader;
            m.SetColor("_SpecColor", new Color(sheen.x, sheen.y, sheen.z, 0f));
        }
    }
}
