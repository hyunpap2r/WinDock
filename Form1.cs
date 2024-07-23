using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Collections;
using Shell32;


namespace WinDock
{
    public partial class Form1 : Form
    {
        private System.Windows.Forms.Timer hideTimer;

        private Panel dockPanel;
        private CircularButton addButton;
        Stack<int> iconPosition = new Stack<int>();
        private Button settingsButton;  // setting mode 추가


        private bool isDragging = false;
        private Point lastCursor;
        private Point originalForm;

        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        private static readonly IntPtr HWND_BOTTOM = new IntPtr(1);
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOACTIVATE = 0x0010;

        private int iconSize = 40;
        private bool isVertical = false; // 도크 방향 (가로: false, 세로: true)


        public Form1()
        {
            InitializeComponent();
            InitializeDock();
            SetWindowPos(this.Handle, HWND_BOTTOM, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
            DockPositioner.PositionDock(this);

        }

        private void InitializeDock()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.Manual;

            // 화면 크기의 1/4로 사이즈 조절
            int dockWidth = Screen.PrimaryScreen.Bounds.Width / 4;
            int dockHeight = 80; 
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

            SetRoundedCorners(20);


            this.Controls.Add(dockPanel);

            addButton = new CircularButton();
            addButton.Text = "+";
            addButton.Size = new Size(30, 30);
            addButton.Location = new Point(dockPanel.Width - 40, 10);
            addButton.Click += AddButton_Click;
            dockPanel.Controls.Add(addButton);

            Stack buttonPosition = new Stack();




            /*
            settingsButton = new Button();
            settingsButton.Text = "se";
            settingsButton.Size = new Size(40, 40);
            settingsButton.BackColor = Color.FromArgb(255, 0, 122, 204);
            settingsButton.ForeColor = Color.White;
            settingsButton.Location = new Point(dockPanel.Width - 80, addButton.Bottom - 40); // + 버튼 아래에 위치
            dockPanel.Controls.Add(settingsButton);
            */



            hideTimer = new System.Windows.Forms.Timer();
            hideTimer.Interval = 1000;
            hideTimer.Tick += HideTimer_Tick;
            this.MouseLeave += (s, e) => hideTimer.Start();
            this.MouseEnter += (s, e) => ShowDock();

            PositionAddButton();


            this.MouseDown += Form1_MouseDown;
            this.MouseMove += Form1_MouseMove;
            this.MouseUp += Form1_MouseUp;

            
            // 도크의 자식 컨트롤 클릭 이벤트를 통해 드래그를 방해하지 않도록 하기 위해
            dockPanel.MouseDown += Form1_MouseDown;
            dockPanel.MouseMove += Form1_MouseMove;
            dockPanel.MouseUp += Form1_MouseUp;

        }


        private void AddButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "All Files|*.*";
                openFileDialog.Title = "Dock File 추가";

                openFileDialog.FilterIndex = 1;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string exePath = openFileDialog.FileName;
                    string fileTitle = Path.GetFileName(exePath);

                    AddApplicationToDock(exePath, fileTitle); 
                }
            }
        }



        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                lastCursor = Cursor.Position;
                originalForm = this.Location;
            }
        }


        private void SetRoundedCorners(int radius)
        {
            GraphicsPath path = new GraphicsPath();
            path.AddArc(new Rectangle(0, 0, radius, radius), 180, 90);
            path.AddArc(new Rectangle(this.Width - radius, 0, radius, radius), -90, 90);
            path.AddArc(new Rectangle(this.Width - radius, this.Height - radius, radius, radius), 0, 90);
            path.AddArc(new Rectangle(0, this.Height - radius, radius, radius), 90, 90);
            path.CloseAllFigures();

            this.Region = new Region(path);
        }



        // Dock Position 재정의
        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                Point newCursor = Cursor.Position;
                Point diff = new Point(newCursor.X - lastCursor.X, newCursor.Y - lastCursor.Y);
                this.Location = new Point(originalForm.X + diff.X, originalForm.Y + diff.Y);
            }
        }


        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = false;
            }
        }



        // + 버튼 위치 조정
        private void PositionAddButton()
        {
            int buttonWidth = addButton.Width;
            int buttonHeight = addButton.Height;
            int panelWidth = dockPanel.Width;
            int panelHeight = dockPanel.Height;
            addButton.Location = new Point(panelWidth - buttonWidth - 10, (panelHeight - buttonHeight) / 2);
        }


        // Button, Panel 
        private void AddApplicationToDock(string exePath, string fileTitle)
        {
            Icon appIcon = Icon.ExtractAssociatedIcon(exePath);
            string appName = exePath;

            // 아이콘을 추가할 위치 계산
            int iconX;
            int iconY = (dockPanel.Height - iconSize) / 2 - 10;


            if (iconPosition.Count == 0)
            {
                iconX = 10 + (dockPanel.Controls.Count - 1) * iconSize;
                iconPosition.Push(iconX);
            }
            else
            {
                iconX = 15 + iconPosition.Peek() + iconSize;
                iconPosition.Push(iconX);
            }

            PictureBox iconPictureBox = new PictureBox
            {
                Size = new Size(40, 40),
                Location = new Point(iconX, iconY),
                Image = appIcon.ToBitmap(),
                SizeMode = PictureBoxSizeMode.StretchImage,
                Tag = exePath,
                BackColor = Color.Transparent 

            };

            Label nameLabel = new Label
            {
                Text = Path.GetFileNameWithoutExtension(exePath),
                Location = new Point(iconPictureBox.Location.X, iconPictureBox.Bottom + 5),
                AutoSize = true,
                ForeColor = Color.White,
                BackColor = Color.Transparent 
            };

            dockPanel.Controls.Add(iconPictureBox);
            dockPanel.Controls.Add(nameLabel);


            // 우클릭시 setting 기능 추가를 위해 좌클릭 한정으로 변경
            iconPictureBox.MouseDoubleClick += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    Process.Start(exePath);
                }
            };

            iconPictureBox.MouseEnter += IconPictureBox_MouseEnter;
            iconPictureBox.MouseLeave += IconPictureBox_MouseLeave;

            PositionAddButton();
        }

       


        private void IconPictureBox_MouseEnter(object sender, EventArgs e)
        {
            PictureBox icon = sender as PictureBox;
            if (icon != null)
            {
                // 아이콘 크기 확대
                icon.Size = new Size(iconSize + 10, iconSize + 10);
                icon.Location = new Point(icon.Location.X - 5, icon.Location.Y - 5);
            }
        }

        private void IconPictureBox_MouseLeave(object sender, EventArgs e)
        {
            PictureBox icon = sender as PictureBox;
            if (icon != null)
            {
                icon.Size = new Size(iconSize, iconSize);
                icon.Location = new Point(icon.Location.X + 5, icon.Location.Y + 5);
            }
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
