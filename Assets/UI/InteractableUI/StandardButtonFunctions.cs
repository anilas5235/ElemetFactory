using System;
using Project.Scripts.UI.InteractableUI;
using UI.Windows;

namespace UI.InteractableUI
{
    public class StandardButtonFunctions : CustomButtonBase
    {
        public UIWindowHandler.StandardUIButtonFunctions myFunction;

        public UIWindowHandler windowHandler;
        public int sceneID;

        protected override void Interact()
        {
            base.Interact();
            switch (myFunction)
            {
                case UIWindowHandler.StandardUIButtonFunctions.Esc:
                    MyWindowHandler.UIEsc();
                    break;
                case UIWindowHandler.StandardUIButtonFunctions.ChangeWindow:
                    MyWindowHandler.ChangeToWindow(windowHandler);
                    break;
                case UIWindowHandler.StandardUIButtonFunctions.OpenWindow:
                    MyWindowHandler.OpenWindow(windowHandler);
                    break;
                case UIWindowHandler.StandardUIButtonFunctions.Quit:
                    MyWindowHandler.QuitApplication();
                    break;
                case UIWindowHandler.StandardUIButtonFunctions.ChangeScene:
                    MyWindowHandler.ChangeScene(sceneID);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
