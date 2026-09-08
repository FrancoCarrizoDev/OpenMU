// <copyright file="ActionRateCheckEventArgs.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlugIns;

/// <summary>
/// Event data produced by an action-rate anti-cheat check.
/// </summary>
public class ActionRateCheckEventArgs : EventArgs
{
    /// <summary>
    /// Gets or sets a value indicating whether the requested action must be rejected.
    /// </summary>
    public bool IsActionRejected { get; set; }
}
