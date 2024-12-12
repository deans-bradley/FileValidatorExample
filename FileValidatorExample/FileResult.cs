namespace FileValidatorExample
{
    public class FileResult
    {
        public string FileName { get; set; } = String.Empty;
        public string FileType { get; set; } = String.Empty;
        public string BinarySignature { get; set; } = String.Empty;
        public bool IsValid { get; set; }
        public bool IsEmpty { get; set; }
    }
}
