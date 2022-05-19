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
        private string CSVpath;
        private string assetFolder;
        private bool autoUpdate = true;
        private string hint;
        
        private static float windowHeight = 300;
        private static float windowWidth = 500;
        

        [MenuItem("Utilities/Import cards")]
        public static void OpenWindow()
        {
            GetWindow<ImportCardsFromCSV>(true, "Import cards")
                .minSize = new Vector2(windowWidth, windowHeight);
        }

        private void OnEnable()
        {
            //open parameters from player prefs
            CSVpath = EditorPrefs.GetString("CSVPath");
            assetFolder = EditorPrefs.GetString("AssetFolder", "Cards");
            autoUpdate = EditorPrefs.GetBool("AutoUpdate", true);
            
            hint = String.Empty;
        }

        private void OnDisable()
        {
            SavePrefs();
        }

        private void SavePrefs()
        {
            EditorPrefs.SetString("CSVPath", CSVpath);
            EditorPrefs.SetString("AssetFolder", assetFolder);
            EditorPrefs.SetBool("AutoUpdate", autoUpdate);
        }

        private void OnGUI()
        {
            var asset = EditorGUILayout.ObjectField("CSV file", 
                AssetDatabase.LoadAssetAtPath<TextAsset>(CSVpath), 
                typeof(TextAsset), false);
            CSVpath = AssetDatabase.GetAssetPath(asset);
            
            GUILayout.BeginHorizontal();
            GUILayout.Label("This file will be updated when the cards change in editor.");
            autoUpdate = GUILayout.Toggle(autoUpdate, "Auto Update", GUILayout.Width(100));
            GUILayout.EndHorizontal();
            
            assetFolder = EditorGUILayout.TextField("Asset folder name", assetFolder);
            if (GUILayout.Button("Import", GUILayout.Width(200)))
            {
                hint = String.Empty;
                Import();
            }
            GUILayout.Label(hint);
        }

        public void Import()
        {
            SavePrefs();
            
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
                int i = CardData.startingIndex;
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
                if (ParseEventRate(data[i + 8], out var ers, out var eRemove, card.id))
                {
                    card.noEventChance = ers;
                    card.noEventRemove = eRemove;
                }

                if (ParseEventRate(data[i + 9], out var ers2, out var eRemove2, card.id))
                {
                    card.yesEventChance = ers2;
                    card.yesEventRemove = eRemove2;
                }
                
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

        static bool ParseEventRate(string data, out CardData.EventRate[] events, out string[] eventsRemove, string cardID)
        {
            string[] pairs = data.Trim().Split(',');
            events = new CardData.EventRate[0];
            eventsRemove = new string[0];
            if (pairs.Length == 0)
            {
                return false;
            }
            var list = new List<CardData.EventRate>(pairs.Length);
            var listToRemove = new List<string>(pairs.Length);
            float percent = 100;
            var defaultRate = new List<CardData.EventRate>(pairs.Length);
            foreach (var pair in pairs)
            {
                string[] pairData = pair.Split(':');
                if (pairData[0].Trim() == String.Empty)
                {
                    return false;
                }

                var newEventRate = new CardData.EventRate();
                newEventRate.id = pairData[0].Trim();
                if (pairData.Length > 1)
                {
                    if (Single.TryParse(pairData[1], out float value) && value<=100)
                    {
                        if (value < 0)
                        {
                            //move this event to the list of events to be removed
                            listToRemove.Add(newEventRate.id);
                        }
                        else
                        {
                            newEventRate.rate = value;
                            percent -= value;
                            list.Add(newEventRate);
                        }
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
                
            }
            int defaultValue = Mathf.FloorToInt(percent / defaultRate.Count);
            foreach (var eventRate in defaultRate)
            {
                eventRate.rate = defaultValue;
                list.Add(eventRate);
            }
            events = list.ToArray();
            eventsRemove = listToRemove.ToArray();
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