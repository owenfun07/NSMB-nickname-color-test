using System;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using UnityEngine;

namespace NSMB.Networking {
    /// <summary>
    /// Reads the public nickname color prepared by the NSMB launcher.
    /// The launcher only sets this value after the security API independently
    /// verifies the exact Owen account on a restricted network. The short-lived
    /// identity token never enters Unity.
    /// </summary>
    public static class WebGLGameIdentityBridge {

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern IntPtr NSMB_GetGameNicknameColor();

        [DllImport("__Internal")]
        private static extern void NSMB_ClearGameNicknameColor();

        [DllImport("__Internal")]
        private static extern void NSMB_FreeGameNicknameColor(IntPtr pointer);
#endif

        /// <summary>
        /// Consumes the launcher's verified solid nickname color.
        /// Returns an empty string for ordinary players, invalid values,
        /// unavailable browser integration, or non-WebGL builds.
        /// </summary>
        public static string ConsumeNicknameColor() {
#if UNITY_WEBGL && !UNITY_EDITOR
            IntPtr pointer = IntPtr.Zero;

            try {
                pointer = NSMB_GetGameNicknameColor();
                if (pointer == IntPtr.Zero) {
                    return string.Empty;
                }

                string color = Marshal.PtrToStringAnsi(pointer) ?? string.Empty;
                if (!Regex.IsMatch(color, "^#[0-9A-Fa-f]{6}$")) {
                    return string.Empty;
                }

                return color.ToUpperInvariant();
            } catch (Exception exception) {
                Debug.LogWarning("[Nickname Identity] Could not read the verified nickname color: " + exception.Message);
                return string.Empty;
            } finally {
                if (pointer != IntPtr.Zero) {
                    NSMB_FreeGameNicknameColor(pointer);
                }

                // Consume the value even if it was malformed, so it cannot leak
                // into a later game session in the same browser tab.
                try {
                    NSMB_ClearGameNicknameColor();
                } catch (Exception exception) {
                    Debug.LogWarning("[Nickname Identity] Could not clear the nickname color: " + exception.Message);
                }
            }
#else
            return string.Empty;
#endif
        }
    }
}
