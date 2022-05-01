using UnityEngine;

namespace Medici
{
    public class CardPending : CardButton
    {
        public override void Display(CardData card)
        {
            base.Display(card);
            if (!(card.type is null))
            {
                typeFrame.sprite = card.type.EventPending;
            }
            else if (!(gm.defaultType is null))
            {
                typeFrame.sprite = gm.defaultType.EventPending;
            }
        }
    }
}