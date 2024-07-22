namespace AbsenceNotifier.Core.Services.FilesManager
{
    public class FileManager
    {
        private string? _filePath;

        public void SetFilePath(string filePath)
        {
            _filePath = filePath;
        }

        public void WriteLine(string text)
        {
            if (string.IsNullOrWhiteSpace(_filePath))
            {
                throw new ArgumentNullException(nameof(_filePath));
            }

            using (var outputFile = new StreamWriter(Path.Combine(_filePath), true) )
            {
                outputFile.WriteLine(text);
            }
        }

        public string ReadAllLines()
        {
            if (string.IsNullOrWhiteSpace(_filePath))
            {
                throw new ArgumentNullException(nameof(_filePath));
            }

            using (var reader = new StreamReader(Path.Combine(_filePath)))
            {
                return reader.ReadToEnd();
            }
        }

    }
}
