using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Medici
{
    public class CardPlayed : MonoBehaviour, ICardPresenter
    {
        [FormerlySerializedAs("name")] [SerializeField] private Text cardName;
        [SerializeField] private Text mainText, yesText, noText, yesHint, noHint;

        private void Awake()
        {
            Hide();
        }

        public void Display(CardData card)
        {
            gameObject.SetActive(true);
            cardName.text = card.eventName;
            mainText.text = card.textEvent;
            yesText.text = card.yesTextPrize;
            yesHint.text = card.yesPrize;
            noText.text = card.noTextPrize;
            noHint.text = card.noPrize;
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}