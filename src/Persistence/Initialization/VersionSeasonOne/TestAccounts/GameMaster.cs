// <copyright file="GameMaster.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonOne.TestAccounts;

using MUnique.OpenMU.DataModel;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.DataModel.Entities;
using MUnique.OpenMU.Persistence.Initialization.CharacterClasses;

/// <summary>
/// Initializes a Season 1-compatible game master account used for local QA.
/// </summary>
internal sealed class GameMaster : VersionSeasonSix.TestAccounts.LowLevel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GameMaster"/> class.
    /// </summary>
    /// <param name="context">The persistence context.</param>
    /// <param name="gameConfiguration">The game configuration.</param>
    /// <param name="accountName">The account name.</param>
    public GameMaster(IContext context, GameConfiguration gameConfiguration, string accountName)
        : base(context, gameConfiguration, accountName, 1)
    {
        this.AddAllSkills = true;
    }

    /// <inheritdoc />
    protected override Account CreateAccount()
    {
        var account = base.CreateAccount();
        account.State = AccountState.GameMaster;

        foreach (var character in account.Characters)
        {
            character.CharacterStatus = CharacterStatus.GameMaster;
            character.LevelUpPoints = 20_000;
        }

        var magicGladiator = this.GameConfiguration.CharacterClasses
            .Single(characterClass => characterClass.Number == (byte)CharacterClassNumber.MagicGladiator);
        account.UnlockedCharacterClasses.Add(magicGladiator);
        return account;
    }
}
