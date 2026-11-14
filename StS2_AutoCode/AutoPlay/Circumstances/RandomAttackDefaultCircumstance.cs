using System.Linq;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace StS2_Auto.StS2_AutoCode.AutoPlay.Circumstances;

internal sealed class RandomAttackDefaultCircumstance : Circumstance
{
    public RandomAttackDefaultCircumstance()
        : base("RandomAttackDefault")
    {
    }

    public override bool Check(AutoPlayGameState state) =>
        state.PlayableCards.Any(option => option.Card.Type == CardType.Attack);

    public override CardPlayOption? GetBestCard(AutoPlayGameState state)
    {
        CardPlayOption[] attacks = state.PlayableCards
            .Where(option => option.Card.Type == CardType.Attack)
            .ToArray();

        if (attacks.Length == 0)
            return null;

        return attacks[Random.Shared.Next(attacks.Length)];
    }
}
