using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace FF12TZAPCPatcher
{
    internal static class Patcher
    {
        public const string PatchDirectory = "patches";

        public static readonly string FullPatchPath = Path.Combine(Environment.CurrentDirectory, PatchDirectory);

        private static readonly ObservableCollection<IPatch> _patches = new ObservableCollection<IPatch>();

        public static readonly ReadOnlyObservableCollection<IPatch> Patches =
            new ReadOnlyObservableCollection<IPatch>(_patches);

        static Patcher()
        {
            if (!Directory.Exists(FullPatchPath))
            {
                Directory.CreateDirectory(FullPatchPath);
            }
        }

        public static void LoadPatches()
        {
            if (!Directory.Exists(FullPatchPath))
            {
                return;
            }

            _patches.Clear();
            var ser = new DataContractJsonSerializer(typeof(BytePatch));
            foreach (var file in Directory.EnumerateFiles(FullPatchPath))
            {
                if (!Path.HasExtension(file))
                {
                    continue;
                }

                var ext = Path.GetExtension(file);
                if (ext.Equals(".json", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            var p = (BytePatch) ser.ReadObject(stream);
                            if (!_patches.Contains(p))
                            {
                                _patches.Add(p);
                            }
                            else
                            {
                                MessageBox.Show($"A patch with the name \"{p.Name}\" already exists.");
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show($"Error on Deserializing: {e.Message}");
                    }
                }
                else if (ext.Equals(".dif", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        var (difs, desc) = DifPatch.LoadFromFile(file);
                        var difPatch = new DifPatch(GetRealFileName(file), difs, desc);
                        if (!_patches.Contains(difPatch))
                        {
                            _patches.Add(difPatch);
                        }
                        else
                        {
                            MessageBox.Show($"A patch with the name \"{difPatch.Name}\" already exists.");
                        }
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show($"Error on loading dif file: {e.Message}");
                    }
                }
            }
        }

        public static string GetRealFileName(string path)
        {
            return Path.GetFileNameWithoutExtension(Directory.GetFiles(new FileInfo(path).Directory.FullName, Path.GetFileName(path))[0]);
        }

        public static ReadOnlyObservableCollection<IPatch> GetPatches(bool forceReload = false)
        {
            if (forceReload || !Patches.Any())
            {
                LoadPatches();
            }

            return Patches;
        }

        public static async Task WaitForFile(FileInfo file)
        {
            var fileReady = false;
            while (!fileReady)
            {
                try
                {
                    using (file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                    {
                        fileReady = true;
                    }
                }
                catch (IOException)
                {
                    Debug.WriteLine($"Waiting for {file.FullName} to be available");
                }

                if (!fileReady)
                {
                    Thread.Sleep(50);
                }
            }
        }
    }
}