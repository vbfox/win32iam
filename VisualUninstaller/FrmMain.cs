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
using Microsoft.Win32;
using System.Reflection;

namespace VisualUninstaller
{
    public partial class FrmMain : Form
    {
        string m_oldTextBoxContent;
        
        ProgramsListBox m_programsListBox = new ProgramsListBox();
        
        public FrmMain()
        {
            InitializeComponent();

            m_programsListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            m_programsListBox.BackColor = System.Drawing.Color.White;
            m_programsListBox.ContextMenuStrip = this.programsContextMenu;
            m_programsListBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            m_programsListBox.FormattingEnabled = true;
            m_programsListBox.IntegralHeight = false;
            m_programsListBox.ItemHeight = 34;
            m_programsListBox.Location = placeholderPanel.Location;
            m_programsListBox.Name = "m_programsListBox";
            m_programsListBox.Size = placeholderPanel.Size;
            m_programsListBox.TabIndex = 1;
            m_programsListBox.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.programsList_MouseDoubleClick);
            m_programsListBox.SelectedIndexChanged += new System.EventHandler(this.programsList_SelectedIndexChanged);
            m_programsListBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.programsList_MouseDown);
            this.Controls.Add(m_programsListBox);

            placeholderPanel.Visible = false;
            
        }

        void OnRegChanged(Object o, EventArgs e)
        {
            MessageBox.Show("changed.");
        }

        void UpdateVersionLabel()
        {
            Version v = Assembly.GetExecutingAssembly().GetName().Version;
            versionLabel.Text = string.Format("Version {0}.{1}", v.Major, v.Minor);
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

        #region Events

        private void Form1_Load(object sender, EventArgs e)
        {
            UpdateVersionLabel();
        }

        private void findTextBox_TextChanged(object sender, EventArgs e)
        {
            if (findTextBox.ForeColor == SystemColors.GrayText) return;
            if (m_oldTextBoxContent == findTextBox.Text) return;

            m_programsListBox.SetFilter(GetRegexpFromTextBox());

            m_oldTextBoxContent = findTextBox.Text;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            m_programsListBox.UninstallSelected();
        }

        private void programsList_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            m_programsListBox.UninstallSelected();
        }

        private void programsList_SelectedIndexChanged(object sender, EventArgs e)
        {
            removeSelectedBtn.Enabled = (m_programsListBox.SelectedItem != null);
        }

        private void findTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            // La touche entrée lance la désinstallation du programme sélectionné
            if (e.KeyChar == '\r')
            {
                e.Handled = true;
                m_programsListBox.UninstallSelected();
            }
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            // Désactive les menus si rien n'est sélectionné
            bool itemSelected = m_programsListBox.SelectedIndex >= 0;
            removeToolStripMenuItem.Enabled = itemSelected;
            removefromthelistToolStripMenuItem.Enabled = itemSelected;
        }

        private void programsList_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                int itemIndex = m_programsListBox.IndexFromPoint(e.X, e.Y);
                if (itemIndex >= 0)
                {
                    if (itemIndex != m_programsListBox.SelectedIndex)
                    {
                        m_programsListBox.SelectedIndex = itemIndex;
                    }
                }
                else
                {
                    m_programsListBox.SelectedIndex = -1;
                }

            }
        }

        private void findTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down)
            {
                if (m_programsListBox.SelectedIndex < m_programsListBox.Items.Count - 1)
                    m_programsListBox.SelectedIndex += 1;
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Up)
            {
                if (m_programsListBox.SelectedIndex > 0)
                    m_programsListBox.SelectedIndex -= 1;
                e.Handled = true;
            }
        }

        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_programsListBox.UninstallSelected();
        }

        private void removefromthelistToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Information selected = (Information)m_programsListBox.SelectedItem;
            string questionText = string.Format("Are you sure to remove the installer of \"{0}\" from the uninstall list, without uninstalling it ?", selected.DisplayName);
            if (MessageBox.Show(questionText, "Visual Uninstaller - Remove from the uninstall list",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                m_programsListBox.RemoveSelectedFromRegistry();
            }
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