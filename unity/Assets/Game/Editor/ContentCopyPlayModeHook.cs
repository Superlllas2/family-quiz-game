using UnityEditor;

namespace FamilyQuiz.EditorTools
{
    /// <summary>Copies the content before entering Play mode.</summary>
    [InitializeOnLoad]
    public static class ContentCopyPlayModeHook
    {
        static ContentCopyPlayModeHook()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        private static void OnPlayModeChanged(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.ExitingEditMode) ContentCopy.CopyAll(true);
        }
    }
}
