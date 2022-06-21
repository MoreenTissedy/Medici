namespace Medici
{
    /// <summary>
    /// Interface for objects representing card data — buttons and windows.
    /// </summary>
    public interface ICardPresenter
    {
        void Display(CardData card);
    }
}