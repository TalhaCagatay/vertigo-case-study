# Vertigo Case Study — Card Wheel

A Unity-based gambling game where players spin a tiered wheel to progress through zones, accumulate rewards, and avoid bombs.

---

## Table of Contents

- [How to Play](#how-to-play)
- [Zone & Tier System](#zone--tier-system)
- [Reward Types](#reward-types)
- [Game States](#game-states)
- [Editor Tools](#editor-tools)
  - [CardWheel Config Editor](#cardwheel-config-editor)
  - [Wheel Tier Config](#wheel-tier-config)
  - [Zone Wheel Mapping](#zone-wheel-mapping)
  - [Reward Definitions](#reward-definitions)
- [Debugging (SRDebugger)](#debugging-srdebugger)
- [Project Architecture](#project-architecture)
- [Getting Started](#getting-started)

---

## How to Play

1. **Pay to spin:** Each spin costs **10 coins**. The game pre-selects a random slice before the wheel animation begins.
2. **Land on a reward:** The wheel spins and lands on the pre-selected slice. You either win a reward or hit a bomb.
3. **Accumulate or die:** Rewards accumulate across spins until you hit a bomb (instant game over) or choose to leave safely.
4. **Revive or give up:** If you hit a bomb, you can pay **50 coins** to revive and continue from the same zone, or give up and reset back to zone 1.
5. **Leave safely:** You can collect your accumulated rewards and leave at any time when the current tier has no bomb (safe zone) or on a super zone (every 30th zone).

| Action | Cost |
|--------|------|
| Spin   | 10 coins |
| Revive | 50 coins |

---

## Zone & Tier System

The wheel changes as you progress through zones. There are three tiers mapped by zone number:

| Tier   | When                                       |
|--------|--------------------------------------------|
| Bronze | Default — most zones                       |
| Silver | Every 5th zone (multiples of 5, except multiples of 30) |
| Gold   | Every 30th zone — also called **Super Zone** |

Each tier can have a different visual theme (spinner/indicator sprites, zone-number colors), spin duration, reward scaling curve, and slice set.

- **Safe zone:** A tier that contains no bomb slices. You can leave at any time in a safe zone.
- **Super zone:** Every 30th zone. You can always leave at a super zone.

---

## Reward Types

All reward types inherit from `ARewardDefinition` (ScriptableObject). Available reward types:

| Type          | Description                                       |
|---------------|---------------------------------------------------|
| Coin          | Adds coins to your balance                        |
| Chest         | Grants a chest reward                             |
| Skill Point   | Awards a skill point                              |
| Cosmetic      | Unlocks a cosmetic item                           |
| Bomb          | Instant game over — ends the run immediately      |

Reward amounts scale with the current zone using a configurable `AnimationCurve` (`RewardScaleCurve`) defined per tier.

---

## Game States

| State      | Description                     |
|------------|---------------------------------|
| Idle       | Waiting for the next action     |
| Spinning   | Wheel animation in progress     |
| Result     | Spin completed, reward shown    |
| GameOver   | Player hit a bomb               |

---

## Editor Tools

### CardWheel Config Editor

Accessible from the Unity menu: **Vertigo → CardWheel → Config Editor**

This is the central editor window for managing all CardWheel configuration assets. It features three tabbed panels, non-destructive editing with Apply/Revert, rename support, and unsaved-change protection dialogs.

<!-- >>> SCREENSHOT PLACEHOLDER: CardWheel Config Editor window (all tabs) <<< -->

#### Key Features

- **Tabbed interface** — Wheel Tiers, Zone Mapping, and Rewards tabs
- **CRUD operations** — New, Duplicate, and Delete assets directly from each tab
- **Non-destructive editing** — Changes are buffered via `ConfigBuffer<T>`; Apply or Revert at any time
- **Dirty tracking** — Visual indicator ("● Unsaved changes") when edits are pending
- **Unsaved-change dialogs** — Prompted before switching tabs or selecting a different asset
- **Rename support** — Edit the asset name inline and Apply to rename on disk

#### Architecture

- **`IEditorTab`** — Interface all tabs implement (`IsDirty`, `OnGUI`, `Apply`, `Revert`)
- **`EditorTabBase<T>`** — Generic base class providing asset list panel, selection handling, buffered editing, Apply/Revert, and CRUD operations
- **`ConfigBuffer<T>`** — Manages an in-memory working copy cloned from the original asset. Tracks dirty state, applies changes back to disk, or reverts to original
- Concrete tabs: `WheelTierConfigEditorTab`, `ZoneWheelMappingEditorTab`, `RewardDefinitionEditorTab`

---

### Wheel Tier Config

<!-- >>> SCREENSHOT PLACEHOLDER: Wheel Tier Config tab with an open asset <<< -->

Defines the visual and gameplay properties of a wheel tier. Stored as `WheelTierConfig` ScriptableObjects under `Assets/_Game/CardWheel/Configs/`.

**Configurable fields:**

| Field                    | Description                                    |
|--------------------------|------------------------------------------------|
| Spinner Sprite           | The wheel spinner visual                       |
| Indicator Sprite         | The indicator/pointer visual                   |
| Slices                   | Array of reward definitions (8 slots)          |
| Zone Number Selected Color | Color of the current zone number            |
| Zone Number Past Color   | Color of already-passed zone numbers           |
| Zone Number Future Color | Color of upcoming zone numbers                 |
| Spin Duration            | How long the wheel spin animation lasts        |
| Reward Scale Curve       | AnimationCurve controlling how rewards scale per zone |

---

### Zone Wheel Mapping

<!-- >>> SCREENSHOT PLACEHOLDER: Zone Wheel Mapping tab <<< -->

Maps zones to wheel tier configs. A single `ZoneWheelMapping` ScriptableObject defines which tier is active for each zone.

**Logic:**
- Multiples of 30 → **Gold** (super zone)
- Multiples of 5 (excluding 30) → **Silver**
- Everything else → **Bronze**

---

### Reward Definitions

<!-- >>> SCREENSHOT PLACEHOLDER: Reward Definitions tab <<< -->

Individual reward assets of types: Coin, Chest, Skill Point, Cosmetic, and Bomb. Each reward has an icon, amount, label, and a unique ID.

---

## Debugging (SRDebugger)

The project uses **SRDebugger** for runtime debugging in builds. The options are defined as `partial class SROptions` in `Assets/_Game/SROptions/`.

### How to Open

**Tap the top-left corner of the screen 3 times** to open the SRDebugger panel.

<!-- >>> SCREENSHOT PLACEHOLDER: SRDebugger panel showing SROptions <<< -->

### Available Options

| Category  | Option         | Description                                                      |
|-----------|----------------|------------------------------------------------------------------|
| Data      | DeleteAllData  | Clears all player data. On Android, auto-restarts the app.       |
| Economy   | Coin           | Read/write slider (increment: 100) to adjust coin balance.       |
| Zone      | AdvanceZone    | Skips to the next zone for testing tier transitions.             |

---

## Project Architecture

- **DI Framework:** Reflex — controllers are registered via `ControllerInstaller` and resolved at runtime
- **Async:** UniTask for async initialization flows
- **Config-driven:** All gameplay parameters live in ScriptableObjects (no hardcoded values)
- **Layered separation:**
  - `Controller/` — game logic (`CardWheelController`)
  - `Data/` — ScriptableObject configs and reward definitions
  - `State/` — state machine (`WheelState`)
  - `UIs/` — UI layer (screens, popups, wheel, zone scroll)
  - `Editor/` — custom editor tooling

---

## Getting Started

1. Open the project in Unity (version specified in `ProjectSettings/ProjectVersion.txt`)
2. Open the main scene: `Assets/_Game/Scenes/main.unity`
3. Press Play to run the game
4. To edit configs, use **Vertigo → CardWheel → Config Editor**
5. For runtime debugging, tap top-left 3 times to open SRDebugger

> **Required packages** are listed in `Packages/manifest.json`. Key dependencies include Reflex (DI), UniTask, DOTween, and SRDebugger.