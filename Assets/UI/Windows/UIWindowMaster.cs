using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Project.Scripts.General;
using UnityEngine;

namespace UI.Windows
{
    public class UIWindowMaster : Singleton<UIWindowMaster>
    {
        [SerializeField] private UIWindowHandler standardWindowToOpen;
        [SerializeField] private bool deactivateCursorOnMenuClose;
        [SerializeField] private bool menuActive;
        [SerializeField] private bool enableSystem = true;
        
        public Action<bool> OnActiveUIChanged;
        public readonly Stack<UIWindowHandler> CurrentlyActiveWindows = new Stack<UIWindowHandler>();

        private void Start()
        {
            AudioOptionsUIWindow audioOptions = FindObjectOfType<AudioOptionsUIWindow>(true);
            if (audioOptions)
            {
                audioOptions.LoadFromSaveText();
            }
        }

        public bool MenuActive
        {
            get => menuActive;
            set
            {
                if (!value == menuActive)
                {
                    menuActive = value;
                    OnActiveUIChanged?.Invoke(menuActive);
                }
            }
        }


        private void Update()
        {
            if (Input.GetButtonDown("Cancel"))
            {
                if (!MenuActive) OpenWindow(); 
                else CurrentlyActiveWindows.Pop().UIEsc();
            }
        }

        public void OpenWindow(UIWindowHandler windowToOpen = null)
        {
            if(!enableSystem) return;
            if (windowToOpen == null) windowToOpen = standardWindowToOpen;
            windowToOpen.ActivateWindow();
            CurrentlyActiveWindows.Push(windowToOpen);
          
            MenuActive = true;
            if(deactivateCursorOnMenuClose)CursorManager.Instance.ActivateCursor();
        }

        public void UpdateState()
        {
            StartCoroutine(UpdateMenuState());
        }
        
        private IEnumerator UpdateMenuState()
        {
            yield return new WaitForEndOfFrame();
            if (CurrentlyActiveWindows.Any()) yield break;
            
            MenuActive = false;
            if(deactivateCursorOnMenuClose) CursorManager.Instance.DeActivateCursor();
        }
    }
}
