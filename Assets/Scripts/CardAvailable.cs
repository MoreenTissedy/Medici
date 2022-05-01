using UnityEngine;

namespace Medici
{
    public class CardAvailable : CardButton
    {
        public override void Display(CardData card)
        {
            base.Display(card);
            if (!(card.type is null))
            {
                typeFrame.sprite = card.type.EventAvailable;
            }
            else if (!(gm.defaultType is null))
            {
                typeFrame.sprite = gm.defaultType.EventAvailable;
            }
        }
    }
}