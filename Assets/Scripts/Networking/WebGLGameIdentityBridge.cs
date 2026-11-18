using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace NSMB.Networking {
    /// <summary>
    /// Reads the short-lived identity token supplied by the NSMB launcher page.
    /// The launcher must set window.nsmbGameIdentityToken only after the existing
    /// access login has been independently verified by the security service.
    /// This bridge transports a token; it does not authenticate the player itself.
    /// </summary>
    public static class WebGLGameIdentityBridge {

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern IntPtr NSMB_GetGameIdentityToken();

        [DllImport("__Internal")]
        private static extern void NSMB_ClearGameIdentityToken();

        [DllImport("__Internal")]
        private static extern void NSMB_FreeGameIdentityToken(IntPtr pointer);
#endif

        /// <summary>
        /// Consumes the launcher's one-time identity token.
        /// Returns an empty string for ordinary players or outside WebGL.
        /// </summary>
        public static string ConsumeToken() {
#if UNITY_WEBGL && !UNITY_EDITOR
            IntPtr pointer = IntPtr.Zero;

            try {
                pointer = NSMB_GetGameIdentityToken();
                if (pointer == IntPtr.Zero) {
                    return string.Empty;
                }

                string token = Marshal.PtrToStringAnsi(pointer);
                return token ?? string.Empty;
            } catch (Exception exception) {
                Debug.LogWarning("[Nickname Identity] Could not read the launcher identity token: " + exception.Message);
                return string.Empty;
            } finally {
                if (pointer != IntPtr.Zero) {
                    NSMB_FreeGameIdentityToken(pointer);
                }

                // Consume even malformed or unreadable values so stale identity
                // data cannot accidentally be reused by a later game session.
                try {
                    NSMB_ClearGameIdentityToken();
                } catch (Exception exception) {
                    Debug.LogWarning("[Nickname Identity] Could not clear the launcher identity token: " + exception.Message);
                }
            }
#else
            return string.Empty;
#endif
        }
    }
}
