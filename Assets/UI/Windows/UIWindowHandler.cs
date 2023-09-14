using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace UI.Windows
{
    public class UIWindowHandler : MonoBehaviour
    {
        [SerializeField] protected UIWindowHandler parentWindow;
        
        [Header("Info"),SerializeField] private bool interactable = true;
        [SerializeField] private bool windowEnabled;
        
        private UIWindowMaster _myUIWindowMaster;
        
        public Action<bool> WindowInteractableUpdate;
        

        public bool Interactable
        {
            get => interactable;
            protected set
            {
                interactable = value;
                WindowInteractableUpdate?.Invoke(interactable);
            }
        }

        public bool WindowEnabled => windowEnabled;

        public enum StandardUIButtonFunctions
        {
            Esc,
            ChangeWindow,
            OpenWindow,
            Quit,
            ChangeScene,
        }
        public virtual void UIEsc()
        {
            if (parentWindow != null) parentWindow.ActivateWindow();
            CloseWindow();
        }

        public virtual void ChangeToWindow(UIWindowHandler windowMaster)
        {
            windowMaster.ActivateWindow();
            CloseWindow();
        }

        public virtual void OpenWindow(UIWindowHandler windowMaster)
        {
            windowMaster.ActivateWindow();
        }

        public virtual void CloseWindow()
        {
            _myUIWindowMaster.UpdateState();
            gameObject.SetActive(false);
        }

        public void QuitApplication() => Application.Quit();
        public void ChangeScene(int id) => SceneManager.LoadScene(id);

        public virtual void ActivateWindow()
        {
            gameObject.SetActive(true);
            _myUIWindowMaster ??= UIWindowMaster.Instance;
        }
    }
}
