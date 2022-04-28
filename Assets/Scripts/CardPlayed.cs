using UnityEngine;
using UnityEngine.UI;

namespace Medici
{
    public class CardPlayed : MonoBehaviour, ICardPresenter
    {
        [SerializeField] private Text name, mainText, yesText, noText, yesHint, noHint;

        private void Awake()
        {
            Hide();
        }

        public void Display(CardData card)
        {
            gameObject.SetActive(true);
            name.text = card.eventName;
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