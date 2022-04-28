using System.Collections.Generic;
using Medici;
using UnityEngine;

namespace Medici
{
    public class CardDictionary : MonoBehaviour, ICardProvider
    {
        [SerializeField] private CardData[] allCards;
        private Dictionary<string, CardData> dictionary;
        private CardData[] rootDeck;

        public void ImportDeck(CardData[] cards)
        {
            allCards = cards;
        }
        
        private void Awake()
        {
            //init dictionary
            //CardData[] allCards = FindAllCardData();
            dictionary = new Dictionary<string, CardData>(allCards.Length);
            var roots = new List<CardData>(allCards.Length/2);
            foreach (CardData card in allCards)
            {
                if (dictionary.ContainsKey(card.id))
                {
                    Debug.LogError($"Duplicate card ID {card.id}");
                    continue;
                }
                dictionary.Add(card.id, card);
                if (!card.id.Contains("."))
                {
                    roots.Add(card);
                }                    
            }
            rootDeck = roots.ToArray();
        }
        
        /*private CardData[] FindAllCardData()
        {
            List<CardData> cards = new List<CardData>();
            string[] guids = AssetDatabase.FindAssets("t: CardData");
            for( int i = 0; i < guids.Length; i++ )
            {
                string assetPath = AssetDatabase.GUIDToAssetPath( guids[i] );
                CardData asset = AssetDatabase.LoadAssetAtPath<CardData>( assetPath );
                if( asset != null )
                {
                    cards.Add(asset);
                }
            }
            return cards.ToArray();
        }*/

        public CardData GetCard(string id)
        {
            if (dictionary.TryGetValue(id, out var card))
            {
                return card;
            }
            Debug.LogError($"Can't find card data with id {id}");
            return null;
        }

        public CardData[] GetRootDeck()
        {
            return rootDeck;
        }
    }
}