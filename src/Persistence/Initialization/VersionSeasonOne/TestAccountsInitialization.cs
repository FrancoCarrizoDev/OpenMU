// <copyright file="TestAccountsInitialization.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonOne;

using MUnique.OpenMU.DataModel.Configuration;

/// <summary>
/// Initializes the test accounts for the classic "Season 1" style server.
///
/// The regular test accounts test0-test9 use Dark Knight/Dark Wizard/Fairy Elf/Dark Lord, all of
/// which exist in this roster. The additional testgm account is a Season-1-compatible QA account;
/// the remaining Season 6 test accounts explicitly create Master-class or Summoner/Rage Fighter
/// characters and are therefore not initialized here. Ten GM accounts (testgm and testgm1-testgm9)
/// are created so local event smoke tests can run with multiple clients.
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

        for (int i = 0; i < 10; i++)
        {
            var accountName = i == 0 ? "testgm" : "testgm" + i;
            new TestAccounts.GameMaster(this.Context, this.GameConfiguration, accountName).Initialize();
        }
    }
}
