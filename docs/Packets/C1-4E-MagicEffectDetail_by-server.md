# C1 4E - MagicEffectDetail (by server)

## Is sent when

A magic effect was added, removed or refreshed for the player himself. It carries the duration and magnitude of the effect so the client can render a tooltip with the remaining and total duration.

## Causes the following actions on the client side

The client may update an extended buff tooltip with the precise remaining/total duration and the magnitude of the effect. Clients which do not understand this opcode ignore it, preserving full backward compatibility.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   17   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0x4E  | Packet header - packet type identifier |
| 3 | 1 | Byte |  | EffectNumber; The magic effect definition number (matches MagicEffectStatus.EffectId and EffectItemConsumption.MagicEffectNumber). |
| 4 | 2 | ShortBigEndian |  | PlayerId; The id of the player whose effect changed. |
| 6 | 1 | Byte |  | Flags; Bit 0 (0x01): IsActive (1 = added/refreshed, 0 = removed). Bit 1 (0x02): HasMagnitude. Bit 2 (0x04): HasRemainingSeconds. Bit 3 (0x08): HasTotalSeconds. |
| 7 | 2 | ShortLittleEndian |  | Magnitude; Unsigned 16-bit magnitude of the first power-up of the effect, in effect-specific units (raw server value, saturated to [0, 65535]). For damage-over-time effects such as poison the magnitude is the value of the configured power-up, NOT the per-tick damage (which is computed from the target's current health and the attacker's poison multiplier at tick time). 0 when the effect has no associated power-up boost. |
| 9 | 4 | IntegerLittleEndian |  | RemainingSeconds; Seconds remaining until the effect expires. 0 when the effect has just been removed or has just expired. On activation or refresh, this equals TotalSeconds. |
| 13 | 4 | IntegerLittleEndian |  | TotalSeconds; Total duration in seconds the effect will be active for, taken from the live MagicEffect.Duration. 0 for effect types which have no meaningful duration (e.g. passive flags). |