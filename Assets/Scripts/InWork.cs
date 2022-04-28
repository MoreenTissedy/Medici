using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Medici
{
    public class InWork
    {
        private class Delay
        {
            public int cooldown = 0;
            public CardData card;

            /// <summary><para>Initializes a new instance of the <see cref="T:System.Object" /> class.</para></summary>
            public Delay(int cooldown, CardData card)
            {
                this.cooldown = cooldown;
                this.card = card;
            }
        }
        
        private List<Delay> delayed;
        private Stack<CardData> pending;

        /// <summary><para>Initializes a new instance of the <see cref="T:System.Object" /> class.</para></summary>
        public InWork()
        {
            pending = new Stack<CardData>(5);
            delayed = new List<Delay>(10);
        }

        public void Add(CardData card)
        {
            int coolDown = Random.Range(card.cooldownMin, card.cooldownMax + 1);
            if (coolDown == 0)
                pending.Push(card);
            else
            {
                delayed.Add(new Delay(coolDown, card));
            }
        }

        public bool IsPending()
        {
            return pending.Count > 0;
        }

        public CardData Next()
        {
            return pending.Pop();
        }

        public CardData PeepNext()
        {
            return pending.Peek();
        }

        public void ForwardDay()
        {
            List<Delay> delete = new List<Delay>(3);
            foreach (var tuple in delayed)
            {
                tuple.cooldown--;
                if (tuple.cooldown == 0)
                {
                    delete.Add(tuple);
                    pending.Push(tuple.card);
                }
            }
            foreach (var delay in delete)
            {
                delayed.Remove(delay);
            }
        }
    }
}