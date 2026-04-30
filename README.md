# CupkekGames Animations — Animancer Bridge

Concrete backend for [CupkekGames.Animations](https://github.com/Cupkek-Games/CupkekGames-Animations) on top of the [Kybernetik Animancer](https://kybernetik.com.au/animancer/) asset.

## What's inside

**Runtime** (`CupkekGames.Animations.Animancer.asmdef`)

- `AnimancerAnimationEngine` — `IAnimationEngine` impl wrapping `AnimancerComponent`
- `CombatAnimationsAnimancer` — `ICombatAnimations` impl with hit / death / win / get-hit playback
- `LocomotionAnimancer` — locomotion (idle / walk / run) blend driver

## Dependencies

- `com.cupkekgames.animations` (UPM)
- `com.cupkekgames.timesystem` (UPM)
- `com.cupkekgames.addressableassets` (UPM)
- Animancer Asset Store package (project-level — bring your own)

## HM-internal dep

Bridge currently references `CupkekGames.Character` which is not yet on the registry. Consumers outside HeroManager need to either fork that asmdef or wait for its extraction.
