using Medici;
using UnityEditor;

namespace Editor
{
    [CustomEditor(typeof(CardData))]
    public class CardDataEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            //cooldown in one row
            //event rate arrays all visible
            base.OnInspectorGUI();
            //save cards
        }
    }
}