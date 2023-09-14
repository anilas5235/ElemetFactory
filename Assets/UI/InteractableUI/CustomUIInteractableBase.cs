using UI.Windows;
using UnityEngine;

namespace Project.Scripts.UI.InteractableUI
{
    public abstract class CustomUIInteractableBase<T> : MonoBehaviour
    {
        protected T myInteractable;
        protected UIWindowHandler MyWindowHandler;

        protected virtual void OnEnable()
        {
            myInteractable ??= GetComponent<T>();
            MyWindowHandler ??= GetComponentInParent<UIWindowHandler>();
        }

        protected virtual void Interact(){}
    }
}
