using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace UI.Windows
{
    public class UIWindowHandler : MonoBehaviour
    {
        [SerializeField] protected UIWindowHandler parentWindow;
        public UIWindowHandler ParentWindow=>parentWindow;
        
        [Header("Info"),SerializeField] private bool interactable = true;
        [SerializeField] private bool windowEnabled;

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

        public virtual void DeactivateWindow()
        {
            gameObject.SetActive(false);
        }

        public virtual void ActivateWindow()
        {
            gameObject.SetActive(true);
        }

        public static void QuitApplication() => Application.Quit();
        public static void ChangeScene(int id) => SceneManager.LoadScene(id);
    }
}
