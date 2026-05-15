# Game Creator Example Browser

A Unity Editor tool for browsing the Game Creator demo/example packs installed in a project.

The browser scans the local Game Creator installs folder dynamically, so it works even when each user has a different set of examples, skins, weapons, UI packs, or module demos installed.

## What It Does

The Example Browser lists everything found under:

```text
Assets/Plugins/GameCreator/Installs
```

It turns those folders into a colorful editor window with searchable, filterable cards. Each card shows the demo pack name, module, version, basic asset counts, and a best-effort preview when Unity can generate one.

## Open The Window

In Unity, use:

```text
Game Creator > Demo Browser...
```

## Features

- Dynamic discovery of installed Game Creator demo/example folders.
- Module filter for packs such as Behavior, Dialogue, Inventory, Melee, Shooter, Stats, Quests, and more.
- Search by display name, raw folder name, or module.
- Colorful card layout built for quick scanning.
- Best-effort previews from textures, prefabs, assets, or scene thumbnails.
- Scene button to open the first `.unity` scene found in a pack.
- Folder button to ping the install folder in the Project window.
- Select button to select the install folder asset.
- Refresh button plus automatic refresh when project assets change.

## Folder Naming

The browser expects the normal Game Creator install folder pattern:

```text
Module.Package@Version
```

Examples:

```text
Dialogue.Examples@1.3.13
Shooter.Weapons@1.1.4
Stats.UI@1.3.5
```

The module name comes from the text before the first dot. The version comes from the text after `@`.

## Files

Editor window:

```text
Assets/Plugins/GameCreator/Packages/Core/Editor/Installs/Windows/Templates/DemoBrowserWindow.cs
```

Unity meta file:

```text
Assets/Plugins/GameCreator/Packages/Core/Editor/Installs/Windows/Templates/DemoBrowserWindow.cs.meta
```

## Requirements

- Unity Editor.
- Game Creator 2 installed in the project.
- Example/demo packs installed under `Assets/Plugins/GameCreator/Installs`.

## Preview Notes

Previews are best effort. Some folders only contain scenes or assets that Unity cannot render as large thumbnails, so the card may show a fallback preview area.

Opening a scene uses Unity's normal scene workflow and prompts to save modified scenes first.
