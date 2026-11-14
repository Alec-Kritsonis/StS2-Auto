using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace StS2_Auto.StS2_AutoCode.AutoPlay.Circumstances;

internal sealed class RandomDefaultCircumstance : Circumstance
{
    public RandomDefaultCircumstance()
        : base("RandomDefault")
    {
    }

    public override bool Check(AutoPlayGameState state) => true;

    public override CardPlayOption? GetBestCard(AutoPlayGameState state)
    {
        IEnumerable<CardPlayOption> candidates = state.PlayableCards;
        if (state.IncomingDamage <= state.Player.Creature.Block)
        {
            candidates = candidates.Where(option =>
                !option.Card.DynamicVars.Any(pair =>
                    pair.Value is BlockVar blockVar && blockVar.PreviewValue > 0));
        }

        CardPlayOption[] eligibleCards = candidates.ToArray();
        if (eligibleCards.Length == 0)
            return null;

        return eligibleCards[Random.Shared.Next(eligibleCards.Length)];
    }
}
