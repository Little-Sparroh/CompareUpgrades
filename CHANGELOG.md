# Changelog

## 2.0.0

### Rewrite
* Ground-up rewrite of comparison mode for stability
* Lock stores `UpgradeInstance` data instead of a pooled UI row
* Safe `HoverInfoDisplay` clone that restores the game singleton after instantiate
* Comparison tooltip positioned beside the main tooltip (right, then left, then clamp)
* Configurable compare hotkey (default **C**)
* Press compare while locked always unlocks (same upgrade, different upgrade, or empty)
* Selection mark on the locked row when visible
* Display-only locked tooltip (no interactive scrap/favorite/unlock bindings)
* Proper cleanup on gear window open/close and plugin unload

## 1.0.0

* Initial release (legacy implementation)
