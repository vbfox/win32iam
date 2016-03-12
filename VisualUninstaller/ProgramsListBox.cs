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
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using BlackFox.Win32.UninstallInformations;
    using VisualUninstaller.Properties;

    internal sealed class ProgramsListBox : ListBox
    {
        private static Task<ICollection<Information>> GetInformationAsync()
        {
            return Task.Factory.StartNew(Informations.GetInformations);
        }

#pragma warning disable SA1310 // Field names must not contain underscore
        private const int WM_ERASEBKGND = 0x0014;
#pragma warning restore SA1310 // Field names must not contain underscore
        private static readonly Color selectedBgColor = Color.FromArgb(61, 128, 223);
        private static readonly Color selectedTextColor = Color.White;
        private static readonly Color normalBgColor = Color.White;
        private static readonly Color bgColorAlt = Color.FromArgb(237, 243, 254);
        private static readonly Color normalTextColor = Color.Black;
        private readonly Dictionary<string, Icon> iconCache;
        private Regex currentRegexFilter;
        private List<Information> infos;

        public ProgramsListBox()
        {
            SetStyle(ControlStyles.ResizeRedraw, true);

            // All informations in registry
            infos = new List<Information>();

            iconCache = new Dictionary<string, Icon>();

            DrawItem += OnDrawItem;

            UpdateInformationAsync();

            DoubleBuffered = true;
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
            return regexFilter == null
                ? infos
                : infos.Where(i => regexFilter.IsMatch(i.DisplayName));
        }

        public void SetFilter(Regex regexFilter)
        {
            Information selectedInfo = SelectedInfo;

            currentRegexFilter = regexFilter;

            BeginUpdate();
            try
            {
                Items.Clear();
                foreach (var info in GetFilteredInfos(regexFilter))
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
                    textColor = normalTextColor;
                    bgColor = e.Index % 2 == 0 ? bgColorAlt : normalBgColor;
                }

                Icon icon;
                if (!iconCache.TryGetValue(info.DisplayIconPath, out icon))
                {
                    UpdateIconCacheFor(info);
                }
                else
                {
                    // No icon at all, we display a default one
                    icon = icon ?? Resources.Windows_Installer;
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

                // Icon can be null if not in cache, we'll invalidate later
                if (icon != null)
                {
                    e.Graphics.DrawIcon(
                        icon,
                        new Rectangle(1 + e.Bounds.X, 1 + e.Bounds.Y, 32, 32));
                }
            }
        }

        private async void UpdateIconCacheFor(Information info)
        {
            var icon = await info.GetIconAsync();
            iconCache[info.DisplayIconPath] = icon;
            Invalidate();
        }

        private async void UpdateInformationAsync()
        {
            var newInfo = await GetInformationAsync();
            infos = newInfo.ToList();
            SetFilter(currentRegexFilter);
        }
    }
}
