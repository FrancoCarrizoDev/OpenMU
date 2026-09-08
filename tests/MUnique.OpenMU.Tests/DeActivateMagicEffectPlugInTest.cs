// <copyright file="DeActivateMagicEffectPlugInTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests;

using MUnique.OpenMU.AttributeSystem;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameServer.RemoteView.World;
using MUnique.OpenMU.Network.Packets.ServerToClient;
using BasicModel = MUnique.OpenMU.Persistence.BasicModel;

/// <summary>
/// Tests for <see cref="DeActivateMagicEffectPlugIn"/>.
/// </summary>
[TestFixture]
public class DeActivateMagicEffectPlugInTest
{
    /// <summary>
    /// Verifies that the duration packet is emitted only when requested by the magic effect definition.
    /// </summary>
    [TestCase(true)]
    [TestCase(false)]
    public async Task ActivateMagicEffectAsync_SendsDurationOnlyWhenConfigured(bool sendDuration)
    {
        var (player, output) = CastleSiegeRemoteViewTestHelper.CreatePlayer();
        var definition = new BasicModel.MagicEffectDefinition
        {
            Number = 1,
            SendDuration = sendDuration,
        };
        definition.PowerUpDefinitions.Add(new BasicModel.PowerUpDefinition { TargetAttribute = Stats.AttackSpeedAny });
        var effect = new MagicEffect(TimeSpan.FromSeconds(42), definition);
        var plugIn = new DeActivateMagicEffectPlugIn(player);

        await plugIn.ActivateMagicEffectAsync(effect, player);

        var packetData = output.ToArray();
        if (sendDuration)
        {
            EffectItemConsumption packet = packetData.AsMemory();
            Assert.Multiple(() =>
            {
                Assert.That(packetData, Has.Length.EqualTo(EffectItemConsumption.Length));
                Assert.That(packet.RemainingSeconds, Is.EqualTo(42));
                Assert.That(packet.MagicEffectNumber, Is.EqualTo(1));
            });
        }
        else
        {
            EffectItemConsumption packet = packetData.AsMemory();
            Assert.Multiple(() =>
            {
                Assert.That(packetData, Has.Length.EqualTo(EffectItemConsumption.Length));
                Assert.That(packet.RemainingSeconds, Is.Zero);
            });
        }

        await effect.DisposeAsync();
    }
}
