using System.Collections.Generic;
using FamilyQuiz.Presentation.Staging;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace FamilyQuiz.Presentation.Graphics
{
    /// <summary>
    /// Puts <see cref="GraphicsOptions"/> on screen (decisions/0021): the window (display mode, resolution), VSync and
    /// the frame limit, texture detail and anisotropic filtering, and on the URP renderer the render scale, MSAA, shadows
    /// and ambient occlusion; anti-aliasing on every camera; bloom and depth of field on the studio's film look volume;
    /// the brightness (D161) on the studio's exposure, which <see cref="StudioStage"/> drives, so never on the UI.
    /// The studio loads later and cameras come and go, so cameras and volumes are checked again twice a second. In the
    /// editor the URP assets are put back on quit, so play mode never changes them.
    /// </summary>
    public sealed class GraphicsApplier : MonoBehaviour
    {
        private const float RecheckSeconds = 0.5f;
        private const string AmbientOcclusionFeature = "ScreenSpaceAmbientOcclusion";

        private GraphicsOptions _options = new GraphicsOptions();
        private float _nextCheck;
        private UniversalRenderPipelineAsset _urp;
        private ScriptableRendererFeature _ssao;
        private bool _saved;
        private bool _applied;
        private float _savedScale;
        private int _savedMsaa;
        private float _savedShadowDistance;
        private int _savedShadowResolution;
        private bool _savedSsao;

        public static GraphicsApplier Create(GraphicsOptions options)
        {
            var go = new GameObject("GraphicsApplier");
            DontDestroyOnLoad(go);
            var applier = go.AddComponent<GraphicsApplier>();
            if (!options.WindowChosen)
            {
                // Show the window the app started with; leave it alone until someone picks another in the panel.
                options = options.Clone();
                options.DisplayMode = Screen.fullScreenMode == FullScreenMode.Windowed ? "windowed"
                    : Screen.fullScreenMode == FullScreenMode.ExclusiveFullScreen ? "fullscreen" : "borderless";
                options.Width = Screen.width;
                options.Height = Screen.height;
            }

            applier.Apply(options, options.WindowChosen);
            return applier;
        }

        public GraphicsOptions Options => _options;

        /// <summary>The resolutions the screen offers, largest first, each once (refresh rates folded).</summary>
        public static List<Vector2Int> Resolutions()
        {
            var list = new List<Vector2Int>();
            foreach (var r in Screen.resolutions)
            {
                var size = new Vector2Int(r.width, r.height);
                if (size.x >= 1024 && !list.Contains(size)) list.Add(size);
            }

            if (list.Count == 0) list.Add(new Vector2Int(Screen.currentResolution.width, Screen.currentResolution.height));
            list.Sort((a, b) => b.x * b.y != a.x * a.y ? (b.x * b.y).CompareTo(a.x * a.y) : b.x.CompareTo(a.x));
            return list;
        }

        /// <summary>Applies the options; the window only when it changed (or on the first call), since that flickers.</summary>
        public void Apply(GraphicsOptions options, bool window = false)
        {
            var before = _options;
            _options = options.Clone();
            bool windowChanged = before.DisplayMode != _options.DisplayMode || before.Width != _options.Width || before.Height != _options.Height;
            if (_applied && windowChanged) _options.WindowChosen = true; // picked in the panel: kept from now on
            if (window || (_applied && windowChanged)) ApplyWindow();
            _applied = true;

            QualitySettings.vSyncCount = _options.VSync ? 1 : 0;
            Application.targetFrameRate = _options.VSync || _options.FrameLimit <= 0 ? -1 : _options.FrameLimit;
            QualitySettings.globalTextureMipmapLimit = _options.Textures == "low" ? 2 : _options.Textures == "medium" ? 1 : 0;
            if (_options.Anisotropic <= 0)
            {
                QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
            }
            else
            {
                QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
                Texture.SetGlobalAnisotropicFilteringLimits(_options.Anisotropic, _options.Anisotropic);
            }

            ApplyRenderer();
            ApplyScene();
        }

        private void ApplyWindow()
        {
            var mode = _options.DisplayMode == "fullscreen" ? FullScreenMode.ExclusiveFullScreen
                : _options.DisplayMode == "windowed" ? FullScreenMode.Windowed
                : FullScreenMode.FullScreenWindow;
            int w = _options.Width > 0 ? _options.Width : Screen.currentResolution.width;
            int h = _options.Height > 0 ? _options.Height : Screen.currentResolution.height;
            if (Application.isEditor) return; // the editor's game view keeps its size
            Screen.SetResolution(w, h, mode);
        }

        private void ApplyRenderer()
        {
            _urp = QualitySettings.renderPipeline as UniversalRenderPipelineAsset
                   ?? GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if (_urp == null) return;
            if (_ssao == null)
            {
                foreach (var feature in Resources.FindObjectsOfTypeAll<ScriptableRendererFeature>())
                {
                    if (feature != null && feature.name == AmbientOcclusionFeature) _ssao = feature;
                }
            }

            if (!_saved)
            {
                _saved = true;
                _savedScale = _urp.renderScale;
                _savedMsaa = _urp.msaaSampleCount;
                _savedShadowDistance = _urp.shadowDistance;
                _savedShadowResolution = _urp.mainLightShadowmapResolution;
                _savedSsao = _ssao != null && _ssao.isActive;
            }

            _urp.renderScale = _options.RenderScale / 100f;
            // MSAA ×4 goes with SMAA on the camera; TAA needs MSAA off.
            _urp.msaaSampleCount = _options.AntiAliasing == "msaa" ? 4 : 1;
            switch (_options.Shadows)
            {
                case "off":
                    _urp.shadowDistance = 0f;
                    break;
                case "low":
                    _urp.shadowDistance = 20f;
                    _urp.mainLightShadowmapResolution = 1024;
                    break;
                case "medium":
                    _urp.shadowDistance = 50f;
                    _urp.mainLightShadowmapResolution = 2048;
                    break;
                default:
                    _urp.shadowDistance = 60f;
                    _urp.mainLightShadowmapResolution = 4096;
                    break;
            }

            if (_ssao != null) _ssao.SetActive(_options.AmbientOcclusion);
        }

        /// <summary>Anti-aliasing on every camera, bloom and depth of field on every volume (the studio's film look).</summary>
        private void ApplyScene()
        {
            var mode = _options.AntiAliasing == "fxaa" ? AntialiasingMode.FastApproximateAntialiasing
                : _options.AntiAliasing == "smaa" || _options.AntiAliasing == "msaa" ? AntialiasingMode.SubpixelMorphologicalAntiAliasing
                : _options.AntiAliasing == "taa" ? AntialiasingMode.TemporalAntiAliasing
                : AntialiasingMode.None;
            foreach (var cam in Camera.allCameras)
            {
                var data = cam.GetUniversalAdditionalCameraData();
                if (data == null) continue;
                if (data.antialiasing != mode) data.antialiasing = mode;
                if (mode == AntialiasingMode.SubpixelMorphologicalAntiAliasing) data.antialiasingQuality = AntialiasingQuality.High;
            }

            foreach (var volume in FindObjectsByType<Volume>(FindObjectsSortMode.None))
            {
                if (!volume.HasInstantiatedProfile() && volume.sharedProfile == null) continue;
                var profile = volume.profile; // an instance: the asset never changes at runtime
                if (profile.TryGet<Bloom>(out var bloom) && bloom.active != _options.Bloom) bloom.active = _options.Bloom;
                if (profile.TryGet<DepthOfField>(out var dof) && dof.active != _options.DepthOfField) dof.active = _options.DepthOfField;
            }

            foreach (var stage in FindObjectsByType<StudioStage>(FindObjectsSortMode.None)) stage.Brightness = (float)_options.Brightness;
        }

        private void Update()
        {
            if (Time.unscaledTime < _nextCheck) return;
            _nextCheck = Time.unscaledTime + RecheckSeconds;
            ApplyScene();
        }

        private void OnApplicationQuit()
        {
            if (!Application.isEditor || !_saved || _urp == null) return;
            _urp.renderScale = _savedScale;
            _urp.msaaSampleCount = _savedMsaa;
            _urp.shadowDistance = _savedShadowDistance;
            _urp.mainLightShadowmapResolution = _savedShadowResolution;
            if (_ssao != null) _ssao.SetActive(_savedSsao);
        }
    }
}
