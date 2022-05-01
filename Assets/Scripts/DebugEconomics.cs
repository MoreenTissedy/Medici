using System;
using UnityEngine;
using UnityEngine.UI;

namespace Medici
{
    [RequireComponent(typeof(Text))]
    public class DebugEconomics : MonoBehaviour
    {
        private Text text;

        private void Awake()
        {
            text = GetComponent<Text>();
        }

        private void Update()
        {
            string mods = $"Florence Power: {Economics.FlorencePower}\n";
            mods += $"Mod_rep: {Economics.Mod(Stat.R)}\nMod_town: {Economics.Mod(Stat.Town)}\n" +
                    $"Mod_sin: {Economics.Mod(Stat.Sin)}\nMod_piety: {Economics.Mod(Stat.Piety)}\n";
            mods += $"Mod_trade: {Economics.Mod(Formula.Trade)}\nMod_produce: {Economics.Mod(Formula.Produce)}\n" +
                    $"Mod_credit:{Economics.Mod(Formula.Credit)}\nMod_debit: {Economics.Mod(Formula.Debit)}";
            mods += $"\nPrice Luxury: {Economics.Price(Goods.Luxury)}\n" +
                    $"Price Foods: {Economics.Price(Goods.Food)}\n" +
                    $"Price Weapons: {Economics.Price(Goods.Weapons)}";
            text.text = mods;
        }
    }
}