# Scene Transition System – Design

## 1. Problem

`GameManager.ChangeState` currently swaps scenes instantly: `DelayedChangeState` calls `OnExit` on the old state(s) and `OnEnter` on the new one in the same frame. We want animated transitions (fade to black, wipe, slide, ...) without:

- making states or scenes know about animations,
- breaking the existing rules (`CanTransisionTo`, additive states, `ActivatePreviousState`, `force`),
- allowing player input or game logic to run while the screen is half-transitioned.

## 2. Goals and non-goals

**Goals**
- Animated "out" (cover the screen) and "in" (reveal the screen) phases around every state change.
- Transition type configurable per state change (e.g. MainMenu → InGame fades, InGame → GameOver wipes).
- States stay unaware of the visuals; adding a new transition type does not touch the FSM.
- Safe against repeated/overlapping requests.
- Fits the future threaded scene loading (the cover hides loading time).

**Non-goals (for now)**
- Implementation. This document is design only.
- Transitions *inside* a scene (e.g. level intro animation). These belong to the scene itself.

## 3. Overview

The key idea: **the FSM decides *what* to change to; the transition system decides *how it looks*.** A new autoload, `TransitionManager`, owns a full-screen overlay. `GameManager` asks it to cover the screen, performs the state swap while the screen is covered, then asks it to reveal.

```
 caller (UI / game code)
        │ ChangeState(Settings, transition?)
        ▼
 ┌─────────────┐  1. PlayOut(transition)   ┌──────────────────┐
 │ GameManager │ ────────────────────────► │ TransitionManager│
 │   (FSM)     │ ◄──── Out finished ────── │  (CanvasLayer)   │
 │             │  2. swap states           │                  │
 │ OnExit/     │     (screen is covered)   │                  │
 │ OnEnter     │  3. PlayIn(transition)    │                  │
 │             │ ────────────────────────► │                  │
 └─────────────┘ ◄──── In finished ─────── └──────────────────┘
```

### Components

| Component | Kind | Responsibility |
|---|---|---|
| `TransitionManager` | Autoload `CanvasLayer` (high layer, e.g. 100) | Owns the overlay, plays a transition's Out/In phase, blocks input while active, emits completion signals. |
| `TransitionBase` | Abstract class / `Resource` | One visual effect. Methods `PlayOut()` and `PlayIn()` each return an awaitable. Subclasses: `FadeTransition`, `WipeTransition`, `NoTransition`, ... |
| `TransitionType` | `enum` | `None`, `Fade`, `Wipe`, ... Used for configuration and lookups, analogous to `GameStateType`. |
| `TransitionRule` | Small data class | Maps `(fromState, toState)` → `TransitionType` + duration. |
| `GameManager` | Existing | Orchestrates: Out → swap → In. Gains a transition phase (see §4). |
| `GameStateBase` | Existing | Optional virtual hooks only (see §6). No visual knowledge. |

## 4. Integration with the FSM

### 4.1 Two-level state: "what" and "when"

`GameStateBase` stays the *game* FSM. On top of it `GameManager` gets a tiny internal *change phase*:

```
Idle ──ChangeState()──► TransitioningOut ──Out done──► Swapping ──swap done──► TransitioningIn ──In done──► Idle
```

- **Idle** – normal operation, game states run.
- **TransitioningOut** – the old state is still active and loaded, but input is blocked; the overlay animates to fully opaque.
- **Swapping** – the screen is fully covered. `OnExit`/`OnEnter` run here (the existing `DelayedChangeState` logic). Scene loading/instantiation happens here, so loading hitches are hidden.
- **TransitioningIn** – the new state is active; the overlay animates away. Input is still blocked (configurable) so the player cannot act on a half-visible screen.

`ActiveState` is **updated only in the Swapping phase**, so the rest of the code never sees an inconsistent state.

### 4.2 Changed `ChangeState`

```csharp
public bool ChangeState(GameStateType target, bool force = false,
                        TransitionType? transition = null)
{
    // Validation stays exactly as today (CanTransisionTo, state exists).
    // New: refuse or queue while a change is already in progress (see §7).

    _nextState = nextState;
    _ = RunChangeAsync(force, transition ?? _rules.Resolve(ActiveState?.StateType, target));
    return true; // "request accepted", not "change finished"
}

private async Task RunChangeAsync(bool force, TransitionType type)
{
    _phase = ChangePhase.TransitioningOut;
    await TransitionManager.Instance.PlayOut(type);

    _phase = ChangePhase.Swapping;
    DelayedChangeState(force);                 // unchanged body
    await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame); // let deferred AddChild finish

    _phase = ChangePhase.TransitioningIn;
    await TransitionManager.Instance.PlayIn(type);

    _phase = ChangePhase.Idle;
    EmitSignal(SignalName.StateChangeFinished, (int)ActiveState.StateType);
}
```

The synchronous `bool` return value keeps meaning "is this request valid", so existing callers (`MainMenuControl`, `GameOverControl`, ...) keep working. Callers who need to know when it finished use the new `StateChangeFinished` signal.

### 4.3 `ActivatePreviousState`

It goes through the same pipeline (`Out → pop/OnEnter → In`), with a rule lookup for `(current → previous)`. This is the natural moment for the TODO in the code ("refactor ChangeState to support this corner case"): both paths become one `RunChangeAsync` taking a *swap action* (`Action`) instead of hard-coding push or pop.

### 4.4 Additive states (Settings, pause menus)

Additive states keep the previous scene loaded underneath. Transitions are still allowed, but the default rule for additive targets is a short fade or `None`, because a full-screen cover over a half-hidden game feels heavy. The rule table decides; the code path is identical.

## 5. Selecting a transition

Resolution order, first match wins:

1. Explicit argument: `ChangeState(GameOver, transition: TransitionType.Wipe)`.
2. Rule for the exact pair `(from, to)`.
3. Rule for the target state only `(*, to)`.
4. Project default (`Fade`, 0.3 s).

Rules live in a Godot `Resource` (`TransitionRules.tres`) so designers can edit them in the Inspector without recompiling. Example:

| From | To | Transition | Duration |
|---|---|---|---|
| MainMenu | InGame | Fade (black) | 0.5 s |
| InGame | GameOver | Wipe | 0.6 s |
| * | Settings | None / quick fade | 0.15 s |
| * | * | Fade | 0.3 s |

## 6. How states and scenes participate

States remain mostly unaware. Two **optional** hooks are added to `GameStateBase` with empty default implementations:

```csharp
// Lets a state veto/skip the transition, e.g. the first state at boot.
public virtual TransitionType? PreferredEnterTransition => null;

// Called when the In phase finished. Good place to start gameplay (ball launch, music).
public virtual void OnTransitionFinished() { }
```

Why this matters: `InGameState` should not start the ball or the timer while the screen is still fading in. Today `OnEnter` runs immediately; with transitions, gameplay-start logic moves to `OnTransitionFinished` (or the scene listens to `StateChangeFinished`). Scene `_Ready` code that starts gameplay on load is the main thing to audit.

## 7. Edge cases and rules

| Situation | Behaviour |
|---|---|
| `ChangeState` called during a transition | **Ignore** with a warning by default (simple, predictable). Alternative: keep a single-slot queue holding the latest request and run it after `Idle`. |
| Double click on a menu button | Input is blocked from the start of `TransitioningOut`; the overlay `Control` uses `MouseFilter = Stop`, and `GetViewport().SetInputAsHandled()` guards keyboard/gamepad. |
| Game paused (`SceneTree.Paused`) | `TransitionManager.ProcessMode = Always`, so it animates while paused. |
| Time scale | Transition tweens ignore `Engine.TimeScale` (`Tween.SetIgnoreTimeScale`) so slow-motion does not stretch fades. |
| `force == true` (reload same state) | Same pipeline; the swap reloads the scene. Useful for "retry". |
| Invalid transition → no change | Validation fails *before* the Out phase, so the screen never fades for nothing. |
| Failure during swap (scene missing) | Log the error, still run `PlayIn` so the player is not stuck on black, and return to the previous state if possible. |
| Skipping | Optional: pressing a key during a long transition calls `Skip()` which completes the tween instantly. |
| Slow loading | Out finishes, the overlay stays opaque while loading continues. With threaded loading, the Swapping phase awaits the loader and can show a loading indicator on the overlay. |

## 8. `TransitionManager` details

- **Overlay**: `CanvasLayer` (layer 100) → `ColorRect`/`TextureRect` filling the viewport, optionally with a `ShaderMaterial`. It is always present in the autoload, hidden when idle (`Visible = false`) to avoid overdraw cost.
- **Animation backend**: Godot `Tween` for simple fades and shader-parameter animation (wipe, circle iris, dissolve with a mask texture). An `AnimationPlayer` can be used for authored transitions. Both are hidden behind `TransitionBase`.
- **Awaiting**: each phase returns a `Task`/`SignalAwaiter` completed by the tween's `Finished` signal, so `GameManager` can simply `await` it.
- **Public API**:
  ```csharp
  Task PlayOut(TransitionType type);
  Task PlayIn(TransitionType type);
  bool IsTransitioning { get; }
  void Skip();
  [Signal] delegate void TransitionStarted(int type);
  [Signal] delegate void TransitionFinished(int type);
  ```
- **Extensibility**: a new effect = a new `TransitionBase` subclass + enum value + registration in a dictionary (the same pattern we plan to move `_gameStates` to).

```csharp
public abstract class TransitionBase
{
    public abstract TransitionType Type { get; }
    public float Duration { get; set; } = 0.3f;
    public abstract Task PlayOut(TransitionOverlay overlay); // 0 → covered
    public abstract Task PlayIn(TransitionOverlay overlay);  // covered → 0
}
```

## 9. Sequence example: MainMenu → InGame

1. Player presses Play. `MainMenuControl` calls `GameManager.Instance.ChangeState(GameStateType.InGame)`.
2. Validation passes → rule `(MainMenu, InGame)` = Fade 0.5 s. Phase = `TransitioningOut`, input blocked.
3. Overlay fades to black (menu still visible/active but unresponsive).
4. Phase = `Swapping`: `MainMenuState.OnExit(false)` frees the menu, `InGameState.OnEnter()` instantiates the level. One frame is awaited so the deferred `AddChild` completes.
5. Phase = `TransitioningIn`: overlay fades out revealing the level; game logic not started yet.
6. Phase = `Idle`; `InGameState.OnTransitionFinished()` launches the ball; input unblocked; `StateChangeFinished` is emitted.

## 10. Alternatives considered

| Alternative | Why not chosen |
|---|---|
| Each state plays its own animation in `OnEnter`/`OnExit` | Duplicates code, couples states to visuals, and `OnExit` would need to become async. |
| Transitions as FSM states (`TransitionState` between A and B) | Makes `CanTransisionTo` tables explode (every pair needs a hop) and complicates the stack of additive states. A *phase* inside the manager is simpler. |
| Put overlay in every scene | Cannot survive a scene swap, which is the whole point. |
| Use Godot signals only, no async | Works, but results in callback chains; `await` is much easier to read and reason about. |

## 11. Implementation plan (suggested order)

1. Add `TransitionManager` autoload with a plain `FadeTransition` and `PlayOut`/`PlayIn`.
2. Make `GameManager.ChangeState` run Out → swap → In with a hard-coded fade; add the phase guard against overlapping requests.
3. Route `ActivatePreviousState` through the same pipeline.
4. Add `TransitionRules` resource and `TransitionType` selection.
5. Move gameplay start into `OnTransitionFinished`.
6. Add more effects (wipe/iris shaders), skip support, loading indicator.

## 12. Testing

- Unit-style: rule resolution (`from`,`to` → type), phase guard (second request while busy is rejected/queued).
- Manual: spam-click buttons during a transition; change state while paused; reload (`force`) the current state; Settings open/close from MainMenu; InGame → GameOver → MainMenu loop for leaks (overlay left visible, scenes not freed).
