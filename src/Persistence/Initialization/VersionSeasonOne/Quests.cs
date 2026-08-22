// <copyright file="Quests.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonOne;

using MUnique.OpenMU.Persistence.Initialization.CharacterClasses;
using MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Items;

/// <summary>
/// Data initialization for quests, without the "Legacy Quest" chain that leads into the 3rd class
/// evolution (Master Level) - explicitly out of scope for Season 1 (see ADR-0001).
///
/// <see cref="VersionSeasonSix.Quests.Initialize"/> can't be reused as-is: besides wiring up the
/// Master-Level-only quests (<c>EvidenceOfStrength</c>, <c>InfiltrationOfBarracksOfBallgass</c>,
/// <c>IntoTheDarknessZone</c> - the last one's reward literally evolves the character from 2nd to
/// 3rd class), those three also hard-crash here: their monster-kill/item-drop requirements reference
/// monsters that only spawn on maps excluded by <see cref="GameMapsInitializer"/> (e.g. Hell Maine
/// on Aida, Balram/Death Spirit/Soram/Dark Elf on Barracks/Refuge of Balgass).
///
/// The rest of <see cref="VersionSeasonSix.Quests"/>'s content (<c>CreateNewQuests</c> in particular)
/// is generic leveling content, not tied to excluded systems - it's kept as-is. Some of its "Random
/// Quest" entries do reference Kanturu/Elvenland-exclusive monsters incidentally (they were written
/// against the full season6 map roster), but that's handled generically by making
/// <see cref="QuestDefinitionExtensions.WithMonsterKillRequirement"/> skip a requirement whose
/// monster doesn't exist in this configuration, rather than by hand-pruning individual quests here.
/// </summary>
internal class Quests : VersionSeasonSix.Quests
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Quests"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="gameConfiguration">The game configuration.</param>
    public Quests(IContext context, GameConfiguration gameConfiguration)
        : base(context, gameConfiguration)
    {
    }

    /// <inheritdoc />
    public override void Initialize()
    {
        // Level 150 Quests:
        this.FindScrollOfEmperor(CharacterClassNumber.DarkKnight);
        this.FindScrollOfEmperor(CharacterClassNumber.FairyElf);
        this.FindScrollOfEmperor(CharacterClassNumber.DarkWizard);
        this.FindScrollOfEmperor(CharacterClassNumber.Summoner);
        this.TreasuresOfMu(CharacterClassNumber.DarkKnight, Quest.BrokenSwordNumber);
        this.TreasuresOfMu(CharacterClassNumber.FairyElf, Quest.TearOfElfNumber);
        this.TreasuresOfMu(CharacterClassNumber.DarkWizard, Quest.SoulShardOfWizardNumber);
        this.TreasuresOfMu(CharacterClassNumber.Summoner, Quest.EyeOfAbyssalNumber);

        // Level 220 Quests:
        this.GainHeroStatus(CharacterClassNumber.BladeKnight);
        this.GainHeroStatus(CharacterClassNumber.SoulMaster);
        this.GainHeroStatus(CharacterClassNumber.MuseElf);
        this.GainHeroStatus(CharacterClassNumber.BloodySummoner);
        this.SecretOfTheDarkStone();

        this.CreateNewQuests();
    }
}
