using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using System.Windows.Forms;

namespace Soundboard
{
    public class CustomButton : Button
    {
        public event EventHandler RightClick;
        public string ButtonName { get; set; }
        public string AudioPath { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }

        public CustomButton(string buttonName, string audioPath, int row, int column)
        {
            ButtonName = buttonName;
            AudioPath = audioPath;
            Row = row;
            Column = column;

            Text = buttonName;
            Dock = DockStyle.Fill;
            this.MouseUp += CustomButton_MouseUp;
        }

        private void CustomButton_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                RightClick?.Invoke(this, EventArgs.Empty);
            }
        }

    }
}
