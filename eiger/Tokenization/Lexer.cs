/*
 * EIGERLANG LEXER (TOKENIZER)
 * WRITTEN BY VARDAN PETROSYAN
*/

using EigerLang.Errors;

namespace EigerLang.Tokenization;

public class Lexer
{
    string source; // the source code
    int ptr = 0; // the index pointer to the current char
    char current_char; // the current char
    int current_pos = 1; // the position in the line
    int current_line = 1; // the current line
    string path; // the path of the file

    // advance to next char
    void Advance()
    {
        current_pos++;
        ptr++;
        if (ptr < source.Length)
            current_char = source[ptr];
    }

    // reverse to previous char
    void Reverse()
    {
        ptr--;
        current_pos--;
        if (ptr < source.Length)
            current_char = source[ptr];
    }

    // make identifier
    Token MakeIdent()
    {
        string val = "";
        // while its a letter and in bounds
        while (char.IsLetter(current_char) && ptr < source.Length)
        {
            val += current_char;
            Advance();
            if (!char.IsLetter(current_char) || ptr >= source.Length)
            {
                Reverse();
                break;
            }
        }
        return new Token(current_line, current_pos, TokenType.IDENTIFIER, val);
    }

    // make number
    Token MakeNumber()
    {
        string strval = ""; // string representation of the number
        bool isFloat = false; // if its a floating point number

        do
        {
            if (current_char == '.') // if there's a dot
                isFloat = true; // it's a floating point number
            strval += current_char;
            Advance();
            if (!(ptr < source.Length && (char.IsDigit(current_char) || (current_char == '.' && !isFloat))))
            {
                Reverse(); break;
            }
        }
        while (ptr < source.Length && (char.IsDigit(current_char) || (current_char == '.' && !isFloat)));

        return new Token(current_line, current_pos, TokenType.NUMBER, isFloat ? Convert.ToDouble(strval) : Convert.ToInt32(strval));
    }

    // make string
    Token MakeString()
    {
        string val = "";
        Advance(); // advance through "
        // while the string is not closed and in bounds
        while (current_char != '"' && ptr < source.Length)
        {
            // if escape character
            if (current_char == '\\')
            {
                Advance();
                if (ptr >= source.Length) break;

                val += current_char switch
                {
                    '"' => '"',
                    '\\' => '\\',
                    'n' => '\n',
                    't' => '\t',
                    _ => "\\" + current_char,
                };
            }
            else
            {
                val += current_char;
            }
            Advance();
        }
        return new Token(current_line, current_pos, TokenType.STRING, val);
    }
    
    // skip line comment
    void SkipComment()
    {
        Advance();
        while(current_char != '\n' && ptr < source.Length)
        {
            Advance();
        }
    }

    // main function to tokenize code
    public List<Token> Tokenize()
    {
        List<Token> result = new List<Token>();

        // reset values (just in case)
        ptr = 0;
        current_char = source[ptr];
        current_pos = 1;
        current_line = 1;

        while (ptr < source.Length)
        {
            if (current_char == '\0') break;
            else if (current_char == ' ' || current_char == '\t' || current_char == '\r') // if it's a whitespace, ignore
            {
                Advance();
                continue;
            }
            else if (current_char == '~') // if it's a comment
            {
                SkipComment();
            }
            else if (current_char == '\n') // if it's a newline
            {
                current_line++;
                current_pos = 1;
                Advance();
                continue;
            }
            else if (char.IsLetter(current_char)) // if it's a letter outside a string
            {
                result.Add(MakeIdent());
            }
            else if (char.IsDigit(current_char)) // if it's a digit outside a string
            {
                result.Add(MakeNumber());
            }
            else if (current_char == '"') // if its a double quote (string)
            {
                result.Add(MakeString());
            }
            else if (current_char == '(') // if it's a left parenthasis
            {
                result.Add(new Token(current_line, current_pos, TokenType.LPAREN, "("));
            }
            else if (current_char == ']') // if it's a right parenthasis
            {
                result.Add(new Token(current_line, current_pos, TokenType.RSQUARE, "]"));
            }
            else if (current_char == '[') // if it's a left parenthasis
            {
                result.Add(new Token(current_line, current_pos, TokenType.LSQUARE, "["));
            }
            else if (current_char == ')') // if it's a right parenthasis
            {
                result.Add(new Token(current_line, current_pos, TokenType.RPAREN, ")"));
            }
            else if (current_char == '.') // if it's a dot
            {
                result.Add(new Token(current_line, current_pos, TokenType.DOT, "."));
            }
            else if (current_char == '+') // if it's a plus
            {
                Advance();
                if (current_char == '=') // if the next one is an equal sign
                    result.Add(new Token(current_line, current_pos, TokenType.PLUSEQ, "+="));
                else
                {
                    result.Add(new Token(current_line, current_pos, TokenType.PLUS, "+"));
                    Reverse();
                }
            }
            else if (current_char == '-') // if it's a minus
            {
                Advance();
                if (current_char == '=') // if the next one is an equal sign
                    result.Add(new Token(current_line, current_pos, TokenType.MINUSEQ, "-="));
                else
                {
                    result.Add(new Token(current_line, current_pos, TokenType.MINUS, "-"));
                    Reverse();
                }
            }
            else if (current_char == '*') // if it's a star
            {
                Advance();
                if (current_char == '=') // if the next one is an equal sign
                    result.Add(new Token(current_line, current_pos, TokenType.MULEQ, "*="));
                else
                {
                    result.Add(new Token(current_line, current_pos, TokenType.MUL, "*"));
                    Reverse();
                }
            }
            else if (current_char == '/') // if it's a division
            {
                Advance();
                if (current_char == '=') // if the next one is an equal sign
                    result.Add(new Token(current_line, current_pos, TokenType.DIVEQ, "/="));
                else
                {
                    result.Add(new Token(current_line, current_pos, TokenType.DIV, "/"));
                    Reverse();
                }
            }
            else if (current_char == '=') // if it's an equal sign
            {
                result.Add(new Token(current_line, current_pos, TokenType.EQ, "="));
            }
            else if (current_char == ',') // if it's a comma
            {
                result.Add(new Token(current_line, current_pos, TokenType.COMMA, ","));
            }
            else if (current_char == '>') // if it's a greater-than
            {
                Advance();
                if (current_char == '=') // if the next one is an equal sign
                    result.Add(new Token(current_line, current_pos, TokenType.GTE, ">="));
                else
                {
                    result.Add(new Token(current_line, current_pos, TokenType.GT, ">"));
                    Reverse();
                }
            }
            else if (current_char == '<') // if it's a less-than
            {
                Advance();
                if (current_char == '=') // if the next one is an equal sign
                    result.Add(new Token(current_line, current_pos, TokenType.LTE, "<="));
                else
                {
                    result.Add(new Token(current_line, current_pos, TokenType.LT, "<"));
                    Reverse();
                }
            }
            else if (current_char == '?') // if it's a question mark
            {
                Advance();
                if (current_char == '=') // if the next one is an equal sign 
                    result.Add(new Token(current_line, current_pos, TokenType.EQEQ, "?="));
                else
                    throw new EigerError(path, current_line, current_pos, $"{Globals.InvalidCharStr} {current_char}",EigerError.ErrorType.LexerError);
            }
            else if (current_char == '!') // if it's an exclamation mark
            {
                Advance();
                if (current_char == '=') // if the next one is an equal sign
                    result.Add(new Token(current_line, current_pos, TokenType.NEQEQ, "!="));
                else
                    throw new EigerError(path, current_line, current_pos, $"{Globals.InvalidCharStr} {current_char}", EigerError.ErrorType.LexerError);
            }
            else
            {
                throw new EigerError(path, current_line, current_pos, $"{Globals.InvalidCharStr} {current_char}", EigerError.ErrorType.LexerError);
            }
            Advance();
        }
        return result;
    }

    // constructor
    public Lexer(string source,string path)
    {
        this.source = source;
        this.path = path;
    }
}
