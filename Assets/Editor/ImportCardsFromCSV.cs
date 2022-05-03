using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Medici;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Editor
{
    public class ImportCardsFromCSV : EditorWindow
    {
        public static string CSVpath;
        public static string assetFolder;
        
        private static string hint;

        [MenuItem("Utilities/Import cards")]
        public static void OpenWindow()
        {
            //open parameters from player prefs
            CSVpath = PlayerPrefs.GetString("CSVPath");
            assetFolder = PlayerPrefs.GetString("AssetFolder", "Cards");
            hint = String.Empty;
            GetWindow<ImportCardsFromCSV>();
        }

        private void OnGUI()
        {
            var asset = EditorGUILayout.ObjectField("CSV file", 
                AssetDatabase.LoadAssetAtPath<TextAsset>(CSVpath), 
                typeof(TextAsset), false);
            CSVpath = AssetDatabase.GetAssetPath(asset);
            GUILayout.Label("This file will be updated when the cards change in editor.");
            assetFolder = EditorGUILayout.TextField("Asset folder name", assetFolder);
            if (GUILayout.Button("Import", GUILayout.Width(200)))
            {
                hint = String.Empty;
                Import();
            }
            GUILayout.Label(hint);
        }

        public static void Import()
        {
            //save parameters to player prefs - we should use project settings, but this is faster for now
            PlayerPrefs.SetString("CSVPath", CSVpath);
            PlayerPrefs.SetString("AssetFolder", assetFolder);
            
            hint += "\nImporting...";
            if (!AssetDatabase.GetSubFolders("Assets").Contains($"Assets/{assetFolder}"))
            {
                AssetDatabase.CreateFolder("Assets", assetFolder);
            }

            string[] alllines = File.ReadAllLines(CSVpath);
            List<CardData> cards = new List<CardData>(alllines.Length);

            for (var index = 1; index < alllines.Length; index++)
            {
                var line = alllines[index];
                string[] data = line.Split(';');
                bool newSO = true;
                //starting data index
                int i = 1;
                //if SO exists in folder - take it
                CardData card = AssetDatabase.LoadAssetAtPath<CardData>($"Assets/{assetFolder}/{data[i]}.asset");
                if (card is null)
                {
                    card = ScriptableObject.CreateInstance<CardData>();
                    newSO = true;
                }
                else
                {
                    newSO = false;
                    Debug.Log($"found existing {data[i]}, will overwrite");
                }
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

                if (newSO)
                {
                    AssetDatabase.CreateAsset(card, $"Assets/{assetFolder}/{card.id}.asset");
                }
                cards.Add(card);
            }
            
            Object.FindObjectOfType<CardDictionary>()?.ImportDeck(cards.ToArray());
            AssetDatabase.SaveAssets();
            hint += "Done!";
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