namespace VisualUninstaller
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    internal class FlickerFreeListBox : ListBox
    {
        public FlickerFreeListBox()
        {
            SetStyle(
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint,
                true);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            DrawMode = DrawMode.OwnerDrawFixed;
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (Items.Count > 0)
            {
                e.DrawBackground();
                e.Graphics.DrawString(
                    Items[e.Index].ToString(),
                    e.Font,
                    new SolidBrush(ForeColor),
                    new PointF(e.Bounds.X, e.Bounds.Y));
            }

            base.OnDrawItem(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var iRegion = new Region(e.ClipRectangle);
            e.Graphics.FillRegion(new SolidBrush(BackColor), iRegion);
            if (Items.Count > 0)
            {
                for (var i = 0; i < Items.Count; ++i)
                {
                    Rectangle irect = GetItemRectangle(i);
                    if (e.ClipRectangle.IntersectsWith(irect))
                    {
                        if ((SelectionMode == SelectionMode.One && SelectedIndex == i)
                            || (SelectionMode == SelectionMode.MultiSimple && SelectedIndices.Contains(i))
                            || (SelectionMode == SelectionMode.MultiExtended && SelectedIndices.Contains(i)))
                        {
                            OnDrawItem(new DrawItemEventArgs(
                                e.Graphics,
                                Font,
                                irect,
                                i,
                                DrawItemState.Selected,
                                ForeColor,
                                BackColor));
                        }
                        else
                        {
                            OnDrawItem(new DrawItemEventArgs(
                                e.Graphics,
                                Font,
                                irect,
                                i,
                                DrawItemState.Default,
                                ForeColor,
                                BackColor));
                        }

                        iRegion.Complement(irect);
                    }
                }
            }

            base.OnPaint(e);
        }
    }
}
