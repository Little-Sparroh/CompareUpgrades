# Changelog

## 2.0.1

- Refactor

## 2.0.0

### Changed

* Ground-up rewrite of comparison mode for stability
* Lock stores `UpgradeInstance` data instead of a pooled UI row
* Comparison tooltip positioned beside the main tooltip (right, then left, then clamp)
* Pressing the compare hotkey while locked always unlocks (same upgrade, different upgrade, or empty space)

### Added

* Configurable compare hotkey (default **C**)
* Selection mark on the locked row when visible
* Safe `HoverInfoDisplay` clone that restores the game singleton after instantiate
* Display-only locked tooltip via proxy (no interactive scrap, favorite, or unlock bindings)

### Fixed

* Proper cleanup on gear window open/close and plugin unload
* Compare hotkey ignored while a search or text input field is focused

## 1.0.0

* Initial release (legacy implementation)
