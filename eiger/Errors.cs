/*
 * EIGERLANG ERRORS
 * WRITTEN BY VARDAN PETROSYAN
*/

namespace EigerLang.Errors;

public class EigerError : Exception
{
    string? file; // the file of the error
    int? line, pos; // the position of the error
    string? message; // the message

    public EigerError(string file, int line, int pos, string message) : base($"Error in file {file} at line {line}, {pos}:\n{message}")
    {
        this.file = file;
        this.line = line;
        this.pos = pos;
        this.message = message;
    }
}
