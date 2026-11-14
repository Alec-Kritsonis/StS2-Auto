using MegaCrit.Sts2.Core.Models;

namespace StS2_Auto.StS2_AutoCode.AutoPlay;

internal abstract class Circumstance(string name)
{
    public string Name { get; } = name;

    public abstract bool Check(AutoPlayGameState state);

    public abstract CardPlayOption? GetBestCard(AutoPlayGameState state);
}
