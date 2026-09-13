// <copyright file="MagicEffectDetailRemoteViewTests.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests;

using MUnique.OpenMU.AttributeSystem;
using MUnique.OpenMU.DataModel.Attributes;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.Views;
using MUnique.OpenMU.GameLogic.Views.World;
using MUnique.OpenMU.GameServer.RemoteView.World;
using MUnique.OpenMU.Network.Packets.ServerToClient;
using BasicModel = MUnique.OpenMU.Persistence.BasicModel;

/// <summary>
/// Tests the emission of the <see cref="MagicEffectDetail"/> extension packet alongside the legacy
/// buff/status packets. The packet is a versioned, backward-compatible addition that exposes the
/// effect magnitude, total duration and remaining duration so the client can render an extended tooltip.
/// </summary>
[TestFixture]
public class MagicEffectDetailRemoteViewTests
{
    private const byte FlagIsActive = 0x01;
    private const byte FlagHasMagnitude = 0x02;
    private const byte FlagHasRemaining = 0x04;
    private const byte FlagHasTotal = 0x08;

    /// <summary>
    /// Verifies that a buff with a non-zero power-up emits the new packet on activation
    /// with the magnitude and duration correctly populated.
    /// </summary>
    [Test]
    public async Task ActivateMagicEffectAsync_EmitsDetailPacketWithMagnitudeAndDurationAsync()
    {
        var (player, output) = CastleSiegeRemoteViewTestHelper.CreatePlayer();
        var definition = CreateEffectDefinition(
            number: 1,
            subType: 0,
            powerUp: (Stats.AttackSpeedAny, 50f),
            sendDuration: false);
        await using var effect = CreateEffectWithPowerUp(definition, TimeSpan.FromSeconds(42), 50f);

        await new DeActivateMagicEffectPlugIn(player).ActivateMagicEffectAsync(effect, player).ConfigureAwait(false);

        var detail = ExtractDetailPacket(output.ToArray());
        Assert.That(detail, Is.Not.Null, "Expected MagicEffectDetail packet to be sent.");
        var d = detail!.Value;
        Assert.Multiple(() =>
        {
            Assert.That(d.EffectNumber, Is.EqualTo(1));
            Assert.That(d.PlayerId, Is.EqualTo(player.GetId(player)));
            Assert.That(d.Flags, Is.EqualTo(FlagIsActive | FlagHasMagnitude | FlagHasRemaining | FlagHasTotal));
            Assert.That(d.Magnitude, Is.EqualTo(50));
            Assert.That(d.TotalSeconds, Is.LessThanOrEqualTo(42));
            Assert.That(d.TotalSeconds, Is.GreaterThanOrEqualTo(41));
            Assert.That(d.RemainingSeconds, Is.LessThanOrEqualTo(42));
            Assert.That(d.RemainingSeconds, Is.GreaterThanOrEqualTo(41));
        });
    }

    /// <summary>
    /// Verifies that deactivating the effect flips the IsActive flag off and zeroes the duration fields.
    /// </summary>
    [Test]
    public async Task DeactivateMagicEffectAsync_EmitsDetailPacketWithIsActiveClearedAsync()
    {
        var (player, output) = CastleSiegeRemoteViewTestHelper.CreatePlayer();
        var definition = CreateEffectDefinition(
            number: 1,
            subType: 0,
            powerUp: (Stats.AttackSpeedAny, 25f),
            sendDuration: false);
        await using var effect = CreateEffectWithPowerUp(definition, TimeSpan.FromSeconds(10), 25f);

        await new DeActivateMagicEffectPlugIn(player).DeactivateMagicEffectAsync(effect, player).ConfigureAwait(false);

        var detail = ExtractDetailPacket(output.ToArray());
        Assert.That(detail, Is.Not.Null, "Expected MagicEffectDetail packet to be sent on deactivate.");
        var d = detail!.Value;
        Assert.Multiple(() =>
        {
            Assert.That(d.EffectNumber, Is.EqualTo(1));
            Assert.That(d.PlayerId, Is.EqualTo(player.GetId(player)));
            Assert.That(d.Flags & FlagIsActive, Is.EqualTo(0), "IsActive should be cleared.");
            Assert.That(d.Magnitude, Is.EqualTo(0), "Magnitude should be zero on removal.");
            Assert.That(d.RemainingSeconds, Is.Zero, "RemainingSeconds should be zero on removal.");
            Assert.That(d.TotalSeconds, Is.Zero, "TotalSeconds should be zero on removal.");
        });
    }

    /// <summary>
    /// Verifies that poison effects, which do NOT map to <see cref="EffectItemConsumption"/>,
    /// still receive the new detail packet with their configured (typically zero) magnitude.
    /// </summary>
    [Test]
    public async Task ActivateMagicEffectAsync_PoisonEffectEmitsDetailPacketWithoutLegacyMappingAsync()
    {
        var (player, output) = CastleSiegeRemoteViewTestHelper.CreatePlayer();
        var definition = CreateEffectDefinition(
            number: (byte)MagicEffectNumberAlias.Poisoned,
            subType: 0,
            powerUp: (Stats.IsPoisoned, 1f),
            sendDuration: false);
        await using var effect = CreateEffectWithPowerUp(definition, TimeSpan.FromSeconds(8), 1f);

        await new DeActivateMagicEffectPlugIn(player).ActivateMagicEffectAsync(effect, player).ConfigureAwait(false);

        var detail = ExtractDetailPacket(output.ToArray());
        Assert.That(detail, Is.Not.Null);
        var d = detail!.Value;
        Assert.Multiple(() =>
        {
            Assert.That(d.EffectNumber, Is.EqualTo((byte)MagicEffectNumberAlias.Poisoned));
            Assert.That(d.Magnitude, Is.EqualTo(1), "Poison power-up is the IsPoisoned flag (value 1).");
            Assert.That(d.TotalSeconds, Is.EqualTo(8));
        });
    }

    /// <summary>
    /// Verifies the new packet reports remaining duration that ticks down from the total,
    /// based on the <see cref="MagicEffect.RemainingDuration"/> property.
    /// </summary>
    [Test]
    public async Task RemainingDuration_DecreasesAsTimeElapsesAsync()
    {
        var (player, output) = CastleSiegeRemoteViewTestHelper.CreatePlayer();
        var definition = CreateEffectDefinition(
            number: 1,
            subType: 0,
            powerUp: (Stats.AttackSpeedAny, 50f),
            sendDuration: false);
        await using var effect = CreateEffectWithPowerUp(definition, TimeSpan.FromSeconds(30), 50f);

        // Pause briefly so the computed remaining is strictly less than total.
        await Task.Delay(1100).ConfigureAwait(false);

        await new DeActivateMagicEffectPlugIn(player).ActivateMagicEffectAsync(effect, player).ConfigureAwait(false);

        var detail = ExtractDetailPacket(output.ToArray());
        Assert.That(detail, Is.Not.Null);
        var d = detail!.Value;
        Assert.Multiple(() =>
        {
            Assert.That(d.TotalSeconds, Is.LessThanOrEqualTo(30));
            Assert.That(d.TotalSeconds, Is.GreaterThanOrEqualTo(29));
            Assert.That(d.RemainingSeconds, Is.LessThan(d.TotalSeconds));
        });
    }

    /// <summary>
    /// Verifies that a stun effect (MagicEffectNumber.Stunned = 0x3D) reports its configured
    /// 2-second duration in the new packet.
    /// </summary>
    [Test]
    public async Task ActivateMagicEffectAsync_StunEffectReportsConfiguredDurationAsync()
    {
        var (player, output) = CastleSiegeRemoteViewTestHelper.CreatePlayer();
        var definition = CreateEffectDefinition(
            number: (byte)MagicEffectNumberAlias.Stunned,
            subType: 0,
            powerUp: (Stats.IsStunned, 1f),
            sendDuration: false);
        await using var effect = CreateEffectWithPowerUp(definition, TimeSpan.FromSeconds(2), 1f);

        await new DeActivateMagicEffectPlugIn(player).ActivateMagicEffectAsync(effect, player).ConfigureAwait(false);

        var detail = ExtractDetailPacket(output.ToArray());
        Assert.That(detail, Is.Not.Null);
        var d = detail!.Value;
        Assert.Multiple(() =>
        {
            Assert.That(d.EffectNumber, Is.EqualTo((byte)MagicEffectNumberAlias.Stunned));
            Assert.That(d.TotalSeconds, Is.LessThanOrEqualTo(2));
            Assert.That(d.TotalSeconds, Is.GreaterThanOrEqualTo(1));
            Assert.That(d.RemainingSeconds, Is.LessThanOrEqualTo(2));
        });
    }

    /// <summary>
    /// Verifies that effects whose power-up value is larger than ushort.MaxValue are saturated
    /// to the maximum representable magnitude instead of overflowing.
    /// </summary>
    [Test]
    public async Task ActivateMagicEffectAsync_HugeMagnitudeIsSaturatedAsync()
    {
        var (player, output) = CastleSiegeRemoteViewTestHelper.CreatePlayer();
        var definition = CreateEffectDefinition(
            number: 1,
            subType: 0,
            powerUp: (Stats.MaximumHealth, 1_000_000f),
            sendDuration: false);
        await using var effect = CreateEffectWithPowerUp(definition, TimeSpan.FromSeconds(5), 1_000_000f);

        await new DeActivateMagicEffectPlugIn(player).ActivateMagicEffectAsync(effect, player).ConfigureAwait(false);

        var detail = ExtractDetailPacket(output.ToArray());
        Assert.That(detail, Is.Not.Null);
        var d = detail!.Value;
        Assert.Multiple(() =>
        {
            Assert.That(d.Magnitude, Is.EqualTo(ushort.MaxValue));
        });
    }

    /// <summary>
    /// Verifies that the new detail packet is NOT sent when the affected object is an observer
    /// (another player), not the owner. Only the legacy MagicEffectStatus packet is delivered
    /// to observers.
    /// </summary>
    [Test]
    public async Task ActivateMagicEffectAsync_DoesNotSendDetailPacketToObserversAsync()
    {
        var (owner, ownerOutput) = CastleSiegeRemoteViewTestHelper.CreatePlayer();
        var (observer, observerOutput) = CastleSiegeRemoteViewTestHelper.CreatePlayer();
        var definition = CreateEffectDefinition(
            number: 1,
            subType: 0,
            powerUp: (Stats.AttackSpeedAny, 50f),
            sendDuration: false);
        await using var effect = CreateEffectWithPowerUp(definition, TimeSpan.FromSeconds(10), 50f);

        var ownerPlugIn = new DeActivateMagicEffectPlugIn(owner);
        var observerPlugIn = new DeActivateMagicEffectPlugIn(observer);

        await ownerPlugIn.ActivateMagicEffectAsync(effect, owner).ConfigureAwait(false);
        await observerPlugIn.ActivateMagicEffectAsync(effect, owner).ConfigureAwait(false);

        Assert.Multiple(() =>
        {
            Assert.That(ExtractDetailPacket(ownerOutput.ToArray()), Is.Not.Null, "Owner should receive the detail packet.");
            Assert.That(ExtractDetailPacket(observerOutput.ToArray()), Is.Null, "Observers should NOT receive the detail packet.");
        });
    }

    /// <summary>
    /// Verifies that <see cref="MagicEffect.RemainingDuration"/> returns zero once the effect is disposed.
    /// </summary>
    [Test]
    public async Task RemainingDuration_IsZeroAfterDisposeAsync()
    {
        var definition = CreateEffectDefinition(
            number: 1,
            subType: 0,
            powerUp: (Stats.AttackSpeedAny, 50f),
            sendDuration: false);
        var effect = CreateEffectWithPowerUp(definition, TimeSpan.FromSeconds(10), 50f);
        await effect.DisposeAsync().ConfigureAwait(false);
        Assert.That(effect.RemainingDuration, Is.EqualTo(TimeSpan.Zero));
    }

    /// <summary>
    /// Verifies that <see cref="MagicEffect.RemainingDuration"/> is reset by <see cref="MagicEffect.ResetTimer"/>
    /// (called when an effect is re-applied over the same id with a new duration).
    /// </summary>
    [Test]
    public void RemainingDuration_IsResetByResetTimer()
    {
        var definition = CreateEffectDefinition(
            number: 1,
            subType: 0,
            powerUp: (Stats.AttackSpeedAny, 50f),
            sendDuration: false);
        using var effect = CreateEffectWithPowerUp(definition, TimeSpan.FromSeconds(10), 50f);

        // Wait a tiny bit so that remaining is strictly less than total before reset.
        var before = effect.RemainingDuration;
        Assert.That(before, Is.LessThan(TimeSpan.FromSeconds(10)));

        effect.Duration = TimeSpan.FromSeconds(20);
        effect.ResetTimer();

        Assert.That(effect.RemainingDuration, Is.GreaterThanOrEqualTo(TimeSpan.FromSeconds(19)));
    }

    private static MagicEffectDetail? ExtractDetailPacket(byte[] data)
    {
        if (data.Length < MagicEffectDetailRef.Length)
        {
            return null;
        }

        // Walk the byte stream looking for a C1 header followed by length=0x11 (=17) and code=0x4E.
        for (var offset = 0; offset + MagicEffectDetailRef.Length <= data.Length; offset++)
        {
            if (data[offset] != MagicEffectDetailRef.HeaderType)
            {
                continue;
            }

            if (offset + 1 >= data.Length)
            {
                break;
            }

            if (data[offset + 1] != MagicEffectDetailRef.Length)
            {
                continue;
            }

            if (data[offset + 2] != MagicEffectDetailRef.Code)
            {
                continue;
            }

            var slice = new byte[MagicEffectDetailRef.Length];
            Array.Copy(data, offset, slice, 0, MagicEffectDetailRef.Length);
            return (MagicEffectDetail)slice.AsMemory();
        }

        return null;
    }

    private static BasicModel.MagicEffectDefinition CreateEffectDefinition(
        byte number,
        byte subType,
        (AttributeDefinition Attribute, float Boost) powerUp,
        bool sendDuration)
    {
        var definition = new BasicModel.MagicEffectDefinition
        {
            Number = number,
            SubType = subType,
            SendDuration = sendDuration,
        };
        definition.PowerUpDefinitions.Add(new BasicModel.PowerUpDefinition
        {
            TargetAttribute = powerUp.Attribute,
            Boost = new BasicModel.PowerUpDefinitionValue { ConstantValue = { Value = powerUp.Boost } },
        });
        return definition;
    }

    private static MagicEffect CreateEffectWithPowerUp(
        BasicModel.MagicEffectDefinition definition,
        TimeSpan duration,
        float boostValue)
    {
        var element = new SimpleElement(boostValue, AggregateType.AddRaw);
        return new MagicEffect(element, definition, duration);
    }

    private enum MagicEffectNumberAlias : short
    {
        Poisoned = 0x37,
        Stunned = 0x3D,
    }
}
