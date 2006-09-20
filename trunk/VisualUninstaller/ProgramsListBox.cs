using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using BlackFox.Win32.UninstallInformations;
using System.Text.RegularExpressions;

namespace VisualUninstaller
{
    class ProgramsListBox : ListBox
    {
        static readonly Color SELECTED_BG_COLOR = Color.FromArgb(61, 128, 223);
        static readonly Color SELECTED_TEXT_COLOR = Color.White;
        static readonly Color BG_COLOR = Color.White;
        static readonly Color BG_COLOR_ALT = Color.FromArgb(237, 243, 254);
        static readonly Color TEXT_COLOR = Color.Black;

        List<Information> m_infos;
        List<Information> m_displayedInfos;
        Dictionary<Information, Icon> m_iconCache;

        public ProgramsListBox() 
            : base()
        {
            SetStyle(ControlStyles.ResizeRedraw, true);

            // All informations in registry
            m_infos = Informations.GetInformations();

            //Information displayed (Filter applied)
            m_displayedInfos = new List<Information>();
            m_displayedInfos.AddRange(m_infos);

            // Building icon cache
            m_iconCache = new Dictionary<Information, Icon>();
            BuildIconCache();

            DrawItem += new System.Windows.Forms.DrawItemEventHandler(__DrawItem);
        }

        const int WM_ERASEBKGND = 0x0014;

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_ERASEBKGND)
            {
                IntPtr hdc = m.WParam;
                Graphics g = Graphics.FromHdcInternal(hdc);

                int itemsHeight = Items.Count * this.ItemHeight;
                int toDraw = ClientSize.Height - itemsHeight;

                if (toDraw > 0)
                {
                    g.FillRectangle(new SolidBrush(this.BackColor),
                        ClientRectangle.Left,
                        ClientRectangle.Top + itemsHeight,
                        ClientSize.Width,
                        toDraw);
                }
                m.Result = (IntPtr)1;
                return;
            }
            base.WndProc(ref m);
        }

        private void __DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index >= 0)
            {
                Bitmap b = new Bitmap(e.Bounds.Width, e.Bounds.Width);
                Graphics g = Graphics.FromImage(b);
                Information info = (Information)Items[e.Index];

                /*
                 * On détermine la position du texte.
                 */
                int y = (int)(e.Bounds.Height / 2);
                y -= (int)(e.Graphics.MeasureString(info.DisplayName, e.Font).Height / 2);
                PointF textPoint = new PointF(35, y);

                /*
                 * On détermine les couleurs du fond et du texte
                 */
                Color textColor;
                Color bgColor;
                if ((e.State & DrawItemState.Selected) != 0)
                {
                    bgColor = SELECTED_BG_COLOR;
                    textColor = SELECTED_TEXT_COLOR;
                }
                else
                {
                    textColor = TEXT_COLOR;
                    if (e.Index % 2 == 0)
                    {
                        bgColor = BG_COLOR_ALT;
                    }
                    else
                    {
                        bgColor = BG_COLOR;
                    }
                }

                /*
                 * On détermine l'icone
                 */
                Icon icon;
                if (m_iconCache[info] != null)
                {
                    icon = m_iconCache[info];
                }
                else
                {
                    icon = Properties.Resources.Windows_Installer;
                }


                /*
                 * Affichage
                 */

                g.FillRectangle(new SolidBrush(bgColor), 0, 0, e.Bounds.Width, e.Bounds.Height);
                g.DrawString(info.DisplayName, this.Font, new SolidBrush(textColor), textPoint);
                g.DrawIcon(icon, new Rectangle(1, 1, 32, 32));
                e.Graphics.DrawImageUnscaled(b, e.Bounds.X, e.Bounds.Y);
            }
        }

        void BuildIconCache()
        {
            foreach (Information info in m_infos)
            {
                m_iconCache[info] = info.Icon;
            }
        }

        public void UninstallSelected()
        {
            Information selected = (Information)SelectedItem;
            if (selected != null)
            {
                selected.Uninstall();
            }
        }

        public void RemoveSelectedFromRegistry()
        {
            Information selected = (Information)SelectedItem;
            if (selected != null)
            {
                selected.RemoveFromRegistry();
            }
        }

        public void SetFilter(Regex regexFilter)
        {
            Information selectedInfo = (Information)SelectedItem;

            BeginUpdate();
            try
            {

                Items.Clear();
                foreach (Information info in m_infos)
                {
                    if (regexFilter.IsMatch(info.DisplayName))
                    {
                        Items.Add(info);
                    }
                }

                if ((selectedInfo != null) && (Items.Contains(selectedInfo)))
                {
                    SelectedItem = selectedInfo;
                }
                else
                {
                    if (Items.Count > 0) SelectedIndex = 0;
                }
            }
            finally
            {
                EndUpdate();
            }
        }
    }
}
