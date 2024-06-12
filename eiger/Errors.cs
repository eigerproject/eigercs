namespace EigerLang.Errors;

public class EigerError : Exception
{
    string? file;
    int? line, pos;
    string? message;

    public EigerError(string file, int line, int pos, string message) : base($"Error in file {file} at line {line}, {pos}:\n{message}")
    {
        this.file = file;
        this.line = line;
        this.pos = pos;
        this.message = message;
    }

    public EigerError(string error) : base(error) { }
}
