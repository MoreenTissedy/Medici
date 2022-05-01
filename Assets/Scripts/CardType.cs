using UnityEngine;

namespace Medici
{
    /// <summary>
    /// Holds the type-dependent parameters of a card (currently just its design),
    /// e.g. Epic card, Red card etc.
    /// </summary>
    [CreateAssetMenu(fileName = "New Card Type", menuName = "Card Type", order = 0)]
    public class CardType : ScriptableObject
    {
        [SerializeField]
        private Sprite eventAvailable;
        public Sprite EventAvailable => eventAvailable;

        [SerializeField]
        private Sprite eventPending;
        public Sprite EventPending => eventPending;

        [SerializeField]
        private Sprite eventInPlay;
        public Sprite EventInPlay => eventInPlay;

        [SerializeField]
        private string description;
        public string Description => description;


    }
}