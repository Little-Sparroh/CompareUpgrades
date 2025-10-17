# CompareUpgrades

A BepInEx mod for MycoPunk that enables side-by-side comparison of gear upgrades with intelligent tooltip positioning.

## Description

This client-side mod enhances the upgrade selection experience in MycoPunk by allowing players to compare gear upgrades side-by-side. Simply press 'C' while viewing gear upgrades to lock an upgrade for comparison, then hover over other upgrades to see a dual tooltip display showing both the locked upgrade and the currently hovered upgrade simultaneously.

The mod uses Harmony patches to integrate seamlessly with the game's existing tooltip system, creating a companion display that intelligently positions itself to avoid overlap and screen boundaries. When comparison mode is active, you'll see both upgrades' details at once, making it easier to make informed upgrade decisions.

## Getting Started

### Dependencies

* MycoPunk (base game)
* [BepInEx](https://github.com/BepInEx/BepInEx) - Version 5.4.2403 or compatible
* .NET Framework 4.8

### Building/Compiling

1. Clone this repository
2. Open the solution file in Visual Studio, Rider, or your preferred C# IDE
3. Build the project in Release mode

Alternatively, use dotnet CLI:
```bash
dotnet build --configuration Release
```

### Installing

**Option 1: Via Thunderstore (Recommended)**
1. Download and install using the Thunderstore Mod Manager
2. Search for "CompareUpgrades" under MycoPunk community
3. Install and enable the mod

**Option 2: Manual Installation**
1. Ensure BepInEx is installed for MycoPunk
2. Copy `CompareUpgrades.dll` from the build folder
3. Place it in `<MycoPunk Game Directory>/BepInEx/plugins/`
4. Launch the game

### Executing program

Once the mod is loaded, comparison mode is available in gear upgrade windows:

1. **Lock an upgrade for comparison:**
   - Open a gear details window
   - Hover over any upgrade
   - Press the 'C' key to lock that upgrade

2. **Compare with other upgrades:**
   - With an upgrade locked, hover over any other upgrade
   - Two tooltips will appear simultaneously - one for the locked upgrade, one for the hovered upgrade
   - The comparison tooltip positions itself automatically to avoid overlap

3. **Unlock comparison mode:**
   - Press 'C' again on the same locked upgrade to unlock
   - Or close/reopen the gear window to reset

Only one upgrade can be locked at a time. Comparison mode only activates when hovering over a different upgrade from the locked one.

## Help

* **Tooltips not showing?** Make sure you're in a gear details window and have BepInEx properly installed
* **'C' key not working?** Ensure you have focus on the gear window and are hovering over an upgrade slot
* **Comparison tooltip in wrong position?** The mod automatically repositions based on screen space and may adjust if both tooltips would overlap
* **Mod not loading?** Check BepInEx console for errors (requires BepInEx console enabled)
* **Conflicts with other mods?** This mod patches HoverInfoDisplay methods. If you have other tooltip modifications, they may interfere
* **Performance issues?** This mod only processes when in gear windows with comparison mode active

## Authors

* Sparroh
* funlennysub (original mod template)
* [@DomPizzie](https://twitter.com/dompizzie) (README template)

## License

* This project is licensed under the MIT License - see the LICENSE.md file for details
