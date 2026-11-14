using System.Collections.Generic;

namespace StS2_Auto.StS2_AutoCode.AutoPlay;

internal abstract class CombatOverride(string name)
{
    public string Name { get; } = name;

    public abstract IReadOnlyList<Circumstance> Priorities { get; }
    public abstract IReadOnlyList<Targetting> TargettingPriorities { get; }

    public abstract bool AppliesTo(AutoPlayGameState state);
}
