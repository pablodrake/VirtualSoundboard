using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using System.Windows.Forms;

namespace Soundboard
{
    public class ModifyButtonForm : Form
    {
        private TextBox buttonNameTextBox;
        private TextBox audioPathTextBox;
        private Button saveButton;
        private Button browseButton;
        private Button resetButton;
        private CustomButton buttonToModify;
        private OpenFileDialog openFileDialog;

        public string ButtonName { get; private set; }
        public string AudioPath { get; private set; }

        public ModifyButtonForm(CustomButton button)
        {
            buttonToModify = button;
            InitializeComponents();

            openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Audio Files|*.mp3;*.wav";
            openFileDialog.Title = "Select Audio File";
            openFileDialog.FileOk += OpenFileDialog_FileOk;

            // Disable maximize control and set form size and properties
            MaximizeBox = false;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Width = 500;
            Height = 150;
            MaximumSize = new Size(500, 150);
            StartPosition = FormStartPosition.CenterParent;
        }

        private void InitializeComponents()
        {
            Text = "Modify Button";

            Label buttonNameLabel = new Label
            {
                Text = "Button Name:",
                Location = new Point(10, 10),
                Width = 80
            };

            buttonNameTextBox = new TextBox
            {
                MaxLength = 15,
                Text = buttonToModify.Text,
                Location = new Point(100, 10),
                Width = 350
            };

            Label audioPathLabel = new Label
            {
                Text = "Audio Path:",
                Location = new Point(10, 40),
                Width = 80
            };

            audioPathTextBox = new TextBox
            {
                Text = buttonToModify.AudioPath,
                Location = new Point(100, 40),
                Width = 280
            };

            browseButton = new Button
            {
                Text = "Browse",
                Location = new Point(390, 38),
                Width = 60
            };

            saveButton = new Button
            {
                Text = "Save",
                Location = new Point(120, 80),
                Width = 80
            };

            resetButton = new Button
            {
                Text = "Reset",
                Location = new Point(210, 80),
                Width = 80
            };

            saveButton.Click += SaveButton_Click;
            browseButton.Click += BrowseButton_Click;
            resetButton.Click += ResetButton_Click;

            Controls.Add(buttonNameLabel);
            Controls.Add(buttonNameTextBox);
            Controls.Add(audioPathLabel);
            Controls.Add(audioPathTextBox);
            Controls.Add(browseButton);
            Controls.Add(saveButton);
            Controls.Add(resetButton);
        }


        private void SaveButton_Click(object sender, EventArgs e)
        {
            ButtonName = buttonNameTextBox.Text;
            AudioPath = audioPathTextBox.Text;

            DialogResult = DialogResult.OK;
            Close();
        }

        private void BrowseButton_Click(object sender, EventArgs e)
        {
            openFileDialog.ShowDialog();
        }

        private void ResetButton_Click(object sender, EventArgs e)
        {
            int row = buttonToModify.Row;
            int col = buttonToModify.Column;
            string buttonName = $"Button {row}.{col}";

            buttonToModify.ButtonName = buttonName;
            buttonToModify.Text = buttonName;
            buttonToModify.AudioPath = "";
            Close();
        }

        private void OpenFileDialog_FileOk(object sender, CancelEventArgs e)
        {
            audioPathTextBox.Text = openFileDialog.FileName;
        }
    }
}
