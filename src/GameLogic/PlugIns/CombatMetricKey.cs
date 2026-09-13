// <copyright file="CombatMetricKey.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlugIns;

using MUnique.OpenMU.DataModel.Statistics;

/// <summary>
/// Identifies one in-memory combat metric aggregation bucket.
/// </summary>
internal readonly record struct CombatMetricKey(Guid CharacterId, CombatType CombatType, short MapNumber, short SkillNumber);
