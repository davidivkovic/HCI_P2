using P2.Primitives;

namespace P2.Views
{
    /// <summary>
    /// Interaction logic for EditStopDialog.xaml
    /// </summary>
    public partial class EditStopDialog : Component
    {
        public string Price { get; set; }

        public string StopName { get; set; }

        public EditStopDialog()
        {
            InitializeComponent();
        }
    }
}
