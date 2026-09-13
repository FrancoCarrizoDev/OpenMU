// <copyright file="CombatMetricSnapshot.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlugIns;

/// <summary>
/// A point-in-time copy of an in-memory combat metric accumulator.
/// </summary>
internal readonly record struct CombatMetricSnapshot(long HitCount, long HealthDamage, long ShieldDamage, long CriticalHitCount, long ExcellentHitCount, long DamageBelow200HitCount, long Damage200To399HitCount, long Damage400To599HitCount, long Damage600To799HitCount, long Damage800OrMoreHitCount);
