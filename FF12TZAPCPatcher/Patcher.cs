using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Windows.Forms;

namespace FF12TZAPCPatcher
{
    internal static class Patcher
    {
        public const string PatchDirectory = "patches";

        public static readonly string FullPatchPath = Path.Combine(Environment.CurrentDirectory, PatchDirectory);

        private static readonly List<IPatch> Patches = new List<IPatch>();

        private static void LoadPatches()
        {
            if (!Directory.Exists(FullPatchPath))
            {
                return;
            }

            Patches.Clear();
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
                        using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read))
                        {
                            var b = new byte[5];
                            //stream.Read(b, 0, b.Length);
                            var p = (BytePatch) ser.ReadObject(stream);
                            if (!Patches.Contains(p))
                            {
                                Patches.Add(p);
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
                        var difs = DifPatch.LoadFromFile(file);
                        var difPatch = new DifPatch(GetRealFileName(file), difs);
                        if (!Patches.Contains(difPatch))
                        {
                            Patches.Add(difPatch);
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
            return Directory.GetFiles(new FileInfo(path).Directory.FullName, Path.GetFileName(path))[0];
        }

        public static List<IPatch> GetPatches(bool forceReload = false)
        {
            if (forceReload || !Patches.Any())
            {
                LoadPatches();
            }

            return new List<IPatch>(Patches);
        }
    }
}