using System;
using System.IO;
using System.Windows.Forms;

namespace FF12TZAPCPatcher
{
    public partial class Form1 : Form
    {
        private const long AddressToPatch = 0x677066;

        public Form1()
        {
            this.InitializeComponent();
        }

        private static byte[] OriginalBytes { get; } = {0xE8, 0xD5, 0xA6, 0x00, 0x00};

        private static byte[] BytesToPatch { get; } = {0x90, 0x90, 0x90, 0x90, 0x90};

        private void btnPatch_Click(object sender, EventArgs e)
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

        private void btnRestore_Click(object sender, EventArgs e)
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

        private string GetFF12TZAPath()
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Filter = "FFXII_TZA.exe (FFXII_TZA.exe)|FFXII_TZA.exe";

                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    return dlg.FileName;
                }
            }

            return string.Empty;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.textBox1.Text = this.GetFF12TZAPath();
        }
    }
}