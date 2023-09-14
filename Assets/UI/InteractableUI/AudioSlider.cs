using UI.Windows;

namespace Project.Scripts.UI.InteractableUI
{
    public class AudioSlider : CustomSliderBase
    {
        private AudioOptionsUIWindow _audioWindow;
        protected override void OnEnable()
        {
            base.OnEnable();
            _audioWindow ??=  GetComponentInParent<AudioOptionsUIWindow>();
        }

        protected override void SliderChanged(float val)
        {
            if(!gameObject.activeInHierarchy) return;
            _audioWindow.UpdateSoundOptions();
        }
    }
}
