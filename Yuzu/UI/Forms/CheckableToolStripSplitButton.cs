using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace Yuzu.UI.Forms
{
    // https://arstechnica.com/civis/viewtopic.php?f=20&t=311656
    public class CheckableToolStripSplitButton : ToolStripSplitButton
    {
        private bool _Checked = false;
        private VisualStyleRenderer renderer = null;
        private readonly VisualStyleElement element = VisualStyleElement.ToolBar.Button.Checked;

        public CheckableToolStripSplitButton()
        {
            if (Application.RenderWithVisualStyles && VisualStyleRenderer.IsElementDefined(element))
            {
                renderer = new VisualStyleRenderer(element);
            }
        }

        public bool Checked
        {
            get
            {
                return _Checked;
            }
            set
            {
                _Checked = value;
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (_Checked)
            {
                if (renderer != null)
                {
                    Rectangle cr = base.ContentRectangle;
                    Image img = Image;

                    // Compute the center of the item's ContentRectangle.
                    int centerY = (cr.Height - img.Height) / 2;
                    var fullRect = new Rectangle(0, 0, Width, Height);

                    var imageRect = new Rectangle(
                        base.ContentRectangle.Left,
                        centerY,
                        base.Image.Width,
                        base.Image.Height);

                    var textRect = new Rectangle(
                        imageRect.Width,
                        base.ContentRectangle.Top,
                        base.ContentRectangle.Width - (imageRect.Width + 10),
                        base.ContentRectangle.Height);

                    renderer.DrawBackground(e.Graphics, fullRect);
                    base.OnPaint(e);
                }
                else
                {
                    e.Graphics.FillRectangle(SystemBrushes.Control, 0, 0, Width, Height);
                    e.Graphics.DrawRectangle(new Pen(SystemColors.Highlight), 0, 0, Width - 1, Height - 1);
                    base.OnPaint(e);
                }
            }
            else
            {
                base.OnPaint(e);
            }
        }
    }
}
