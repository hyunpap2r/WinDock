using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDock
{
    public class InputBox : Form
    {
        public string InputText { get; private set; }

        private TextBox textBox;
        private Button okButton;
        private Button cancelButton;

        public InputBox(string prompt, string title, string defaultText)
        {
            this.Text = title;

            Label promptLabel = new Label { Text = prompt, Dock = DockStyle.Top };
            textBox = new TextBox { Text = defaultText, Dock = DockStyle.Top };

            okButton = new Button { Text = "OK", DialogResult = DialogResult.OK, Dock = DockStyle.Left };
            cancelButton = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Dock = DockStyle.Right };

            okButton.Click += (sender, e) => { InputText = textBox.Text; };
            cancelButton.Click += (sender, e) => { InputText = null; };

            this.Controls.Add(textBox);
            this.Controls.Add(promptLabel);
            this.Controls.Add(okButton);
            this.Controls.Add(cancelButton);
        }
    }
}
