using System;
using System.Collections.Generic;
using System.IO;
using Medici;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Editor
{
    public class ImportCardsFromExcel
    {
        private static string CSVpath = "/medici_GameData.csv";
        [MenuItem("Utilities/Import cards")]
        public static void Import()
        {
            AssetDatabase.CreateFolder("Assets", "Cards");

            string[] alllines = File.ReadAllLines(Application.dataPath + CSVpath);
            List<CardData> cards = new List<CardData>(alllines.Length);

            for (var index = 1; index < alllines.Length; index++)
            {
                var line = alllines[index];
                string[] data = line.Split(';');
                CardData card = ScriptableObject.CreateInstance<CardData>();
                //starting data index
                int i = 1;
                //0, 1 ID	type
                card.id = data[i];
                //2, 3 Event_name	Text_Event
                card.eventName = data[i + 2];
                card.textEvent = data[i + 3];
                //4, 5 NO_text_Prize	Yes_Text_prize
                card.noTextPrize = data[i + 4];
                card.yesTextPrize = data[i + 5];
                
                //6, 7 No_Prize	Yes_Prize
                card.noPrize = data[i + 6];
                card.yesPrize = data[i + 7];
                
                //8, 9 No_Event_Chanse	Yes_Event_Chanse
                if (EventRate(data[i + 8], out var ers, card.id))
                    card.noEventChance = ers;
                if (EventRate(data[i + 9], out var ers2, card.id))
                    card.yesEventChance = ers2;
                
                //10, 11 Coldown	Repeat	
                string[] cooldownData = data[i + 10].Trim().Split('-');
                card.cooldownMin = IntFromString(cooldownData[0]);
                card.cooldownMax = cooldownData.Length > 1 ? IntFromString(cooldownData[1]) : card.cooldownMin;    
                card.repeat = BoolFromString(data[i + 11]);
                if (card.repeat && card.cooldownMin == 0)
                {
                    Debug.LogWarning($"Card {card.id} set to repeat with possible cooldown 0, " +
                                     $"this leads to infinite loop, repeat set to false");
                    card.repeat = false;
                }
                
                //parsing test
                card.ParseStatValue(card.yesPrize);
                card.ParseStatValue(card.noPrize);
                
                AssetDatabase.CreateAsset(card, $"Assets/Cards/{card.id}.asset");
                cards.Add(card);
            }
            
            Object.FindObjectOfType<CardDictionary>()?.ImportDeck(cards.ToArray());
            AssetDatabase.SaveAssets();
        }

        static bool EventRate(string data, out CardData.EventRate[] events, string cardID)
        {
            string[] pairs = data.Trim().Split(',');
            events = new CardData.EventRate[0];
            if (pairs.Length == 0)
            {
                return false;
            }
            var list = new List<CardData.EventRate>(pairs.Length);
            float percent = 100;
            var defaultRate = new List<CardData.EventRate>(pairs.Length);
            foreach (var pair in pairs)
            {
                string[] pairData = pair.Split(':');
                var newEventRate = new CardData.EventRate();
                if (pairData[0].Trim() == String.Empty)
                {
                    return false;
                }
                newEventRate.id = pairData[0].Trim();
                if (pairData.Length > 1)
                {
                    if (Single.TryParse(pairData[1], out float value) || value<=0 || value>100)
                    {
                        newEventRate.rate = value;
                        percent -= value;
                    }
                    else
                    {
                        Debug.LogError($"Parse error: card {cardID}, rate was not parsed for event {pairData[0]}, assumed default");
                    }
                }
                //default rate
                else
                {
                    defaultRate.Add(newEventRate);
                }
                list.Add(newEventRate);
            }
            int defaultValue = Mathf.FloorToInt(percent / defaultRate.Count);
            foreach (var eventRate in defaultRate)
            {
                eventRate.rate = defaultValue;
            }
            events = list.ToArray();
            return true;
        }
        
        static int IntFromString(string data)
        {
            if (Int32.TryParse(data, out int number))
            {
                return number;
            }

            return 0;
        }

        static bool BoolFromString(string data)
        {
            if (bool.TryParse(data, out bool trueFalse))
            {
                return trueFalse;
            }

            if (data == "+" || data == "1")
            {
                return true;
            }

            return false;
        }

        
    }
}