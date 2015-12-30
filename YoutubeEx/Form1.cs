using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using YoutubeExtractor;
using System.Text.RegularExpressions;
using System.IO;
using System.Net;
using System.Threading;
using NReco.VideoConverter;


namespace YoutubeEx
{
    public partial class Form1 : Form
    {
        //Class variables
        int vidQuality;
        string link;

        private void DownloadAudio(IEnumerable<VideoInfo> videoInfos)
        {
            /*
             * We want the first extractable video with the highest audio quality.
             */
            VideoInfo video = videoInfos
                .Where(info => info.CanExtractAudio)
                .OrderByDescending(info => info.AudioBitrate)
                .First();


            /*
             * If the video has a decrypted signature, decipher it
             */
            if (video.RequiresDecryption)
            {
                DownloadUrlResolver.DecryptDownloadUrl(video);
            }

            /*
             * Create the audio downloader.
             * The first argument is the video where the audio should be extracted from.
             * The second argument is the path to save the audio file.
             */

            var audioDownloader = new AudioDownloader(video,
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                RemoveIllegalPathCharacters(video.Title) + video.AudioExtension));

            // Register the progress events. We treat the download progress as 85% of the progress
            // and the extraction progress only as 15% of the progress, because the download will
            // take much longer than the audio extraction.
            audioDownloader.DownloadProgressChanged += (sender, args) => progressBar1.Value = (int)args.ProgressPercentage;
            
            

            /*
             * Execute the audio downloader.
             * For GUI applications note, that this method runs synchronously.
             */
            audioDownloader.Execute();
        }

        private void DownloadVideo(IEnumerable<VideoInfo> videoInfos)
        {

            VideoInfo video = videoInfos
                .First(info => info.VideoType == VideoType.Mp4 && info.Resolution == vidQuality);
            textBox2.Text = video.Title;
            /*
             * If the video has a decrypted signature, decipher it
             */

            if (video.RequiresDecryption)
            {
                DownloadUrlResolver.DecryptDownloadUrl(video);
            }

            /*
             * Create the video downloader.
             * The first argument is the video to download.
             * The second argument is the path to save the video file.
             */
            var videoDownloader = new VideoDownloader(video,
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                RemoveIllegalPathCharacters(video.Title) + video.VideoExtension));

            // Register the ProgressChanged event and send the progress to progressbar(downloadbar)
            videoDownloader.DownloadProgressChanged += (sender, args) => progressBar1.Value = (int)args.ProgressPercentage;

            //Start download

            videoDownloader.Execute();
        }

        //Searches for the video when called + activate the download button.
        private void SearchVideo(IEnumerable<VideoInfo> videoInfos)
        {
            try
            {
                VideoInfo video = videoInfos
                .First(info => info.VideoType == VideoType.Mp4 && info.Resolution == vidQuality);
            textBox2.Text = video.Title;
            Button1.Enabled = true;
            Button4.Enabled = true;
            }
            catch (System.InvalidOperationException e)
            {
                MessageBox.Show("(" + e.Message + "):" + " The quality picked is not available.");
                
            }
            /*
            VideoInfo video = videoInfos
                .First(info => info.VideoType == VideoType.Mp4 && info.Resolution == vidQuality);
            textBox2.Text = video.Title;
            Button1.Enabled = true;
            Button4.Enabled = true; */
        }
        

        private static string RemoveIllegalPathCharacters(string path)
        {
            string regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            var r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
            return r.Replace(path, "");
        }
        
        public Form1()
        {
            InitializeComponent();
            Button1.Enabled = false;
            comboBox1.SelectedIndex = 4;
            button3.Enabled = false;
            Button4.Enabled = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
            link = textBox1.Text;
            IEnumerable<VideoInfo> videoInfos = DownloadUrlResolver.GetDownloadUrls(link, false);
            DownloadVideo(videoInfos);
            timer1.Enabled = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //Close button
            this.Close();
        }

        public IEnumerable<VideoInfo> videoInfos { get; set; }

        
        //Search for the video.
        private void button3_Click(object sender, EventArgs e)
        {
            link = textBox1.Text;
            IEnumerable<VideoInfo> videoInfos = DownloadUrlResolver.GetDownloadUrls(link, false);
            SearchVideo(videoInfos);
        }

        //setting video quality
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex==0)
            {
                vidQuality = 144;
            }
            else if (comboBox1.SelectedIndex == 1)
            {
                vidQuality = 240;
            }
            else if (comboBox1.SelectedIndex == 2)
            {
                vidQuality = 360;
            }
            else if (comboBox1.SelectedIndex == 3)
            {
                vidQuality = 480;
            }
            else if (comboBox1.SelectedIndex == 4)
            {
                vidQuality = 720;
            }
            else if (comboBox1.SelectedIndex == 5)
            {
                vidQuality = 1080;
            }
            
        }

        //Reseting the download bar for a new download.
        private void timer1_Tick(object sender, EventArgs e)
        {
            progressBar1.PerformStep();
            {
                if (progressBar1.Value == 100)
                {
                    timer1.Enabled = false;
                    DialogResult result = MessageBox.Show("Download Complete!","Download", MessageBoxButtons.OK);
                    //This "if" was my best try at reseting the progressbar AFTER clicking ok. It's purely a graphical improvement.
                    if (result == DialogResult.OK) {
                        progressBar1.Value = 0;
                        
                    }
                }
                
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (textBox1.Text.Contains("https://www.youtube.com") || textBox1.Text.Contains("http://www.youtube.com"))
            {
                button3.Enabled = true;
            }
            else
            {
                button3.Enabled = false;
            }
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            link = textBox1.Text;
            IEnumerable<VideoInfo> videoInfos = DownloadUrlResolver.GetDownloadUrls(link, false);
            DownloadAudio(videoInfos);
            timer1.Enabled = true;
            
        }
        

        
        
    }
}
