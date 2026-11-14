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
        if (state.PlayableCards.Count == 0)
            return null;

        return state.PlayableCards[Random.Shared.Next(state.PlayableCards.Count)];
    }
}
