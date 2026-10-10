# Neon Rift 3D — Week 8 Collaboration Demo

A Godot 4 **.NET/C#** teaching project to demonstrate OOP, game rules,
Godot APIs, and GitHub collaboration. All source-code comments are in English.

## Running

1. Open this directory as a project in a compatible Godot 4 .NET editor.
2. Build the C# project in Godot, then run `Scenes3D/Main3D.tscn`.
3. Use **W/A/S/D** to move; **move the mouse** to aim; hold **left mouse button** to shoot.
4. Observe kills, score, victory, and defeat messages in the **Godot Output panel**.

The project targets `Godot.NET.Sdk/4.7.1` and `net8.0` as in the supplied
project. The assembly is named `NeonRiftStage1` for compatibility; this
legacy assembly name is **not** the current gameplay stage or dimension.

## Game rules (values defined by our C# code)

| Rule | Value | File |
| --- | --- | --- |
| Victory | Survive 60 seconds; losing all health earlier is defeat | `Game3D.cs` |
| Initial enemy spawn | 1.5 s after start | `EnemySpawnSchedule.cs` |
| Subsequent spawns | 2.5 s gaps, then 1.75 s at 20 s, then 1.0 s at 40 s | `EnemySpawnSchedule.cs` |
| Enemy progression | Chasers while kills < 4; Strikers from 4 onward | `Game3D.cs` |
| Player max health | 100 HP | `Player3D.cs` |
| Player movement | 7 world units/s | `Player3D.cs` |
| Player firing rate | 5 shots/s (0.2 s apart) | `Player3D.cs` |
| Player damage immunity | 0.6 s after taking damage | `Player3D.cs` |
| Player bullet | 14 units/s, 24 damage, 2 s lifetime | `Game3D.cs`, `Bullet3D.cs` |
| Enemy bullet | 8 units/s, 11 damage, 4 s lifetime | `Game3D.cs`, `Bullet3D.cs` |
| Chaser | 38 HP, 2.0 units/s, 14 contact damage, 100 score | `ChaserEnemy3D.cs` |
| Chaser stopping distance | 1.0 world unit | `ChaserEnemy3D.cs` |
| Striker | 72 HP, 3.2 units/s, 22 contact damage, 180 score | `StrikerEnemy3D.cs` |
| Striker preferred distance | 4.5–6.5 world units | `StrikerEnemy3D.cs` |
| Striker fire | First at 1.0 s, then every 1.6 s if within 12 units | `StrikerEnemy3D.cs` |
| Enemy contact | Up to 1 hit per 0.6 s within 1.1 units | `Enemy3D.cs` |

**Engine vs user code:** `Node3D`, `CharacterBody3D`, `Vector3`,
`_PhysicsProcess`, `MoveAndSlide`, `MoveAndCollide`, `GD.Load`, etc. are
**Godot-provided**. Game parameters, behaviours, class responsibilities,
and win/lose conditions are **our own design**.

## Week 8 GitHub collaboration

**Student A**: edit `Scripts3D/StrikerEnemy3D.cs` — change the shooting
interval from `1.6f` to `1.2f`; also update the corresponding comment.

**Student B**: edit `Scripts3D/Player3D.cs` — change the player speed
from `7f` to `8.5f`; also update the corresponding comment.

These independent edits are normally **mergeable** because they modify
different files. To intentionally demonstrate a **merge conflict**, let
both students branch from the same commit and change
`Game3D.cs`'s `SurvivalSeconds = 60f` line to different values. Merge one
branch into `main` first, then merge the other **without first reconciling
those conflicting changes**; Git may require manual resolution. The
existence of two pull requests alone does not necessarily create a conflict.

For detailed call flow, signals, and collision layers, see `ARCHITECTURE.md`.

## Survival Mode: enemy spawning and difficulty

The difficulty schedule reads elapsed time from the existing 60-second countdown
in `Game3D`. It does not own a second round timer or change victory, defeat,
score, player controls, enemy stats, or UI. Kills still unlock Strikers after four
kills, but do not end the round.

Each spawn schedules the next one using the interval for its scheduled timestamp.
Crossing 20 or 40 seconds does not force an immediate spawn or move an already
scheduled spawn. If the player stays alive, there are 8 scheduled enemies in
0-20 seconds, 11 in 20-40 seconds, and 20 in 40-60 seconds (39 total).
Long physics steps catch up due spawns while the round is running. At 60 seconds,
victory takes priority and no late spawns are created. Starting a new round resets
the opening delay and difficulty.

### Validation

Requires the .NET 8 SDK or newer, plus the compatible Godot .NET editor for runtime
tests. The console tests do not require a Godot editor or third-party test library:

```powershell
dotnet build NeonRiftStage1.csproj
dotnet run --project Tests/SpawnScheduleTests/SpawnScheduleTests.csproj
./Tests/RunGodotSmoke.ps1 -GodotPath "C:/path/to/Godot_mono_console.exe"
```

The Godot smoke tests run a temporary copy of the real scenes and C# scripts.
They check generation, difficulty, enemy configuration, ten-kill continuation,
victory, defeat, and restart. They do not test keyboard/mouse input, rendering,
or gameplay balance. In the editor, also play a full round with WASD and the left
mouse button. Timer and results currently appear through gameplay state and the
Output panel; this version does not add a HUD.

### Team integration

This feature builds on the teammate's `Add Survival Mode v1` commit (`af5bb54`).
Keep the teammate's `_remainingTime`, 60-second victory check and early return,
`OnEnemyDestroyed`, and victory/defeat handlers when merging. Keep the schedule
reset in `StartGame` and the `CountDueSpawns` call after the victory check. Avoid
restoring the original ten-kill victory or the separate capped spawn timer.
The `Compile Remove` in the project file keeps validation sources out of the game
assembly; the SDK version remains `Godot.NET.Sdk/4.7.1`.
