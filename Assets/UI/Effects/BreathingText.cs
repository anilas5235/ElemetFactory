using TMPro;
using UnityEngine;

namespace Project.Scripts.UI.Effects
{
    public class BreathingText : MonoBehaviour
    {
        private TextMeshProUGUI myText;

        [SerializeField] private float cycleTime = 2f;

        [Header("Parameters")]
        [SerializeField] private float minVisibility =.5f;
        [SerializeField] private float maxAddedFontSize = 5f, maxSubtractedFontSize = 5f;

        private float alphaValStep, fontSizeStep;

        private bool growing;

        private void Awake()
        {
            myText = GetComponent<TextMeshProUGUI>();
            alphaValStep = (1 - minVisibility)/((cycleTime/2) * (1/Time.fixedDeltaTime));
            fontSizeStep =   (maxAddedFontSize+ maxSubtractedFontSize)/((cycleTime/2) * (1/Time.fixedDeltaTime));
        }

        private void FixedUpdate()
        {
            if (myText.color.a >= 1 && growing) growing = false;
            if (myText.color.a <= minVisibility) growing = true;
            if (growing)
            {
                myText.fontSize += fontSizeStep;
                myText.color += new Color(0, 0, 0, alphaValStep);
            }
            else
            {
                myText.fontSize -= fontSizeStep;
                myText.color -= new Color(0, 0, 0, alphaValStep);
            }
        }
    }
}
