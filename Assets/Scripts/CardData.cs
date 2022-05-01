using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Medici
{
    /// <summary>
    /// This SO holds essential card data.
    /// They can be created through the import tool from a csv.
    /// TODO: save editor changes to csv in OnValidate
    /// </summary>
    [CreateAssetMenu(fileName = "New card", menuName = "Card", order = 0)]
    public class CardData : ScriptableObject
    {
        [Serializable]
        public class EventRate
        {
            public string id;
            public float rate;
        }
        
        public string id;
        public CardType type;
        public string eventName;
        [TextArea(4, 10)] public string textEvent;

        public int cooldownMin, cooldownMax;
        public bool repeat;

        [Header("YES option")] public string yesTextPrize;
        public string yesPrize;
        public EventRate[] yesEventChance;

        [Header("NO option")] public string noTextPrize;
        public string noPrize;
        public EventRate[] noEventChance;

        /// <summary>
        /// Get a random extra card to add to the inwork stack when choosing a card option.
        /// </summary>
        /// <param name="yes">True if the option is positive (right), false - if negative (left)</param>
        /// <returns></returns>
        public string GetExtraID(bool yes)
        {
            float rnd = Random.Range(0, 100);
            float cumulative = 0;
            if (yes)
                foreach (var eventRate in yesEventChance)
                {
                    if (rnd < cumulative + eventRate.rate)
                        return eventRate.id;
                    cumulative += eventRate.rate;
                }
            else
                foreach (var eventRate in noEventChance)
                {
                    if (rnd < cumulative + eventRate.rate)
                        return eventRate.id;
                    cumulative += eventRate.rate;
                }

            Debug.LogWarning($"Event rates are wrong for card {id}, please check.");
            return string.Empty;
        }

        /// <summary>
        /// Is there an extra card to be added to the inwork stack when choosing a card option?
        /// </summary>
        /// <param name="yes">True if the option is positive (right), false - if negative (left)</param>
        /// <returns></returns>
        public bool HasExtraCard(bool yes)
        {
            if (yes)
                return yesEventChance.Length > 0;
            return noEventChance.Length > 0;
        }

        /// <summary>
        /// Parse the given prize string of this card and apply changes to economics.
        /// TODO: parse to interface hint and do not apply changes.
        /// </summary>
        /// <param name="data">the string (yesPrize/noPrize)</param>
        /// <returns></returns>
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

                if (!int.TryParse(pairData[2], out amount))
                {
                    amount = 0;
                    Debug.LogError($"Parse error: card {id}, stat {pairData[0]}, value was not parsed");
                    return false;
                }

                return true;
            }

            bool TryApplyFunction(string[] pairData, Func<int, bool> runFunction)
            {
                if (int.TryParse(pairData[1], out var value))
                {
                    if (!runFunction(value))
                        Debug.LogError($"Parse error: card {id}, stat {pairData[0]}, failed to apply value");
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
                if (int.TryParse(pairData[1], out var value))
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

            if (data.Trim() == string.Empty) return false;
            Debug.Log("parsing " + data);
            var pairs = data.Trim().Split(',');
            if (pairs.Length == 0)
                return false;
            foreach (var pair in pairs)
            {
                //example gold:100, trade:food:20
                var pairData = pair.Split(':');
                pairData[0] = pairData[0].Trim();
                if (pairData[0] == string.Empty)
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
                        if (!int.TryParse(pairData[2], out var percent))
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

                        if (!int.TryParse(pairData[2], out var pricePercent))
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