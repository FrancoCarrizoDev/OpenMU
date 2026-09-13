// <copyright file="CombatType.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.DataModel.Statistics;

/// <summary>
/// Identifies whether a combat metric was recorded against a player or an NPC.
/// </summary>
public enum CombatType : byte
{
    /// <summary>
    /// Combat against an NPC or monster.
    /// </summary>
    PlayerVersusEnvironment,

    /// <summary>
    /// Combat against another player.
    /// </summary>
    PlayerVersusPlayer,
}
