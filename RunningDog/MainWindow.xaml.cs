using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace RunningDog
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer timer;
        private DispatcherTimer gifTimer;
        private GifBitmapDecoder? decoder;
        private int currentFrame = 0;

        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
            int X, int Y, int cx, int cy, uint uFlags);

        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private const UInt32 SWP_NOSIZE = 0x0001;
        private const UInt32 SWP_NOMOVE = 0x0002;
        private const UInt32 SWP_SHOWWINDOW = 0x0040;

        public MainWindow()
        {
            InitializeComponent();

            // GIFを読み込む
            using (FileStream fs = new FileStream("dog.gif", FileMode.Open, FileAccess.Read))
            {
                decoder = new GifBitmapDecoder(fs, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
            }

            if (decoder != null && decoder.Frames.Count > 0)
            {
                DogImage.Source = decoder.Frames[0];

                // フレーム切替タイマー
                gifTimer = new DispatcherTimer();
                gifTimer.Interval = TimeSpan.FromMilliseconds(100); // フレーム間隔
                gifTimer.Tick += (s, e) =>
                {
                    DogImage.Source = decoder.Frames[currentFrame];
                    currentFrame = (currentFrame + 1) % decoder.Frames.Count;
                };
                gifTimer.Start();
            }

            // ウィンドウを右下に配置
            var screen = SystemParameters.WorkArea;
            this.Left = screen.Right - this.Width;
            this.Top = screen.Bottom - this.Height;

            // 25分タイマー
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMinutes(25);
            // テスト用なら ↓ に変更
            // timer.Interval = TimeSpan.FromSeconds(5);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
            SetWindowPos(hwnd, HWND_TOPMOST, 0, 0, 0, 0,
                SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            timer.Stop();
            gifTimer.Stop();
            MessageBox.Show("25分経過！お疲れさま！", "完了",
                MessageBoxButton.OK, MessageBoxImage.Information);
            this.Close();
        }
    }
}
