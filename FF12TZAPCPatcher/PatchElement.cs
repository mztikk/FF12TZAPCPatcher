using System.IO;
using System.Windows.Forms;

namespace FF12TZAPCPatcher
{
    public partial class PatchElement : UserControl
    {
        private PatchStatus _status = PatchStatus.None;

        internal IPatch Patch;

        public PatchElement(IPatch patch)
        {
            this.Patch = patch;
            this.InitializeComponent();
            this.lbName.Text = "Name: " + patch.Name;
        }

        public void RefreshStatus(FileStream stream)
        {
            var st = this.Patch.GetStatus(stream);
            this.lbStatus.Text = "Status: " + st;
            this._status = st;
        }
    }
}