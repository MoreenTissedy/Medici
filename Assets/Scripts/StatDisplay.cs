using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Medici
{
    public class StatDisplay : MonoBehaviour
    {
        [SerializeField] private Stat stat;
        [SerializeField] private Text value;
        [SerializeField] private Color highlight = Color.yellow;
        [SerializeField] private float highlightTime = 0.2f;
        
        private Image picture;
        private void Awake()
        {
            picture = GetComponent<Image>();
            Economics.BasicStatChanged += UpdateValue;
        }

        private void UpdateValue(Stat parameter, int i)
        {
            if (parameter != stat)
                return;
            value.text = i.ToString();
            picture.DOColor(highlight, highlightTime).SetLoops(2, LoopType.Yoyo);
        }

    }
}