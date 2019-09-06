using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Yuzu.Core;

namespace Yuzu.UI.Forms
{
    public partial class BookPropertiesForm : Form
    {
        public string Title
        {
            get => titleBox.Text;
            set => titleBox.Text = value;
        }

        public string ArtistName
        {
            get => artistBox.Text;
            set => artistBox.Text = value;
        }

        public string NotesDesignerName
        {
            get => notesDesignerBox.Text;
            set => notesDesignerBox.Text = value;
        }

        public BookPropertiesForm(ScoreBook book)
        {
            InitializeComponent();

            AcceptButton = buttonOK;
            CancelButton = buttonCancel;
            buttonOK.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            titleBox.Text = book.Title;
            artistBox.Text = book.ArtistName;
            notesDesignerBox.Text = book.NotesDesignerName;
        }
    }
}
