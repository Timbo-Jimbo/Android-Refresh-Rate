using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace TimboJimboEditor.AndroidRefreshRate
{
    /// <summary>
    /// Turns Optimized Frame Pacing off for Android builds, and leaves it off: with it on, Unity's frame pacing (Swappy)
    /// paces to the rate Android last reported, and when Android stops reporting display changes to the app (a stuck
    /// event in the display service's queue for it), it keeps pacing for 60 Hz on a 120 Hz screen. With it off, the
    /// runtime follows <see cref="Application.targetFrameRate"/> with a display mode of its own.
    /// </summary>
    internal sealed class FramePacingBuildStep : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.Android || !PlayerSettings.Android.optimizedFramePacing) return;
            PlayerSettings.Android.optimizedFramePacing = false;
            Debug.Log("Android Refresh Rate: turned Optimized Frame Pacing off (Player Settings > Android > Resolution and Presentation); " +
                      "the display's rate now follows Application.targetFrameRate.");
        }
    }
}
