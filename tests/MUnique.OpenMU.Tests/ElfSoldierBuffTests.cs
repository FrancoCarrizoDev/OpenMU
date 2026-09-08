// <copyright file="ElfSoldierBuffTests.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests;

using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.PlayerActions.Quests;

/// <summary>
/// Tests for the Season 1 Elf Soldier buff policy.
/// </summary>
[TestFixture]
public class ElfSoldierBuffTests
{
    /// <summary>
    /// Verifies that the reset boundary is inclusive at reset 5 and rejects reset 6.
    /// </summary>
    [TestCase(0, true)]
    [TestCase(5, true)]
    [TestCase(6, false)]
    public async Task EligibilityUsesResetCount(int resetCount, bool expected)
    {
        var player = await PlayerTestHelper.CreatePlayerAsync().ConfigureAwait(false);
        player.Attributes![Stats.Resets] = resetCount;

        Assert.That(ElfSoldierBuff.IsEligible(player), Is.EqualTo(expected));
    }

    /// <summary>
    /// Verifies that an expired persisted grant is cleared instead of restored.
    /// </summary>
    [Test]
    public async Task ExpiredGrantIsClearedAsync()
    {
        var player = await PlayerTestHelper.CreatePlayerAsync().ConfigureAwait(false);
        player.SelectedCharacter!.ElfSoldierBuffExpirationUtc = DateTime.UtcNow.AddMinutes(-1);

        await ElfSoldierBuff.RestoreAsync(player).ConfigureAwait(false);

        Assert.That(player.SelectedCharacter.ElfSoldierBuffExpirationUtc, Is.Null);
    }

    /// <summary>
    /// Verifies that a persisted grant is not restored after the character crosses reset 5.
    /// </summary>
    [Test]
    public async Task GrantIsClearedWhenResetIsAboveBoundaryAsync()
    {
        var player = await PlayerTestHelper.CreatePlayerAsync().ConfigureAwait(false);
        player.Attributes![Stats.Resets] = 6;
        player.SelectedCharacter!.ElfSoldierBuffExpirationUtc = DateTime.UtcNow.AddHours(1);

        await ElfSoldierBuff.RestoreAsync(player).ConfigureAwait(false);

        Assert.That(player.SelectedCharacter.ElfSoldierBuffExpirationUtc, Is.Null);
    }
}
