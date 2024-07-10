using System;
using System.Windows.Forms;

public class RenameForm : Form
{
    private TextBox textBox;
    private Button okButton;
    private Button cancelButton;

    public string NewLabel { get; private set; }

    public RenameForm(string currentLabel)
    {
        this.Text = "Rename";
        this.Size = new Size(300, 150);

        textBox = new TextBox
        {
            Text = currentLabel,
            Location = new Point(15, 15),
            Width = 250
        };

        okButton = new Button
        {
            Text = "OK",
            DialogResult = DialogResult.OK,
            Location = new Point(50, 50),
            Width = 75
        };

        cancelButton = new Button
        {
            Text = "Cancel",
            DialogResult = DialogResult.Cancel,
            Location = new Point(150, 50),
            Width = 75
        };

        this.Controls.Add(textBox);
        this.Controls.Add(okButton);
        this.Controls.Add(cancelButton);

        this.AcceptButton = okButton;
        this.CancelButton = cancelButton;

        okButton.Click += (s, e) => NewLabel = textBox.Text;
    }
}
