// <copyright file="GameMapsInitializer.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonOne;

using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Maps;

/// <summary>
/// Initializes the <see cref="GameMapDefinition"/>s for the classic "Season 1" style server.
///
/// This is <see cref="VersionSeasonSix.GameMapsInitializer"/>'s map list, filtered down to maps that
/// fit the Season 1 flavor (see ADR-0001/ADR-0002/ADR-0004 in docs/adr and docs/backlog.md for the
/// reasoning behind each exclusion). Excluded, with the Season/feature they're tied to:
/// - Karutan 1/2: Season 5 content tied to the later character roster.
/// - Valley of Loren and Land of Trials: Castle Siege is disabled in this initial deployment until
///   there are clans and enough population to run it.
/// - Crywolf Fortress, Vulcanus, Loren Market, Silent Map, Elvenland, Santa Village: later seasons
///   or purely cosmetic/utility maps not part of the selected roster (Elvenland is also the
///   Summoner's home map - moot, Summoner isn't a playable class here).
/// - Illusion Temple 1-6, Doppelgaenger 1-4, Barracks of Balgass, Balgass Refuge: Season 3, tied
///   directly to the 3rd class evolution quest - explicitly out of scope.
/// - Fortress of Imperial Guardian 1-4: late-season (post Season 5) event content.
/// - Devil Square 5-7, Chaos Castle 7: higher-tier extensions added well after these events'
///   original Season 1 release, requiring (or numbered alongside) later-season level ranges.
/// - Duel Arena: 1v1 PvP room whose only entrance is via the (excluded) Vulcanus map.
///
/// Kalima 1-7, Kanturu, Aida, Raklion and Devil Square 1-4 / Blood Castle / Chaos Castle 1-6 are
/// kept as the selected content roster. Kanturu's Elpis NPC remains present, but Harmony crafting
/// is still disabled by <see cref="GameConfigurationInitializer"/>.
///
/// Removing a map here also requires removing every gate/warp pointing at it in
/// <see cref="Gates"/> - see the remarks there.
/// </summary>
public class GameMapsInitializer : VersionSeasonSix.GameMapsInitializer
{
    private static readonly HashSet<Type> ExcludedMapTypes = new()
    {
        typeof(Karutan1),
        typeof(Karutan2),
        typeof(ValleyOfLoren),
        typeof(LandOfTrials),
        typeof(CrywolfFortress),
        typeof(Vulcanus),
        typeof(LorenMarket),
        typeof(SilentMap),
        typeof(Elvenland),
        typeof(SantaVillage),
        typeof(IllusionTemple1),
        typeof(IllusionTemple2),
        typeof(IllusionTemple3),
        typeof(IllusionTemple4),
        typeof(IllusionTemple5),
        typeof(IllusionTemple6),
        typeof(Doppelgaenger1),
        typeof(Doppelgaenger2),
        typeof(Doppelgaenger3),
        typeof(Doppelgaenger4),
        typeof(BarracksOfBalgass),
        typeof(BalgassRefuge),
        typeof(FortressOfImperialGuardian1),
        typeof(FortressOfImperialGuardian2),
        typeof(FortressOfImperialGuardian3),
        typeof(FortressOfImperialGuardian4),
        typeof(DevilSquare5),
        typeof(DevilSquare6),
        typeof(DevilSquare7),
        typeof(ChaosCastle7),
        typeof(DuelArena),
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="GameMapsInitializer"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="gameConfiguration">The game configuration.</param>
    public GameMapsInitializer(IContext context, GameConfiguration gameConfiguration)
        : base(context, gameConfiguration)
    {
    }

    /// <inheritdoc />
    protected override IEnumerable<Type> MapInitializerTypes => base.MapInitializerTypes.Where(t => !ExcludedMapTypes.Contains(t));

    /// <inheritdoc />
    public override void Initialize()
    {
        base.Initialize();
        new ClassicSpotsInitializer(this.Context, this.GameConfiguration).Initialize();
    }
}
