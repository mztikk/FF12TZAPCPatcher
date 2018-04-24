using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using Microsoft.Win32;

namespace FF12TZAPCPatcher
{
    public class FileSelector : INotifyPropertyChanged
    {
        private string _path;

        public string Path
        {
            get
            {
                return this._path;
            }
            set
            {
                this._path = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("Path"));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsPathValid()
        {
            return IsPathValid(this.Path);
        }

        public static bool IsPathValid(string path)
        {
            if (!File.Exists(path))
            {
                return false;
            }

            if (!string.Equals(System.IO.Path.GetFileName(path), "FFXII_TZA.exe", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return true;
        }

        public bool SelectPath()
        {
            try
            {
                var dlg = new OpenFileDialog {Filter = "FFXII_TZA.exe (FFXII_TZA.exe)|FFXII_TZA.exe"};
                var res = dlg.ShowDialog();
                if (res.HasValue && res.Value)
                {
                    var file = dlg.FileName;
                    if (File.Exists(file))
                    {
                        this.Path = dlg.FileName;
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }

            return false;
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            this.PropertyChanged?.Invoke(this, e);
        }
    }
}