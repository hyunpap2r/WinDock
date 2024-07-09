using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

public class DockPositioner
{
    [DllImport("shell32.dll")]
    static extern IntPtr SHAppBarMessage(int dwMessage, ref APPBARDATA pData);

    [StructLayout(LayoutKind.Sequential)]
    public struct APPBARDATA
    {
        public int cbSize;
        public IntPtr hWnd;
        public int uCallbackMessage;
        public int uEdge;
        public RECT rc;
        public IntPtr lParam;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left, Top, Right, Bottom;
    }

    public enum ABMsg : int
    {
        ABM_GETTASKBARPOS = 5,
    }

    public static void PositionDock(Form dockForm)
    {
        // 작업 표시줄의 위치 및 크기 정보를 가져옴
        var taskbarRect = GetTaskbarInfo();
        int taskbarHeight = taskbarRect.Height;

        // 화면 크기와 도크의 크기 정보를 얻음
        Rectangle screenBounds = Screen.PrimaryScreen.Bounds;
        Size dockSize = dockForm.Size;

        // 도크의 위치를 화면 하단 중앙으로 설정
        int dockX = (screenBounds.Width - dockSize.Width) / 2;
        int dockY = screenBounds.Bottom - taskbarHeight - dockSize.Height;

        // 도크의 위치를 설정
        dockForm.Location = new Point(dockX, dockY);
    }

    private static Rectangle GetTaskbarInfo()
    {
        APPBARDATA appBarData = new APPBARDATA
        {
            cbSize = Marshal.SizeOf(typeof(APPBARDATA))
        };
        IntPtr taskBarPtr = SHAppBarMessage((int)ABMsg.ABM_GETTASKBARPOS, ref appBarData);
        return new Rectangle(appBarData.rc.Left, appBarData.rc.Top, appBarData.rc.Right - appBarData.rc.Left, appBarData.rc.Bottom - appBarData.rc.Top);
    }
}
