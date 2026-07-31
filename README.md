# CompareUpgrades

A BepInEx mod for Mycopunk that lets you lock one upgrade tooltip and compare it side-by-side with another.

## How to use

1. Open a gear details window.
2. Hover an upgrade and press **C** (configurable) to lock it for comparison.
3. Hover a different upgrade — the locked upgrade's tooltip appears next to the normal one.
4. Press **C** again (on the same upgrade, a different one, or empty space) to unlock.
5. Closing or reopening the gear window also clears the lock.

The compare hotkey is ignored while a search or text input field is focused.

## Configuration

`BepInEx/config/sparroh.compareupgrades.cfg`

| Section  | Setting     | Default | Description                                                                                        |
|----------|-------------|---------|----------------------------------------------------------------------------------------------------|
| Keybinds | Compare Key | C       | Hotkey to lock/unlock an upgrade for side-by-side comparison while the gear details window is open |

## Dependencies

* Mycopunk
* [BepInEx](https://github.com/BepInEx/BepInEx) 5.4.2403+

## Building

```bash
dotnet build --configuration Release
```

Output: `bin/Release/netstandard2.1/CompareUpgrades.dll`

## Install

Place `CompareUpgrades.dll` in `BepInEx/plugins/`, or install via Thunderstore.

## Notes

* Client-side only.
* Locks the upgrade **data**, not the list row, so sorting and scrolling do not break the lock.
* The comparison tooltip is display-only (no scrap, favorite, or unlock bindings).
* A selection mark highlights the locked row when that row is still on screen.
* The comparison tooltip is placed beside the main tooltip (right first, then left, then clamped on screen).

## License

MIT
