using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace StS2_Auto.StS2_AutoCode.AutoPlay.Circumstances;

internal sealed class CanKillCircumstance : Circumstance
{
    public CanKillCircumstance()
        : base("CanKill")
    {
    }

    public override bool Check(AutoPlayGameState state) =>
        TryGetLethalPlan(state, out _);

    public override CardPlayOption? GetBestCard(AutoPlayGameState state)
    {
        return TryGetLethalPlan(state, out List<CardPlayOption>? plan) &&
            plan is { Count: > 0 }
            ? plan[0]
            : null;
    }

    private static bool TryGetLethalPlan(
        AutoPlayGameState state,
        out List<CardPlayOption>? plan)
    {
        plan = null;
        if (state.LivingEnemies.Count == 0 ||
            state.Player.PlayerCombatState is not { } playerCombatState)
        {
            return false;
        }

        int bestEnergyCost = int.MaxValue;
        Creature[] nonMinions = state.LivingEnemies
            .Where(enemy => !enemy.HasPower<MinionPower>())
            .ToArray();
        IReadOnlyList<Creature> enemiesToConsider = nonMinions.Length > 0
            ? nonMinions
            : state.LivingEnemies;

        foreach (Creature enemy in enemiesToConsider)
        {
            var damagingCards = state.PlayableCards
                .SelectMany(option => GetOptionsForEnemy(option, enemy))
                .Select(option => (Option: option, Damage: GetDamage(option, enemy)))
                .Where(candidate => candidate.Damage > 0)
                .ToArray();

            var selectedCards = new List<CardPlayOption>();
            if (!TryFindLethalCombination(
                    damagingCards,
                    0,
                    playerCombatState.Energy,
                    enemy.CurrentHp + enemy.Block,
                    selectedCards))
            {
                continue;
            }

            int energyCost = GetPlanEnergyCost(selectedCards, playerCombatState.Energy);
            if (energyCost >= bestEnergyCost)
                continue;

            bestEnergyCost = energyCost;
            plan = selectedCards;
        }

        return plan is { Count: > 0 };
    }

    private static IEnumerable<CardPlayOption> GetOptionsForEnemy(
        CardPlayOption option,
        Creature enemy)
    {
        if (option.Card.TargetType == TargetType.AnyEnemy)
        {
            if (option.ValidTargets.Any(target => ReferenceEquals(target, enemy)))
                yield return option.WithTarget(enemy);
        }
        else if (option.Card.TargetType == TargetType.AllEnemies)
        {
            yield return option;
        }
    }

    private static int GetPlanEnergyCost(
        IReadOnlyList<CardPlayOption> plan,
        int energyAvailable)
    {
        int totalCost = 0;
        foreach (CardPlayOption option in plan)
        {
            int cost = option.Card.EnergyCost.CostsX
                ? energyAvailable
                : option.Card.EnergyCost.GetAmountToSpend();
            totalCost += cost;
            energyAvailable -= cost;
        }

        return totalCost;
    }

    private static bool TryFindLethalCombination(
        (CardPlayOption Option, int Damage)[] damagingCards,
        int startIndex,
        int energyRemaining,
        int damageRemaining,
        List<CardPlayOption> selectedCards)
    {
        if (damageRemaining <= 0)
            return true;

        for (int i = startIndex; i < damagingCards.Length; i++)
        {
            (CardPlayOption option, int damage) = damagingCards[i];
            int energyCost = option.Card.EnergyCost.CostsX
                ? energyRemaining
                : option.Card.EnergyCost.GetAmountToSpend();

            if (energyCost > energyRemaining)
                continue;

            selectedCards.Add(option);
            if (TryFindLethalCombination(
                    damagingCards,
                    i + 1,
                    energyRemaining - energyCost,
                    damageRemaining - damage,
                    selectedCards))
            {
                return true;
            }

            selectedCards.RemoveAt(selectedCards.Count - 1);
        }

        return false;
    }

    private static int GetDamage(CardPlayOption option, Creature enemy)
    {
        if (option.Card.TargetType is not (TargetType.AnyEnemy or TargetType.AllEnemies) ||
            option.Card.TargetType == TargetType.AnyEnemy &&
            !ReferenceEquals(option.Target, enemy) ||
            option.Card.DynamicVars.FirstOrDefault(pair => pair.Value is DamageVar).Value
                is not DamageVar damageVar)
        {
            return 0;
        }

        decimal damage = damageVar.PreviewValue;
        if (option.Card.DynamicVars.ContainsKey("Hits"))
            damage *= option.Card.DynamicVars["Hits"].PreviewValue;

        return Math.Max(0, (int)damage);
    }
}
