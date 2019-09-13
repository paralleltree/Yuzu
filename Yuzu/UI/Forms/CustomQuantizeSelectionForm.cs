using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Yuzu.UI.Forms
{
    public partial class CustomQuantizeSelectionForm : Form
    {
        private int BarTick { get; }

        public double QuantizeTick => Math.Max(BarTick / Math.Pow(2, lengthBox.SelectedIndex) / (int)divisionBox.Value, 1);

        public CustomQuantizeSelectionForm(int barTick)
        {
            InitializeComponent();

            AcceptButton = buttonOK;
            CancelButton = buttonCancel;
            buttonOK.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            BarTick = barTick;

            lengthBox.DropDownStyle = ComboBoxStyle.DropDownList;
            lengthBox.Items.AddRange(Enumerable.Range(0, 7).Select(p => ((int)Math.Pow(2, p)).ToString()).ToArray());
            lengthBox.SelectedIndex = 2;

            divisionBox.Minimum = 1;
            divisionBox.Maximum = 30;
            divisionBox.Value = 1;
        }
    }
}
