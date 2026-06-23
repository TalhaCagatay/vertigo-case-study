# v0.1.0 — Initial Release: Card Wheel Game

A risk/reward gambling game where you spin a prize wheel across escalating zones to accumulate rewards — but hitting a bomb wipes everything out. Know when to walk away.

---

## Gameplay

- **Spin to Win**: Each spin costs 25 coins. Land on coin rewards, which accumulate in your stash across multiple zones. The reward multiplier scales up with zone progression via an `AnimationCurve`, so the deeper you go, the higher the payout.
- **Bomb Mechanic**: Slices can contain bombs. Landing on one triggers **Game Over** — you lose all accumulated rewards for that run unless you **revive** (50 coins) to continue from the same zone, or **give up** to reset back to zone 1.
- **3-Tier Zone System**: Every 5th zone is a **Silver** tier (safe — bombs removed, can leave), and every 30th zone is a **Gold** tier (safe + super zone). All other zones are **Bronze** (may contain bombs). You can only collect rewards and leave on safe or super zones.
- **Pre-Determined Spins**: The outcome is randomly selected server-side style *before* the wheel animation plays — the spin is purely visual feedback.
- **Reward Screen**: View all collected rewards and coin balance from a dedicated reward summary screen.

---

## Architecture & Design

| Pattern | Description |
|---|---|
| **State Machine** | `WheelState` enum drives all flow: `Idle → Spinning → Result` (on reward) or `GameOver` (on bomb). UI and controller behaviour is gated by state. |
| **Event-Driven** | `CardWheelController` exposes C# events (`StateChanged`, `ZoneChanged`, `RewardsUpdated`, `BombDetonated`, `SpinStarted`, etc.) decoupling game logic from presentation. `CardWheelUIController` subscribes and reacts without tight coupling. |
| **IController Lifecycle** | All controllers implement `IController` with an `IsInitialized` flag and async `Initialize()` via **UniTask**, ensuring orderly startup. |
| **MVC-Style Separation** | `CardWheelController` handles all game logic, state transitions, and coin economy. `CardWheelUIController` handles screen management, animations, popups, and zone bar updates. No UI code exists in the game logic controller. |
| **Polymorphic Reward System** | `ARewardDefinition` is a `ScriptableObject` base class with concrete types (`CoinReward`, `BombReward`) — each zone tier config can mix reward types freely. New reward types are trivial to add. |

---

## Key Technical Highlights

### ⚡ UniTask (v2.5.11)
All async operations use Cysharp's **UniTask** instead of C# `Task` or coroutines for zero-allocation async/await:
- Screen transitions (`ShowScreenAsync`, `PushPopupAsync`) are fully async
- Popup lifecycle (bomb popup with revive/give-up flow) uses `await` for clean sequential logic
- Fire-and-forget patterns via `.Forget()` where appropriate
- Editor/safe-mode detection built into the framework

### 💾 MemoryPack (v1.10.0)
Player data persistence uses Cysharp's **MemoryPack** — a zero-allocation, high-performance binary serializer:
- `PlayerData` is decorated with `[MemoryPackable]` for source-generator-powered serialization
- Player inventory (`Dictionary<string, int>`) and coin balance are serialized to binary with no reflection overhead
- `DataController` wraps load/save operations behind `PLAYER_DATA_SAVE_KEY`

### 🎬 DOTween
Wheel spin animation is powered by **DOTween** for smooth, production-quality tweens:
- 5 full rotations + random overshoot + snap-back via `Ease.OutBack`
- `RotateMode.FastBeyond360` for infinite-seeming spin effect
- Two-phase animation: fast spin → brief pause → snap to final position
- Zero runtime allocation — tweens are killed and recreated cleanly on each spin

### 🏊 LeanPool
Object pooling via **Lean Pool** for optimized prefab instantiation:
- `LeanGameObjectPool` components manage prefab recycling
- Reduces GC pressure from repetitive `Instantiate`/`Destroy` calls
- `IPoolable` interface support for custom spawn/despawn logic
- `LeanPoolDebugger` component catches incorrect spawn/despawn patterns during development

### 🧩 Reflex DI (v14.3.0)
Pure C# dependency injection via **Reflex**:
- `ControllerInstaller` registers all controllers as singletons with eager resolution
- `ZoneWheelMapping` ScriptableObject is registered as a value binding
- Clean constructor injection throughout — no service locator pattern

### 📦 ScriptableObject-Driven Configuration
All game data is designer-friendly via ScriptableObjects with custom editors:
- **`ZoneWheelMapping`** — maps zone numbers to tier configs (Bronze/Silver/Gold)
- **`WheelTierConfig`** — per-tier settings: reward slices, spinner/indicator sprites, spin duration, zone number colors, and reward multiplier curve
- **`ARewardDefinition`** — polymorphic reward definitions with icons, labels, amounts, and `Grant()` logic
- Every config has a `[CreateAssetMenu]` attribute for easy creation from the Unity Editor

### 🛠 Custom Editor Tooling
A tabbed editor window (`CardWheelConfigEditor`) built with the `IEditorTab` / `EditorTabBase` pattern:
- **Wheel Tier Config Tab** — edit tier configs visually
- **Zone Wheel Mapping Tab** — assign configs to zone ranges
- **Reward Definition Tab** — manage reward types
- `ConfigBuffer` handles dirty tracking and batch saves

### 🧱 Base Framework (`com.core`)
The project is built on a lightweight in-house framework:
- **`com.core`** — `IController` interface, `GameConfigController`
- **`com.core.data`** — `DataController` with MemoryPack-backed persistence
- **`com.core.ui`** — `UIController` for screen/popup stack management, `UIParent` for canvas hierarchy

---

## Project Structure

```
Assets/
├── _Game/
│   ├── ControllerInstaller.cs          # Reflex DI bindings
│   └── CardWheel/
│       ├── Editor/                     # Custom editor tooling
│       │   ├── CardWheelConfigEditor.cs
│       │   ├── IEditorTab.cs
│       │   ├── EditorTabBase.cs
│       │   ├── ConfigBuffer.cs
│       │   ├── WheelTierConfigEditorTab.cs
│       │   ├── ZoneWheelMappingEditorTab.cs
│       │   └── RewardDefinitionEditorTab.cs
│       └── Scripts/
│           ├── Controller/
│           │   └── CardWheelController.cs    # Core game logic & state
│           ├── Data/
│           │   ├── WheelTierConfig.cs        # Per-tier SO config
│           │   ├── ZoneWheelMapping.cs       # Zone→tier mapping SO
│           │   ├── PlayerData.cs             # MemoryPack-serialized player state
│           │   ├── AccumulatedReward.cs      # Runtime reward accumulator
│           │   └── Rewards/
│           │       ├── ARewardDefinition.cs  # Polymorphic reward base
│           │       ├── CoinReward.cs
│           │       └── BombReward.cs
│           ├── State/
│           │   └── WheelState.cs             # State machine enum
│           └── UIs/
│               ├── CardWheelUIController.cs  # UI orchestration
│               ├── Wheel/
│               │   ├── CardWheelSpinner.cs   # DOTween spin animation
│               │   ├── CardWheelSliceView.cs
│               │   └── CardWheelIndicator.cs
│               ├── Screens/
│               │   └── CardWheelScreen.cs    # Main game screen
│               ├── Popups/
│               │   └── BombPopup.cs          # Bomb game-over popup
│               ├── Rewards/                  # Reward display components
│               └── ZoneScroll/              # Zone bar horizontal scroll
├── VertigoBase/
│   ├── com.core/                    # Core framework
│   ├── com.core.data/               # Data persistence layer
│   ├── com.core.ui/                 # UI screen/popup system
│   └── com.unity.uiextensions/      # Unity UI Extensions
└── Plugins/
    └── CW/LeanPool/                 # Lean Pool (v2.1.0)
```

---

## Dependencies

| Package | Version | Purpose |
|---|---|---|
| **MemoryPack** (Cysharp) | 1.10.0 | Zero-allocation binary serialization for player data |
| **UniTask** (Cysharp) | 2.5.11 | Async/await for Unity — screen transitions, popups, initialization |
| **DOTween** | — | Wheel spin tween animations |
| **Reflex** (gustavopsantos) | 14.3.0 | Pure C# dependency injection container |
| **Lean Pool** (CW) | 2.1.0 | GameObject pooling for UI/effect prefabs |
| Unity UI Extensions | — | Additional UI components |
| TextMeshPro | 3.0.6 | Text rendering |
| Unity 2D Feature | 2.0.1 | 2D sprite and tilemap support |

---

## Notes

- This release targets Unity **2021.3 LTS** (compatible with `packages-lock.json` entries).
- All gameplay values (spin cost, revive cost, reward amounts, multiplier curves, zone progression) are configurable via ScriptableObjects — no hardcoded balancing.
- The architecture is designed to be extended: adding new reward types, zone tiers, or UI screens follows established patterns with minimal friction.