// <copyright file="ElfSoldierBuff.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlayerActions.Quests;

using MUnique.OpenMU.DataModel.Attributes;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic.Attributes;

/// <summary>
/// Shared rules for the persistent Elf Soldier buff used by Season 1 Classic.
/// </summary>
internal static class ElfSoldierBuff
{
    /// <summary>
    /// The Elf Soldier NPC number.
    /// </summary>
    public const short NpcNumber = 257;

    /// <summary>
    /// The magic effect number used by the Elf Soldier buff.
    /// </summary>
    public const short EffectNumber = 3;

    /// <summary>
    /// The greatest reset count which may receive the buff.
    /// </summary>
    public const int MaximumResetCount = 5;

    /// <summary>
    /// The initial Season 1 Classic duration. The value is also written to the seeded effect definition.
    /// </summary>
    public static readonly TimeSpan DefaultDuration = TimeSpan.FromHours(24);

    /// <summary>
    /// Determines whether the given effect is the Elf Soldier buff requested from the Elf Soldier NPC.
    /// </summary>
    public static bool IsElfSoldierEffect(Player player, MagicEffectDefinition effectDefinition)
        => player.OpenedNpc?.Definition is { Number: NpcNumber }
           && effectDefinition.Number == EffectNumber;

    /// <summary>
    /// Determines whether the player may receive the Elf Soldier buff.
    /// </summary>
    public static bool IsEligible(Player player)
        => player.Attributes is { } attributes && attributes[Stats.Resets] <= MaximumResetCount;

    /// <summary>
    /// Gets the configured Elf Soldier effect definition.
    /// </summary>
    public static MagicEffectDefinition? GetDefinition(Player player)
        => player.GameContext.Configuration.MagicEffects.FirstOrDefault(e => e.Number == EffectNumber);

    /// <summary>
    /// Creates the effect with the configured power-ups and the requested remaining duration.
    /// </summary>
    public static MagicEffect? CreateEffect(Player player, MagicEffectDefinition definition, TimeSpan duration)
    {
        if (player.Attributes is not { } attributes || duration <= TimeSpan.Zero)
        {
            return null;
        }

        var boosts = definition.PowerUpDefinitions
            .Where(def => def.Boost is not null && def.TargetAttribute is not null)
            .Select(def => new MagicEffect.ElementWithTarget(attributes.CreateElement(def), def.TargetAttribute!))
            .ToArray();

        return boosts.Length == 0 ? null : new MagicEffect(duration, definition, boosts);
    }

    /// <summary>
    /// Restores the persisted effect when the character enters the world.
    /// </summary>
    public static async ValueTask RestoreAsync(Player player)
    {
        if (player.SelectedCharacter is not { ElfSoldierBuffExpirationUtc: { } expiration } character
            || player.Attributes is not { } attributes)
        {
            return;
        }

        if (expiration <= DateTime.UtcNow || attributes[Stats.Resets] > MaximumResetCount)
        {
            character.ElfSoldierBuffExpirationUtc = null;
            await player.SaveProgressAsync().ConfigureAwait(false);
            return;
        }

        if (player.MagicEffectList.ActiveEffects.ContainsKey(EffectNumber))
        {
            return;
        }

        var definition = GetDefinition(player);
        var effect = definition is null ? null : CreateEffect(player, definition, expiration - DateTime.UtcNow);
        if (effect is not null)
        {
            await player.MagicEffectList.AddEffectAsync(effect).ConfigureAwait(false);
        }
    }
}
