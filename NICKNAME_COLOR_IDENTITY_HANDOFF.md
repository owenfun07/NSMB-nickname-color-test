# Owen nickname-color identity handoff (test project)

This test-project change is deliberately limited to the Unity side. It does not deploy or change the live Apps Script or the production launcher.

## Flow

1. The existing launcher verifies a password on a restricted network.
2. The launcher calls `issueGameIdentityToken` with the verified IP and password hash.
3. Apps Script independently confirms that the hash belongs to the enabled account named exactly `Owen`, and confirms the IP is still in the restricted-network list.
4. The launcher immediately exchanges the short-lived, one-use token through `consumeGameIdentityToken`.
5. Only the returned public color is placed in `window.nsmbGameNicknameColor`.
6. Unity consumes that color once when creating RuntimePlayer. Existing Quantum game logic synchronizes RuntimePlayer.NicknameColor to all clients.

The password hash and one-time token are never passed into Unity. Guests, daily-code access, unrestricted networks, other accounts, invalid tokens, and expired tokens get no special color.

## Apps Script update

Use the complete updated Apps Script file supplied with this project update:
`NSMB_Access_API_with_Game_Identity.gs`

It preserves the current APIs and adds two routes:
- `issueGameIdentityToken`
- `consumeGameIdentityToken`

The new token is valid for five minutes and can be consumed only once. Do not deploy until you have reviewed the code and are ready to test it.

## Launcher integration snippet

Add this helper beside the existing `jsonp()` helper in the launcher page:

```js
async function prepareOwenNicknameColor(ip, hash, person) {
  window.nsmbGameNicknameColor = "";

  // Never infer identity from a display name stored in browser storage.
  // This helper is called only after a fresh server verify response.
  if (person !== "Owen" || !ip || !hash) return;

  try {
    const issued = await jsonp({
      action: "issueGameIdentityToken",
      ip: ip,
      hash: hash
    });

    if (!issued || !issued.ok || !issued.token) return;

    const consumed = await jsonp({
      action: "consumeGameIdentityToken",
      token: issued.token
    });

    if (
      consumed &&
      consumed.ok &&
      consumed.person === "Owen" &&
      /^#[0-9A-Fa-f]{6}$/.test(consumed.color || "")
    ) {
      window.nsmbGameNicknameColor = consumed.color.toUpperCase();
    }
  } catch (error) {
    // Identity integration is optional: never block normal gameplay.
    console.warn("Could not load Owen's nickname color:", error);
    window.nsmbGameNicknameColor = "";
  }
}
```

Call it only in the two places where the server has just returned a successful restricted-network `verify` response:

**Saved-login verification**, inside `if (sessionResult.allowed)`, before `allowEntry(silent)`:

```js
await prepareOwenNicknameColor(currentIP, savedHash, sessionResult.person);
allowEntry(silent);
return;
```

**Fresh password verification**, inside the successful password-submit branch, before the existing delayed `allowEntry` call:

```js
await prepareOwenNicknameColor(currentIP, hash, result.person);
setTimeout(allowEntry, 650);
```

Also clear the global at the beginning of each `checkNetwork()` run:

```js
window.nsmbGameNicknameColor = "";
```

Do not call the helper in the unrestricted-network path or daily-code path. Do not call it using `sessionStorage.nsmb_player_name`; the `person` argument must come directly from the current successful `verify` response.

## Important scope note

The current Apps Script color validator still supports solid `#RRGGBB` only. Gradient serialization is a separate change because the Quantum synced field is currently `QStringUtf8<48>`; do not try to squeeze the existing 12-float gradient representation into that field.

This integration does not yet constitute a tested Unity WebGL build. The project still needs a Unity WebGL build and an end-to-end test using the test launcher before any production rollout.
