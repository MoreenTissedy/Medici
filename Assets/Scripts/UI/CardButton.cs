using System;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.UI;

namespace Medici
{
    public class CardButton : MonoBehaviour, ICardPresenter, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField]
        protected float amplitude = 1.2f;
        [SerializeField]
        protected float time = 0.2f;
        [SerializeField] protected Color disabledColor = Color.gray;
        [SerializeField] protected GameLoop gm;
        [SerializeField] protected Text cardName;
        [SerializeField] protected Text typeText;
        [SerializeField] protected Image typeFrame;
        
        private float initialScaleX;
        private CardData currentCard;
        private bool buttonEnabled = true;
        private void Awake()
        {
            initialScaleX = transform.localScale.x;
        }

        private void OnValidate()
        {
            if (gm is null)
            {
                gm = FindObjectOfType<GameLoop>();
            }
        }

        public virtual void Display(CardData card)
        {
            cardName.text = card.eventName;
            currentCard = card;
            Enable(true);
            if (card.type is null)
            {
                typeText.text = gm?.defaultType?.Description ?? String.Empty;
            }
            else
            {
                typeText.text = card.type.Description;
            }
        }

        public void Enable(bool on)
        {
            buttonEnabled = on;
            if (on)
            {
                typeFrame.color = Color.white;
            }
            else
            {
                typeFrame.color = disabledColor;
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
            typeFrame.transform.DOScaleX(initialScaleX*amplitude, time);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!buttonEnabled)
                return;
            typeFrame.transform.DOScaleX(initialScaleX, time);
        }
    }
}