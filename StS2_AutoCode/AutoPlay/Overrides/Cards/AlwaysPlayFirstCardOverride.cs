using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using StS2_Auto.StS2_AutoCode.AutoPlay;

namespace StS2_Auto.StS2_AutoCode.AutoPlay.Overrides.Cards;

internal sealed class AlwaysPlayFirstCardOverride : CardOverride
{
    private readonly IReadOnlyList<Type> _cardPriorities =
    [
        typeof(BattleTrance),
        typeof(Stampede),
        typeof(Rage),
        typeof(Bloodletting),
        typeof(Rampage)
    ];

    public AlwaysPlayFirstCardOverride()
        : base("AlwaysPlayFirst")
    {
    }

    public override bool TryGetCard(
        AutoPlayGameState state,
        out CardPlayOption? selectedOption)
    {
        foreach (Type cardType in _cardPriorities)
        {
            selectedOption = state.PlayableCards
                .FirstOrDefault(option => option.Card.GetType() == cardType);
            if (selectedOption != null)
                return true;
        }

        selectedOption = null;
        return false;
    }
}
