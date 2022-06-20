using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SZ_FFmpegLibrary;
using static SZ_FFmpegLibrary.SZFFmpeg;

namespace SZVideoConverter_WPF
{
    /// <summary>
    /// VideoBlock.xaml 的交互逻辑
    /// </summary>
    public partial class VideoBlock : UserControl
    {
        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern bool DeleteObject(IntPtr hObject);

        public struct VideoBlockInfo
        {
            public string id;
            public FileInfo fileInfo;
            public Xabe.FFmpeg.MediaInfo mediaInfo;
        }

        public struct ConvertParams
        {
            public List<string> prepearFormats;
            public Xabe.FFmpeg.PixelFormat pixelFormat;
        }

        public ConvertParams convertParams = new ConvertParams();
        public bool isSelected = false;
        public VideoBlockInfo videoBlockInfo;

        public VideoBlock()
        {
            this.InitializeComponent();
            this.convertParams.prepearFormats = new List<string>();
        }

        private MainWindow Host;

        public void InitVideoBlock(FileInfo fi, string id, MainWindow host)
        {
            this.Width = 260;
            this.Height = 240;
            this.Host = host;

            videoBlockInfo = new VideoBlockInfo();
            videoBlockInfo.fileInfo = fi;
            videoBlockInfo.id = id;
            file_name.Text = videoBlockInfo.fileInfo.Name;
            new Task(async () =>
            {
                var img_path = await SZFFmpeg.GetVideoSnapshotAsync(this.videoBlockInfo.fileInfo.FullName);
                this.videoBlockInfo.mediaInfo = (Xabe.FFmpeg.MediaInfo)await SZFFmpeg.GetVideoDetails(this.videoBlockInfo.fileInfo.FullName);
                this.Dispatcher.Invoke(new Action(() =>
                {
                    this.img.Source = GetPic(img_path);
                }));
                    

            }).Start();

        }

        private static BitmapImage GetPic(string path)
        {
            var bmp = new BitmapImage(new Uri(path));
            return bmp;

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
#if DEBUG
            if (Host == null)
                return;
#endif
            this.Host.OnVideoBlockClick(this);
        }

        public void OnSelected()
        {
            btn.BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255,0,0,0));
            isSelected = true;
        }

        public void OnUnSelected()
        {
            btn.BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 0, 0, 0));
            isSelected = false;
        }

        public void AddFormat(string format)
        {
            if (this.convertParams.prepearFormats.Contains(format))
                return;
            this.convertParams.prepearFormats.Add(format);
        }

        public void RemoveFormat(string format)
        {
            this.convertParams.prepearFormats.Remove(format);
        }

        internal void BeginTransCode()
        {
            throw new NotImplementedException();
        }

        public void SetPercentage(int v)
        {
            this.pg.Value = v;
            this.pg_txt.Text = v.ToString() + "%";
        }
    }
}
