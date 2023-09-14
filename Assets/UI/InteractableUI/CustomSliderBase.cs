using UnityEngine;
using UnityEngine.UI;

namespace Project.Scripts.UI.InteractableUI
{
    [RequireComponent(typeof(Slider))]
    public abstract class CustomSliderBase : CustomUIInteractableBase<Slider>
    {
        public float Value
        {
            get
            {
                if (!myInteractable) OnEnable();
                return myInteractable.value;
            }
            set
            {
                if (!myInteractable) OnEnable();
                myInteractable.value = value;
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            myInteractable.onValueChanged.AddListener(SliderChanged);
        }

        protected virtual void OnDisable()
        {
            myInteractable.onValueChanged.RemoveListener(SliderChanged);
        }

        protected abstract void SliderChanged(float val);
    }
}
