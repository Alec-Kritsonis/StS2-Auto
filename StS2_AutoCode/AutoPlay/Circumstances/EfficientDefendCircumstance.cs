using System;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace StS2_Auto.StS2_AutoCode.AutoPlay.Circumstances;

internal sealed class EfficientDefendCircumstance : Circumstance
{
    private const decimal CurrentHealthDamageThreshold = 0.1m;

    public EfficientDefendCircumstance()
        : base("EfficientDefend")
    {
    }

    public override bool Check(AutoPlayGameState state) =>
        GetUnblockedIncomingDamage(state) > state.Player.Creature.CurrentHp * CurrentHealthDamageThreshold &&
        state.PlayableCards.Any(option => GetBlockValue(option) > 0);

    public override CardPlayOption? GetBestCard(AutoPlayGameState state) =>
        state.PlayableCards
            .Where(option => GetBlockValue(option) > 0)
            .OrderBy(option => GetRemainingDamageDistance(option, state))
            .ThenBy(option => GetEnergyCost(option, state))
            .ThenByDescending(GetBlockEfficiency)
            .FirstOrDefault();

    private static decimal GetUnblockedIncomingDamage(AutoPlayGameState state) =>
        Math.Max(0, state.IncomingDamage - state.Player.Creature.Block);

    private static decimal GetRemainingDamageDistance(
        CardPlayOption option,
        AutoPlayGameState state)
    {
        decimal remainingDamage = Math.Max(
            0,
            GetUnblockedIncomingDamage(state) - GetBlockValue(option));
        decimal threshold = state.Player.Creature.CurrentHp * CurrentHealthDamageThreshold;
        return Math.Abs(remainingDamage - threshold);
    }

    private static decimal GetBlockValue(CardPlayOption option) =>
        option.Card.DynamicVars.FirstOrDefault(pair => pair.Value is BlockVar).Value
            is BlockVar blockVar
                ? blockVar.PreviewValue
                : 0;

    private static int GetEnergyCost(CardPlayOption option, AutoPlayGameState state) =>
        option.Card.EnergyCost.CostsX
            ? state.Player.PlayerCombatState?.Energy ?? 0
            : option.Card.EnergyCost.GetAmountToSpend();

    private static decimal GetBlockEfficiency(CardPlayOption option)
    {
        int energyCost = option.Card.EnergyCost.CostsX
            ? 1
            : Math.Max(1, option.Card.EnergyCost.GetAmountToSpend());
        return GetBlockValue(option) / energyCost;
    }
}
