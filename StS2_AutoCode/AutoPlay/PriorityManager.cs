using System;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Models;
using StS2_Auto.StS2_AutoCode.AutoPlay.Overrides.Cards;
using StS2_Auto.StS2_AutoCode.AutoPlay.Overrides.Enemies;
using StS2_Auto.StS2_AutoCode.AutoPlay.Overrides.Enemies.Cultist;
using StS2_Auto.StS2_AutoCode.AutoPlay.Overrides.Enemies.FatGremlin;
using StS2_Auto.StS2_AutoCode.Logging;

namespace StS2_Auto.StS2_AutoCode.AutoPlay;

internal sealed class PriorityManager
{
    private readonly List<CombatOverride> _overrides =
    [
        new FatGremlinOverride(),
        new CultistOverride(),
        new DefaultCombatOverride()
    ];
    private readonly List<CardOverride> _cardOverrides =
    [
        new AlwaysPlayFirstCardOverride()
    ];
    private string? _activeOverrideName;

    public bool TryGetCard(
        AutoPlayGameState state,
        out CardPlayOption? selectedOption)
    {
        CombatOverride? combatOverride = null;
        foreach (CombatOverride candidate in _overrides)
        {
            if (!candidate.AppliesTo(state))
                continue;

            combatOverride = candidate;
            break;
        }

        if (combatOverride == null)
            throw new InvalidOperationException("No combat override applies to the current combat.");

        if (_activeOverrideName != combatOverride.Name)
        {
            _activeOverrideName = combatOverride.Name;
            AutoPlayLogger.postInfo($"Using {combatOverride.Name} combat override.");
        }

        state.ActiveOverride = combatOverride;

        foreach (CardOverride cardOverride in _cardOverrides)
        {
            if (!cardOverride.TryGetCard(state, out selectedOption) ||
                selectedOption == null)
            {
                continue;
            }

            if (!TryResolveTarget(state, combatOverride, ref selectedOption))
                continue;

            if (selectedOption == null)
                continue;

            AutoPlayLogger.postInfo(
                $"{cardOverride.Name} selected card '{selectedOption.Card.Title}'.");
            return true;
        }

        foreach (Circumstance circumstance in combatOverride.Priorities)
        {
            if (!circumstance.Check(state))
                continue;

            selectedOption = circumstance.GetBestCard(state);
            if (selectedOption == null)
                continue;

            if (!TryResolveTarget(state, combatOverride, ref selectedOption))
                continue;

            if (selectedOption == null)
                continue;

            AutoPlayLogger.postInfo(
                $"{circumstance.Name} selected card '{selectedOption.Card.Title}'.");
            return true;
        }

        selectedOption = null;
        return false;
    }

    private static bool TryResolveTarget(
        AutoPlayGameState state,
        CombatOverride combatOverride,
        ref CardPlayOption? selectedOption)
    {
        if (selectedOption?.Target != null || selectedOption?.ValidTargets.Count == 0)
            return true;

        if (selectedOption == null ||
            !state.TargettingManager.TryGetTarget(
                selectedOption,
                TargettingIntent.Default,
                combatOverride,
                out CardPlayOption? resolvedOption) ||
            resolvedOption == null)
        {
            return false;
        }

        selectedOption = resolvedOption;
        return true;
    }
}
