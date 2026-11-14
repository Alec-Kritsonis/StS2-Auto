namespace StS2_Auto.StS2_AutoCode.AutoPlay.Overrides.Cards;

internal abstract class CardOverride(string name)
{
    public string Name { get; } = name;

    public abstract bool TryGetCard(
        AutoPlayGameState state,
        out CardPlayOption? selectedOption);
}
