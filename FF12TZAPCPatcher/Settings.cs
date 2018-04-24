using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Xml.Linq;

namespace FF12TZAPCPatcher
{
    internal static class Settings
    {
        internal static EventHandler<SettingChangedEventArgs> SettingChanged;

        public static EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;

        private static string _lastUsedPath = string.Empty;

        private static bool _autoWatchPatchDir = true;

        private static bool _checkForUpdtsStart;

        internal static string LastUsedPath
        {
            get
            {
                return _lastUsedPath;
            }
            set
            {
                var oldVal = _lastUsedPath;
                _lastUsedPath = value;
                OnStaticPropertyChanged(new PropertyChangedEventArgs(nameof(LastUsedPath)));
                OnSettingChanged(new SettingChangedEventArgs(nameof(LastUsedPath), oldVal, value));
            }
        }

        internal static bool AutoWatchPatchDir
        {
            get
            {
                return _autoWatchPatchDir;
            }
            set
            {
                var oldVal = _autoWatchPatchDir;
                _autoWatchPatchDir = value;
                OnStaticPropertyChanged(new PropertyChangedEventArgs(nameof(AutoWatchPatchDir)));
                OnSettingChanged(new SettingChangedEventArgs(nameof(AutoWatchPatchDir), oldVal, value));
            }
        }

        internal static bool CheckForUpdtsStart
        {
            get
            {
                return _checkForUpdtsStart;
            }
            set
            {
                var oldVal = _checkForUpdtsStart;
                _checkForUpdtsStart = value;
                OnStaticPropertyChanged(new PropertyChangedEventArgs(nameof(CheckForUpdtsStart)));
                OnSettingChanged(new SettingChangedEventArgs(nameof(CheckForUpdtsStart), oldVal, value));
            }
        }

        internal static void Save(string path = "FF12TZAPCPatcher.xml")
        {
            var xml = new XDocument();
            var e = new XElement("Config");
            foreach (var property in typeof(Settings).GetProperties(BindingFlags.Static | BindingFlags.NonPublic))
            {
                object v;
                v = property.GetValue(null);

                var attr = new XAttribute(property.Name, v);
                e.Add(attr);
            }

            xml.Add(e);
            using (var stream = new FileStream(path, FileMode.Create))
            {
                xml.Save(stream);
            }
        }

        internal static void Load(string path = "FF12TZAPCPatcher.xml")
        {
            var xml = XDocument.Load(path);
            foreach (var e in xml.Elements())
            {
                foreach (var a in e.Attributes())
                {
                    try
                    {
                        var f = typeof(Settings).GetProperty(a.Name.LocalName,
                            BindingFlags.Static | BindingFlags.NonPublic);
                        if (f == null)
                        {
                            continue;
                        }

                        f.SetValue(null, Convert.ChangeType(a.Value, f.PropertyType));
                    }
                    catch
                    {
                    }
                }
            }
        }

        private static void OnSettingChanged(SettingChangedEventArgs e)
        {
            SettingChanged?.Invoke(null, e);
        }

        public static void OnStaticPropertyChanged(PropertyChangedEventArgs e)
        {
            StaticPropertyChanged?.Invoke(null, e);
        }

        public class SettingChangedEventArgs : EventArgs
        {
            public object NewValue;

            public object OldValue;

            public string SettingName;

            public SettingChangedEventArgs(string name, object oldValue, object newValue)
            {
                this.SettingName = name;
                this.OldValue = oldValue;
                this.NewValue = newValue;
            }
        }
    }
}