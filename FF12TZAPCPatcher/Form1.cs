using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using FF12TZAPCPatcher.Properties;

namespace FF12TZAPCPatcher
{
    public partial class Form1 : Form
    {
        private const long AddressToPatch = 0x677066;

        public Form1()
        {
            this.InitializeComponent();
            //AutoPause = new BytePatch("AutoPause", AddressToPatch, OriginalBytes, BytesToPatch);
            //if (!Directory.Exists(Patcher.PatchDirectory))
            //{
            //    Directory.CreateDirectory(Patcher.PatchDirectory);
            //}
            //using (var stream = new FileStream(Patcher.PatchDirectory + "\\AutoPause.json", FileMode.Create, FileAccess.ReadWrite))
            //{
            //    var ser = new DataContractJsonSerializer(typeof(BytePatch));
            //    ser.WriteObject(stream, AutoPause);
            //    stream.Flush();
            //}
            //Console.WriteLine(Patcher.FullPatchPath);
            
            
            this.LoadPatches();
            if (File.Exists(Settings.Default.ffxiitzaPath))
            {
                this.textBox1.Text = Settings.Default.ffxiitzaPath;
                this.TextBox1_Validating(this.textBox1, new CancelEventArgs());
            }
            //this.tableLayoutPanel2.RowStyles.Clear();

            //for (int i = 0; i < 20; i++)
            //{
            //    var btn = new Button();
            //    btn.Text = "Test" + i;
            //    btn.Dock = DockStyle.Fill;
            //    btn.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            //    btn.AutoSize = true;
            //    this.tableLayoutPanel2.Controls.Add(btn, 0, this.AddTableRow() - 1);
            //}
        }

        private void LoadPatches()
        {
            foreach (Control control in this.tableLayoutPanel2.Controls)
            {
                control.Dispose();
            }

            this.tableLayoutPanel2.RowStyles.Clear();
            foreach (var patch in Patcher.GetPatches())
            {
                var pe = new PatchElement(patch);
                pe.btnApply.Click += (sender, args) =>
                {
                    var path = this.textBox1.Text;
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
                            pe.Patch.Apply(stream);
                            pe.RefreshStatus(stream);
                        }
                        MessageBox.Show("Successfully patched!");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }

                };

                pe.btnRemove.Click += (sender, args) =>
                {
                    var path = this.textBox1.Text;
                    if (!File.Exists(path))
                    {
                        MessageBox.Show("No/Invalid path selected.");
                        return;
                    }

                    try
                    {
                        using (var stream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite))
                        {
                            pe.Patch.Remove(stream);
                            pe.RefreshStatus(stream);
                        }
                        MessageBox.Show("Successfully restored original bytes!");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                };

                pe.Dock = DockStyle.Fill;
                pe.AutoSize = true;
                pe.AutoSizeMode = AutoSizeMode.GrowAndShrink;
                pe.MaximumSize = new Size(300, 100);
                pe.Padding = new Padding(0, 10, 0, 10);
                this.tableLayoutPanel2.Controls.Add(pe, 0, this.AddTableRow());
            }
        }

        private int AddTableRow()
        {
            int i = this.tableLayoutPanel2.RowCount++;
            var style = new RowStyle(SizeType.AutoSize);
            this.tableLayoutPanel2.RowStyles.Add(style);
            return i;
        }
        private static byte[] OriginalBytes { get; } = {0xE8, 0xD5, 0xA6, 0x00, 0x00};

        private static byte[] BytesToPatch { get; } = {0x90, 0x90, 0x90, 0x90, 0x90};

        private void BtnPatch_Click(object sender, EventArgs e)
        {
            var path = this.textBox1.Text;
            if (!File.Exists(path))
            {
                MessageBox.Show("No/Invalid path selected.");
                return;
            }

            try
            {
                using (var stream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite))
                {
                    if (!File.Exists(path + ".bk"))
                    {
                        File.Copy(path, path + ".bk");
                    }

                    stream.Seek(AddressToPatch, SeekOrigin.Current);
                    stream.Write(BytesToPatch, 0, BytesToPatch.Length);
                    stream.Flush();
                }

                MessageBox.Show("Successfully patched!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void BtnRestore_Click(object sender, EventArgs e)
        {
            var path = this.textBox1.Text;
            if (!File.Exists(path))
            {
                MessageBox.Show("No/Invalid path selected.");
                return;
            }

            try
            {
                using (var stream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite))
                {
                    stream.Seek(AddressToPatch, SeekOrigin.Current);
                    stream.Write(OriginalBytes, 0, OriginalBytes.Length);
                    stream.Flush();
                }

                MessageBox.Show("Successfully restored original bytes!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private string GetPathToExe()
        {
            /*
            using (var dlg = new OpenFileDialog())
            {
                dlg.Filter = "FFXII_TZA.exe (FFXII_TZA.exe)|FFXII_TZA.exe";

                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    return dlg.FileName;
                }
            }
            */
            if (this.openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                return this.openFileDialog1.FileName;
            }

            return string.Empty;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            var path = this.GetPathToExe();
            if (!File.Exists(path))
            {
                return;
            }

            Settings.Default.ffxiitzaPath = path;
            Settings.Default.Save();

            this.textBox1.Text = path;
            this.RefreshAllStatus(path);
        }

        private void RefreshAllStatus(string path)
        {
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                foreach (PatchElement element in this.tableLayoutPanel2.Controls)
                {
                    element.RefreshStatus(stream);
                }
            }
        }

        private void TextBox1_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var path = this.textBox1.Text;
            if (!File.Exists(path))
            {
                return;
            }

            this.RefreshAllStatus(path);
        }

        private void btnReload_Click(object sender, EventArgs e)
        {
            this.LoadPatches();
        }
    }
}