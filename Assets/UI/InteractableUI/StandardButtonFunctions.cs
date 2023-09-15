using System;
using Project.Scripts.UI.InteractableUI;
using UI.InteractableUI.Attribute;
using UI.Windows;

namespace UI.InteractableUI
{
    public enum StandardUIButtonFunctions
    {
        Esc,
        ChangeWindow,
        OpenWindow,
        Quit,
        ChangeScene,
    }
    public class StandardButtonFunctions : CustomButtonBase
    {
        public StandardUIButtonFunctions myFunction;

        public UIWindowHandler windowHandler;
        [SceneIndex] public int sceneID;

        protected override void Interact()
        {
            base.Interact();
            switch (myFunction)
            {
                case StandardUIButtonFunctions.Esc:
                    UIWindowMaster.Instance.UIEsc();
                    break;
                case StandardUIButtonFunctions.ChangeWindow:
                    UIWindowMaster.Instance.ChangeToWindow(windowHandler);
                    break;
                case StandardUIButtonFunctions.OpenWindow:
                    UIWindowMaster.Instance.OpenWindow(windowHandler);
                    break;
                case StandardUIButtonFunctions.Quit:
                    UIWindowHandler.QuitApplication();
                    break;
                case StandardUIButtonFunctions.ChangeScene:
                    UIWindowHandler.ChangeScene(sceneID);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
