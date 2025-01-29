using Avalonia.Controls;

namespace SignalLabelingApp.Views
{
    public abstract partial class EditorBase : UserControl
    {
        public EditorBase()
        {
            InitializeComponent();
        }

        public abstract void LoadFromFile(string filePath);
    }
}