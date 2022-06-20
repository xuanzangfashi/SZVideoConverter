using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xabe.FFmpeg;

namespace SZ_FFmpegLibrary
{
    public class SZFFmpeg
    {
        public struct ConvertMissionInfo
        {
            public string input_path;
            public string output_path;
            public string id;
            public PixelFormat pix_fmt;
        }
        public static void InitFFmpeg()
        {
            var exepath = Path.Combine(Environment.CurrentDirectory, "ffmpeg", "bin");
            FileInfo fi = new FileInfo(exepath);
            FFmpeg.SetExecutablesPath(fi.FullName);
        }

        public static async Task<IMediaInfo> GetVideoDetails(string path)
        {
            var mediaInfo = await FFmpeg.GetMediaInfo(path);
            return mediaInfo;
        }
        public static async Task ConvertVideo_Async(ConvertMissionInfo info, Action<int,string> on_progress)
        {
            var conversion = await FFmpeg.Conversions.FromSnippet.Convert(info.input_path, info.output_path);
            var conversion1 = FFmpeg.Conversions.New();
            
            conversion.OnProgress += (sender, e) =>
            {
                on_progress(e.Percent, info.id);
            };
            await conversion.Start();
        }


        public static void ConvertVideos(ConvertMissionInfo[] infos, Action<int, string> on_progress)
        {
            new Task(() =>
            {
                List<Task> missions = new List<Task> { };
                var processor_count = Environment.ProcessorCount;
                var done_count = 0;
                for (int i = 0; i < infos.Length; i++)
                {
                    var info = infos[i];
                    new Task(async () =>
                    {
                        await ConvertVideo_Async(info, on_progress);
                        on_progress(100, info.id);
                        done_count++;
                    }).Start();
                    while (processor_count == (i + 1 - done_count))
                    {
                    }
                }
            }).Start();
        }

        public static async Task<string> GetVideoSnapshotAsync(string input_path)
        {
            var output_path = "./temp/" + Guid.NewGuid().ToString() + ".jpg";
            FileInfo fileInfo = new FileInfo(output_path);
            var conversion = await FFmpeg.Conversions.FromSnippet.Snapshot(input_path, fileInfo.FullName, TimeSpan.FromSeconds(0));
            var result = await conversion.Start();
            return fileInfo.FullName;
        }
    }
}
