using System.Collections;
using Project.Scripts.General;
using UnityEngine;

namespace Project.Scripts.UI.Effects
{
    public class ScreenFade : Singleton<ScreenFade>
    {
        private CanvasGroup blackScreen;

        [SerializeField] private float fadeDuration = 3f;
        public float FadeDuration
        {
            get => fadeDuration;
            private set => fadeDuration = value;
        }
        protected override void Awake()
        {
            base.Awake();
            blackScreen = GetComponent<CanvasGroup>();
        }

        private void Start()
        {
            StartFadeIn();
        }

        public void StartFadeIn()
        {
            gameObject.SetActive(true);
            StartCoroutine(FadeIn( FadeDuration));
        }
    
        public void StartFadeOut()
        {
            gameObject.SetActive(true);
            StartCoroutine(FadeOut( FadeDuration));
        }
        
        public void StartFadeOut(float delay)
        {
            gameObject.SetActive(true);
            StartCoroutine(FadeOut( FadeDuration,delay));
        }
    
        private IEnumerator FadeIn(float fadeDuration)
        {
            blackScreen.alpha = 1;

            float fadeStep = 1/( fadeDuration/Time.fixedDeltaTime);

            while (blackScreen.alpha >0)
            {
                blackScreen.alpha -= fadeStep;

                yield return new WaitForFixedUpdate();
            }

            blackScreen.alpha = 0;
        }
    
        private IEnumerator FadeOut( float fadeDuration, float delay =0)
        {
            yield return new WaitForSeconds(delay);
            blackScreen.alpha = 0;

            float fadeStep = 1/( fadeDuration/Time.fixedDeltaTime);

            while (blackScreen.alpha <1)
            {
                blackScreen.alpha += fadeStep;

                yield return new WaitForFixedUpdate();
            }

            blackScreen.alpha = 1;
        }
    }
}
