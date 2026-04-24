
namespace NovaLine.Script.UI.Container
{
    public class MainMenuContainerUI : NovaContainerUI
    {
        public static MainMenuContainerUI Instance { get; private set; }

        protected override void Awake()
        {
            Instance = this;
            base.Awake();
        }

        public void OpenMainMenu()
        {
            Open();
        }

        public void CloseMainMenu()
        {
            Close();
        }
    }
}
