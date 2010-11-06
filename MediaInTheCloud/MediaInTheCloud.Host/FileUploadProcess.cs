// based from http://silverlightfileupld.codeplex.com/

using System.IO;
using System.Web;
using System.Threading;

namespace MediaInTheCloud.Host
{
    public delegate void FileUploadCompletedEvent(object sender, FileUploadCompletedEventArgs args);
    public class FileUploadCompletedEventArgs
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }

        public FileUploadCompletedEventArgs() { }

        public FileUploadCompletedEventArgs(string fileName, string filePath)
        {
            FileName = fileName;
            FilePath = filePath;
        }
    }

    public class FileUploadProcess
    {
        int bitsPerSample;
        int sampleRate;
        int channels;

        public event FileUploadCompletedEvent FileUploadCompleted;

        public FileUploadProcess()
        {
        }

        public void ProcessRequest(HttpContextBase context, string uploadPath)
        {
            string filename = context.Request.QueryString["filename"];

            int.TryParse(context.Request.QueryString["bitspersample"], out bitsPerSample);
            int.TryParse(context.Request.QueryString["samplerate"], out sampleRate);
            int.TryParse(context.Request.QueryString["channels"], out channels);

            bool complete = string.IsNullOrEmpty(context.Request.QueryString["Complete"]) ? true : bool.Parse(context.Request.QueryString["Complete"]);
            bool getBytes = string.IsNullOrEmpty(context.Request.QueryString["GetBytes"]) ? false : bool.Parse(context.Request.QueryString["GetBytes"]);
            long startByte = string.IsNullOrEmpty(context.Request.QueryString["StartByte"]) ? 0 : long.Parse(context.Request.QueryString["StartByte"]); ;

            string filePath = Path.Combine(uploadPath, filename);

            if (getBytes)
            {
                FileInfo fi = new FileInfo(filePath);
                if (!fi.Exists)
                    context.Response.Write("0");
                else
                    context.Response.Write(fi.Length.ToString());

                context.Response.Flush();
                return;
            }
            else
            {

                if (startByte > 0 && File.Exists(filePath))
                {
                    using (FileStream fs = File.Open(filePath, FileMode.Append))
                    {
                        SaveFile(context.Request.InputStream, fs);
                        fs.Close();
                    }
                }
                else
                {
                    using (FileStream fs = File.Create(filePath))
                    {
                        SaveFile(context.Request.InputStream, fs);
                        fs.Close();
                    }
                }
                if (complete)
                {
                    if (FileUploadCompleted != null)
                    {
                        FileUploadCompletedEventArgs args = new FileUploadCompletedEventArgs(filename, filePath);
                        FileUploadCompleted(this, args);
                    }
                }
            }
        }

        void SaveFile(Stream stream, FileStream fs)
        {
            byte[] buffer = new byte[4096];
            int bytesRead;
            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
            {
                Thread.Sleep(100);
                fs.Write(buffer, 0, bytesRead);
            }
        }

    }

}

