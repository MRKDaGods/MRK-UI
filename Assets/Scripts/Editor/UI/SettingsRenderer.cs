namespace MRK.UI
{
    public abstract class SettingsRenderer
    {
        protected UIEditor UIEditor
        {
            get; private set;
        }

        public SettingsRenderer(UIEditor editor)
        {
            UIEditor = editor;
        }

        public virtual void OnEnable()
        {
        }

        public abstract void Draw();
    }
}
