namespace Medici
{
    public interface ICardProvider
    {
        CardData GetCard(string id);
        CardData[] GetRootDeck();
    }
}