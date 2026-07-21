# CompareUpgrades

A BepInEx mod for Mycopunk that lets you lock one upgrade tooltip and compare it side-by-side with another.

## How to use

1. Open a gear details window.
2. Hover an upgrade and press **C** (configurable) to lock it for comparison.
3. Hover a different upgrade — the locked upgrade's tooltip appears next to the normal one.
4. Press **C** again (on the same upgrade, a different one, or empty space) to unlock.
5. Closing the gear window also clears the lock.

## Configuration

`BepInEx/config/sparroh.compareupgrades.cfg`

| Setting | Default | Description |
|---------|---------|-------------|
| Compare Key | C | Hotkey to lock/unlock comparison |

## Dependencies

* Mycopunk
* [BepInEx](https://github.com/BepInEx/BepInEx) 5.4.2403+

## Building

```bash
dotnet build --configuration Release
```

Output: `bin/Release/net48/CompareUpgrades.dll`

## Install

Place `CompareUpgrades.dll` in `BepInEx/plugins/`, or install via Thunderstore.

## Notes

* Client-side only.
* Locks the upgrade **data**, not the list row, so sorting/scrolling won't break the lock.
* The comparison tooltip is display-only (no scrap/favorite bindings).
* A selection mark highlights the locked row when that row is still on screen (feedback may change later).

## License

MIT
