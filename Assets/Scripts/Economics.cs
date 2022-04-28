using System;
using UnityEngine;

namespace Medici
{
    public static class Economics
    {
        #region Variables&Init

        private static int gold = -1, rep = -1, assassin = -1, town = -1, piety = -1;
        private static float mod_rep = 1f, mod_assassin = 1f, mod_town = 1f, mod_piety = 1f;
        private static float mod_trade = 1f, mod_production = 1f, mod_credit = 1f, mod_debit = 1f;
        private static float florence_power = 1f;
        private static float[] prices = {1,1,1};
        
        public const float InRate = 0.08f;

        public static event Action Bankrupt;
        public static event Action<Stat, int> BasicStatChanged;

        public static void Init(int gold1, int rep1, int town1 = 0, int sin1 = 0, int piety1 = 0)
        {
            Gold = gold1;
            Rep = rep1;
            Town = town1;
            Assassin = sin1;
            Piety = piety1;
        }
        
        #endregion

        #region Getters&Setters
        
        /// <summary>
        /// One call to set them all
        /// </summary>
        public static void SetPrices(float luxury, float food, float weapons)
        {
            prices[0] = luxury;
            prices[1] = food;
            prices[2] = weapons;
        }

        public static float Price(Goods type)
        {
            return prices[(int) type];
        }

        public static bool ChangePrice(Goods type, int percentChange)
        {
            if ((int) type >= prices.Length)
                return false;
            ChangeModifier(ref prices[(int) type], percentChange);
            return true;
        }

        public static float Mod(Stat stat)
        {
            switch (stat)
            {
                case Stat.R:
                    return mod_rep;
                case Stat.Sin:
                    return mod_assassin;
                case Stat.Town:
                    return mod_town;
                case Stat.Piety:
                    return mod_piety;
                default:
                    throw new ArgumentOutOfRangeException(nameof(stat), stat, null);
            }
        }

        public static float Mod(Formula formula)
        {
            switch (formula)
            {
                case Formula.Trade:
                    return mod_trade;
                case Formula.Produce:
                    return mod_production;
                case Formula.Credit:
                    return mod_credit;
                case Formula.Debit:
                    return mod_debit;
                default:
                    throw new ArgumentOutOfRangeException(nameof(formula), formula, null);
            }
        }

        public static int Gold
        {
            get => gold;
            set
            {
                gold = value;
                if (gold < 0)
                {
                    Bankrupt?.Invoke();
                }
                BasicStatChanged?.Invoke(Stat.Gold, gold);
            }
        }

        public static int Rep
        {
            get => rep;
            set
            {
                if (ChangeBasicStat(ref rep, value, mod_rep))
                    BasicStatChanged?.Invoke(Stat.R, rep);
            }
        }
        public static int Town
        {
            get => town;
            set
            {
                if (ChangeBasicStat(ref town, value, mod_town))
                    BasicStatChanged?.Invoke(Stat.Town, town);
            }
        }

        public static int Assassin
        {
            get => assassin;
            set
            {
                if (ChangeBasicStat(ref assassin, value, mod_assassin))
                    BasicStatChanged?.Invoke(Stat.Sin, assassin);
            }
        }

        public static int Piety
        {
            get => piety;
            set
            {
                if (ChangeBasicStat(ref piety, value, mod_piety))
                    BasicStatChanged?.Invoke(Stat.Piety, piety);
            }
        }

        public static float FlorencePower
        {
            get => florence_power;
        }

        private static bool ChangeBasicStat(ref int value, int newValue, float mod)
        {
            if (value == newValue)
                return false;
            int dif = newValue - value;
            if (dif > 0)
                value += Mathf.RoundToInt(mod * dif);
            else if (dif < 0)
                value = newValue;
            if (value < 0)
                value = 0;
            return true;
        }

        private static void ChangeModifier(ref float mod, int percentChange)
        {
            if (percentChange == 0) return;
            mod += (percentChange / 100f);
            if (mod < 0.1f)
                mod = 0.1f;
        }

        public static void ChangeFlorencePower(int value)
        {
            ChangeModifier(ref florence_power, value);
        }
          
        /// <summary>
        /// Call this function to change any basic economics modifiers (e.g. +20 mod_Piety, -10 mod_Rep).
        /// </summary>
        /// <param name="modifier">Modifier stat</param>
        /// <param name="percentChange">Change to modifier in percents</param>
        public static void ChangeModifier(Stat modifier, int percentChange)
        {
            void ChangeMod(ref float mod)
            {
                ChangeModifier(ref mod, percentChange);
            }
            switch (modifier)
            {
                case Stat.R:
                    ChangeMod(ref mod_rep);
                    break;
                case Stat.Sin:
                    ChangeMod(ref mod_assassin);
                    break;
                case Stat.Town:
                    ChangeMod(ref mod_town);
                    break;
                case Stat.Piety:
                    ChangeMod(ref mod_piety);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(modifier), modifier, null);
            }
        }

        /// <summary>
        /// Call this function to change any formula economics modifiers (e.g. +20 mod_trade).
        /// </summary>
        /// <param name="modifier">Formula related to modifier</param>
        /// <param name="percentChange">Change to modifier in percents</param>
        public static void ChangeModifier(Formula modifier, int percentChange)
        {
            void ChangeMod(ref float mod)
            {
                ChangeModifier(ref mod, percentChange);
            }

            switch (modifier)
            {
                case Formula.Trade:
                    ChangeMod(ref mod_trade);
                    break;
                case Formula.Produce:
                    ChangeMod(ref mod_production);
                    break;
                case Formula.Credit:
                    ChangeMod(ref mod_credit);
                    break;
                case Formula.Debit:
                    ChangeMod(ref mod_debit);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(modifier), modifier, null);
            }
        }

        #endregion

        #region Formulas

        public static bool Credit(int value)
        {
            if (value <= 0)
            {
                Debug.LogWarning("Credit formula was used to reduce gold!");
                return false;
            }

            Gold += value + Mathf.RoundToInt(value * InRate * florence_power * mod_credit);
            return true;
        }

        public static bool Debit(int value)
        {
            if (value >= 0)
            {
                Debug.LogWarning("Debit formula was used to add gold!");
                return false;
            }
            Gold += Mathf.RoundToInt(value * mod_debit);
            return true;
        }

        public static bool Trade(Goods type, int value)
        {
            Gold += Mathf.RoundToInt(value * mod_trade * prices[(int) type] * florence_power);
            return true;
        }
        
        public static bool Produce(Goods type, int value)
        {
            Gold += Mathf.RoundToInt(value * mod_production * prices[(int) type] * florence_power);
            return true;
        }

        #endregion
    }
}