using System.Collections.Generic;
using UnityEngine;

namespace Medici
{
    public class Deck
    {
        private List<CardData> deck;

        /// <summary><para>Initializes a new instance of the <see cref="T:System.Object" /> class.</para></summary>
        public Deck(CardData[] startDeck)
        {
            deck = new List<CardData>(startDeck.Length);
            deck.AddRange(startDeck);
        }

        public CardData GetRandom()
        {
            if (deck.Count == 0)
            {
                Debug.LogError("The deck is empty!");
                return null;
            }
            int rnd = Random.Range(0, deck.Count);
            return deck[rnd];
        }

        public void Remove(CardData data)
        {
            if (deck.Contains(data))
                deck.Remove(data);
        }

        public CardData[] GetRandom(int num)
        {
            if (num > deck.Count)
            {
                Debug.LogWarning("Only few cards left in the deck!");
            }
            var list = new List<CardData>(num);
            while (list.Count < num && list.Count < deck.Count)
            {
                int rnd = Random.Range(0, deck.Count);
                if (!list.Contains(deck[rnd]))
                    list.Add(deck[rnd]);
            }
            return list.ToArray();
        }
    }
}