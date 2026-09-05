// <copyright file="PlayerLevelExtensions.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic;

using MUnique.OpenMU.GameLogic.Attributes;

/// <summary>
/// Provides level-limit helpers for players.
/// </summary>
public static class PlayerLevelExtensions
{
    /// <summary>
    /// Gets the effective maximum character level for the player.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <returns>The configured maximum level, reduced for VIP accounts when configured.</returns>
    public static short GetMaximumCharacterLevel(this Player player)
    {
        var maximumLevel = player.GameContext.Configuration.MaximumLevel;
        if (player.Attributes is not { } attributes || attributes[Stats.IsVip] <= 0)
        {
            return maximumLevel;
        }

        var vipMaximumLevel = (short)attributes[Stats.VipMaximumLevel];
        return vipMaximumLevel > 0 ? Math.Min(maximumLevel, vipMaximumLevel) : maximumLevel;
    }
}
