using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.UI;

namespace Medici
{
    [RequireComponent(typeof(Image))]
    public class CardButton : MonoBehaviour, ICardPresenter, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField]
        private float amplitude = 1.2f;
        [SerializeField]
        private float time = 0.2f;
        [SerializeField] Color disabledColor = Color.gray;
        [SerializeField] private GameLoop gm;
        [SerializeField] private Text cardName;
        
        private float initialScaleX;
        private Image picture;
        private CardData currentCard;
        private bool buttonEnabled = true;
        private void Awake()
        {
            picture = GetComponent<Image>();
            initialScaleX = transform.localScale.x;
        }

        private void OnValidate()
        {
            if (gm is null)
            {
                gm = FindObjectOfType<GameLoop>();
            }
        }

        public void Display(CardData card)
        {
            cardName.text = card.eventName;
            currentCard = card;
            Enable(true);
            //type
        }

        public void Enable(bool on)
        {
            buttonEnabled = on;
            if (on)
            {
                picture.color = Color.white;
            }
            else
            {
                picture.color = disabledColor;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!buttonEnabled)
                return;
            gm.PlayCard(currentCard);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!buttonEnabled)
                return;
            transform.DOScaleX(initialScaleX*amplitude, time);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!buttonEnabled)
                return;
            transform.DOScaleX(initialScaleX, time);
        }
    }
}