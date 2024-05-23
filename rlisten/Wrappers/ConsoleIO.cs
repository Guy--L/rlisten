using System.IO;

namespace rlisten.Wrappers;

public interface IConsoleIO
{
    void Write(string message);
    void WriteLine(string message);
    string ReadLine();
    ConsoleKeyInfo ReadKey(bool intercept);
    ConsoleColor ForegroundColor { get; set; }
}

public class ConsoleIO : IConsoleIO
{
    public void Write(string message) => Console.Write(message);
    public void WriteLine(string message) => Console.WriteLine(message);
    public string ReadLine() => Console.ReadLine();
    public ConsoleKeyInfo ReadKey(bool intercept) => Console.ReadKey(intercept);
    public ConsoleColor ForegroundColor
    {
        get => Console.ForegroundColor;
        set => Console.ForegroundColor = value;
    }
}
