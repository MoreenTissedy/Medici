using System.Collections.Generic;
using UnityEngine;

namespace Medici
{
    public class Interface : MonoBehaviour
    {
        [SerializeField] private CardPlayed eventPanel;
        [SerializeField] private CardAvailable[] availableButtons;
        [SerializeField] private CardPending pendingButton;
        
        public void StartEvent(CardData card)
        {
            eventPanel.Display(card);    
        }

        public void MorningUpdate(InWork stack, List<CardData> cardsAvailable)
        {
            eventPanel.Hide();
            
            //display available cards
            DisplayAvailableCards(cardsAvailable);

            //display stack & deactivate cards
            if (stack.IsPending())
            {
                pendingButton.gameObject.SetActive(true);
                pendingButton.Display(stack.Next());
                foreach (var button in availableButtons)
                {
                    button.Enable(false);
                }
            }
            else
            {
                pendingButton.gameObject.SetActive(false);
            }

        }

        private void DisplayAvailableCards(List<CardData> cardsAvailable)
        {
            for (var index = 0; index < availableButtons.Length; index++)
            {
                var button = availableButtons[index];
                if (index < cardsAvailable.Count)
                {
                    button.gameObject.SetActive(true);
                    button.Display(cardsAvailable[index]);
                }
                else
                {
                    button.gameObject.SetActive(false);
                }
            }
        }

        public void UnlockChoosing(List<CardData> cardsAvailable)
        {
            eventPanel.Hide();
            pendingButton.gameObject.SetActive(false);
            DisplayAvailableCards(cardsAvailable);
        }
    }
}