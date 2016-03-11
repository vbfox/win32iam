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

namespace VisualUninstaller
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Windows.Forms;
    using BlackFox.Win32.UninstallInformations;
    using VisualUninstaller.Properties;

    internal class ProgramsListBox : ListBox
    {
#pragma warning disable SA1310 // Field names must not contain underscore
        private const int WM_ERASEBKGND = 0x0014;
#pragma warning restore SA1310 // Field names must not contain underscore
        private static readonly Color selectedBgColor = Color.FromArgb(61, 128, 223);
        private static readonly Color selectedTextColor = Color.White;
        private static readonly Color bgColor = Color.White;
        private static readonly Color bgColorAlt = Color.FromArgb(237, 243, 254);
        private static readonly Color textColor = Color.Black;
        private readonly List<Information> displayedInfos;
        private readonly Dictionary<Information, Icon> iconCache;

        private readonly List<Information> infos;

        public ProgramsListBox()
        {
            SetStyle(ControlStyles.ResizeRedraw, true);

            // All informations in registry
            infos = Informations.GetInformations().ToList();

            // Information displayed (Filter applied, we start with no filter)
            displayedInfos = new List<Information>();
            displayedInfos.AddRange(infos);

            iconCache = new Dictionary<Information, Icon>();

            DrawItem += OnDrawItem;
        }

        public Information SelectedInfo => (Information)SelectedItem;

        public void UninstallSelected()
        {
            SelectedInfo?.Uninstall();
        }

        public void RemoveSelectedFromRegistry()
        {
            SelectedInfo?.RemoveFromRegistry();
        }

        public IEnumerable<Information> GetFilteredInfos(Regex regexFilter)
        {
            return infos.Where(i => regexFilter.IsMatch(i.DisplayName));
        }

        public void SetFilter(Regex regexFilter)
        {
            Information selectedInfo = SelectedInfo;

            BeginUpdate();
            try
            {
                Items.Clear();
                foreach (Information info in GetFilteredInfos(regexFilter))
                {
                    Items.Add(info);
                }

                if ((selectedInfo != null) && Items.Contains(selectedInfo))
                {
                    SelectedItem = selectedInfo;
                }

                if (Items.Count == 1)
                {
                    SelectedIndex = 0;
                }
            }
            finally
            {
                EndUpdate();
            }

            OnSelectedIndexChanged(EventArgs.Empty);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_ERASEBKGND)
            {
                int maxItemsOnScreen = ClientSize.Height / ItemHeight;
                int bottomIndex = TopIndex + maxItemsOnScreen;
                bool atEndOfListBox = bottomIndex > Items.Count - 1;
                if (atEndOfListBox)
                {
                    IntPtr hdc = m.WParam;
                    Graphics g = Graphics.FromHdcInternal(hdc);

                    int itemsOnScreen = Math.Min(Items.Count, maxItemsOnScreen);
                    int itemsHeight = itemsOnScreen * ItemHeight;

                    int toDraw = ClientSize.Height - itemsHeight;
                    g.FillRectangle(
                        new SolidBrush(BackColor),
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

        private void OnDrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index >= 0)
            {
                var info = (Information)Items[e.Index];

                /*
                 * Find where to put the text
                 */
                int y = e.Bounds.Height / 2;
                y -= (int)(e.Graphics.MeasureString(info.DisplayName, e.Font).Height / 2);
                var textPoint = new PointF(35, y);

                /*
                 * Text & background colors
                 */
                Color textColor;
                Color bgColor;
                if ((e.State & DrawItemState.Selected) != 0)
                {
                    bgColor = selectedBgColor;
                    textColor = selectedTextColor;
                }
                else
                {
                    textColor = ProgramsListBox.textColor;
                    bgColor = e.Index % 2 == 0 ? bgColorAlt : ProgramsListBox.bgColor;
                }

                Icon icon;
                if (!iconCache.TryGetValue(info, out icon))
                {
                    icon = info.Icon ?? Resources.Windows_Installer;

                    iconCache[info] = icon;
                }

                /*
                 * Display
                 */

                e.Graphics.FillRectangle(
                    new SolidBrush(bgColor),
                    e.Bounds.Left,
                    e.Bounds.Top,
                    e.Bounds.Width,
                    e.Bounds.Height);
                textPoint.X += e.Bounds.X;
                textPoint.Y += e.Bounds.Y;
                e.Graphics.DrawString(info.DisplayName, Font, new SolidBrush(textColor), textPoint);
                e.Graphics.DrawIcon(icon, new Rectangle(1 + e.Bounds.X, 1 + e.Bounds.Y, 32, 32));
            }
        }
    }
}
