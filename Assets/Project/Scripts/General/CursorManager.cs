using System;
using UnityEngine;

namespace Project.Scripts.General
{
    public class CursorManager : Singleton<CursorManager>
    {
        public bool IsCursorActive { get; private set; }
        private Texture2D[] cursorCollection;

        private Cursors currentCursor;

        public enum Cursors
        {
            OpenHand,
            ClosedHand,
        }
        protected override void Awake()
        {
            base.Awake();
            cursorCollection = new[]
            {
                Resources.Load<Texture2D>("ArtWork/Cursor/open"),
                Resources.Load<Texture2D>("ArtWork/Cursor/Close")
            };
            ActivateCursor();
        }

        public void ChangeCursor(Cursors newCursor)
        {
            if (currentCursor == newCursor)return;
            currentCursor = newCursor;
            switch (currentCursor)
            {
                case Cursors.OpenHand: Cursor.SetCursor(cursorCollection[0],new Vector2(11,14), CursorMode.Auto);
                    break;
                case Cursors.ClosedHand: Cursor.SetCursor(cursorCollection[1],new Vector2(11,14), CursorMode.Auto);
                    break;
            }
        }

        public void ActivateCursor()
        {
            if (IsCursorActive) return;
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
            IsCursorActive = true;
        }
        
        public void DeActivateCursor()
        {
            if (!IsCursorActive) return;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            IsCursorActive = false;
        }
    }
}
