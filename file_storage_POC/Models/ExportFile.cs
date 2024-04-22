using file_storage_POC.Enums;

namespace file_storage_POC.Models
{
    public class ExportFile 
    {
        public ExportFileType FileType { get; set; }
        public string FileName { get; set; }
        public byte[] FileContent { get; set; }
    }
}
