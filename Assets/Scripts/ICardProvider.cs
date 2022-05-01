namespace Medici
{
    /// <summary>
    /// Interface for an object providing a set of cards for the game.
    /// There is only one such object right now.
    /// Interface added for convenience and future use.
    /// </summary>
    public interface ICardProvider
    {
        CardData GetCard(string id);
        CardData[] GetRootDeck();
    }
}