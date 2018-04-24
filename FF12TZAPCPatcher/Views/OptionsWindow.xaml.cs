using System.Windows;

namespace FF12TZAPCPatcher.Views
{
    /// <summary>
    ///     Interaction logic for OptionsWindow.xaml
    /// </summary>
    public partial class OptionsWindow : Window
    {
        public OptionsWindow()
        {
            this.InitializeComponent();
            this.cbAutowatch.IsChecked = Settings.AutoWatchPatchDir;
            this.cbCheckUpdtStart.IsChecked = Settings.CheckForUpdtsStart;
        }

        private void BtnSave_OnClick(object sender, RoutedEventArgs e)
        {
            var cbAutowatchIsChecked = this.cbAutowatch.IsChecked;
            if (cbAutowatchIsChecked != null)
            {
                Settings.AutoWatchPatchDir = cbAutowatchIsChecked.Value;
            }

            var isChecked = this.cbCheckUpdtStart.IsChecked;
            if (isChecked != null)
            {
                Settings.CheckForUpdtsStart = isChecked.Value;
            }

            this.DialogResult = true;
            this.Close();
        }

        private void BtnCancel_OnClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}