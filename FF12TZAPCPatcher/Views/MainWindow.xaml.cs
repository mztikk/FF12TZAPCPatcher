using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Microsoft.Win32;

namespace FF12TZAPCPatcher.Views
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly FileSelector _fileSelector = new FileSelector();

        private readonly FileSystemWatcher _watcher = new FileSystemWatcher {Filter = "*.*"};

        public MainWindow()
        {
            ToolTipService.ShowDurationProperty.OverrideMetadata(typeof(DependencyObject),
                new FrameworkPropertyMetadata(int.MaxValue));

            try
            {
                Settings.Load();
            }
            catch
            {
            }

            this.InitializeComponent();
            this.borderPatchDisplay.BorderBrush = SystemColors.ActiveBorderBrush;
            this.SetupBindings();
        }

        private void SettingChanged(object o, Settings.SettingChangedEventArgs settingChangedEventArgs)
        {
            var name = settingChangedEventArgs.SettingName;
            if (name == nameof(Settings.AutoWatchPatchDir))
            {
                var b = (bool) settingChangedEventArgs.NewValue;
                if (b)
                {
                    this.Watch();
                }
                else
                {
                    this.StopWatching();
                }
            }
        }

        private void ClearPatchGrid()
        {
            this.spPatchDisplay.Children.Clear();
            this.patchDisplayBtnApply.Children.Clear();
            this.patchDisplayBtnRemove.Children.Clear();
        }

        private void SetPatchToGrid(IPatch patch)
        {
            this.ClearPatchGrid();

            this.spPatchDisplay.Children.Add(new Label {Content = $"Name: {patch.Name}"});
            var statusLabel = new Label {Content = $"Status: {PatchStatus.None}"};
            this.spPatchDisplay.Children.Add(statusLabel);
            if (!string.IsNullOrWhiteSpace(patch.Description))
            {
                var desc = new TextBlock
                {
                    Text = patch.Description,
                    Margin = new Thickness(5, 0, 0, 0),
                    TextWrapping = TextWrapping.Wrap
                };
                this.spPatchDisplay.Children.Add(desc);
            }

            const int maxButtonWidth = 250;
            var btnApply = new Button {Content = "Apply"};
            btnApply.Click += (sender, args) =>
            {
                var path = this._fileSelector.Path;
                if (!File.Exists(path))
                {
                    MessageBox.Show("No/Invalid path selected.");
                    return;
                }

                try
                {
                    if (!File.Exists(path + ".bk"))
                    {
                        File.Copy(path, path + ".bk");
                    }

                    using (var stream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite))
                    {
                        patch.Apply(stream);
                    }

                    this.RefreshStatus(path);

                    MessageBox.Show("Successfully patched!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            };
            //this.spPatchDisplay.Children.Add(btnApply);
            this.patchDisplayBtnApply.Children.Add(btnApply);

            var btnRemove = new Button {Content = "Remove"};
            btnRemove.Click += (sender, args) =>
            {
                var path = this._fileSelector.Path;
                if (!File.Exists(path))
                {
                    MessageBox.Show("No/Invalid path selected.");
                    return;
                }

                try
                {
                    using (var stream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite))
                    {
                        patch.Remove(stream);
                    }

                    this.RefreshStatus(path);

                    MessageBox.Show("Successfully restored original bytes!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            };

            //this.spPatchDisplay.Children.Add(btnRemove);
            this.patchDisplayBtnRemove.Children.Add(btnRemove);
            if (this._fileSelector.IsPathValid())
            {
                this.RefreshStatus(this._fileSelector.Path);
            }
        }

        private void SetupBindings()
        {
            void Path()
            {
                var b = new Binding("Path")
                {
                    Source = this._fileSelector,
                    Path = new PropertyPath("Path"),
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };
                BindingOperations.SetBinding(this.tbPath, TextBox.TextProperty, b);
            }

            Path();
        }

        private void RefreshStatus(string path)
        {
            if (this.lbPatches.SelectedIndex == -1)
            {
                return;
            }

            PatchStatus status;
            if (FileSelector.IsPathValid(path))
            {
                using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    status = ((IPatch) this.lbPatches.SelectedItem).GetStatus(stream);
                }
            }
            else
            {
                status = PatchStatus.None;
            }

            var tb = new TextBlock {Text = $"Status: {status}"};
            tb.TextEffects.Add(new TextEffect
            {
                Foreground = this.GetBrushForStatus(status),
                PositionStart = 8,
                PositionCount = status.ToString().Length
            });
            ((Label) this.spPatchDisplay.Children[1]).Content = tb;
            if (status == PatchStatus.Error)
            {
                this.patchDisplayBtnApply.Children[0].IsEnabled = false;
                this.patchDisplayBtnRemove.Children[0].IsEnabled = false;
                var errorStatus = new TextBlock
                {
                    Text = $"Neither patched nor original bytes match the file.",
                    Margin = new Thickness(5, 0, 0, 0)
                };
                this.spPatchDisplay.Children.Add(errorStatus);
            }
        }

        private Brush GetBrushForStatus(PatchStatus status)
        {
            switch (status)
            {
                case PatchStatus.None:
                    return Brushes.Black;
                case PatchStatus.Error:
                    return Brushes.Red;
                case PatchStatus.Applied:
                    return Brushes.Green;
                case PatchStatus.Normal:
                    return Brushes.Blue;
                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, null);
            }
        }

        private void BtnSelectPath_OnClick(object sender, RoutedEventArgs e)
        {
            if (this._fileSelector.SelectPath())
            {
                this.RefreshStatus(this._fileSelector.Path);
            }
        }

        private void LbPatches_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.borderPatchDisplay.BorderThickness =
                this.lbPatches.SelectedIndex == -1 ? new Thickness(0) : new Thickness(0, 1, 1, 1);

            // TODO: Add logic to retain selected item if index changed cuz of reloading patches from disk.
            if (e.AddedItems.Count <= 0)
            {
                return;
            }

            this.SetPatchToGrid(e.AddedItems[0] as IPatch);
        }

        private void TbPath_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            this.RefreshStatus(this._fileSelector.Path);

            if (!this._fileSelector.IsPathValid())
            {
                return;
            }

            Settings.LastUsedPath = this._fileSelector.Path;
        }

        private void RefreshPatches()
        {
            this.ClearPatchGrid();
            Patcher.LoadPatches();
        }

        private void BtnRefresh_OnClick(object sender, RoutedEventArgs e)
        {
            this.RefreshPatches();
        }

        private void Watch()
        {
            //if (!Directory.Exists(Patcher.FullPatchPath))
            //{
            //    return;
            //}

            this._watcher.Path = Patcher.FullPatchPath;

            this._watcher.EnableRaisingEvents = true;
            this._watcher.Changed += this.OnPatchDirChange;
            this._watcher.Created += this.OnPatchDirChange;
            this._watcher.Deleted += this.OnPatchDirChange;
        }

        private void StopWatching()
        {
            this._watcher.EnableRaisingEvents = false;
            this._watcher.Changed -= this.OnPatchDirChange;
            this._watcher.Created -= this.OnPatchDirChange;
            this._watcher.Deleted -= this.OnPatchDirChange;
        }

        private async void OnPatchDirChange(object sender, FileSystemEventArgs args)
        {
            if (args.ChangeType == WatcherChangeTypes.Created || args.ChangeType == WatcherChangeTypes.Changed)
            {
                await Patcher.WaitForFile(new FileInfo(args.FullPath));
            }

            this.Dispatcher.Invoke(this.RefreshPatches);
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            Settings.Save();
        }

        private void Options_OnClick(object sender, RoutedEventArgs e)
        {
            var ow = new OptionsWindow {WindowStartupLocation = WindowStartupLocation.CenterOwner, Owner = this};
            ow.ShowDialog();
        }

        private void Exit_OnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void AddPatch_OnClick(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog {Filter = "patch file (*.json, *.dif)|*.json;*.dif"};
            var res = dlg.ShowDialog(this);
            if (res.HasValue && res.Value)
            {
                var file = dlg.FileName;
                if (File.Exists(file))
                {
                    var newPath = Path.Combine(Patcher.FullPatchPath, Path.GetFileName(file));
                    if (File.Exists(newPath))
                    {
                        MessageBox.Show($"{newPath} already exists.");
                    }
                    else
                    {
                        File.Copy(file, newPath);
                    }
                }
            }
        }

        private void CreatePatch_OnClick(object sender, RoutedEventArgs e)
        {
            var crp = new CreatePatchWindow {Owner = this, WindowStartupLocation = WindowStartupLocation.CenterOwner};
            var b = new Binding("Path")
            {
                Source = this._fileSelector,
                Path = new PropertyPath("Path"),
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            BindingOperations.SetBinding(crp.tbFilePath, TextBox.TextProperty, b);
            crp.Show();
        }

        private void CheckForUpdate_OnClick(object sender, RoutedEventArgs e)
        {
            var updtw = new UpdateWindow {Owner = this, WindowStartupLocation = WindowStartupLocation.CenterOwner};
            updtw.ShowDialog();
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            this.tbPath.Text = Settings.LastUsedPath;
            Settings.SettingChanged += this.SettingChanged;

            Patcher.LoadPatches();

            if (Settings.AutoWatchPatchDir)
            {
                this.Watch();
            }

            if (Settings.CheckForUpdtsStart && Updater.IsOnlineDiff())
            {
                this.CheckForUpdate_OnClick(null, new RoutedEventArgs());
            }
        }

        private void About_OnClick(object sender, RoutedEventArgs e)
        {
            var abtw = new AboutWindow {Owner = this, WindowStartupLocation = WindowStartupLocation.CenterOwner};
            abtw.ShowDialog();
        }
    }
}