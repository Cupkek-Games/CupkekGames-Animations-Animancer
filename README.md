# CupkekGames Animations — Animancer Bridge

Concrete backend for [CupkekGames.Animations](https://github.com/Cupkek-Games/CupkekGames-Animations) on top of the [Kybernetik Animancer](https://kybernetik.com.au/animancer/) asset.

## What's inside

**Runtime** (`CupkekGames.Animations.Animancer.asmdef`)

- `AnimancerAnimationEngine` — `IAnimationEngine` impl wrapping `AnimancerComponent`
- `CharacterAnimationStateAnimancer` — `IAnimationStateController` impl. Starts from a shared `AnimationKindSet` (optional) and adds its own `List<AnimationKindEntry>` mapping kind strings (Idle / Walk / GetHit / Death / Win + game-defined kinds) to `ClipTransition`s, with optional per-entry `ReturnToKind` / `OnlyFromKind` rules; its own entries win on the same kind.
- `AnimationKindSet` — a shared kind list (ScriptableObject, `Create > CupkekGames > Animations > Animation Kind Set`), for example one per body type, so each character lists only her signature moves. A set can build on a base set the same way. Shared transitions are safe: each character plays them on her own Animancer states.
- `AnimationKindEntry` — one kind and its clip transition with its rules (used by both).
- `FootGrounding` — a humanoid's feet never sink below the floor she stands on (clips made for longer legs, retargeted onto a short body, sink a foot in a lunge); each foot goes to its clip's IK goal, never lower than the floor plus the sole.

## Dependencies

- `com.cupkekgames.animations` (UPM)
- `com.cupkekgames.timesystem` (UPM)
- `com.cupkekgames.addressableassets` (UPM)
- Animancer Asset Store package (project-level — bring your own)

## HM-internal dep

Bridge currently references `CupkekGames.Character` which is not yet on the registry. Consumers outside HeroManager need to either fork that asmdef or wait for its extraction.

## Changes

- **0.6.1:** `Grounded` does nothing on a body that is not a humanoid (it has no feet to ground).
- **0.6.0:** implements animations 0.5.0's `IsPlayingOneShot` and `PlayAfterCurrent`. `FootGrounding` is a switch (`OnEnable` runs the IK pass, `OnDisable` puts it back; no `RequireComponent(AnimancerComponent)`), exposed as `CharacterAnimationStateAnimancer.Grounded`.
- **0.4.0:** shared kind lists: `AnimationKindSet` (with an optional base set) and `CharacterAnimationStateAnimancer.Set`; `AnimationKindEntry` is its own type (it was nested in the controller; serialized data is unchanged). Implements `Kinds` and `TryGetClip` from `com.cupkekgames.animations` 0.3.0.
