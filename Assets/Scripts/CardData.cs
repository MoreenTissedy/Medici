using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Medici
{
    [CreateAssetMenu(fileName = "New card", menuName = "Card", order = 0)]
    public class CardData : ScriptableObject
    {
        [Serializable]
        public class EventRate
        {
            public string id;
            public float rate;
        }

        [Serializable]
        public class StatValue
        {
            public string stat;
            public int value;
        }

        public string id;

        //SO link
        public string type;
        public string eventName;

        [TextArea(4, 10)] public string textEvent;
        
        public int cooldownMin, cooldownMax;
        public bool repeat;

        [Header("YES option")] public string yesTextPrize;

        //public StatValue[] yesPrize;
        //public int yesGold, yesRep, yesTown, yesAssassin, yesPiety, yesFlorencePower;
        public string yesPrize;
        public EventRate[] yesEventChance;

        [Header("NO option")] public string noTextPrize;

        //public StatValue[] noPrize;
        public string noPrize;

        //public int noGold, noRep, noTown, noAssassin, noPiety, noFlorencePower;
        public EventRate[] noEventChance;


        public string GetExtraID(bool yes)
        {
            float rnd = Random.Range(0, 100);
            float cumulative = 0;
            if (yes)
            {
                foreach (var eventRate in yesEventChance)
                {
                    if (rnd < cumulative + eventRate.rate)
                        return eventRate.id;
                    cumulative += eventRate.rate;
                }
            }
            else
            {
                foreach (var eventRate in noEventChance)
                {
                    if (rnd < cumulative + eventRate.rate)
                        return eventRate.id;
                    cumulative += eventRate.rate;
                }
            }
            Debug.LogWarning($"Event rates are wrong for card {id}, please check.");
            return String.Empty;
        }

        public bool HasExtraCard(bool yes)
        {
            if (yes)
            {
                return yesEventChance.Length > 0;
            }
            else
            {
                return noEventChance.Length > 0;
            }
        }

        public bool ParseStatValue(string data)
        {
            bool TryParseFormula(string[] pairData, out Goods goods, out int amount)
            {
                if (!Enum.TryParse(pairData[1], true, out goods))
                {
                    amount = 0;
                    Debug.LogError($"Parse error: card {id}, {pairData[1]} not recognized");
                    return false;
                }

                if (!Int32.TryParse(pairData[2], out amount))
                {
                    amount = 0;
                    Debug.LogError($"Parse error: card {id}, stat {pairData[0]}, value was not parsed");
                    return false;
                }

                return true;
            }

            bool TryApplyFunction(string[] pairData, Func<int, bool> runFunction)
            {
                if (Int32.TryParse(pairData[1], out int value))
                {
                    if (!runFunction(value))
                    {
                        Debug.LogError($"Parse error: card {id}, stat {pairData[0]}, failed to apply value");
                    }
                    
                }
                else
                {
                    Debug.LogError($"Parse error: card {id}, stat {pairData[0]}, value was not parsed");
                    return false;
                }

                return true;
            }
            
            bool TryApply(string[] pairData, Action<int> setProperty)
            {
                if (Int32.TryParse(pairData[1], out int value))
                {
                    setProperty(value);
                }
                else
                {
                    Debug.LogError($"Parse error: card {id}, stat {pairData[0]}, value was not parsed");
                    return false;
                }
                return true;
            }

            if (data.Trim() == String.Empty)
            {
                return false;
            }
            Debug.Log("parsing "+data);
            string[] pairs = data.Trim().Split(',');
            if (pairs.Length == 0)
                return false;
            foreach (var pair in pairs)
            {
                //example gold:100, trade:food:20
                string[] pairData = pair.Split(':');
                pairData[0] = pairData[0].Trim();
                if (pairData[0] == String.Empty)
                    return false;
                switch (pairData[0])
                {
                    case "gold":
                        if (!TryApply(pairData, value => Economics.Gold += value)) return false;
                        break;
                    case "r":
                    case "rep":
                        if (!TryApply(pairData, value => Economics.Rep += value)) return false;
                        break;
                    case "town":
                        if (!TryApply(pairData, value => Economics.Town += value)) return false;
                        break;
                    case "assasin":
                    case "sin":
                    case "assassin":
                        if (!TryApply(pairData, value => Economics.Assassin += value)) return false;
                        break;
                    case "piety":
                        if (!TryApply(pairData, value => Economics.Piety += value)) return false;
                        break;
                    case "fp":
                    case "Florence_Power":
                        if (!TryApply(pairData, Economics.ChangeFlorencePower)) return false;
                        break;
                    //modifiers
                    case "mod":
                        if (!Int32.TryParse(pairData[2], out int percent))
                        {
                            Debug.LogError(
                                $"Parse error: card {id}, stat {pairData[0]}:{pairData[1]}, value was not parsed");
                            return false;
                        }

                        if (Enum.TryParse(pairData[1], true, out Stat stat))
                        {
                            Economics.ChangeModifier(stat, percent);
                        }
                        else if (Enum.TryParse(pairData[1], true, out Formula formula))
                        {
                            Economics.ChangeModifier(formula, percent);
                        }
                        else
                        {
                            Debug.LogError($"Parse error: card {id}, {pairData[1]} not recognized");
                            return false;
                        }

                        break;
                    //prices
                    case "price":
                        if (!Enum.TryParse(pairData[1], true, out Goods type))
                        {
                            Debug.LogError($"Parse error: card {id}, type of goods {pairData[1]} not recognized.");
                            return false;
                        }
                        if (!Int32.TryParse(pairData[2], out int pricePercent))
                        {
                            Debug.LogError(
                                $"Parse error: card {id}, price {pairData[1]}, value was not parsed");
                            return false;
                        }
                        Economics.ChangePrice(type, pricePercent);
                        break;
                    //formulas
                    case "trade":
                        if (!TryParseFormula(pairData, out var goods, out var amount)) return false;
                        Economics.Trade(goods, amount);
                        break;
                    case "produce":
                        if (!TryParseFormula(pairData, out var goods2, out var amount2)) return false;
                        Economics.Produce(goods2, amount2);
                        break;
                    case "credit":
                        if (!TryApplyFunction(pairData, Economics.Credit)) return false;
                        break;
                    case "debit":
                        if (!TryApplyFunction(pairData, Economics.Debit)) return false;
                        break;
                    default:
                        Debug.LogError($"Parse error: card {id}, {pairData[0]} not recognized");
                        return false;
                }
            }
            return true;
        }
    }


}