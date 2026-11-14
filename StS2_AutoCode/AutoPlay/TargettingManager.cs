using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using StS2_Auto.StS2_AutoCode.Logging;

namespace StS2_Auto.StS2_AutoCode.AutoPlay;

internal sealed class TargettingManager
{
    internal static IReadOnlyList<Targetting> DefaultPriorities { get; } =
    [
        new SummonsTargetting(),
        new MultipleDefaultTargetting()
    ];

    public bool TryGetTarget(
        CardPlayOption option,
        TargettingIntent intent,
        CombatOverride combatOverride,
        out CardPlayOption? selectedOption)
    {
        IReadOnlyList<Creature> validTargets = option.ValidTargets;
        if (validTargets.Count == 0)
        {
            selectedOption = null;
            return false;
        }

        foreach (Targetting targetting in combatOverride.TargettingPriorities)
        {
            if (!targetting.Check(validTargets))
                continue;

            Creature? selectedTarget = targetting.GetBestTarget(validTargets, intent);
            if (selectedTarget == null)
                continue;

            selectedOption = option.WithTarget(selectedTarget);
            AutoPlayLogger.postInfo(
                $"{targetting.Name} selected a target for '{option.Card.Title}' " +
                $"({intent}).");
            return true;
        }

        selectedOption = null;
        return false;
    }

    internal static IReadOnlyList<Creature> ApplyIntent(
        IReadOnlyList<Creature> validTargets,
        TargettingIntent intent)
    {
        if (intent != TargettingIntent.ApplyDebuff)
            return validTargets;

        int minimumDebuffs = validTargets.Min(CountDebuffs);
        return validTargets
            .Where(target => CountDebuffs(target) == minimumDebuffs)
            .ToArray();
    }

    private static int CountDebuffs(Creature creature) =>
        creature.Powers.Sum(power => power.HoverTips.Count(hoverTip => hoverTip.IsDebuff));
}
