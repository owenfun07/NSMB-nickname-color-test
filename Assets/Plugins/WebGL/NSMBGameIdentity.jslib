mergeInto(LibraryManager.library, {
  NSMB_GetGameNicknameColor: function() {
    var color = "";
    try {
      color = (typeof window !== "undefined" && typeof window.nsmbGameNicknameColor === "string")
        ? window.nsmbGameNicknameColor
        : "";
    } catch (_) {
      color = "";
    }
    var size = lengthBytesUTF8(color) + 1;
    var pointer = _malloc(size);
    if (!pointer) return 0;
    stringToUTF8(color, pointer, size);
    return pointer;
  },
  NSMB_ClearGameNicknameColor: function() {
    try {
      if (typeof window !== "undefined") window.nsmbGameNicknameColor = "";
    } catch (_) {}
  },
  NSMB_FreeGameNicknameColor: function(pointer) {
    if (pointer) _free(pointer);
  }
});
