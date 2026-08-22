// <copyright file="TestAccountsInitialization.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonOne;

using MUnique.OpenMU.DataModel.Configuration;

/// <summary>
/// Initializes the test accounts for the classic "Season 1" style server.
///
/// Only test0-test9 (<see cref="VersionSeasonSix.TestAccounts.LowLevel"/>) are created - they only
/// use Dark Knight/Dark Wizard/Fairy Elf/Dark Lord, all of which exist in this roster. The rest of
/// season6's test accounts (Level300, Level400, Ancient, Socket, the Quest* accounts, GameMaster,
/// GameMaster2, Unlocked) explicitly create Master-class or Summoner/Rage Fighter characters (see
/// e.g. <see cref="VersionSeasonSix.TestAccounts.Level300.CreateKnight"/> which creates a
/// BladeMaster), which don't exist here and throw during initialization. Porting a Season-1-
/// appropriate GM/quest-testing account is tracked as a separate backlog item.
/// </summary>
internal class TestAccountsInitialization : InitializerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TestAccountsInitialization"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="gameConfiguration">The game configuration.</param>
    public TestAccountsInitialization(IContext context, GameConfiguration gameConfiguration)
        : base(context, gameConfiguration)
    {
    }

    /// <inheritdoc />
    public override void Initialize()
    {
        for (int i = 0; i < 10; i++)
        {
            var level = (i * 10) + 1;
            new VersionSeasonSix.TestAccounts.LowLevel(this.Context, this.GameConfiguration, "test" + i, level).Initialize();
        }
    }
}
