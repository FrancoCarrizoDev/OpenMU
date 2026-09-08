// <copyright file="IActionRateCheatCheckPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlugIns;

using System.Runtime.InteropServices;
using System.Threading.Tasks;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// A plugin point for checks which protect client-triggered actions from packet replay and macros.
/// </summary>
[Guid("8D79E7C7-8ED7-4D5E-A65D-5D4EE896C20B")]
[PlugInPoint("Action-rate cheat check", "Is called before an item consumption is processed.")]
public interface IActionRateCheatCheckPlugIn
{
    /// <summary>
    /// Checks whether an item consumption is allowed at the current rate.
    /// </summary>
    /// <param name="player">The player who requested the consumption.</param>
    /// <param name="item">The item to consume.</param>
    /// <param name="eventArgs">The event data to mark a rejected request.</param>
    ValueTask ItemConsumptionCheatCheckAsync(Player player, Item item, ActionRateCheckEventArgs eventArgs);
}
