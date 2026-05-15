# Stealth

A Unity stealth game project using Game Creator 2, with custom editor tooling for working with installed Game Creator demo packs.

## Game Creator Demo Browser

This project includes a custom Unity Editor window that dynamically lists demo packs installed under:

```text
Assets/Plugins/GameCreator/Installs
```

Open it from the Unity menu:

```text
Game Creator > Demo Browser...
```

The browser is designed for projects where different users may install different Game Creator examples, skins, weapons, UI packs, and module demos.

## Features

- Dynamically scans installed demo folders at editor time.
- Groups and filters demos by module, such as Behavior, Dialogue, Inventory, Melee, Shooter, Stats, and Quests.
- Search by demo name, folder name, or module.
- Shows colorful demo cards with version and asset counts.
- Displays best-effort previews from textures, prefabs, assets, or scene thumbnails when Unity can provide one.
- Opens the first scene found in a demo pack.
- Pings or selects the demo folder in the Unity Project window.
- Refreshes automatically when project assets change.

## Tool Location

Main editor window:

```text
Assets/Plugins/GameCreator/Packages/Core/Editor/Installs/Windows/Templates/DemoBrowserWindow.cs
```

Unity metadata:

```text
Assets/Plugins/GameCreator/Packages/Core/Editor/Installs/Windows/Templates/DemoBrowserWindow.cs.meta
```

## Requirements

- Unity project with Game Creator 2 installed.
- Demo packs installed under `Assets/Plugins/GameCreator/Installs`.
- The tool runs in the Unity Editor only.

## Notes

Preview images are best effort. Some demo folders contain scenes or assets that Unity cannot render as a large preview thumbnail, so those cards may show a simple fallback preview area.

When opening a demo scene, Unity will ask to save modified scenes first.
