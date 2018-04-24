using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace FF12TZAPCPatcher.Views
{
    /// <summary>
    ///     Interaction logic for UpdateWindow.xaml
    /// </summary>
    public partial class UpdateWindow : Window
    {
        public UpdateWindow()
        {
            this.InitializeComponent();
            this.lbCurVers.Content = Updater.GetAssemblyVersion();
            this.lbGitVers.Content = Updater.GetGithubVersion();
            var diff = Updater.IsOnlineDiff();
            if (diff)
            {
                var r1 = new Run("Newer version available at: ");
                var r2 = new Run("https://github.com/mztikk/FF12TZAPCPatcher/releases");
                var hyperlink = new Hyperlink(r2)
                {
                    NavigateUri = new Uri("https://github.com/mztikk/FF12TZAPCPatcher/releases")
                };
                hyperlink.RequestNavigate += (sender, args) =>
                {
                    Process.Start("https://github.com/mztikk/FF12TZAPCPatcher/releases");
                    args.Handled = true;
                };
                var tb = new TextBlock {Margin = new Thickness(5, 0, 0, 0)};
                tb.Inlines.Add(r1);
                tb.Inlines.Add(hyperlink);
                this.spUpdateInfo.Children.Add(tb);
            }
            else
            {
                var tb = new TextBlock
                {
                    Text = "Your version is the latest one, no need to update.",
                    Margin = new Thickness(5, 0, 0, 0)
                };
                this.spUpdateInfo.Children.Add(tb);
            }
        }

        private void BtnOk_OnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}