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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using BlackFox.Win32.UninstallInformations;
using System.Text.RegularExpressions;
using System.Resources;
using RegistryUtils;
using Microsoft.Win32;

namespace VisualUninstaller
{
    public partial class FrmMain : Form
    {
        static readonly Color SELECTED_BG_COLOR = Color.FromArgb(61, 128, 223);
        static readonly Color SELECTED_TEXT_COLOR = Color.White;
        static readonly Color BG_COLOR = Color.White;
        static readonly Color BG_COLOR_ALT = Color.FromArgb(237, 243, 254);
        static readonly Color TEXT_COLOR = Color.Black;

        List<Information> m_infos;
        List<Information> m_displayedInfos;
        Dictionary<Information, Icon> m_iconCache;
        RegistryUtils.RegistryMonitor m_monitor;

        string m_oldTextBoxContent;

        public FrmMain()
        {
            InitializeComponent();

            // All informations in registry
            m_infos = Informations.GetInformations();

            //Information displayed (Filter applied)
            m_displayedInfos = new List<Information>();
            m_displayedInfos.AddRange(m_infos);

            // Building icon cache
            m_iconCache = new Dictionary<Information, Icon>();
            BuildIconCache();

            m_monitor = new RegistryMonitor();
            m_monitor.RegistryKey = Informations.Key;
            m_monitor.RegChanged += new EventHandler(OnRegChanged);
            m_monitor.Start();
        }

        void BuildIconCache()
        {
            foreach (Information info in m_infos)
            {
                m_iconCache[info] = info.Icon;
            }
        }

        void UpdateListBox()
        {
            programsList.Items.Clear();
            foreach (Information info in m_displayedInfos)
            {
                programsList.Items.Add(info);
            }
            programsList.SelectedIndex = (programsList.Items.Count > 0) ? 0 : -1;
            programsList_SelectedIndexChanged(this, new EventArgs());
        }

        /// <summary>
        /// Transforme le contenu de la listbox en une regexp.
        /// </summary>
        /// <returns>La regexp</returns>
        Regex GetRegexpFromTextBox()
        {
            StringBuilder result = new StringBuilder();
            foreach(string str in findTextBox.Text.Split(' '))
            {
                result.Append(Regex.Escape(str));
                result.Append(".*");
            }
            return new Regex(result.ToString(), RegexOptions.IgnoreCase); 
        }

        void UpdateInformations()
        {
            Regex regexp = GetRegexpFromTextBox();

            m_displayedInfos.Clear();
            foreach (Information info in m_infos)
            {
                if (regexp.IsMatch(info.DisplayName))
                {
                    m_displayedInfos.Add(info);
                }
            }
        }

        void UninstallSelected()
        {
            Information selected = (Information)programsList.SelectedItem;
            if (selected != null)
            {
                selected.Uninstall();
            }
        }

        void RemoveSelectedFromRegistry()
        {
            Information selected = (Information)programsList.SelectedItem;
            if (selected != null)
            {
                selected.RemoveFromRegistry();
            }
        }

        #region Events

        private void Form1_Load(object sender, EventArgs e)
        {
            UpdateListBox();
        }

        private void findTextBox_TextChanged(object sender, EventArgs e)
        {
            if (findTextBox.ForeColor == SystemColors.GrayText) return;
            if (m_oldTextBoxContent == findTextBox.Text) return;

            UpdateInformations();
            UpdateListBox();
            m_oldTextBoxContent = findTextBox.Text;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            UninstallSelected();
        }

        private void programsList_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            UninstallSelected();
        }

        private void programsList_SelectedIndexChanged(object sender, EventArgs e)
        {
            removeSelectedBtn.Enabled = (programsList.SelectedItem != null);
        }

        private void programsList_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index >= 0)
            {
                Information info = (Information)programsList.Items[e.Index];

                /*
                 * On détermine la position du texte.
                 */
                int y = e.Bounds.Top;
                y += (int)(e.Bounds.Height / 2);
                y -= (int)(e.Graphics.MeasureString(info.DisplayName, e.Font).Height / 2);
                PointF textPoint = new PointF(e.Bounds.Left + 35, y);

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
                e.Graphics.FillRectangle(new SolidBrush(bgColor), e.Bounds);
                e.Graphics.DrawString(info.DisplayName, this.Font, new SolidBrush(textColor), textPoint);
                e.Graphics.DrawIcon(icon, new Rectangle(e.Bounds.Left+1, e.Bounds.Top+1, 32, 32));
            }
        }

        private void findTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            // La touche entrée lance la désinstallation si il n'y as qu'un
            // seul programme listé.
            if (e.KeyChar == '\r')
            {
                e.Handled = true;
                if (programsList.Items.Count == 1)
                {
                    ((Information)programsList.Items[0]).Uninstall();
                }
            }
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            // Désactive les menus si rien n'est sélectionné
            bool itemSelected = programsList.SelectedIndex >= 0;
            removeToolStripMenuItem.Enabled = itemSelected;
            removefromthelistToolStripMenuItem.Enabled = itemSelected;
        }

        private void programsList_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                int itemIndex = programsList.IndexFromPoint(e.X, e.Y);
                if (itemIndex >= 0)
                {
                    if (itemIndex != programsList.SelectedIndex)
                    {
                        programsList.SelectedIndex = itemIndex;
                    }
                }
                else
                {
                    programsList.SelectedIndex = -1;
                }

            }
        }

        private void findTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down)
            {
                programsList.Focus();
            }
        }

        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UninstallSelected();
        }

        private void removefromthelistToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Information selected = (Information)programsList.SelectedItem;
            string questionText = string.Format("Are you sure to remove the installer of \"{0}\" from the uninstall list, without uninstalling it ?", selected.DisplayName);
            if (MessageBox.Show(questionText, "Visual Uninstaller - Remove from the uninstall list",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                RemoveSelectedFromRegistry();
            }
        }
        
        void OnRegChanged(object sender, EventArgs e)
        {
            MessageBox.Show("hello world");
        }
        
        #endregion

        #region Gray text in textbox

        private void findTextBox_Enter(object sender, EventArgs e)
        {
            if (findTextBox.ForeColor == SystemColors.GrayText)
            {
                findTextBox.ForeColor = SystemColors.WindowText;
                findTextBox.Text = "";
            }
        }

        private void findTextBox_Leave(object sender, EventArgs e)
        {
            if (findTextBox.Text.Trim() == "")
            {
                findTextBox.ForeColor = SystemColors.GrayText;
                findTextBox.Text = "Search by name";
            }
        }

        #endregion
    }
}