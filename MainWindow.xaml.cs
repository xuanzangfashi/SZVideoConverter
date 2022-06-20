using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
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
using static SZ_FFmpegLibrary.SZFFmpeg;

namespace SZVideoConverter_WPF
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private KeyboardManager keyboardManager;
        private bool isControlDown = false;
        List<VideoBlock> VideoBlocks = new List<VideoBlock>();
        Dictionary<string, VideoBlock> VideoBlocksIdDictionary = new Dictionary<string, VideoBlock>();
        Dictionary<string, VideoBlock> VideoBlocksPathDictionary = new Dictionary<string, VideoBlock>();
        List<VideoBlock> SelectedVideoBlocks = new List<VideoBlock>();
        Dictionary<string, CheckBox> CheckBoxDictionary = new Dictionary<string, CheckBox>();
        private bool disableCheckBoxCallback = false;
        public MainWindow()
        {
            this.InitializeComponent();
            SZ_FFmpegLibrary.SZFFmpeg.InitFFmpeg();

            #region keyboard manager init
            keyboardManager = new KeyboardManager(this);
            keyboardManager.BindKeyTrigger(System.Windows.Input.Key.LeftCtrl, (key, state) =>
            {
                if (state == KeyboardManager.KeyState.Down)
                {
                    isControlDown = true;
                }
                else
                {
                    isControlDown = false;
                }
            });
            #endregion

            #region video format options
            foreach (var i in CheckBoxContiner.Children)
            {
                var cb = i as CheckBox;
                if (cb != null)
                {
                    cb.Checked += Cb_Checked;
                    cb.Unchecked += Cb_Unchecked;
                    CheckBoxDictionary.Add(cb.Content.ToString(), cb);
                }
            }
            #endregion

            #region conver pixel format options
            foreach (var i in Enum.GetValues(typeof(Xabe.FFmpeg.PixelFormat)))
            {
                this.PixelFormatComboBox.Items.Add(i.ToString());
            }
            #endregion



#if false
            #region debug
            foreach(var i in VideosContiner.Children)
            {
                var vd = i as VideoBlock;
                if(vd != null)
                {
                    vd.InitVideoBlock("", this);
                    VideoBlocks.Add(vd);
                }
            }
            
            #endregion
#endif
        }

        

        private void Cb_Unchecked(object sender, RoutedEventArgs e)
        {
            if (disableCheckBoxCallback)
                return;
            var cb = sender as CheckBox;
            foreach (var i in SelectedVideoBlocks)
            {
                i.RemoveFormat(cb.Content.ToString());
            }
        }

        private void Cb_Checked(object sender, RoutedEventArgs e)
        {
            if (disableCheckBoxCallback)
                return;
            var cb = sender as CheckBox;
            foreach (var i in SelectedVideoBlocks)
            {
                i.AddFormat(cb.Content.ToString());
            }
        }

        internal void OnVideoBlockClick(VideoBlock videoBlock)
        {
            if (!videoBlock.isSelected)
            {
                videoBlock.OnSelected();
                SelectedVideoBlocks.Add(videoBlock);
                if (!isControlDown)
                {
                    foreach (var i in VideoBlocks)
                    {
                        if (i == videoBlock)
                            continue;
                        i.OnUnSelected();
                        SelectedVideoBlocks.Remove(i);
                    }
                }
            }
            else
            {
                if (!isControlDown && SelectedVideoBlocks.Count > 1)
                {
                    foreach (var i in VideoBlocks)
                    {
                        if (i == videoBlock)
                            continue;
                        i.OnUnSelected();
                        SelectedVideoBlocks.Remove(i);
                    }
                    goto End;
                }
                videoBlock.OnUnSelected();
                SelectedVideoBlocks.Remove(videoBlock);
            }
        End:;
            if (SelectedVideoBlocks.Count > 0)
            {
                SetEnableOperationGroup(true);
            }
            else
            {
                SetEnableOperationGroup(false);
            }
            UpdateCheckBoxState();
            UpdateVideoDetails();
        }

        void UpdateVideoDetails()
        {
            if (SelectedVideoBlocks.Count > 1)
                return;
            var vb = SelectedVideoBlocks[0];
            Duration.Text = "Duration:" + vb.videoBlockInfo.mediaInfo.Duration.ToString();
            Size.Text = "Size:" + vb.videoBlockInfo.mediaInfo.Size.ToString();
            CreateTime.Text = "Create Time:" + vb.videoBlockInfo.mediaInfo.CreationTime.ToString();
            Path.Text = "Path:" + vb.videoBlockInfo.mediaInfo.Path.ToString();

            if (vb.videoBlockInfo.mediaInfo.VideoStreams.Count() > 0)
            {
                Width.Text = "Width:" + vb.videoBlockInfo.mediaInfo.VideoStreams.ToList()[0].Width.ToString();
                Height.Text = "Height:" + vb.videoBlockInfo.mediaInfo.VideoStreams.ToList()[0].Height.ToString();
                Framerate.Text = "Framerate:" + vb.videoBlockInfo.mediaInfo.VideoStreams.ToList()[0].Framerate.ToString();
                Ratio.Text = "Ratio:" + vb.videoBlockInfo.mediaInfo.VideoStreams.ToList()[0].Ratio.ToString();
                VideoBitrate.Text = "Bitrate:" + vb.videoBlockInfo.mediaInfo.VideoStreams.ToList()[0].Bitrate.ToString();
                PixelFormat.Text = "PixelFormat:" + vb.videoBlockInfo.mediaInfo.VideoStreams.ToList()[0].PixelFormat.ToString();
                Rotation.Text = "Rotation:" + vb.videoBlockInfo.mediaInfo.VideoStreams.ToList()[0].Rotation.ToString();
            }

            if (vb.videoBlockInfo.mediaInfo.AudioStreams.Count() > 0)
            {
                AudioBitrate.Text = "Bitrate:" + vb.videoBlockInfo.mediaInfo.AudioStreams.ToList()[0].Bitrate.ToString();
                SampleRate.Text = "SampleRate:" + vb.videoBlockInfo.mediaInfo.AudioStreams.ToList()[0].SampleRate.ToString();
                Channels.Text = "Channels:" + vb.videoBlockInfo.mediaInfo.AudioStreams.ToList()[0].Channels.ToString();
                AudioLanguage.Text = "Language:" + vb.videoBlockInfo.mediaInfo.AudioStreams.ToList()[0].Language.ToString();
                //AudioTitle.Text = "Title:" + vb.videoBlockInfo.mediaInfo.AudioStreams.ToList()[0].Title.ToString();
            }

            if (vb.videoBlockInfo.mediaInfo.SubtitleStreams.Count() > 0)
            {
                SubtitleLanguage.Text = "Language:" + vb.videoBlockInfo.mediaInfo.SubtitleStreams.ToList()[0].Language.ToString();
                //6SubtitleTitle.Text = "Title:" + vb.videoBlockInfo.mediaInfo.SubtitleStreams.ToList()[0].Title.ToString();
            }
        }
        void UpdateCheckBoxState()
        {
            disableCheckBoxCallback = true;
            List<string> temp_formats = new List<string>();
            List<string> format_times = new List<string>();
            foreach (var i in SelectedVideoBlocks)
            {
                temp_formats.AddRange(i.convertParams.prepearFormats.ToArray());
            }
            foreach (var i in CheckBoxDictionary)
            {
                if (temp_formats.Contains(i.Value.Content.ToString()))
                {
                    if (format_times.Contains(i.Value.Content.ToString()))
                    {
                        i.Value.IsChecked = null;
                    }
                    else
                    {
                        i.Value.IsChecked = true;
                        format_times.Add(i.Value.Content.ToString());
                    }
                }
                else
                {
                    i.Value.IsChecked = false;
                }
            }
            disableCheckBoxCallback = false;
        }

        void SetEnableOperationGroup(bool isEnable)
        {
            foreach (var i in CheckBoxDictionary)
            {
                var cb = i.Value;
                if (cb != null)
                {
                    cb.IsEnabled = isEnable;
                }
            }
            transcode_btn.IsEnabled = isEnable;
            remove_item_btn.IsEnabled = isEnable;
        }

        private void VideosContiner_DragEnter(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
            //e.AcceptedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Copy;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            List<ConvertMissionInfo> convertMissionInfos = new List<ConvertMissionInfo>();
            foreach (var i in VideoBlocks)
            {
                foreach (var j in i.convertParams.prepearFormats)
                {
                    ConvertMissionInfo temp_convertMissionInfo = new ConvertMissionInfo();
                    temp_convertMissionInfo.id = i.videoBlockInfo.id;
                    temp_convertMissionInfo.input_path = i.videoBlockInfo.fileInfo.FullName;
                    temp_convertMissionInfo.output_path = System.IO.Path.ChangeExtension(i.videoBlockInfo.fileInfo.FullName, j);
                    temp_convertMissionInfo.pix_fmt = i.convertParams.pixelFormat;
                    convertMissionInfos.Add(temp_convertMissionInfo);
                }
            }

            SZ_FFmpegLibrary.SZFFmpeg.ConvertVideos(convertMissionInfos.ToArray(), new Action<int, string>((p, i) =>
               {
                   this.Dispatcher.Invoke(new Action(() =>
                           {
                               VideoBlocksIdDictionary[i].SetPercentage(p);
                           }));
               }));
        }

        private void VideosContiner_Drop(object sender, DragEventArgs e)
        {
            var files = ((System.Array)e.Data.GetData(System.Windows.DataFormats.FileDrop));
            //= await e.DataView.GetStorageItemsAsync();
            if (files != null && files.Length > 0)
            {
                foreach (string i in files)
                {
                    FileInfo item = new FileInfo(i);


                    if (VideoBlocksPathDictionary.ContainsKey(item.FullName))
                        continue;

                    var filetype = item.Extension.ToLower();
                    if (filetype == ".mp4" ||
                        filetype == ".avi" ||
                        filetype == ".mov" ||
                        filetype == ".mkv" ||
                        filetype == ".flv" ||
                        filetype == ".wmv")
                    {
                        var temp_id = Guid.NewGuid().ToString();
                        var temp_path = item.FullName;
                        VideoBlock videoBlock = new VideoBlock();
                        videoBlock.InitVideoBlock(item, temp_id, this);
                        Grid.SetRow(videoBlock, VideosContiner.Children.Count / 4);
                        Grid.SetColumn(videoBlock, VideosContiner.Children.Count % 4);
                        VideosContiner.Children.Add(videoBlock);
                        VideoBlocks.Add(videoBlock);
                        VideoBlocksIdDictionary.Add(temp_id, videoBlock);
                        VideoBlocksPathDictionary.Add(temp_path, videoBlock);
                    }
                }
            }
        }

        private void remove_item_btn_Click(object sender, RoutedEventArgs e)
        {
            while (SelectedVideoBlocks.Count > 0)
            {
                var temp = SelectedVideoBlocks[0];
                VideoBlocks.Remove(temp);
                VideoBlocksIdDictionary.Remove(temp.videoBlockInfo.id);
                VideoBlocksPathDictionary.Remove(temp.videoBlockInfo.fileInfo.FullName);
                VideosContiner.Children.Remove(temp);
                SelectedVideoBlocks.RemoveAt(0);
            }
            for (int i = 0; i < VideosContiner.Children.Count; i++)
            {
                Grid.SetRow(VideosContiner.Children[i] as FrameworkElement, i / 4);
                Grid.SetColumn(VideosContiner.Children[i] as FrameworkElement, i % 4);
            }
        }

        private void PixelFormatComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach(var i in SelectedVideoBlocks)
            {
                i.convertParams.pixelFormat = (Xabe.FFmpeg.PixelFormat)Enum.Parse(typeof(Xabe.FFmpeg.PixelFormat),e.Source as string);
            }
        }
    }
}
