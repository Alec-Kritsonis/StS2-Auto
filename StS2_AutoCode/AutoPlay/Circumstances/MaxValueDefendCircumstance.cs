using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace StS2_Auto.StS2_AutoCode.AutoPlay.Circumstances;

internal sealed class MaxValueDefendCircumstance : Circumstance
{
    public MaxValueDefendCircumstance()
        : base("MaxValueDefend")
    {
    }

    public override bool Check(AutoPlayGameState state) =>
        state.PlayableCards.Any(option => UsesAllBlock(option, state));

    public override CardPlayOption? GetBestCard(AutoPlayGameState state) =>
        state.PlayableCards
            .Where(option => UsesAllBlock(option, state))
            .OrderByDescending(GetBlockValue)
            .FirstOrDefault();

    private static bool UsesAllBlock(CardPlayOption option, AutoPlayGameState state)
    {
        if (option.Card.DynamicVars.FirstOrDefault(pair => pair.Value is BlockVar).Value
            is not BlockVar blockVar)
            return false;

        int unusedIncomingDamage = state.IncomingDamage - state.Player.Creature.Block;
        int blockValue = (int)blockVar.PreviewValue;
        return blockValue > 0 && blockValue <= unusedIncomingDamage;
    }

    private static decimal GetBlockValue(CardPlayOption option)
    {
        return option.Card.DynamicVars.FirstOrDefault(pair => pair.Value is BlockVar).Value
            is BlockVar blockVar
                ? blockVar.PreviewValue
                : 0;
    }
}
