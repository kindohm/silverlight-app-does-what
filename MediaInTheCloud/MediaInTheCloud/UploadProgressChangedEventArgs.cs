//based from http://silverlightfileupld.codeplex.com/

namespace MediaInTheCloud
{
    public delegate void ProgressChangedEvent(object sender, UploadProgressChangedEventArgs args);

    public class UploadProgressChangedEventArgs
    {
        public UploadProgressChangedEventArgs() { }

        public UploadProgressChangedEventArgs(
            int progressPercentage, long bytesUploaded, long totalBytesUploaded, long totalBytes, string fileName)
        {
            ProgressPercentage = progressPercentage;
            BytesUploaded = bytesUploaded;
            TotalBytes = totalBytes;
            FileName = fileName;
            TotalBytesUploaded = totalBytesUploaded;
        }

        public int ProgressPercentage { get; set; }
        public long BytesUploaded { get; set; }
        public long TotalBytesUploaded { get; set; }
        public long TotalBytes { get; set; }
        public string FileName { get; set; }

    }
}
