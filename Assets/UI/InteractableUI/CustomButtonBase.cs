using UnityEngine;
using UnityEngine.UI;

namespace Project.Scripts.UI.InteractableUI
{
    [RequireComponent(typeof(Button))]
    public abstract class CustomButtonBase : CustomUIInteractableBase<Button>
    {
        protected override void OnEnable()
        {
            base.OnEnable();
            myInteractable.onClick.AddListener(Interact);
            if (MyWindowHandler) MyWindowHandler.WindowInteractableUpdate += WindowIntractabilityChanged;
        }

        protected virtual void OnDisable()
        {
            myInteractable.onClick.RemoveListener(Interact);
        }

        protected virtual void WindowIntractabilityChanged(bool newVal) => myInteractable.interactable = newVal;

        protected override void Interact()
        {
            //AudioManager.Instance.ButtonClicked();
        }
    }
}
