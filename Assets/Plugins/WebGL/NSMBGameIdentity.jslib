mergeInto(LibraryManager.library, {
  NSMB_GetGameIdentityToken: function() {
    var token = "";
    try {
      token = (typeof window !== "undefined" && typeof window.nsmbGameIdentityToken === "string")
        ? window.nsmbGameIdentityToken
        : "";
    } catch (_) {
      token = "";
    }

    var size = lengthBytesUTF8(token) + 1;
    var pointer = _malloc(size);
    if (!pointer) return 0;
    stringToUTF8(token, pointer, size);
    return pointer;
  },

  NSMB_ClearGameIdentityToken: function() {
    try {
      if (typeof window !== "undefined") {
        window.nsmbGameIdentityToken = "";
      }
    } catch (_) {}
  },

  NSMB_FreeGameIdentityToken: function(pointer) {
    if (pointer) _free(pointer);
  }
});
