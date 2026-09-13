// <copyright file="DeActivateMagicEffectPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.RemoteView.World;

using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using MUnique.OpenMU.AttributeSystem;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.Views;
using MUnique.OpenMU.GameLogic.Views.World;
using MUnique.OpenMU.Network.Packets.ServerToClient;
using MUnique.OpenMU.Network.PlugIns;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// The default implementation of the <see cref="IActivateMagicEffectPlugIn"/> which is forwarding everything to the game client with specific data packets.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.DeActivateMagicEffectPlugIn_Name), Description = nameof(PlugInResources.DeActivateMagicEffectPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("67642604-8abb-44b9-a668-989cb3b28e89")]
[MinimumClient(0, 90, ClientLanguage.Invariant)]
public class DeActivateMagicEffectPlugIn : IActivateMagicEffectPlugIn, IDeactivateMagicEffectPlugIn
{
    private static readonly ReadOnlyDictionary<AttributeDefinition, EffectItemConsumption.EffectType> EffectTypeMapping = new(
        new Dictionary<AttributeDefinition, EffectItemConsumption.EffectType>
        {
            { Stats.AttackSpeedAny, EffectItemConsumption.EffectType.AttackSpeed },
            { Stats.BaseDamageBonus, EffectItemConsumption.EffectType.Damage },
            { Stats.GreaterDamageBonus, EffectItemConsumption.EffectType.Damage },
            { Stats.DefenseBase, EffectItemConsumption.EffectType.Defense },
            { Stats.DefenseFinal, EffectItemConsumption.EffectType.Defense },
            { Stats.MaximumHealth, EffectItemConsumption.EffectType.MaximumHealth },
            { Stats.MaximumMana, EffectItemConsumption.EffectType.MaximumMana },
        });

    private readonly RemotePlayer _player;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeActivateMagicEffectPlugIn"/> class.
    /// </summary>
    /// <param name="player">The player.</param>
    public DeActivateMagicEffectPlugIn(RemotePlayer player) => this._player = player;

    /// <inheritdoc/>
    public ValueTask ActivateMagicEffectAsync(MagicEffect effect, IAttackable affectedObject)
    {
        return this.SendMagicEffectStatusAsync(effect, affectedObject, true, effect.Definition.SendDuration ? effect.Duration : TimeSpan.Zero);
    }

    /// <inheritdoc/>
    public ValueTask DeactivateMagicEffectAsync(MagicEffect effect, IAttackable affectedObject)
    {
        return this.SendMagicEffectStatusAsync(effect, affectedObject, false, TimeSpan.Zero);
    }

    private async ValueTask SendMagicEffectStatusAsync(MagicEffect effect, IAttackable affectedObject, bool isActive, TimeSpan duration)
    {
        if (!(this._player.Connection?.Connected ?? false)
            || effect.Definition.Number <= 0)
        {
            return;
        }

        bool effectWasSent = false;
        var objectId = affectedObject.GetId(this._player);
        if (isActive && affectedObject == this._player)
        {
            foreach (var powerUpDefinition in effect.Definition.PowerUpDefinitions)
            {
                if (powerUpDefinition.TargetAttribute is { } targetAttribute
                    && EffectTypeMapping.TryGetValue(targetAttribute, out var effectType))
                {
                    var origin = EffectItemConsumption.EffectOrigin.HalloweenAndCherryBlossomEvent; // Basically, all normal consumable items which add effects
                    var action = EffectItemConsumption.EffectAction.Add;
                    await this._player.Connection.SendEffectItemConsumptionAsync(origin, effectType, action, (uint)duration.TotalSeconds, (byte)effect.Definition.Number).ConfigureAwait(false);
                    effectWasSent = true;
                }
            }
        }

        if (!effectWasSent)
        {
            await this._player.Connection.SendMagicEffectStatusAsync(isActive, objectId, (byte)effect.Id).ConfigureAwait(false);
        }

        if (affectedObject == this._player)
        {
            await this.SendMagicEffectDetailAsync(effect, isActive, (ushort)objectId).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Sends the <see cref="MagicEffectDetail"/> packet to the owning player.
    /// The packet carries the precise remaining and total duration and the magnitude of the effect,
    /// so the client can render an extended buff tooltip. Clients that do not understand the opcode ignore it.
    /// </summary>
    /// <param name="effect">The effect whose detail should be reported.</param>
    /// <param name="isActive">Whether the effect was added/refreshed or removed.</param>
    /// <param name="playerId">The id of the affected (owning) player.</param>
    private async ValueTask SendMagicEffectDetailAsync(MagicEffect effect, bool isActive, ushort playerId)
    {
        if (this._player.Connection is null)
        {
            return;
        }

        const byte flagIsActive = 0x01;
        const byte flagHasMagnitude = 0x02;
        const byte flagHasRemaining = 0x04;
        const byte flagHasTotal = 0x08;

        byte flags = isActive ? flagIsActive : (byte)0;

        // On removal we don't include magnitude/duration: the client already received them
        // on activation. Keeping the packet compact and unambiguous about the removal event
        // also avoids leaking stale values when the player gets the effect cancelled before
        // it would have naturally timed out.
        if (!isActive)
        {
            await this._player.Connection.SendMagicEffectDetailAsync(
                (byte)effect.Definition.Number,
                playerId,
                flags,
                0,
                0u,
                0u).ConfigureAwait(false);
            return;
        }

        var magnitude = Math.Max(0f, effect.Value);
        if (magnitude > ushort.MaxValue)
        {
            magnitude = ushort.MaxValue;
        }

        if (magnitude > 0)
        {
            flags |= flagHasMagnitude;
        }

        flags |= flagHasRemaining | flagHasTotal;

        await this._player.Connection.SendMagicEffectDetailAsync(
            (byte)effect.Definition.Number,
            playerId,
            flags,
            (ushort)magnitude,
            ToUInt32Seconds(effect.RemainingDuration),
            ToUInt32Seconds(effect.Duration)).ConfigureAwait(false);
    }

    private static uint ToUInt32Seconds(TimeSpan value)
    {
        if (value <= TimeSpan.Zero)
        {
            return 0u;
        }

        if (value.TotalSeconds >= uint.MaxValue)
        {
            return uint.MaxValue;
        }

        return (uint)value.TotalSeconds;
    }
}
