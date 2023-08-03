using CsvHelper;
using NAudio.Gui;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Soundboard
{
    public partial class Form1 : Form
    {
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;  // Turn on WS_EX_COMPOSITED
                return cp;
            }
        }

        private WaveOutEvent waveOut; // WaveOutEvent for audio playback
        private MediaFoundationReader audioReader; // MediaFoundationReader for audio file reading
        private List<int> outputDeviceIndex;
        private CustomButton[,] customButtons; // 2D array of CustomButtons
        private int rowCount = 1;
        private int columnCount = 1;
        string filePath = Directory.GetCurrentDirectory() + "\\soundboard_state.csv";

        public Form1()
        {
            InitializeComponent();
            FormBorderStyle = FormBorderStyle.FixedSingle;
            InitializeCustomButtons();
            LoadStateFromCsv(filePath);
            PopulateTableLayoutPanel();
        }

        private void InitializeCustomButtons()
        {
            customButtons = new CustomButton[11, 11]; // Creating 11x11 array

            for (int row = 0; row < 11; row++)
            {
                for (int col = 0; col < 11; col++)
                {
                    string buttonName = $"{row}.{col}";
                    string audioPath = ""; // Set the audio path as needed
                    CustomButton button = new CustomButton(buttonName, audioPath, row, col);
                    button.Click += CustomButton_Click;
                    button.RightClick += CustomButton_RightClick;
                    customButtons[row, col] = button;
                }
            }


        }

        private void PopulateTableLayoutPanel()
        {
            tableLayoutPanel1.SuspendLayout();

            tableLayoutPanel1.Controls.Clear();
            tableLayoutPanel1.RowStyles.Clear();
            tableLayoutPanel1.ColumnStyles.Clear();
            tableLayoutPanel1.RowCount = rowCount;
            tableLayoutPanel1.ColumnCount = columnCount;

            for (int row = 0; row < rowCount; row++)
            {
                tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / rowCount));

                for (int col = 0; col < columnCount; col++)
                {
                    tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / columnCount));

                    CustomButton button = customButtons[row, col];
                    tableLayoutPanel1.Controls.Add(button, col, row);
                }
            }
            tableLayoutPanel1.ResumeLayout();
        }

        private void CustomButton_Click(object sender, EventArgs e)
        {
            CustomButton button = (CustomButton)sender;
            string audioPath = button.AudioPath;
            this.Focus();

            // Check if the audio file exists
            if (!File.Exists(audioPath))
            {
                MessageBox.Show("Audio file not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            DisposeWave();

            // Create an instance of WaveOutEvent for audio playback
            using (waveOut = new WaveOutEvent())
            {
                // Create a MediaFoundationReader to read the audio file (supports multiple formats)
                using (audioReader = new MediaFoundationReader(audioPath))
                {
                    // Connect the reader to the WaveOutEvent
                    waveOut.DeviceNumber = comboBox1.SelectedIndex;
                    waveOut.Init(audioReader);
                    waveOut.Volume = volumeSlider1.Volume;

                    // Set the desired audio output device

                    // You can choose a specific device using waveOut.DeviceNumber,
                    // or let the user select the output device through a UI.

                    // Start audio playback
                    waveOut.Play();

                    // Wait for playback to complete
                    while (waveOut.PlaybackState == PlaybackState.Playing || waveOut.PlaybackState == PlaybackState.Paused)
                    {
                        // Do other processing or wait
                        Application.DoEvents();
                        comboBox1.Enabled = false;
                    }

                    // Stop audio playback
                    comboBox1.Enabled = true;
                    waveOut.Stop();
                    DisposeWave();
                }
            }
        }

        private void CustomButton_RightClick(object sender, EventArgs e)
        {
            CustomButton button = (CustomButton)sender;

            using (ModifyButtonForm modifyForm = new ModifyButtonForm(button))
            {
                if (modifyForm.ShowDialog() == DialogResult.OK)
                {
                    // Update the button properties in the array
                    for (int row = 0; row < rowCount; row++)
                    {
                        for (int col = 0; col < columnCount; col++)
                        {
                            if (customButtons[row, col] == button)
                            {
                                customButtons[row, col].ButtonName = modifyForm.ButtonName;
                                customButtons[row, col].AudioPath = modifyForm.AudioPath;
                                break;
                            }
                        }
                    }

                    // Update the text of the modified button
                    button.Text = modifyForm.ButtonName;
                }
            }
        }
        private void SaveStateToCsv(string filePath)
        {
            using (var writer = new StreamWriter(filePath))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                // Write the row and column count as the first line in the CSV
                csv.WriteField(rowCount);
                csv.WriteField(columnCount);
                csv.NextRecord();

                // Write the button data
                for (int row = 0; row < rowCount; row++)
                {
                    for (int col = 0; col < columnCount; col++)
                    {
                        CustomButton button = customButtons[row, col];
                        csv.WriteField(button.ButtonName);
                        csv.WriteField(button.AudioPath);
                        csv.WriteField(row);
                        csv.WriteField(col);
                        csv.NextRecord();
                    }
                }
            }
        }
        private void LoadStateFromCsv(string filePath)
        {
            if (!File.Exists(filePath))
                return;

            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                // Read the row and column count from the first line of the CSV
                csv.Read();
                rowCount = csv.GetField<int>(0);
                columnCount = csv.GetField<int>(1);

                // Skip the header line
                csv.ReadHeader();

                // Read the button data
                while (csv.Read())
                {
                    string buttonName = csv.GetField<string>(0);
                    string audioPath = csv.GetField<string>(1);
                    int row = csv.GetField<int>(2);
                    int col = csv.GetField<int>(3);

                    if (row >= rowCount || col >= columnCount)
                        continue;

                    CustomButton button = new CustomButton(buttonName, audioPath, row, col);
                    button.Click += CustomButton_Click;
                    button.RightClick += CustomButton_RightClick;
                    customButtons[row, col] = button;
                }
            }

            PopulateTableLayoutPanel();
        }

        private void DisposeWave()
        {
            if (waveOut != null)
            {
                // Stop audio playback
                waveOut.Stop();
                waveOut.Dispose();
                waveOut = null;
            }

            if (audioReader != null)
            {
                // Dispose the audio reader
                audioReader.Dispose();
                audioReader = null;
            }
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveStateToCsv(filePath);
            DisposeWave();
        }


        private void IncreaseRows()
        {
            if (rowCount <= 10)
            {
                rowCount++;
                PopulateTableLayoutPanel();
            }
            else
            {
                ShowErrorMessage("Maximum number of rows reached!");
            }
        }

        private void DecreaseRows()
        {
            if (rowCount > 1)
            {
                rowCount--;
                PopulateTableLayoutPanel();
            }
            else
            {
                ShowErrorMessage("Minimum number of rows reached!");
            }
        }

        private void IncreaseColumns()
        {
            if (columnCount <= 10)
            {
                columnCount++;
                PopulateTableLayoutPanel();
            }
            else
            {
                ShowErrorMessage("Maximum number of columns reached!");
            }
        }

        private void DecreaseColumns()
        {
            if (columnCount > 1)
            {
                columnCount--;
                PopulateTableLayoutPanel();
            }
            else
            {
                ShowErrorMessage("Minimum number of columns reached!");
            }
        }

        private void ShowErrorMessage(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            PopulateComboBox(); 
        }

        private void button1_Click(object sender, EventArgs e)
        {
            IncreaseRows();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DecreaseRows();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            IncreaseColumns();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            DecreaseColumns();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (waveOut != null)
            {
                if (waveOut.PlaybackState == PlaybackState.Playing)
                {
                    waveOut.Pause();
                }
                else if (waveOut.PlaybackState == PlaybackState.Paused)
                {
                    waveOut.Play();
                }
            }
        }

        private void volumeSlider1_VolumeChanged(object sender, EventArgs e)
        {
            if (waveOut != null)
            {
                waveOut.Volume = volumeSlider1.Volume;
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (waveOut != null)
            {
                // Stop and dispose the current waveOut object
                waveOut.Stop();
            }

        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (waveOut != null)
            {
                waveOut.Stop();
            }

        }

        private void comboBox1_DropDown(object sender, EventArgs e)
        {
            PopulateComboBox();
        }

        private void PopulateComboBox()
        {
           
            waveOut = new WaveOutEvent();

            comboBox1.Items.Clear();
            // Populate the output device combo box
            outputDeviceIndex = new List<int>();
            for (int device = 0; device < WaveOut.DeviceCount; device++)
            {
                outputDeviceIndex.Add(device);
                comboBox1.Items.Add(WaveOut.GetCapabilities(device).ProductName);
            }

            // Set the default output device
            comboBox1.SelectedIndex = 0;
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void fileSystemWatcher1_Changed(object sender, FileSystemEventArgs e)
        {

        }
    }

}