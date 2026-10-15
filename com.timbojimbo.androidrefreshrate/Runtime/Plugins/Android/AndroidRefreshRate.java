package com.timbojimbo.androidrefreshrate;

import android.app.Activity;
import android.util.Log;
import android.view.Display;
import android.view.Window;
import android.view.WindowManager;

import java.util.Locale;

/**
 * Asks the window for the display mode that best fits a frame rate: one at the current resolution, at the lowest whole
 * multiple of the rate if there is one, else the lowest rate above it, else the highest. Without a request, Android holds
 * a game at its default (60 Hz on Pixels); with frame pacing off, nothing else in Unity asks.
 */
public final class AndroidRefreshRate {
    private static final String TAG = "RefreshRate";
    // How far a mode's rate may be from a multiple of the target and still count (59.94 Hz for 60, 120.00001 for 120).
    private static final float TOLERANCE = 1f;

    /** Requests the mode for {@code target} frames a second, or clears the request when it is not above zero. */
    public static void request(final Activity activity, final float target) {
        activity.runOnUiThread(new Runnable() {
            @Override public void run() {
                Window window = activity.getWindow();
                Display display = activity.getWindowManager().getDefaultDisplay();
                Display.Mode mode = target > 0 ? choose(display, target) : null;
                int modeId = mode != null ? mode.getModeId() : 0;

                WindowManager.LayoutParams params = window.getAttributes();
                if (params.preferredDisplayModeId == modeId) return;
                params.preferredDisplayModeId = modeId;
                window.setAttributes(params);

                if (mode != null)
                    Log.i(TAG, String.format(Locale.US, "target %.0f: mode %d (%.1f Hz, %dx%d)", target, modeId,
                        mode.getRefreshRate(), mode.getPhysicalWidth(), mode.getPhysicalHeight()));
                else
                    Log.i(TAG, String.format(Locale.US, "target %.0f: no request", target));
            }
        });
    }

    private static Display.Mode choose(Display display, float target) {
        Display.Mode current = display.getMode();
        Display.Mode multiple = null, above = null, highest = null;
        for (Display.Mode m : display.getSupportedModes()) {
            if (m.getPhysicalWidth() != current.getPhysicalWidth() || m.getPhysicalHeight() != current.getPhysicalHeight())
                continue;
            float rate = m.getRefreshRate();
            if (highest == null || rate > highest.getRefreshRate()) highest = m;
            if (rate < target - TOLERANCE) continue;
            if (above == null || rate < above.getRefreshRate()) above = m;
            float times = Math.round(rate / target);
            if (Math.abs(rate - times * target) <= TOLERANCE && (multiple == null || rate < multiple.getRefreshRate()))
                multiple = m;
        }
        return multiple != null ? multiple : above != null ? above : highest;
    }
}
