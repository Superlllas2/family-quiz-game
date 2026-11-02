using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace FamilyQuiz.EditorTools
{
    /// <summary>Copies the content before every build, so the player ships the current content.</summary>
    public sealed class ContentCopyBuildStep : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            int changed = ContentCopy.CopyAll(false);
            Debug.Log("Content copy before build: " + changed + " file(s) changed");
        }
    }
}
