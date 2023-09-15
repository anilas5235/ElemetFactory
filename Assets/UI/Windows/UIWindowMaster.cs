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
        [SerializeField] private bool deactivateCursorOnMenuClose,
        menuActive,
        enableSystem = true;
        
        public Action<bool> OnActiveUIChanged;
        private readonly Stack<UIWindowHandler> _currentlyActiveWindows = new Stack<UIWindowHandler>();

        private Coroutine _updateUIState;

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
            private set
            {
                if (value == menuActive) return;
                menuActive = value;
                OnActiveUIChanged?.Invoke(menuActive);
            }
        }

        private void Update()
        {
            if (Input.GetButtonDown("Cancel")) UIEsc();
        }
        
        public void UIEsc()
        {
            if (!MenuActive) OpenWindow(standardWindowToOpen);
            else ChangeToWindow(_currentlyActiveWindows.Peek().ParentWindow);
        }

        public void ChangeToWindow(UIWindowHandler windowHandler)
        {
            CloseWindow(_currentlyActiveWindows.Pop());
            OpenWindow(windowHandler);
            UpdateState();
        }

        public void OpenWindow(UIWindowHandler windowToOpen)
        {
            if(!enableSystem || !windowToOpen || _currentlyActiveWindows.Contains(windowToOpen)) return;
            windowToOpen.ActivateWindow();
            _currentlyActiveWindows.Push(windowToOpen);
          
            if(!MenuActive) MenuActive = true;
            if(deactivateCursorOnMenuClose)CursorManager.Instance.ActivateCursor();
        }
        
        public void CloseWindow(UIWindowHandler window)
        {
            if(!window) return;
            window.DeactivateWindow();
            UpdateState();
        }

        public void UpdateState()
        {
            _updateUIState ??= StartCoroutine(UpdateMenuState());
        }
        
        private IEnumerator UpdateMenuState()
        {
            yield return new WaitForEndOfFrame();
            if (!_currentlyActiveWindows.Any())
            {
                MenuActive = false;
                if (deactivateCursorOnMenuClose) CursorManager.Instance.DeActivateCursor();
            }
            _updateUIState = null;
        }
    }
}
