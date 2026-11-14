using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Models;
using StS2_Auto.StS2_AutoCode.AutoPlay.Circumstances;
using StS2_Auto.StS2_AutoCode.Logging;

namespace StS2_Auto.StS2_AutoCode.AutoPlay;

internal sealed class PriorityManager
{
    private readonly List<Circumstance> _priorities =
    [
        new CanKillCircumstance(),
        new EfficientDefendCircumstance(),
        new DebuffSetupCircumstance(),
        new RandomAttackDefaultCircumstance(),
        new RandomDefaultCircumstance()
    ];

    public bool TryGetCard(
        AutoPlayGameState state,
        out CardPlayOption? selectedOption)
    {
        foreach (Circumstance circumstance in _priorities)
        {
            if (!circumstance.Check(state))
                continue;

            selectedOption = circumstance.GetBestCard(state);
            if (selectedOption == null)
                continue;

            if (selectedOption.Target == null &&
                selectedOption.ValidTargets.Count > 0)
            {
                if (!state.TargettingManager.TryGetTarget(
                        selectedOption,
                        TargettingIntent.Default,
                        out CardPlayOption? resolvedOption) ||
                    resolvedOption == null)
                {
                    continue;
                }

                selectedOption = resolvedOption;
            }

            AutoPlayLogger.postInfo(
                $"{circumstance.Name} selected card '{selectedOption.Card.Title}'.");
            return true;
        }

        selectedOption = null;
        return false;
    }
}
