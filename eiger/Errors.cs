/*
 * EIGERLANG ERRORS
*/

namespace EigerLang.Errors;

public class EigerError : Exception
{
    string? file; // the file of the error
    int? line, pos; // the position of the error
    string? message; // the message
    ErrorType type = ErrorType.RuntimeError;

    public enum ErrorType
    {
        RuntimeError, IndexError, ParserError, LexerError, ZeroDivisionError, ArgumentError, InvalidOperationError, IOError
    }

    public EigerError(string file, int line, int pos, string message, ErrorType type) : base($"Error in file {file} at line {line}, {pos}:\n{type}: {message}")
    {
        this.file = file;
        this.line = line;
        this.pos = pos;
        this.message = message;
        this.type = type;
    }
}
