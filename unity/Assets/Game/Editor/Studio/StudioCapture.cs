using System.IO;
using FamilyQuiz.Presentation.Staging;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace FamilyQuiz.EditorTools.Studio
{
    /// <summary>
    /// Renders every shot of the open Studio scene to PNG files, with stand in guests, hosts and answer cards, so the
    /// three looks can be compared from screenshots (D99). Nothing is saved to the scene.
    /// </summary>
    public static class StudioCapture
    {
        private const int Width = 1920;
        private const int Height = 1080;

        private static readonly Color[] GuestColors =
        {
            new Color32(0xE5, 0x7A, 0x9A, 0xFF), new Color32(0xDD, 0xE3, 0xEA, 0xFF), new Color32(0xC0, 0x84, 0x57, 0xFF),
            new Color32(0xE0, 0x53, 0x3D, 0xFF), new Color32(0xF2, 0xC1, 0x4E, 0xFF), new Color32(0x4F, 0xA3, 0xA5, 0xFF),
            new Color32(0xB7, 0x84, 0xF0, 0xFF), new Color32(0x6E, 0xE0, 0x7A, 0xFF)
        };

        private static readonly string[] Letters = { "A", "C", "B", "A", "D", "A", "", "B" };

        public static void CaptureAll(string outDir, string look, string hide = null)
        {
            var scene = EditorSceneManager.OpenScene(StudioBuilder.ScenePath);
            if (hide != null)
                foreach (var t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
                    if (t.name == hide) t.gameObject.SetActive(false);
            var rig = Object.FindFirstObjectByType<StudioRig>();
            Directory.CreateDirectory(outDir);
            var stand = new GameObject("CaptureStandIns");
            var cards = new GameObject("Cards");
            cards.transform.SetParent(stand.transform);
            for (int i = 0; i < rig.Seats.Length; i++) AddGuest(stand.transform, cards.transform, rig.Seats[i], GuestColors[i], Letters[i]);
            AddHost(stand.transform, rig.HostKonstantin, new Color(0.45f, 0.12f, 0.14f));
            AddHost(stand.transform, rig.HostIlja, new Color(0.13f, 0.2f, 0.38f));

            var color = rig.Volume.sharedProfile.TryGet<ColorAdjustments>(out var c) ? c : null;
            var vignette = rig.Volume.sharedProfile.TryGet<Vignette>(out var v) ? v : null;
            float baseExposure = color != null ? color.postExposure.value : 0f;
            float baseVignette = vignette != null ? vignette.intensity.value : 0f;
            float guestKey = rig.GuestKey.intensity;

            void Shot(string name, Transform pose, float fov, bool curtainOpen, bool showCards, bool dark)
            {
                rig.Camera.transform.SetPositionAndRotation(pose.position, pose.rotation);
                rig.Camera.fieldOfView = fov;
                rig.CurtainLeft.localScale = new Vector3(curtainOpen ? 0.16f : 1f, 1f, 1f);
                rig.CurtainRight.localScale = new Vector3(curtainOpen ? 0.16f : 1f, 1f, 1f);
                cards.SetActive(showCards);
                if (color != null) color.postExposure.value = baseExposure - (dark ? rig.DarkStops : 0f);
                if (vignette != null) vignette.intensity.value = dark ? Mathf.Max(baseVignette, 0.5f) : baseVignette;
                rig.GuestKey.intensity = guestKey * (dark ? Mathf.Pow(2f, rig.DarkStops) : 1f);
                Render(rig.Camera, Path.Combine(outDir, look + "_" + name + ".png"));
            }

            Shot("1_seats_dark", rig.SeatsShot, rig.SeatsFieldOfView, false, false, true);
            Shot("2_establish", rig.EstablishStart, rig.EstablishFieldOfView, false, false, false);
            Shot("3_wide", rig.WideShot, rig.WideFieldOfView, false, false, false);
            Shot("4_wide_cards", rig.WideShot, rig.WideFieldOfView, false, true, false);
            Shot("5_hostsolo", rig.HostSoloKonstantin, rig.HostSoloFieldOfView, false, false, false);
            Shot("6_curtain", rig.CurtainShot, rig.CurtainFieldOfView, false, false, false);
            Shot("7_stage", rig.StageShot, rig.StageFieldOfView, true, false, false);
            Shot("8_seats_lit", rig.SeatsShot, rig.SeatsFieldOfView, false, false, false);

            if (color != null) color.postExposure.value = baseExposure;
            if (vignette != null) vignette.intensity.value = baseVignette;
            Object.DestroyImmediate(stand);
            Debug.Log("Studio: captured look " + look + " to " + outDir);
        }

        private static void AddGuest(Transform parent, Transform cards, Transform seat, Color color, string letter)
        {
            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Guest";
            body.transform.SetParent(parent);
            body.transform.SetPositionAndRotation(seat.position + new Vector3(0, 0.5f, 0), seat.rotation);
            body.transform.localScale = new Vector3(0.6f, 0.5f, 0.6f);
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.SetColor("_BaseColor", color);
            mat.SetFloat("_Smoothness", 0.35f);
            body.GetComponent<Renderer>().sharedMaterial = mat;
            foreach (float side in new[] { -0.09f, 0.09f })
            {
                var eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                eye.transform.SetParent(body.transform, false);
                eye.transform.localPosition = new Vector3(side / 0.6f, 0.42f, 0.4f);
                eye.transform.localScale = new Vector3(0.11f / 0.6f, 0.11f / 0.5f, 0.06f / 0.6f) * 1.4f;
                var em = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                em.SetColor("_BaseColor", Color.white);
                eye.GetComponent<Renderer>().sharedMaterial = em;
            }

            var card = GameObject.CreatePrimitive(PrimitiveType.Cube);
            card.name = "Card";
            card.transform.SetParent(cards);
            var forward = -(seat.rotation * Vector3.forward); // the seat marker's back faces the audience
            card.transform.position = seat.position + new Vector3(0, 1.15f, 0) + seat.forward * 0.25f;
            card.transform.rotation = Quaternion.LookRotation(-seat.forward); // the seat marker faces the audience
            card.transform.localScale = new Vector3(0.5f, 0.38f, 0.02f);
            var cm = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            cm.SetColor("_BaseColor", new Color(0.95f, 0.93f, 0.88f));
            card.GetComponent<Renderer>().sharedMaterial = cm;
            var text = new GameObject("Letter").AddComponent<TextMeshPro>();
            text.transform.SetParent(card.transform, false);
            text.transform.localPosition = new Vector3(0, 0, -0.6f);
            text.transform.localScale = new Vector3(1f / 0.5f, 1f / 0.38f, 1f) * 0.1f;
            text.text = letter;
            text.fontSize = 28;
            text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Center;
            text.color = new Color32(0x1D, 0x4E, 0xD8, 0xFF);
            text.rectTransform.sizeDelta = new Vector2(4.5f, 3.4f);
            _ = forward;
        }

        private static void AddHost(Transform parent, Transform at, Color color)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = "Host";
            cube.transform.SetParent(parent);
            cube.transform.SetPositionAndRotation(at.position + new Vector3(0, 0.6f, 0), at.rotation);
            cube.transform.localScale = Vector3.one * 0.6f;
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.SetColor("_BaseColor", color);
            mat.SetFloat("_Smoothness", 0.4f);
            cube.GetComponent<Renderer>().sharedMaterial = mat;
            foreach (float side in new[] { -0.14f, 0.14f })
            {
                var eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                eye.transform.SetParent(cube.transform, false);
                eye.transform.localPosition = new Vector3(side, 0.15f, 0.5f);
                eye.transform.localScale = new Vector3(0.16f, 0.16f, 0.06f);
                var em = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                em.SetColor("_BaseColor", Color.white);
                eye.GetComponent<Renderer>().sharedMaterial = em;
            }
        }

        public static void Render(Camera camera, string file)
        {
            var rt = new RenderTexture(Width, Height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB) { antiAliasing = 1 };
            rt.Create();
            var request = new RenderPipeline.StandardRequest { destination = rt };
            if (RenderPipeline.SupportsRenderRequest(camera, request)) RenderPipeline.SubmitRenderRequest(camera, request);
            else
            {
                camera.targetTexture = rt;
                camera.Render();
                camera.targetTexture = null;
            }

            var previous = RenderTexture.active;
            RenderTexture.active = rt;
            var tex = new Texture2D(Width, Height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
            tex.Apply();
            RenderTexture.active = previous;
            File.WriteAllBytes(file, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            rt.Release();
            Object.DestroyImmediate(rt);
        }
    }
}
