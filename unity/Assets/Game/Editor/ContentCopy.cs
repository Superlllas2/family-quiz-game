using System.IO;
using UnityEditor;
using UnityEngine;

namespace FamilyQuiz.EditorTools
{
    /// <summary>
    /// Copies the repo's content/*.json into Assets/StreamingAssets/content/ (docs/13_architecture.md,
    /// "Content copy"). Only the top level files: content/ideation/ is never copied. The copy is git-ignored;
    /// the TV app reads only the copy.
    /// </summary>
    public static class ContentCopy
    {
        public static string SourceFolder =>
            Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "content"));

        public static string TargetFolder => Path.Combine(Application.dataPath, "StreamingAssets", "content");

        [MenuItem("Family Quiz/Copy content to StreamingAssets")]
        public static void CopyFromMenu()
        {
            CopyAll(true);
        }

        /// <summary>Copies every content/*.json; returns how many files changed.</summary>
        public static int CopyAll(bool refreshAssets)
        {
            var source = SourceFolder;
            var target = TargetFolder;
            if (!Directory.Exists(source))
            {
                Debug.LogError("Content copy: folder not found: " + source);
                return 0;
            }

            Directory.CreateDirectory(target);
            int changed = 0;
            foreach (var file in Directory.GetFiles(source, "*.json", SearchOption.TopDirectoryOnly))
            {
                var destination = Path.Combine(target, Path.GetFileName(file));
                var bytes = File.ReadAllBytes(file);
                if (File.Exists(destination) && SameBytes(bytes, File.ReadAllBytes(destination))) continue;
                File.WriteAllBytes(destination, bytes);
                changed++;
            }

            // Remove copies whose source file is gone.
            foreach (var copy in Directory.GetFiles(target, "*.json", SearchOption.TopDirectoryOnly))
            {
                if (File.Exists(Path.Combine(source, Path.GetFileName(copy)))) continue;
                File.Delete(copy);
                if (File.Exists(copy + ".meta")) File.Delete(copy + ".meta");
                changed++;
            }

            if (changed > 0)
            {
                Debug.Log("Content copy: " + changed + " file(s) updated in " + target);
                if (refreshAssets) AssetDatabase.Refresh();
            }

            return changed;
        }

        private static bool SameBytes(byte[] a, byte[] b)
        {
            if (a.Length != b.Length) return false;
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i]) return false;
            }

            return true;
        }
    }
}
