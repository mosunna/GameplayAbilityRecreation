# GameplayAbilityRecreation

A sandbox project recreating select Overwatch hero abilities in Unity using C#, built to study and reproduce third-person ability logic (steering, collision prediction, state rewind) on top of a from-scratch first-person controller.

Currently features three heroes' worth of abilities: Tracer's Blink/Recall, Sombra's Translocator, and D.Va's Boosters. As well as a runtime hero-select terminal for swapping hero abilities without restarting the scene.

## Tech Stack

- **Engine:** Unity 6000.4.6f1
- **Language:** C#
- **UI:** Immediate-mode `OnGUI`

## Features

### Hero Abilities

#### Tracer
- **Blink:** Short range dash in the direction of current movement input (or look direction if standing still), drawing from a three charge pool that recharges over time.
- **Recall:** Rewinds Tracer's position along a three second rolling history buffer, played back smoothly rather than snapping between saved positions.

#### Sombra
- **Translocator:** Throws a gravity affected beacon that sweeps for terrain collisions mid-flight, then teleports Sombra to its landed or current position on second activation.

#### D.Va
- **Boosters:** Launches D.Va along the camera's look direction and continuously re-reads that direction every frame (via slerp-based steering), allowing the player to curve, climb, or dive mid-flight instead of being locked to the initial launch vector. Limited by both a maximum duration and maximum travel distance, with an early-cancel window.

### Core Systems
- **Hero Select Terminal:** An in-world, interative menu that lets the player swap active hero loadouts, maintaining base movement script.
- **First-Person Controller:** Mouse-look with pitch clamping, WASD movement, and jump/gravity handled through `CharacterController`.

## Technical Highlights

- **Per-frame directional steering (D.Va):** flight direction is re-targeted every frame with `Vector3.Slerp` against `playerPOV.transform.forward` inside the boost coroutine, so the flight path responds continuously to camera input rather than committing to a single vector captured at activation.
- **Predictive collision sweeps (Sombra):** the translocator beacon uses `Physics.SphereCast` to test its *next* step before moving, preventing high speed throws from tunneling through thin or skinny geometry.
- **Fixed capacity rewind buffer (Tracer):** position history is stored in a `Queue<Vector3>` sized from `recallDuration / timeInterval`, with index-interpolated `Lerp` playback so Recall plays back smoothly instead of snapping the saved positions.
- **Displacement based range tracking (D.Va):** boost distance is accumulated from `Vector3.Distance` between actual positions each frame, not intended movement, so grinding along a wall doesn't silently eat into the range budget.
- **Data driven ability roster (Hero Select Terminal):** each hero is a `HeroLoadout` entry holding an array of ability `MonoBehaviour` references, so adding a new hero to the roster is an Inspector only change with no code edits.

## Project Structure

| Script | Responsibility |
|---|---|
| `CharacterMovement.cs` | Base WASD movement, jump, and gravity via `CharacterController`. |
| `FirstPersonPOV.cs` | Mouse-look camera with clamped pitch, decoupled from player yaw rotation. |
| `TracerAbilities.cs` | Blink dash and Recall position-rewind, including charge/cooldown management. |
| `SombraTranslocator.cs` | Beacon throw, arc simulation, and delayed teleport on activation. |
| `DvaBoosters.cs` | Camera steered flight with duration/distance caps and early cancel. |
| `HeroSelectTerminal.cs` | Look based interaction, loadout menu, and ability script enable/disable swapping. |

## Controls

| Input | Action |
|---|---|
| `W` `A` `S` `D` | Move |
| `Mouse` | Look |
| `Space` | Jump |
| `Left Shift` | Blink (Tracer) / Boosters (D.Va) |
| `E` | Recall (Tracer) / Throw or Trigger Translocator (Sombra) |
| `F` | Open/close Hero Select Terminal |

## Getting Started

1. Install Unity **6000.4.6f1** (or later) via Unity Hub.
2. Clone this repository and open it as a Unity project.
3. Open `Assets/Scenes/SampleScene.unity` and press Play.
4. Press `F` while looking at the terminal to select a hero.
