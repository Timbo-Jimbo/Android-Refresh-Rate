using UnityEngine;
#if UNITY_ANDROID && !UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
#endif

namespace TimboJimbo.AndroidRefreshRate
{
    /// <summary>
    /// Follows <see cref="Application.targetFrameRate"/> on Android: whenever it changes, asks the display for the mode that
    /// fits it best (at the current resolution, the lowest whole multiple of the rate, else the lowest rate above it), and
    /// clears the request when the rate is -1. With Optimized Frame Pacing off, which the build step sees to, nothing else
    /// asks, and Android holds a game at its default rate (60 Hz on Pixels). It starts with the player, with nothing to
    /// call; in the editor and on other platforms it does nothing.
    /// </summary>
    internal static class RefreshRateFollower
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        private struct Follow { }

        private static AndroidJavaClass s_java;
        private static AndroidJavaObject s_activity;
        private static int s_requested = int.MinValue;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Start()
        {
            s_java = new AndroidJavaClass("com.timbojimbo.androidrefreshrate.AndroidRefreshRate");
            using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                s_activity = player.GetStatic<AndroidJavaObject>("currentActivity");

            // First thing after LateUpdate, so a rate set anywhere in the frame's scripts is asked for that frame.
            var loop = PlayerLoop.GetCurrentPlayerLoop();
            for (int i = 0; i < loop.subSystemList.Length; i++)
            {
                if (loop.subSystemList[i].type != typeof(PostLateUpdate)) continue;
                var systems = new List<PlayerLoopSystem>(loop.subSystemList[i].subSystemList);
                systems.Insert(0, new PlayerLoopSystem { type = typeof(Follow), updateDelegate = Update });
                loop.subSystemList[i].subSystemList = systems.ToArray();
            }
            PlayerLoop.SetPlayerLoop(loop);
            Update();
        }

        private static void Update()
        {
            int rate = Application.targetFrameRate;
            if (rate == s_requested) return;
            s_requested = rate;
            s_java.CallStatic("request", s_activity, (float)rate);
        }
#endif
    }
}
