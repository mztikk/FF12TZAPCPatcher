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

        private static readonly List<BytePatch> Patches = new List<BytePatch>();

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
                if (ext == ".json")
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
            }
        }

        public static List<BytePatch> GetPatches(bool forceReload = false)
        {
            if (forceReload || !Patches.Any())
            {
                LoadPatches();
            }

            return new List<BytePatch>(Patches);
        }
    }
}