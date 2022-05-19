using Medici;
using UnityEditor;
using UnityEngine;

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
            if (GUILayout.Button("Save", GUILayout.Width(200)))
            {
                CardData card = target as CardData;
                if (!(card is null)) card.SaveData();
            }
            bool autoUpdate = EditorPrefs.HasKey("AutoUpdate") && EditorPrefs.GetBool("AutoUpdate");
            string hint = autoUpdate ? "Auto Update enabled" : "No Auto Update";
            GUILayout.Label(hint);
        }
    }
}