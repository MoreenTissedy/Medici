using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Medici
{
    public class CardPlayed : MonoBehaviour, ICardPresenter
    {
        [FormerlySerializedAs("name")] [SerializeField] private Text cardName;
        [SerializeField] private Text mainText, yesText, noText, yesHint, noHint;
        [SerializeField] private Image typeFrame;
        [SerializeField] private GameLoop gm;

        private void OnValidate()
        {
            if (gm is null)
            {
                gm = FindObjectOfType<GameLoop>();
            }
        }
        
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
            if (!(card.type is null))
            {
                typeFrame.sprite = card.type.EventInPlay;
            }
            else if (!(gm.defaultType is null))
            {
                typeFrame.sprite = gm.defaultType.EventInPlay;
            }
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}