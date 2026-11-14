using System.Linq;
using MegaCrit.Sts2.Core.Models;

namespace StS2_Auto.StS2_AutoCode.AutoPlay.Circumstances;

internal sealed class DebuffSetupCircumstance : Circumstance
{
    public DebuffSetupCircumstance()
        : base("DebuffSetup")
    {
    }

    public override bool Check(AutoPlayGameState state) =>
        HasFullEnergy(state) &&
        state.PlayableCards.Any(option => AppliesDebuff(option));

    public override CardPlayOption? GetBestCard(AutoPlayGameState state)
    {
        if (!HasFullEnergy(state))
            return null;

        CardPlayOption[] debuffCards = state.PlayableCards
            .Where(AppliesDebuff)
            .ToArray();

        if (debuffCards.Length == 0)
            return null;

        CardPlayOption selectedCard = debuffCards[Random.Shared.Next(debuffCards.Length)];
        if (selectedCard.ValidTargets.Count == 0)
            return selectedCard;

        return state.TargettingManager.TryGetTarget(
            selectedCard,
            TargettingIntent.ApplyDebuff,
            state.ActiveOverride ?? throw new System.InvalidOperationException(
                "A combat override must be selected before targetting."),
            out CardPlayOption? selectedOption)
            ? selectedOption
            : null;
    }

    private static bool HasFullEnergy(AutoPlayGameState state) =>
        state.Player.PlayerCombatState is { } playerCombatState &&
        playerCombatState.Energy >= playerCombatState.MaxEnergy;

    private static bool AppliesDebuff(CardPlayOption option) =>
        option.Card.HoverTips.Any(hoverTip => hoverTip.IsDebuff);
}
