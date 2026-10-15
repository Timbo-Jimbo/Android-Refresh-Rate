# Timbo Jimbo - Android Refresh Rate

Add and forget: on Android, the display's refresh rate follows `Application.targetFrameRate`, with Optimized Frame Pacing off.

With Optimized Frame Pacing on, Unity's frame pacing (Android's Swappy) picks the display's rate and paces each frame to it. That relies on Android telling the app whenever the display's rate changes, and on recent Android (seen on Android 17) that sometimes stops for the life of the app: the display service leaves one event undelivered in the app's queue, and every later display event queues behind it. When the screen then moves from 60 Hz to 120, Unity and Swappy keep pacing for 60: UI Next's Circle demo ran at about 80 fps of a 120 target on a Pixel 8, and a flat 120 with frame pacing off. But with frame pacing off, nothing in Unity asks for a refresh rate, and Android holds a game at its default rate (60 Hz on Pixels), whatever `targetFrameRate` says.

This package does both halves: a build step turns Optimized Frame Pacing off for Android builds (and leaves it off), and at runtime, whenever `targetFrameRate` changes, the window asks for the display mode that fits it best.

Requires Unity 6000.0 or newer.

## Installation

Open **Window > Package Manager**, click Add, choose **Add package from git URL...** and paste `https://github.com/Timbo-Jimbo/Android-Refresh-Rate.git?path=com.timbojimbo.androidrefreshrate#0.1.0`.

There is nothing to set up or call: set `Application.targetFrameRate` as usual.

## How it picks a mode

Among the display's modes at its current resolution (so the screen never changes size), it picks:

1. the lowest rate that is a whole multiple of the target (60 for a target of 60 or 30, 120 for 120),
2. else the lowest rate above it (120 for a target of 90 on a 60/120 Hz screen: a rate below the target would cap the frame rate, since presenting waits on the display),
3. else the highest rate there is.

A target of -1 (Unity's default, 30 fps on Android) clears the request and leaves the rate to the system. Screens with an adaptive refresh rate list their lower rates as modes too (a Pixel 10 Pro XL's 60 Hz), and those count.

It asks through the window's `preferredDisplayModeId`, once per change of `targetFrameRate`, checked once a frame just after `LateUpdate`. Unity itself never sets it, through `Screen.SetResolution`, orientation changes or the app going to the background and back, so the request holds until the target changes. Each request is logged to logcat under `RefreshRate`.

In the editor and on other platforms it does nothing.
