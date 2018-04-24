using System;
using System.Globalization;
using System.IO;
using System.Windows;

namespace FF12TZAPCPatcher.Views
{
    /// <summary>
    ///     Interaction logic for CreatePatchWindow.xaml
    /// </summary>
    public partial class CreatePatchWindow : Window
    {
        public const string Signature = "Created by FF12TZAPCPatcher";

        public CreatePatchWindow()
        {
            this.InitializeComponent();
        }

        private void BtnCreate_OnClick(object sender, RoutedEventArgs e)
        {
            var name = this.tbName.Text;
            foreach (var c in Path.GetInvalidFileNameChars())
            {
                if (name.IndexOf(c) != -1)
                {
                    MessageBox.Show($"Name contains illegal file char {c}");
                    return;
                }
            }

            foreach (var c in Path.GetInvalidPathChars())
            {
                if (name.IndexOf(c) != -1)
                {
                    MessageBox.Show($"Name contains illegal path char {c}");
                    return;
                }
            }

            var path = Path.Combine(Patcher.FullPatchPath, name);
            if (!Path.HasExtension(path))
            {
                path += ".dif";
            }

            if (File.Exists(path))
            {
                MessageBox.Show($"File \"{path}\" already exists.");
                return;
            }

            var offsetsInput = this.tbOffsets.Text.Split(new[] {',', ' '}, StringSplitOptions.RemoveEmptyEntries);
            if (offsetsInput.Length < 1)
            {
                MessageBox.Show("Offsets can't be empty.");
                return;
            }

            var patchedBytesInput =
                this.tbPatchedBytes.Text.Split(new[] {',', ' '}, StringSplitOptions.RemoveEmptyEntries);
            if (patchedBytesInput.Length < 1)
            {
                MessageBox.Show("Patched Bytes can't be empty.");
                return;
            }

            string[] originalBytesInput;
            var readFromFile = this.cbOgFromFile.IsChecked.HasValue && this.cbOgFromFile.IsChecked.Value;
            if (readFromFile)
            {
                originalBytesInput = new string[patchedBytesInput.Length];
            }
            else
            {
                originalBytesInput =
                    this.tbOriginalBytes.Text.Split(new[] {',', ' '}, StringSplitOptions.RemoveEmptyEntries);
            }

            if (originalBytesInput.Length < 1)
            {
                MessageBox.Show("Original Bytes can't be empty.");
                return;
            }

            if (patchedBytesInput.Length != originalBytesInput.Length)
            {
                MessageBox.Show(
                    $"Patched Bytes Length({patchedBytesInput.Length}) and Original Bytes Length({originalBytesInput.Length}) has to be equal.");
                return;
            }

            if (offsetsInput.Length > 1 && offsetsInput.Length != patchedBytesInput.Length)
            {
                MessageBox.Show(
                    $"Offsets Length({offsetsInput.Length}) and Patched Bytes Length({patchedBytesInput.Length}) has to be equal.");
                return;
            }

            long[] offsets;
            if (offsetsInput.Length == 1)
            {
                offsets = new long[patchedBytesInput.Length];
                if (!long.TryParse(offsetsInput[0], NumberStyles.HexNumber, CultureInfo.InvariantCulture,
                    out var parsed))
                {
                    MessageBox.Show($"Failed to parse {offsetsInput[0]}");
                    return;
                }

                offsets[0] = parsed;
                for (var i = 1; i < offsets.Length; i++)
                {
                    offsets[i] = offsets[0] + i;
                }
            }
            else
            {
                offsets = new long[offsetsInput.Length];
                for (var i = 0; i < offsets.Length; i++)
                {
                    if (!long.TryParse(offsetsInput[i], NumberStyles.HexNumber, CultureInfo.InvariantCulture,
                        out var parsed))
                    {
                        MessageBox.Show($"Failed to parse offset: {offsetsInput[i]}");
                        return;
                    }

                    offsets[i] = parsed;
                }
            }

            var patchedBytes = new byte[patchedBytesInput.Length];
            for (var i = 0; i < patchedBytes.Length; i++)
            {
                if (!byte.TryParse(patchedBytesInput[i], NumberStyles.HexNumber, CultureInfo.InvariantCulture,
                    out var parsed))
                {
                    MessageBox.Show($"Failed to parse patched byte: {patchedBytesInput[i]}");
                    return;
                }

                patchedBytes[i] = parsed;
            }

            byte[] originalBytes;
            if (readFromFile)
            {
                var filePath = this.tbFilePath.Text;

                if (!FileSelector.IsPathValid(filePath))
                {
                    MessageBox.Show($"Path \"{filePath}\" is not valid.");
                    return;
                }

                originalBytes = new byte[patchedBytes.Length];
                try
                {
                    using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        for (var i = 0; i < offsets.Length; i++)
                        {
                            stream.Position = offsets[i];
                            originalBytes[i] = (byte) stream.ReadByte();
                        }
                    }
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message);
                    return;
                }
            }
            else
            {
                originalBytes = new byte[originalBytesInput.Length];
                for (var i = 0; i < originalBytes.Length; i++)
                {
                    if (!byte.TryParse(originalBytesInput[i], NumberStyles.HexNumber, CultureInfo.InvariantCulture,
                        out var parsed))
                    {
                        MessageBox.Show($"Failed to parse patched byte: {originalBytesInput[i]}");
                        return;
                    }

                    originalBytes[i] = parsed;
                }
            }

            using (var stream = new FileStream(path, FileMode.CreateNew, FileAccess.ReadWrite))
            {
                using (var writer = new StreamWriter(stream))
                {
                    writer.WriteLine(Signature);
                    writer.WriteLine();
                    for (var i = 0; i < offsets.Length; i++)
                    {
                        writer.WriteLine($"{offsets[i]:X16}: {originalBytes[i]:X2} {patchedBytes[i]:X2}");
                    }

                    writer.WriteLine(this.tbDesc.Text);
                    writer.Flush();
                }
            }

            MessageBox.Show($"File \"{path}\" successfully created.");
        }

        private void CbOgFromFile_OnChecked(object sender, RoutedEventArgs e)
        {
            this.tbOriginalBytes.IsEnabled = false;
            this.tbFilePath.IsEnabled = true;
        }

        private void CbOgFromFile_OnUnchecked(object sender, RoutedEventArgs e)
        {
            this.tbOriginalBytes.IsEnabled = true;
            this.tbFilePath.IsEnabled = false;
        }
    }
}