# Stage Timing Validation Report

Date: 2026-06-11
Branch: `claude/vigilant-babbage-k5kn02`

## Scope

- Code changes in this commit: none.
- Delivered artifact: validation report only.
- Important local context: unowned dirty-tree changes add `LevelStretcher`, a `BoostRing.png` asset, and `PlayerDash.RechargeDash()`. I audited the observed behavior but did not commit or edit those unowned implementation files.
- `BoostRing` placement/collision logic was not found in visible `.cs` files at validation time. The existing `MagnetItem` only pulls coins for 7 seconds and does not change player speed.

## Source Constants

- Player start X: `-7.6796966`
- Base goal X: `52`
- Observed stretched goal X by stage: `1-1=52`, `1-2=100`, `1-3=105`, `1-4=100`, `1-5=110`
- Base run speed: `5.0 u/s`
- Repeated dash theoretical average: `(11.5 * 0.24 + 5.0 * 0.86) / 1.1 = 6.42 u/s`
- Stomp-row rule from task: bounce horizontal reach `0.9s * 5 = 4.5u`

## Stage Summary

| Stage | Death -> CP max loss | 8-combo physics | Medal / ring check | parTimes check | Result |
| --- | --- | --- | --- | --- | --- |
| 1-1 | ✅ `6.34s <= 12s` | ✅ N/A | ✅ all medals reachable | ❌ observed par `35s`, target about `17s` | ❌ par too loose |
| 1-2 | ❌ current CP at `26.5` gives `14.70s` last segment | ✅ N/A | ✅ lift covers high medal | ❌ observed stretched par `70s`, target about `32s` | ❌ CP/par need numeric fix |
| 1-3 | ❌ CP2 at `38.6` gives `13.28s` last segment | ✅ spacing `3.4u < 4.5u`; strict 8-count note below | ✅ spring/lift cover high medals | ❌ observed stretched par `80s`, target about `38s` | ❌ CP/par need numeric fix |
| 1-4 | ❌ CP2 at `31.8` gives `13.64s` last segment | ✅ N/A | ✅ magma/lift medals reachable | ❌ observed stretched par `80s`, target about `41s` | ❌ CP/par need numeric fix |
| 1-5 | ❌ CP2 at `38.5` gives `14.30s` last segment to X=`110` | ✅ N/A | ✅ lift/spring routes cover medals; boost-ring placement unresolved | ❌ observed stretched par `95s`, target about `59s` | ❌ CP/par need numeric fix |

## 1. Death To Checkpoint Loss

`RespawnSystem.KillPlayer` immediately warps the player to the saved spawn/checkpoint, zeroes velocity, and grants 2 seconds of invulnerability. There is no fixed respawn animation delay, so the replay loss can be estimated by segment distance / base speed.

| Stage | Spawn/CP/goal X positions | Segment distances | Max loss |
| --- | --- | --- | --- |
| 1-1 | `-7.68 -> 24 -> 52` | `31.68`, `28.00` | ✅ `6.34s` |
| 1-2 | `-7.68 -> 26.5 -> 100` | `34.18`, `73.50` | ❌ `14.70s` |
| 1-3 | `-7.68 -> 16.5 -> 38.6 -> 105` | `24.18`, `22.10`, `66.40` | ❌ `13.28s` |
| 1-4 | `-7.68 -> 15.5 -> 31.8 -> 100` | `23.18`, `16.30`, `68.20` | ❌ `13.64s` |
| 1-5 | `-7.68 -> 17 -> 38.5 -> 110` | `24.68`, `21.50`, `71.50` | ❌ `14.30s` |

Recommended numeric-only checkpoint fixes if the long-stage stretcher is kept:

| Stage | Current CPs | Suggested CPs | New max loss |
| --- | --- | --- | --- |
| 1-2 | `26.5` | `46.0` | `10.77s` |
| 1-3 | `16.5`, `38.6` | `16.5`, `55.0` | `10.00s` |
| 1-4 | `15.5`, `31.8` | `15.5`, `52.0` | `9.60s` |
| 1-5 | `17.0`, `38.5` | `17.0`, `58.0` | `10.40s` |

## 2. Stage 1-3 Combo Row Physics

Core auto-spawned row:

```text
23.4 -> 26.8 -> 30.2 -> 33.6 -> 37.0
```

Intervals:

```text
3.4, 3.4, 3.4, 3.4
```

Physics verdict: ✅ Valid. The requested reach budget is `4.5u`; the row uses `3.4u`, leaving about `1.1u` margin per stomp bounce.

Strict count note: the code expresses five explicit row enemies, with existing scene slimes around X=`24.0` and X=`39.73` extending the practical chain. If QA requires exactly eight clearly authored consecutive stomp targets, that is still a content follow-up; I did not add enemies because this task forbids ownership expansion beyond report/par/checkpoint numeric work.

## 3. Medal And Ring Checks

An unowned dirty-tree asset `Assets/Resources/Gimmicks/BoostRing.png` exists, and `PlayerDash` has an unowned `RechargeDash()` addition. However, no visible `.cs` placement/collision logic for a boost ring was present at validation time. Therefore the report can validate current medal reachability without relying on boost rings, but cannot prove final boost-ring timing balance until the ring spawner/trigger code is available.

`MagnetItem` appears in stages 1-2, 1-3, and 1-4, but it only calls `ScoreSystem.ActivateMagnet(7f)`. Coins move toward the player; player speed and clear timer are not directly boosted.

| Stage | Medal positions | Reachability |
| --- | --- | --- |
| 1-1 | X=`13,27,41`, height `+2.3` | ✅ Base jump lower bound is about `2.37u`; reachable. |
| 1-2 | X=`8 +2.3`, `21 +1.5`, `33 +4.4` | ✅ Third medal aligns with lift X=`33`, top path about `+4.2u` plus player/medal trigger radius. |
| 1-3 | X=`12 +4.6`, `20 +4.4`, `45 +1.5` | ✅ X=`12` spring launch height about `5.33u`; X=`20` lift top about `+4.6u`; X=`45` is low. |
| 1-4 | X=`20 +1.8`, `29 +1.8`, `36.5 +4.6` | ✅ First two are low above magma; third is paired with lift X=`36.5` and jump. |
| 1-5 | X=`8.5 +2.3`, `28 +4.8`, `47 +3.6` | ✅ X=`28` lift route reaches trigger envelope; X=`47` is reachable from arena/spring-assisted route. |

Verdict: ✅ No medal appears locked behind a missing boost-ring mechanic; all listed medals have a non-ring route. ⚠️ Boost-ring-only clear-time abuse is unresolved until the missing ring placement/trigger code exists. ✅ The existing magnet ring does not alter speed.

## 4. Theory Shortest And parTimes

Observed long-stage runtime par values come from unowned `LevelStretcher.parTimesByStage`, not only from `ScoreSystem.parTimes`.

| Stage | Goal X | Theory shortest | Ordinary clear estimate | ordinary x1.2 target | Observed runtime par | Verdict |
| --- | ---: | ---: | ---: | ---: | ---: | --- |
| 1-1 | `52` | `9.3s` | `14.2s` | `17s` | `35s` | ❌ too loose |
| 1-2 | `100` | `16.8s` | `26.7s` | `32s` | `70s` | ❌ too loose |
| 1-3 | `105` | `17.6s` | `31.7s` | `38s` | `80s` | ❌ too loose |
| 1-4 | `100` | `16.8s` | `34.2s` | `41s` | `80s` | ❌ too loose |
| 1-5 | `110` | `18.3s + boss` | `49.2s` | `59s` | `95s` | ❌ too loose |

Recommended par values if the long-stage stretcher is kept:

```text
{ 0f, 17f, 32f, 38f, 41f, 59f }
```

Assumptions:

- Ordinary clear means forward movement with normal jumps, non-perfect dash use, cautious enemy/hazard handling, and no death.
- 1-5 includes a readable 3-hit boss cycle, not a perfect stomp lock.
- FEVER speed is not assumed for ordinary timing because it depends on maintaining combo and is not guaranteed on every stage route.
