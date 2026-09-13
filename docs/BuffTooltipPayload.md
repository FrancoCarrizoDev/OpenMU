# Buff tooltip payload extension (C1 0x4E — MagicEffectDetail)

## Motivation

The historical `MagicEffectStatus` (C1 0x07), `MagicEffectCancelled075` / `MagicEffectCancelled` (C1 0x1B)
and `EffectItemConsumption` (C1 0x2D) packets only carry the effect identifier; the only duration field
that ever crosses the wire is `EffectItemConsumption.RemainingSeconds`, which is only populated when
`MagicEffectDefinition.SendDuration == true` and the effect's first power-up matches one of seven
`EffectType` values. As a result, the client cannot show:

* the precise **remaining** duration of debuffs such as Freeze, Stun, Sleep, Blind or
  Spell-of-Restriction (because their `SendDuration` is `false` and their power-ups do not map to any
  `EffectItemConsumption.EffectType`),
* the **total** configured duration of any effect (only the original snapshot is ever sent),
* a meaningful **magnitude** for damage-over-time effects like Poison or Bleeding.

This document describes the OpenMU-side extension that closes the gap. **Only the OpenMU server and
its packet definitions / serialization / tests are touched.** The client side (MuMain) is out of
scope and is expected to add an opt-in handler for opcode `0x4E`.

## Design constraints

1. **Versioned, backward-compatible.** A client that does not understand opcode `0x4E` must keep
   working unchanged. The new packet therefore uses a brand-new opcode (`0x4E`) that legacy clients
   silently ignore — the same strategy the protocol already uses for other extension packets.
2. **Retro-compatible with old clients.** The pre-existing `MagicEffectStatus`, `EffectItemConsumption`
   and `MagicEffectCancelled` flows are left fully intact. The new packet is emitted **in addition**
   to them, never as a replacement.
3. **No invented values.** Every field sent is derived from real server-side state. When the
   server does not track a piece of data (e.g. "damage per tick" for Poison, which is a
   tick-time function of the target's current HP and the attacker's `Stats.PoisonDamageMultiplier`),
   the field is sent as `0` with the corresponding flag cleared and the omission is documented.
4. **Version-gated on the server side.** The existing `DeActivateMagicEffectPlugIn` already carries
   `[MinimumClient(0, 90, ClientLanguage.Invariant)]` and continues to do so. Clients with a version
   older than `0.90` are served by `DeActivateMagicEffectPlugIn075` which only emits the legacy
   `MagicEffectCancelled075` packet and never sends the new opcode.

## New packet — `MagicEffectDetail` (C1 0x4E, length 17)

| Index | Length | Type                | Field           | Description |
|-------|--------|---------------------|-----------------|-------------|
| 0     | 1      | `byte` (0xC1)       | HeaderType      | C1 header. |
| 1     | 1      | `byte` (0x11)       | Header length   | Total packet length (17). |
| 2     | 1      | `byte` (0x4E)       | Header code     | Opcode. |
| 3     | 1      | `byte`              | `EffectNumber`  | The `MagicEffectDefinition.Number`; matches `MagicEffectStatus.EffectId` and `EffectItemConsumption.MagicEffectNumber`. |
| 4–5   | 2      | `ushort` big-endian | `PlayerId`      | The id of the player whose effect changed. For the owner, the server passes `affectedObject.GetId(playerOfView)`; for observers the packet is **not** sent. |
| 6     | 1      | `byte`              | `Flags`         | Bit field. Bit 0 (0x01): `IsActive` — 1 when the effect was added or refreshed, 0 when it was removed. Bit 1 (0x02): `HasMagnitude`. Bit 2 (0x04): `HasRemainingSeconds`. Bit 3 (0x08): `HasTotalSeconds`. |
| 7–8   | 2      | `ushort` little-endian | `Magnitude`  | First power-up's value, saturated to `[0, 65535]`. |
| 9–12  | 4      | `uint` little-endian | `RemainingSeconds` | Seconds remaining until the effect expires; on activation it equals `TotalSeconds`; on removal it is `0`. |
| 13–16 | 4      | `uint` little-endian | `TotalSeconds` | Configured total duration of the effect. `0` when the effect has no meaningful duration (e.g. passive flags). |

### Semantics per effect class

| Effect (`EffectNumber`) | `Flags` on add | `Magnitude` | `TotalSeconds` | Notes |
|------------------------|----------------|-------------|----------------|-------|
| Damage buff (`0x01`)   | `0x0F`         | boost value | effect duration | Sent in addition to `EffectItemConsumption`. |
| Defense buff (`0x02`)  | `0x0F`         | boost value | effect duration | Sent in addition to `EffectItemConsumption`. |
| Elf Soldier (`0x03`)   | `0x0F`         | boost value | effect duration | Sent in addition to `EffectItemConsumption`. |
| Soul Barrier / Infinity Arrow / etc. | `0x0F` | boost value | effect duration | `EffectItemConsumption` is *not* sent for these (their power-ups do not map), but the new packet still surfaces duration and magnitude. |
| Poisoned (`0x37`)      | `0x0F`         | 1 (IsPoisoned flag) | 8 (configurable) | The actual **per-tick damage** is NOT representable as a fixed value — it is recomputed at every poison tick from `target.CurrentHealth * attacker.PoisonDamageMultiplier`. The `Magnitude` field therefore carries only the configured power-up value (the IsPoisoned flag). Document this in the tooltip. |
| Iced (`0x38`) / Freeze (`0x39`) | `0x0F` | 1 | duration | Pure state flag; clients should render the tooltip as "Frozen for X seconds (was Y)". |
| Stunned (`0x3D`)       | `0x0F`         | 1           | 2 (configurable) | Same as Iced. |
| Sleep (`0x48`)         | `0x0F`         | 1           | duration        | Same. |
| Bleeding / Requiem / Explosion (`0x4A`, `0x4B`) | `0x0F` | 1 | duration | Per-tick damage is computed from the initial hit and is not recoverable as a single value here; `Magnitude` carries the configured power-up flag only. |

### Removal semantics

When an effect is removed, the server sends the same packet with `IsActive = 0` and
`Magnitude = RemainingSeconds = TotalSeconds = 0`. The `EffectNumber` and `PlayerId` fields stay
populated so the client can match the removal event to its local cache.

## Server-side state additions (`MagicEffect`)

To support `RemainingSeconds`, `MagicEffect` now tracks when the current `Duration` started ticking:

* `StartedAt` (`DateTime UtcNow`) — set on construction and on every `ResetTimer()` call.
* `RemainingDuration` (`TimeSpan`) — `Duration - (UtcNow - StartedAt)`, clamped to `>= 0`; returns
  `TimeSpan.Zero` once the effect has been disposed (guarded by an internal `_hasExpired` flag set at
  the start of `DisposeAsyncCore` to avoid the existing `AsyncDisposable` race where `IsDisposed` is
  not flipped along the async dispose path).
* `ResetTimer()` now also updates `StartedAt = UtcNow`. The existing call site
  (`MagicEffectsList.UpdateEffect`) calls `ResetTimer()` whenever a same-id effect is reapplied with a
  new duration, so re-applications get a freshly counted-down timer without further changes.

## Wiring (`DeActivateMagicEffectPlugIn`)

* Existing `MagicEffectStatus` (0x07) and `EffectItemConsumption` (0x2D) emission is unchanged.
* A new private `SendMagicEffectDetailAsync` is invoked only when `affectedObject == this._player`,
  i.e. the owner. Observers (`ForEachWorldObserverAsync` path) do **not** receive the new packet —
  they keep getting just the byte-sized `MagicEffectStatus` (0x07) like before, so the new traffic is
  O(active effects on the owner) and not O(active effects × observers).
* `EffectNumber` is the `byte` cast of `effect.Definition.Number`. `PlayerId` is the result of
  `affectedObject.GetId(this._player)` (i.e. `0x0200` for self, real id for other players on the same
  map).
* `Magnitude` saturates `effect.Value` (first power-up value) to `[0, 65535]`. Negative boosts (e.g.
  `Weakness`) are clamped to `0` and reported via the `HasMagnitude` flag.
* `TotalSeconds` saturates `effect.Duration` to `[0, uint.MaxValue]`. The 24h Elf Soldier buff
  (`86_400 s`) fits comfortably.

## Test coverage

* `tests/MUnique.OpenMU.Network.Packets.Tests/ServerToClientPacketTests.cs` —
  auto-generated `MagicEffectDetail_PacketSizeValidation` asserts the declared length (17) and
  every field boundary.
* `tests/MUnique.OpenMU.Tests/MagicEffectDetailRemoteViewTests.cs` — new fixture covering:
  * activation with magnitude and duration,
  * deactivation with all duration fields zeroed,
  * poison (no legacy `EffectItemConsumption`) reporting magnitude and total duration,
  * stun / freeze / sleep configured duration end-to-end,
  * magnitude saturation when the power-up exceeds `ushort.MaxValue`,
  * observers do **not** receive the new packet,
  * `MagicEffect.RemainingDuration` decreases as time elapses,
  * `MagicEffect.RemainingDuration` returns `0` after dispose,
  * `MagicEffect.RemainingDuration` is reset by `ResetTimer()`.
* `tests/MUnique.OpenMU.Tests/DeActivateMagicEffectPlugInTest.cs` — existing test was updated to
  acknowledge the appended `MagicEffectDetail` packet and to assert its `EffectNumber` /
  `IsActive` / duration fields.

## Limitations & open items

* **Magnitude is the first power-up's value only.** For multi-stat effects the remaining power-ups are
  not surfaced individually. If a future client needs them, the cleanest path is a new packet version
  (or a second sub-packet) — extending the existing fixed layout is intentionally avoided to keep
  the wire format stable.
* **Poison per-tick damage is not exposed.** OpenMU computes the per-tick value at the moment of the
  tick (`Stats.CurrentHealth * Stats.PoisonDamageMultiplier`); there is no per-effect scalar to send.
  Exposing it would require sending the attacker's `PoisonDamageMultiplier` and the target's
  `CurrentHealth` at activation time, which would be brittle and is out of scope.
* **Magnitude for negative effects (Weakness, Innovation, DefenseReduction, etc.) is clamped to 0.**
  A future revision could expose a signed magnitude by switching the field to `short`; this would be a
  breaking change to clients that already understand the packet, so it is left for a v2 packet.
* **Observers don't get the new packet.** If a future client wants to render the magnitude for
  non-owner observers, we can either route the new packet through `ForEachWorldObserverAsync` or
  extend the existing observer-only `MagicEffectStatus` (0x07). Both are additive changes.
* **The 17-byte fixed layout is at capacity for our field set.** Any additional metadata (skill id,
  source player id, elemental type, etc.) should be added through a v2 packet rather than by
  repurposing the `Flags` byte or a padding slot.
