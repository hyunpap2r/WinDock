using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace WinDock
{
    public partial class Form1 : Form
    {
        private Panel dockPanel;
        private System.Windows.Forms.Timer hideTimer;
        private Button addButton;


        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        private static readonly IntPtr HWND_BOTTOM = new IntPtr(1);
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOACTIVATE = 0x0010;

        public Form1()
        {
            InitializeComponent();
            InitializeDock();
            SetWindowPos(this.Handle, HWND_BOTTOM, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
        }

        private void InitializeDock()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.Manual;

            // 화면 크기의 1/4로 사이즈 조절
            int dockWidth = Screen.PrimaryScreen.Bounds.Width / 4;
            int dockHeight = 100; 
            this.Size = new Size(dockWidth, dockHeight);

            // 화면 최하단 정중앙에 위치
            int dockX = (Screen.PrimaryScreen.Bounds.Width - dockWidth) / 2;
            int dockY = Screen.PrimaryScreen.Bounds.Height - dockHeight;
            this.Location = new Point(dockX, dockY);

            this.TopMost = true;
            this.BackColor = Color.Black;
            this.Opacity = 0.5;

            dockPanel = new Panel();
            dockPanel.Size = this.Size;
            dockPanel.Location = new Point(0, 0);
            dockPanel.BackColor = Color.FromArgb(128, 0, 0, 0);



            this.Controls.Add(dockPanel);

            addButton = new Button();
            addButton.Text = "+";
            addButton.Size = new Size(30, 30);
            addButton.Location = new Point(dockPanel.Width - 40, 10);
            addButton.Click += AddButton_Click;
            dockPanel.Controls.Add(addButton);



            hideTimer = new System.Windows.Forms.Timer();
            hideTimer.Interval = 1000;
            hideTimer.Tick += HideTimer_Tick;
            this.MouseLeave += (s, e) => hideTimer.Start();
            this.MouseEnter += (s, e) => ShowDock();

            PositionAddButton();


        }

        // .exe파일 추가
        private void AddButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Files|*.exe";
                openFileDialog.Title = "Dock 추가";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string exePath = openFileDialog.FileName;
                    AddApplicationToDock(exePath);
                }
            }
        }

        // + 버튼 위치 조정
        private void PositionAddButton()
        {
            int buttonWidth = addButton.Width;
            int buttonHeight = addButton.Height;
            int panelWidth = dockPanel.Width;
            int panelHeight = dockPanel.Height;
            addButton.Location = new Point(panelWidth - buttonWidth - 10, (panelHeight - buttonHeight) / 2 - 18);
        }


        private void AddApplicationToDock(string exePath)
        {
            Icon appIcon = Icon.ExtractAssociatedIcon(exePath);
            string appName = Path.GetFileNameWithoutExtension(exePath);

            PictureBox iconPictureBox = new PictureBox
            {
                Size = new Size(32, 32),
                Location = new Point((dockPanel.Controls.Count - 1) * 40 + 10, 10),
                Image = appIcon.ToBitmap(),
                SizeMode = PictureBoxSizeMode.StretchImage,
                Tag = exePath
            };
            iconPictureBox.Click += (s, e) => Process.Start(exePath);

            Label nameLabel = new Label
            {
                Text = Path.GetFileNameWithoutExtension(exePath),
                Location = new Point((dockPanel.Controls.Count - 1) * 40 + 10, 45),
                AutoSize = true,
                ForeColor = Color.White,
                BackColor = Color.Transparent
            };

            dockPanel.Controls.Add(iconPictureBox);
            dockPanel.Controls.Add(nameLabel);

            PositionAddButton();
        }

        private void HideTimer_Tick(object sender, EventArgs e)
        {
            if (!this.Bounds.Contains(Cursor.Position))
            {
                HideDock();
                hideTimer.Stop();
            }
        }

        private void ShowDock()
        {
            this.Location = new Point(0, Screen.PrimaryScreen.Bounds.Height - 100);
        }

        private void HideDock()
        {
            this.Location = new Point(0, Screen.PrimaryScreen.Bounds.Height);
        }
    }
}
