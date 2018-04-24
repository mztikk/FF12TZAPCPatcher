using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;

namespace FF12TZAPCPatcher.Views
{
    /// <summary>
    ///     Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            this.InitializeComponent();
            this.lbVersion.Content = Updater.GetAssemblyVersion();
        }

        private void BtnOk_OnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start("https://github.com/mztikk/FF12TZAPCPatcher");
            e.Handled = true;
        }
    }
}