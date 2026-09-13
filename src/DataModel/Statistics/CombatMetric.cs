// <copyright file="CombatMetric.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.DataModel.Statistics;

/// <summary>
/// An aggregated amount of damage caused by one character during a short time interval.
/// </summary>
public class CombatMetric
{
    /// <summary>
    /// Gets or sets the UTC time at which the aggregation interval began.
    /// </summary>
    public DateTime IntervalStartUtc { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the game server that produced the metric.
    /// </summary>
    public byte GameServerId { get; set; }

    /// <summary>
    /// Gets or sets the kind of combat which was measured.
    /// </summary>
    public CombatType CombatType { get; set; }

    /// <summary>
    /// Gets or sets the number of the map on which the damage was dealt.
    /// </summary>
    public short MapNumber { get; set; }

    /// <summary>
    /// Gets or sets the skill which caused the damage, or zero for a basic attack.
    /// </summary>
    public short SkillNumber { get; set; }

    /// <summary>
    /// Gets or sets the number of successful hits.
    /// </summary>
    public long HitCount { get; set; }

    /// <summary>
    /// Gets or sets the accumulated health damage.
    /// </summary>
    public long HealthDamage { get; set; }

    /// <summary>
    /// Gets or sets the accumulated shield damage.
    /// </summary>
    public long ShieldDamage { get; set; }

    /// <summary>
    /// Gets or sets the number of critical hits.
    /// </summary>
    public long CriticalHitCount { get; set; }

    /// <summary>
    /// Gets or sets the number of excellent hits.
    /// </summary>
    public long ExcellentHitCount { get; set; }

    /// <summary>
    /// Gets or sets the number of hits with less than 200 total damage.
    /// </summary>
    public long DamageBelow200HitCount { get; set; }

    /// <summary>
    /// Gets or sets the number of hits with 200 through 399 total damage.
    /// </summary>
    public long Damage200To399HitCount { get; set; }

    /// <summary>
    /// Gets or sets the number of hits with 400 through 599 total damage.
    /// </summary>
    public long Damage400To599HitCount { get; set; }

    /// <summary>
    /// Gets or sets the number of hits with 600 through 799 total damage.
    /// </summary>
    public long Damage600To799HitCount { get; set; }

    /// <summary>
    /// Gets or sets the number of hits with 800 or more total damage.
    /// </summary>
    public long Damage800OrMoreHitCount { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the character that caused the damage.
    /// </summary>
    public Guid AttackerCharacterId { get; set; }
}
