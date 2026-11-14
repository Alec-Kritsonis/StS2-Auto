using System.Linq;
using MegaCrit.Sts2.Core.Models;

namespace StS2_Auto.StS2_AutoCode.AutoPlay.Circumstances;

internal sealed class GoldGlowCircumstance : Circumstance
{
    public GoldGlowCircumstance()
        : base("GoldGlow")
    {
    }

    public override bool Check(AutoPlayGameState state) =>
        state.PlayableCards.Any(option => option.Card.ShouldGlowGold);

    public override CardPlayOption? GetBestCard(AutoPlayGameState state) =>
        state.PlayableCards.FirstOrDefault(option => option.Card.ShouldGlowGold);
}
