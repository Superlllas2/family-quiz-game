using System.Text;
using UnityEditor;
using UnityEngine;

namespace FamilyQuiz.EditorTools
{
    /// <summary>
    /// Logs every mesh of an imported model with its world bounds, to check the Blender to Unity scale and axes.
    /// Unity -batchmode -quit -projectPath unity -executeMethod FamilyQuiz.EditorTools.ModelReport.Run -model Assets/... -logFile x.log
    /// </summary>
    public static class ModelReport
    {
        public static void Run()
        {
            var args = System.Environment.GetCommandLineArgs();
            string path = null;
            for (int i = 0; i < args.Length - 1; i++)
                if (args[i] == "-model") path = args[i + 1];
            AssetDatabase.Refresh();
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (model == null)
            {
                Debug.LogError("ModelReport: no model at " + path);
                return;
            }

            var go = Object.Instantiate(model);
            var sb = new StringBuilder("ModelReport " + path + "\n");
            foreach (var r in go.GetComponentsInChildren<Renderer>())
            {
                var b = r.bounds;
                sb.Append("  ").Append(r.name).Append(" center=").Append(b.center.ToString("F2")).Append(" size=")
                    .Append(b.size.ToString("F2")).Append(" scale=").Append(r.transform.lossyScale.ToString("F2"))
                    .Append(" rot=").Append(r.transform.rotation.eulerAngles.ToString("F0")).Append(" mats=");
                foreach (var m in r.sharedMaterials) sb.Append(m != null ? m.name : "null").Append(',');
                sb.Append('\n');
            }

            Debug.Log(sb.ToString());
            Object.DestroyImmediate(go);
        }
    }
}
