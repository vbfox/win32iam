/*
 * VisualUninstaller - Add/Remove programs replacement
 * 
 * Copyright (C) 2006 Julien Roncaglia
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using BlackFox.Win32.UninstallInformations;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;

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

            //Information displayed (Filter applied, we start with no filter)
            m_displayedInfos = new List<Information>();
            m_displayedInfos.AddRange(m_infos);

            m_iconCache = new Dictionary<Information, Icon>();

            DrawItem += new System.Windows.Forms.DrawItemEventHandler(__DrawItem);
        }

        const int WM_ERASEBKGND = 0x0014;

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_ERASEBKGND)
            {
                int maxItemsOnScreen = (int)(ClientSize.Height / ItemHeight);
                int bottomIndex = TopIndex + maxItemsOnScreen;
                bool atEndOfListBox = bottomIndex > Items.Count-1;
                if (atEndOfListBox)
                {
                    IntPtr hdc = m.WParam;
                    Graphics g = Graphics.FromHdcInternal(hdc);

                    int itemsOnScreen = Math.Min(Items.Count, maxItemsOnScreen);
                    int itemsHeight = itemsOnScreen * this.ItemHeight;

                    int toDraw = ClientSize.Height - itemsHeight;
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
                Information info = (Information)Items[e.Index];

                /*
                 * Find where to put the text
                 */
                int y = (int)(e.Bounds.Height / 2);
                y -= (int)(e.Graphics.MeasureString(info.DisplayName, e.Font).Height / 2);
                PointF textPoint = new PointF(35, y);

                /*
                 * Text & background colors
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

                Icon icon;
                if (!m_iconCache.TryGetValue(info, out icon))
                {
                    if (info.Icon != null)
                    {
                        icon = info.Icon;
                    }
                    else
                    {
                        icon = Properties.Resources.Windows_Installer;
                    }
                    m_iconCache[info] = icon;
                }

                /*
                 * Display
                 */
                
                e.Graphics.FillRectangle(new SolidBrush(bgColor), e.Bounds.Left, e.Bounds.Top, e.Bounds.Width, e.Bounds.Height);
                textPoint.X += e.Bounds.X;
                textPoint.Y += e.Bounds.Y;
                e.Graphics.DrawString(info.DisplayName, this.Font, new SolidBrush(textColor), textPoint);
                e.Graphics.DrawIcon(icon, new Rectangle(1 + e.Bounds.X, 1 + e.Bounds.Y, 32, 32));
            }
        }

        public Information SelectedInfo
        {
            get
            {
                return (Information)SelectedItem;
            }
        }

        public void UninstallSelected()
        {
            if (SelectedInfo != null)
            {
                SelectedInfo.Uninstall();
            }
        }

        public void RemoveSelectedFromRegistry()
        {
            if (SelectedInfo != null)
            {
                SelectedInfo.RemoveFromRegistry();
            }
        }

        public void SetFilter(Regex regexFilter)
        {
            Information selectedInfo = SelectedInfo;
            Predicate<Information> filerPredicate = delegate(Information i) { return regexFilter.IsMatch(i.DisplayName); };
            // C# 3.0: var filterPredicate = (Information i) => regexFilter.IsMatch(i.DisplayName);

            BeginUpdate();
            try
            {

                Items.Clear();
                foreach (Information info in Utils.Filter(m_infos, filerPredicate))
                {
                    Items.Add(info);
                }

                if ((selectedInfo != null) && (Items.Contains(selectedInfo)))
                {
                    SelectedItem = selectedInfo;
                }
                else
                {
                    if (Items.Count == 1) SelectedIndex = 0;
                }
            }
            finally
            {
                EndUpdate();
            }
            OnSelectedIndexChanged(EventArgs.Empty);
        }
    }
}
