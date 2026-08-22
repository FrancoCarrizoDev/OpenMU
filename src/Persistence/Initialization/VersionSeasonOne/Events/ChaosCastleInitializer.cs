// <copyright file="ChaosCastleInitializer.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonOne.Events;

using MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Maps;

/// <summary>
/// The initializer for the chaos castle event, without Chaos Castle 7.
///
/// Chaos Castle 7 requires a Master class (<see cref="DataModel.Configuration.MiniGameDefinition.RequiresMasterClass"/>)
/// and its map isn't part of <see cref="GameMapsInitializer"/> (see the remarks there) - it's a
/// higher-tier extension added well after Season 1, not part of Chaos Castle's original release.
/// The base <see cref="VersionSeasonSix.Events.ChaosCastleInitializer"/> can't be reused for levels
/// 1-6 alone: it builds all 7 levels in one non-virtual <c>Initialize()</c>, so this overrides it
/// entirely rather than trying to subtract from it.
/// </summary>
internal class ChaosCastleInitializer : VersionSeasonSix.Events.ChaosCastleInitializer
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChaosCastleInitializer" /> class.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="gameConfiguration">The game configuration.</param>
    public ChaosCastleInitializer(IContext context, GameConfiguration gameConfiguration)
        : base(context, gameConfiguration)
    {
    }

    /// <inheritdoc />
    public override void Initialize()
    {
        var chaosCastle1 = this.CreateChaosCastleDefinition(1, ChaosCastle1.Number, 25000);
        chaosCastle1.MinimumCharacterLevel = 15;
        chaosCastle1.MaximumCharacterLevel = 49;
        chaosCastle1.MinimumSpecialCharacterLevel = 15;
        chaosCastle1.MaximumSpecialCharacterLevel = 29;

        var chaosCastle2 = this.CreateChaosCastleDefinition(2, ChaosCastle2.Number, 80000);
        chaosCastle2.MinimumCharacterLevel = 50;
        chaosCastle2.MaximumCharacterLevel = 119;
        chaosCastle2.MinimumSpecialCharacterLevel = 30;
        chaosCastle2.MaximumSpecialCharacterLevel = 99;

        var chaosCastle3 = this.CreateChaosCastleDefinition(3, ChaosCastle3.Number, 150000);
        chaosCastle3.MinimumCharacterLevel = 120;
        chaosCastle3.MaximumCharacterLevel = 179;
        chaosCastle3.MinimumSpecialCharacterLevel = 100;
        chaosCastle3.MaximumSpecialCharacterLevel = 159;

        var chaosCastle4 = this.CreateChaosCastleDefinition(4, ChaosCastle4.Number, 250000);
        chaosCastle4.MinimumCharacterLevel = 180;
        chaosCastle4.MaximumCharacterLevel = 239;
        chaosCastle4.MinimumSpecialCharacterLevel = 160;
        chaosCastle4.MaximumSpecialCharacterLevel = 219;

        var chaosCastle5 = this.CreateChaosCastleDefinition(5, ChaosCastle5.Number, 400000);
        chaosCastle5.MinimumCharacterLevel = 240;
        chaosCastle5.MaximumCharacterLevel = 299;
        chaosCastle5.MinimumSpecialCharacterLevel = 220;
        chaosCastle5.MaximumSpecialCharacterLevel = 279;

        var chaosCastle6 = this.CreateChaosCastleDefinition(6, ChaosCastle6.Number, 650000);
        chaosCastle6.MinimumCharacterLevel = 300;
        chaosCastle6.MaximumCharacterLevel = 400;
        chaosCastle6.MinimumSpecialCharacterLevel = 280;
        chaosCastle6.MaximumSpecialCharacterLevel = 400;
    }
}
