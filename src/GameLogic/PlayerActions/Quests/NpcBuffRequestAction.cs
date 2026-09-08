// <copyright file="NpcBuffRequestAction.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlayerActions.Quests;

using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.Views;
using MUnique.OpenMU.Interfaces;

/// <summary>
/// Action which applies the <see cref="Buff"/>s of the currently opened NPC.
/// </summary>
public class NpcBuffRequestAction
{
    /// <summary>
    /// Requests the buffs from the opened NPC and adds them to the <see cref="Player.MagicEffectList"/>.
    /// </summary>
    /// <param name="player">The player.</param>
    public async ValueTask RequestBuffAsync(Player player)
    {
        if (player.OpenedNpc?.Definition is not { NpcWindow: NpcWindow.NpcDialog, Buffs: { } buffs } || !buffs.Any())
        {
            return;
        }

        var anyApplied = false;
        var anyTooLow = false;
        var anyTooStrong = false;
        var anyValidEffect = false;
        var persistentStateChanged = false;
        foreach (var buff in buffs)
        {
            if (buff.MagicEffectDefinition is not { } effectDef)
            {
                continue;
            }

            anyValidEffect = true;

            if (buff.MinimumLevel.HasValue && player.Level < buff.MinimumLevel.Value)
            {
                anyTooLow = true;
                continue;
            }

            if (buff.MaximumLevel.HasValue && player.Level > buff.MaximumLevel.Value)
            {
                anyTooStrong = true;
                continue;
            }

            var isElfSoldierBuff = ElfSoldierBuff.IsElfSoldierEffect(player, effectDef);
            if (isElfSoldierBuff && !ElfSoldierBuff.IsEligible(player))
            {
                anyTooStrong = true;
                continue;
            }

            var duration = TimeSpan.FromSeconds(effectDef.Duration?.ConstantValue?.Value ?? 0);
            if (duration <= TimeSpan.Zero)
            {
                continue;
            }

            if (isElfSoldierBuff)
            {
                var now = DateTime.UtcNow;
                var expiration = player.SelectedCharacter?.ElfSoldierBuffExpirationUtc;
                if (expiration > now)
                {
                    duration = expiration.Value - now;
                    if (player.MagicEffectList.ActiveEffects.ContainsKey(effectDef.Number))
                    {
                        anyApplied = true;
                        continue;
                    }
                }
                else
                {
                    expiration = now + duration;
                    if (player.SelectedCharacter is { } selectedCharacter)
                    {
                        selectedCharacter.ElfSoldierBuffExpirationUtc = expiration;
                        persistentStateChanged = true;
                    }
                }
            }

            var effect = ElfSoldierBuff.CreateEffect(player, effectDef, duration);
            if (effect is null)
            {
                continue;
            }

            await player.MagicEffectList.AddEffectAsync(effect).ConfigureAwait(false);
            anyApplied = true;
        }

        if (persistentStateChanged)
        {
            await player.SaveProgressAsync().ConfigureAwait(false);
        }

        if (anyApplied)
        {
            return;
        }

        if (anyValidEffect && anyTooLow)
        {
            await player.ShowLocalizedBlueMessageAsync(nameof(PlayerMessage.CharacterNotStrongEnoughMessage)).ConfigureAwait(false);
            return;
        }

        if (anyValidEffect && anyTooStrong)
        {
            await player.ShowLocalizedBlueMessageAsync(nameof(PlayerMessage.CharacterTooStrongMessage)).ConfigureAwait(false);
        }
    }
}
