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
    /// Verifies that the duration packet is emitted only when requested by the magic effect definition,
    /// and that the <see cref="MagicEffectDetail"/> extension packet is always appended to the owner's stream.
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
        EffectItemConsumption packet = packetData.AsMemory();
        if (sendDuration)
        {
            Assert.Multiple(() =>
            {
                Assert.That(packet.RemainingSeconds, Is.EqualTo(42));
                Assert.That(packet.MagicEffectNumber, Is.EqualTo(1));
            });
        }
        else
        {
            Assert.Multiple(() =>
            {
                Assert.That(packet.RemainingSeconds, Is.Zero);
            });
        }

        // The MagicEffectDetail extension packet is always appended right after the EffectItemConsumption.
        Assert.That(packetData, Has.Length.GreaterThanOrEqualTo(EffectItemConsumption.Length + MagicEffectDetail.Length),
            "Expected MagicEffectDetail to be appended after EffectItemConsumption.");
        MagicEffectDetail detail = packetData.AsMemory(EffectItemConsumption.Length, MagicEffectDetail.Length);
        Assert.Multiple(() =>
        {
            Assert.That(detail.EffectNumber, Is.EqualTo(1));
            Assert.That(detail.Flags & 0x01, Is.EqualTo(0x01), "Effect should be marked active.");
            Assert.That(detail.TotalSeconds, Is.LessThanOrEqualTo(42));
            Assert.That(detail.TotalSeconds, Is.GreaterThanOrEqualTo(41));
            Assert.That(detail.RemainingSeconds, Is.LessThanOrEqualTo(detail.TotalSeconds));
        });

        await effect.DisposeAsync();
    }
}
