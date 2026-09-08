// <copyright file="MagicEffectDurationRemoteViewTests.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests;

using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameServer.RemoteView.World;
using MUnique.OpenMU.Network.Packets.ServerToClient;
using MUnique.OpenMU.Persistence;
using MUnique.OpenMU.Persistence.InMemory;
using MUnique.OpenMU.Persistence.Initialization.Skills;

/// <summary>
/// Tests magic-effect duration packets sent to the player who receives the effect.
/// </summary>
[TestFixture]
public class MagicEffectDurationRemoteViewTests
{
    /// <summary>
    /// Verifies that a timed potion effect forwards its actual remaining duration to the client.
    /// </summary>
    [Test]
    public async ValueTask SendsRemainingDurationForTimedPotionEffectAsync()
    {
        var contextProvider = new InMemoryPersistenceContextProvider();
        using var context = contextProvider.CreateNewContext();
        var configuration = context.CreateNew<Persistence.BasicModel.GameConfiguration>();
        AddPersistentAttributes(context, configuration);
        new SoulPotionEffectInitializer(context, configuration).Initialize();
        var definition = configuration.MagicEffects.Single();
        await using var effect = new MagicEffect(TimeSpan.FromSeconds(60), definition);
        var (player, output) = CastleSiegeRemoteViewTestHelper.CreatePlayer();

        await new DeActivateMagicEffectPlugIn(player).ActivateMagicEffectAsync(effect, player).ConfigureAwait(false);

        var packet = (EffectItemConsumption)output.ToArray().AsMemory();
        Assert.Multiple(() =>
        {
            Assert.That(definition.SendDuration, Is.True);
            Assert.That(packet.RemainingSeconds, Is.EqualTo(60));
            Assert.That(packet.MagicEffectNumber, Is.EqualTo((byte)definition.Number));
        });
    }

    private static void AddPersistentAttributes(IContext context, Persistence.BasicModel.GameConfiguration configuration)
    {
        foreach (var attribute in new[]
                 {
                     Stats.AttackSpeedAny,
                     Stats.AbilityRecoveryAbsolute,
                     Stats.LightningResistance,
                     Stats.IceResistance,
                 })
        {
            configuration.Attributes.Add(context.CreateNew<Persistence.BasicModel.AttributeDefinition>(
                attribute.Id,
                attribute.Designation,
                attribute.Description));
        }
    }
}
