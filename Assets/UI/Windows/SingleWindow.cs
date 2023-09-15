using Project.Scripts.General;

namespace UI.Windows
{
    public class SingleWindow<T> : UIWindowHandler,IShouldForceAwake where T : UIWindowHandler
    {
        public static T Instance { get; private set; }

        private bool woken;

        protected virtual void Awake()
        {
            if(woken)return;
            if (!Instance) Instance = gameObject.GetComponent<T>();
            else if(Instance.GetInstanceID() != GetInstanceID())
            {
                Destroy(gameObject);
            }

            woken = true;
        }

        public virtual void OpenSingleWindow()
        {
            if(Instance.WindowEnabled) return;
            UIWindowMaster.Instance.OpenWindow(this);
        }

        public void ForceAwake()
        {
            if(woken)return;
            Awake();
        }
    }
}
