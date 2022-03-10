namespace MRK.UI
{
    public class EditorSettings
    {
        public static float ViewRendererZoom
        {
            get; set;
        }

        static EditorSettings()
        {
            ViewRendererZoom = 0.3f;
        }
    }
}
