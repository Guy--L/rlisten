using System.IO;

namespace rlisten.Services;

public interface IFileProvider
{
    string ReadAllText(string path);
    void WriteAllText(string path, string contents);
}

public class PhysicalFileProvider : IFileProvider
{
    public string ReadAllText(string path)
    {
        return File.ReadAllText(path);
    }

    public void WriteAllText(string path, string contents)
    {
        File.WriteAllText(path, contents);
    }
}
