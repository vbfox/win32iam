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
    using System.ComponentModel;
    using System.Drawing;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Windows.Forms;
    using BlackFox.Win32.UninstallInformations;

    public partial class FrmMain : Form
    {
        private readonly ProgramsListBox programsListBox = new ProgramsListBox();
        private string oldTextBoxContent;

        public FrmMain()
        {
            InitializeComponent();

            programsListBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom
                                     | AnchorStyles.Left
                                     | AnchorStyles.Right;
            programsListBox.BackColor = Color.White;
            programsListBox.ContextMenuStrip = programsContextMenu;
            programsListBox.DrawMode = DrawMode.OwnerDrawFixed;
            programsListBox.FormattingEnabled = true;
            programsListBox.IntegralHeight = false;
            programsListBox.ItemHeight = 34;
            programsListBox.Location = placeholderPanel.Location;
            programsListBox.Name = "m_programsListBox";
            programsListBox.Size = placeholderPanel.Size;
            programsListBox.TabIndex = 1;
            programsListBox.MouseDoubleClick += ProgramsListMouseDoubleClick;
            programsListBox.SelectedIndexChanged += ProgramsListSelectedIndexChanged;
            programsListBox.MouseDown += ProgramsListMouseDown;
            Controls.Add(programsListBox);

            placeholderPanel.Visible = false;
        }

        private void UpdateVersionLabel()
        {
            Version v = Assembly.GetExecutingAssembly().GetName().Version;
            versionLabel.Text = $"Version {v.Major}.{v.Minor}";
        }

        /// <summary>
        ///     Transforme le contenu de la listbox en une regexp.
        /// </summary>
        /// <returns>La regexp</returns>
        private Regex GetRegexpFromTextBox()
        {
            var result = new StringBuilder();
            foreach (string str in findTextBox.Text.Split(' '))
            {
                result.Append(Regex.Escape(str));
                result.Append(".*");
            }

            return new Regex(result.ToString(), RegexOptions.IgnoreCase);
        }

        private void Form1Load(object sender, EventArgs e)
        {
            UpdateVersionLabel();
        }

        private void FindTextBoxTextChanged(object sender, EventArgs e)
        {
            if (findTextBox.ForeColor == SystemColors.GrayText)
            {
                return;
            }

            if (oldTextBoxContent == findTextBox.Text)
            {
                return;
            }

            programsListBox.SetFilter(GetRegexpFromTextBox());

            oldTextBoxContent = findTextBox.Text;
        }

        private void Button1Click(object sender, EventArgs e)
        {
            programsListBox.UninstallSelected();
        }

        private void ProgramsListMouseDoubleClick(object sender, MouseEventArgs e)
        {
            programsListBox.UninstallSelected();
        }

        private void ProgramsListSelectedIndexChanged(object sender, EventArgs e)
        {
            removeSelectedBtn.Enabled = programsListBox.SelectedItem != null;
        }

        private void FindTextBoxKeyPress(object sender, KeyPressEventArgs e)
        {
            // La touche entrée lance la désinstallation du programme sélectionné
            if (e.KeyChar == '\r')
            {
                e.Handled = true;
                programsListBox.UninstallSelected();
            }
        }

        private void ContextMenuStrip1Opening(object sender, CancelEventArgs e)
        {
            // Désactive les menus si rien n'est sélectionné
            bool itemSelected = programsListBox.SelectedIndex >= 0;
            removeToolStripMenuItem.Enabled = itemSelected;
            removefromthelistToolStripMenuItem.Enabled = itemSelected;
        }

        private void ProgramsListMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                int itemIndex = programsListBox.IndexFromPoint(e.X, e.Y);
                if (itemIndex >= 0)
                {
                    // ReSharper disable once RedundantCheckBeforeAssignment
                    if (itemIndex != programsListBox.SelectedIndex)
                    {
                        programsListBox.SelectedIndex = itemIndex;
                    }
                }
                else
                {
                    programsListBox.SelectedIndex = -1;
                }
            }
        }

        private void FindTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down)
            {
                if (programsListBox.SelectedIndex < programsListBox.Items.Count - 1)
                {
                    programsListBox.SelectedIndex += 1;
                }

                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Up)
            {
                if (programsListBox.SelectedIndex > 0)
                {
                    programsListBox.SelectedIndex -= 1;
                }

                e.Handled = true;
            }
        }

        private void RemoveToolStripMenuItemClick(object sender, EventArgs e)
        {
            programsListBox.UninstallSelected();
        }

        private void RemovefromthelistToolStripMenuItemClick(object sender, EventArgs e)
        {
            var selected = (Information)programsListBox.SelectedItem;
            string questionText =
                $"Are you sure to remove the installer of \"{selected.DisplayName}\" from the uninstall list, without uninstalling it ?";
            if (MessageBox.Show(
                questionText,
                "Visual Uninstaller - Remove from the uninstall list",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) == DialogResult.Yes)
            {
                programsListBox.RemoveSelectedFromRegistry();
            }
        }

        private void FindTextBoxEnter(object sender, EventArgs e)
        {
            if (findTextBox.ForeColor == SystemColors.GrayText)
            {
                findTextBox.ForeColor = SystemColors.WindowText;
                findTextBox.Text = "";
            }
        }

        private void FindTextBoxLeave(object sender, EventArgs e)
        {
            if (findTextBox.Text.Trim() == "")
            {
                findTextBox.ForeColor = SystemColors.GrayText;
                findTextBox.Text = "Search by name";
            }
        }
    }
}
