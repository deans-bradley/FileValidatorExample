﻿using FileValidatorExample;

while (true)
{
    Main();
}

void Main()
{
    // Assuming your VS solution is located in the following path
    string filePath = $@"C:\Users\{Environment.UserName}\source\repos\FileValidatorExample\FileValidatorExample\TestDocuments\";

    string[] folders = GetFolders(filePath);

    int i = 1;
    foreach (string folder in folders)
    {
        Console.WriteLine($"{i}. {folder}");
        i++;
    }

    Console.Write("Select file type:");
    string fileType = folders[int.Parse(Console.ReadLine() ?? "") - 1];
    filePath += $@"/{fileType}";
    Console.WriteLine();

    string[] files = GetFiles(filePath);

    i = 1;
    foreach (string file in files)
    {
        Console.WriteLine($"{i}. {file}");
        i++;
    }

    Console.Write("Select file name:");
    string fileName = files[int.Parse(Console.ReadLine() ?? "") - 1];
    filePath += $@"/{fileName}";

    // You can find other binary signatures by visisting: https://www.garykessler.net/library/file_sigs.html
    Dictionary<string, byte[]> allowedFileTypes = new Dictionary<string, byte[]>
    {
        { "doc", new byte[] { 0xD0, 0xCF, 0x11, 0xE0 } }, // .doc files
        { "docx", new byte[] {0x50, 0x4B, 0x03, 0x04} },  // .docx files
        { "pdf", new byte[] { 0x25, 0x50, 0x44, 0x46 } }, // .pdf files
        { "jpg", new byte[] { 0xFF, 0xD8, 0xFF } },       // .jpg files
        { "png", new byte[] { 0x89, 0x50, 0x4E, 0x47 } },  // .png files
        { "tif", new byte[] { 0x49, 0x49, 0x2A, 0x00 } }, // .tif files 
        { "tiff", new byte[] { 0x4D, 0x4D, 0x00, 0x2B } }, // .tiff files
        { "bmp", new byte[] { 0x42, 0x4D } } // .bmp files                                                      
    };

    Console.WriteLine("\n1. Validate by path");
    Console.WriteLine("2. Validate by Base64 string");
    Console.Write("Select an option:");
    int opt = int.Parse(Console.ReadLine() ?? "1");

    FileResult? fileResult = new FileResult();

    if (opt == 1)
    {
        fileResult = ValidateFileTypeByPath(filePath, allowedFileTypes);
    }
    else if (opt == 2)
    {
        byte[] fileBytes = File.ReadAllBytes(filePath);
        string base64String = Convert.ToBase64String(fileBytes);
        fileResult = ValidateFileTypeByBase64(base64String, fileName, allowedFileTypes);
    }
        

    if (fileResult != null)
    {
        Console.WriteLine();
        Console.WriteLine("File name: " + fileResult.FileName);
        Console.WriteLine("File type: " + fileResult.FileType);
        Console.WriteLine("Binary signature: " + fileResult.BinarySignature);
        Console.WriteLine("Is valid: " + fileResult.IsValid);
        Console.WriteLine("Is empty: " + fileResult.IsEmpty);
    }
    else
    {
        Console.WriteLine("File does not exist.");
    }

    Console.ReadLine();
}

FileResult? ValidateFileTypeByPath(string filePath, Dictionary<string, byte[]> allowedFileTypes)
{
    FileResult fileResult = new FileResult();

    if (!File.Exists(filePath))
    {
        return null;
    }

    try
    {
        using FileStream reader = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        var buffer = new byte[allowedFileTypes.Values.Max(sig => sig.Length)];
        int bytesRead = reader.Read(buffer, 0, buffer.Length);

        if (bytesRead == 0)
        {
            fileResult.IsEmpty = true;
            return fileResult;
        }

        fileResult.FileName = Path.GetFileName(filePath);
        fileResult.FileType = Path.GetExtension(fileResult.FileName).TrimStart('.');
        fileResult.BinarySignature = BitConverter.ToString(buffer.Take(bytesRead).ToArray()).Replace("-", "");

        fileResult.IsValid = allowedFileTypes.Any(type =>
            type.Value.SequenceEqual(buffer.Take(type.Value.Length)));

        fileResult.IsEmpty = false;

        return fileResult;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred: {ex.Message}");
        return null;
    }
}

// Only passing fileName to display in the result
FileResult? ValidateFileTypeByBase64(string base64String, string fileName, Dictionary<string, byte[]> allowedFileTypes)
{
    FileResult fileResult = new FileResult();

    try
    {
        byte[] fileBytes = Convert.FromBase64String(base64String);

        if (fileBytes.Length == 0)
        {
            fileResult.IsEmpty = true;
            return fileResult;
        }

        var buffer = fileBytes.Take(allowedFileTypes.Values.Max(sig => sig.Length)).ToArray();

        fileResult.FileName = fileName;
        fileResult.FileType = Path.GetExtension(fileName).TrimStart('.');
        fileResult.BinarySignature = BitConverter.ToString(buffer).Replace("-", "");

        fileResult.IsValid = allowedFileTypes.Any(type =>
            type.Value.SequenceEqual(buffer.Take(type.Value.Length)));

        fileResult.IsEmpty = false;

        return fileResult;
    }
    catch (FormatException)
    {
        Console.WriteLine("Invalid Base64 string provided.");
        return null;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred: {ex.Message}");
        return null;
    }
}


string[] GetFolders(string filePath)
{
    string[] directories = Directory.GetDirectories(filePath);

    List<string> folderNames = new List<string>();

    foreach (string dir in directories)
    {
        folderNames.Add(Path.GetFileName(dir));
    }

    return folderNames.ToArray();
}

string[] GetFiles(string filePath)
{
    string[] files = Directory.GetFiles(filePath);

    List<string> fileNames = new List<string>();

    foreach (string file in files)
    {
        fileNames.Add(Path.GetFileName(file));
    }

    return fileNames.ToArray();
}