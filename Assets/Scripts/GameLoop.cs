using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Medici
{
    [RequireComponent(typeof(ICardProvider))]
    public class GameLoop : MonoBehaviour
    {
        [SerializeField]
        private int cardsToChooseFrom = 4;
        [SerializeField]
        private int cardsPerDay = 2;
        [SerializeField]
        private Interface ui;

        private int cardsChosenThisDay = 0;
        private CardData currentCard;
        private List<CardData> availableCards;
        private ICardProvider cardProvider;
        private InWork inwork;
        private Deck deck;

        private void Start()
        {
            //init
            cardProvider = GetComponent<ICardProvider>();
            deck = new Deck(cardProvider.GetRootDeck());
            inwork = new InWork();
            Economics.Init(300, 0, 0, 0, 0);
            
            //first day
            availableCards = deck.GetRandom(cardsToChooseFrom).ToList();
            ui.MorningUpdate(inwork, availableCards);
        }

        private void OnValidate()
        {
            if (ui is null)
            {
                ui = FindObjectOfType<Interface>();
            }
        }

        void DisplayAvailableCards()
        {
            //for console
            string text = String.Empty;
            for (var i = 0; i < availableCards.Count; i++)
            {
                text += $"{i + 1}: {availableCards[i].eventName} ";
            }
            Debug.Log(text);
        }

        public void ChooseCard(int i)
        {
            if (i >= availableCards.Count||i<0)
                return;
            PlayCard(availableCards[i]);
        }

        public void PlayCard(CardData card)
        {
            deck.Remove(card);
            if (availableCards.Contains(card))
            {
                availableCards.Remove(card);
                cardsChosenThisDay++;
            }
            currentCard = card;
            
            //console
            Debug.Log(currentCard.eventName+" "+currentCard.id);

            ui.StartEvent(currentCard);
        }

        public void Response(bool proceed)
        {
            if (proceed)
            {
                Debug.Log("-> "+currentCard.yesTextPrize);
                currentCard.ParseStatValue(currentCard.yesPrize);
            }
            else
            {
                
                Debug.Log("<- "+currentCard.noTextPrize);
                currentCard.ParseStatValue(currentCard.noPrize);
            }

            if (currentCard.HasExtraCard(proceed))
            {
                CardData extra = cardProvider.GetCard(currentCard.GetExtraID(proceed));
                if (extra != null)
                {
                    inwork.Add(extra);
                    Debug.Log($"card added: {extra.eventName}");
                }
            }

            if (currentCard.repeat)
            {
                inwork.Add(currentCard);
                Debug.Log($"card added back to deck");
            }

            //Debug.Log($"Gold: {Economics.Gold}, Rep: {Economics.Rep}, " +
            //          $"Town: {Economics.Town}, Piety: {Economics.Piety}, Assassin: {Economics.Assassin}, FlPower: {Economics.FlorencePower}");
            currentCard = null;
            if (inwork.IsPending())
                PlayCard(inwork.Next());
            else
            {
                if (cardsChosenThisDay < cardsPerDay)
                {
                    ui.UnlockChoosing(availableCards);
                }
                else
                {
                    NewDay();
                }
            }
                
        }

        public void NewDay()
        {
            Debug.Log("A new day!");
            inwork.ForwardDay();
            availableCards = deck.GetRandom(cardsToChooseFrom).ToList();
            cardsChosenThisDay = 0;
            ui.MorningUpdate(inwork, availableCards);
        }

        private void Update()
        {
            //keyboard inputs
            if (currentCard != null)
            {
                //choose yes or no
                if (Input.GetKeyDown(KeyCode.RightArrow))
                    Response(true);
                else if (Input.GetKeyDown(KeyCode.LeftArrow))
                    Response(false);
            }
            else 
            {
                //choose available card
                if (Input.GetKeyDown(KeyCode.Keypad1))
                    ChooseCard(0);
                else if (Input.GetKeyDown(KeyCode.Keypad2))
                    ChooseCard(1);
                else if (Input.GetKeyDown(KeyCode.Keypad3))
                    ChooseCard(2);
                else if (Input.GetKeyDown(KeyCode.Keypad4))
                    ChooseCard(3);
            }
        }
    }
}