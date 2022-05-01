using System.Collections.Generic;
using UnityEngine;

namespace Medici
{
    /// <summary>
    /// High-level UI manager
    /// </summary>
    public class Interface : MonoBehaviour
    {
        //autofill
        [SerializeField] private CardPlayed eventPanel;
        [SerializeField] private CardAvailable[] availableButtons;
        [SerializeField] private CardPending pendingButton;
       
        private void OnValidate()
        {
            availableButtons = GetComponentsInChildren<CardAvailable>();
            eventPanel = GetComponentInChildren<CardPlayed>();
            pendingButton = GetComponentInChildren<CardPending>();
        }
        
        /// <summary>
        /// Play a card — open a card panel.
        /// </summary>
        /// <param name="card"></param>
        public void StartEvent(CardData card)
        {
            eventPanel.Display(card);    
        }


        /// <summary>
        /// Update UI in the beginning of a new round.
        /// </summary>
        /// <param name="stack">Pending cards</param>
        /// <param name="cardsAvailable">Available cards</param>
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

        /// <summary>
        /// Display available cards when the pending cards are all done.
        /// </summary>
        /// <param name="cardsAvailable">Available cards</param>
        public void UnlockChoosing(List<CardData> cardsAvailable)
        {
            eventPanel.Hide();
            pendingButton.gameObject.SetActive(false);
            DisplayAvailableCards(cardsAvailable);
        }
    }
}