/*
 * EIGERLANG LEXER (TOKENIZER)
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
        while ((char.IsLetter(current_char) || current_char == '_') && ptr < source.Length)
        {
            val += current_char;
            Advance();
            if (!(char.IsLetter(current_char) || current_char == '_') || ptr >= source.Length)
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
            {
                if (ptr + 1 < source.Length && char.IsDigit(source[ptr + 1]))
                    isFloat = true; // it's a floating point number
                else { Reverse(); break; };
            }

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
        while (current_char != '\n' && ptr < source.Length)
        {
            Advance();
        }
    }

    // main function to tokenize code
    public List<Token> Tokenize()
    {
        List<Token> result = [];
        ptr = 0;

        if (source.Length == 0) return result;

        current_char = source[ptr];
        current_pos = 1;
        current_line = 1;

        while (ptr < source.Length)
        {
            if (current_char == '\0') break;
            else if (current_char == '~')
                SkipComment();
            else if (current_char == '\n')
            {
                current_line++;
                current_pos = 1;
            }
            else if (char.IsWhiteSpace(current_char))
            { }
            else if (char.IsLetter(current_char) || current_char == '_')
                result.Add(MakeIdent());
            else if (char.IsDigit(current_char))
                result.Add(MakeNumber());
            else if (current_char == '"')
                result.Add(MakeString());
            else
                HandleSpecialCharacters(result);

            Advance();
        }
        return result;
    }

    private void HandleSpecialCharacters(List<Token> result)
    {
        var singleCharTokens = new Dictionary<char, TokenType>
        {
            { '(', TokenType.LPAREN },
            { ')', TokenType.RPAREN },
            { '[', TokenType.LSQUARE },
            { ']', TokenType.RSQUARE },
            { '.', TokenType.DOT },
            { '^', TokenType.CARET },
            { '%', TokenType.PERC },
            { ',', TokenType.COMMA },
            { '=', TokenType.EQ }
        };

        var compoundCharTokens = new Dictionary<char, (TokenType single, TokenType compound)>
        {
            { '+', (TokenType.PLUS, TokenType.PLUSEQ) },
            { '-', (TokenType.MINUS, TokenType.MINUSEQ) },
            { '*', (TokenType.MUL, TokenType.MULEQ) },
            { '/', (TokenType.DIV, TokenType.DIVEQ) },
            { '>', (TokenType.GT, TokenType.GTE) },
            { '<', (TokenType.LT, TokenType.LTE) }
        };

        if (singleCharTokens.TryGetValue(current_char, out TokenType value))
            result.Add(new Token(current_line, current_pos, value, current_char.ToString()));
        else if (compoundCharTokens.TryGetValue(current_char, out (TokenType single, TokenType compound) value2))
            HandleCompoundOrSingleToken(result, value2.single, value2.compound, current_char);
        else if (current_char == '?' || current_char == '!')
            HandleInvalidCompoundToken(result, current_char == '?' ? TokenType.EQEQ : TokenType.NEQEQ, current_char);
        else
            throw new EigerError(path, current_line, current_pos, $"{Globals.InvalidCharStr} {current_char}", EigerError.ErrorType.LexerError);
    }


    private void HandleCompoundOrSingleToken(List<Token> result, TokenType singleTokenType, TokenType compoundTokenType, char compoundChar)
    {
        Advance();
        if (current_char == '=')
            result.Add(new Token(current_line, current_pos, compoundTokenType, $"{compoundChar}="));
        else
        {
            result.Add(new Token(current_line, current_pos, singleTokenType, $"{compoundChar}"));
            Reverse();
        }
    }

    private void HandleInvalidCompoundToken(List<Token> result, TokenType compoundTokenType, char compoundChar)
    {
        Advance();
        if (current_char == '=')
            result.Add(new Token(current_line, current_pos, compoundTokenType, $"{compoundChar}="));
        else
        {
            throw new EigerError(path, current_line, current_pos, $"{Globals.InvalidCharStr} {compoundChar}", EigerError.ErrorType.LexerError);
        }
    }


    // constructor
    public Lexer(string source, string path)
    {
        this.source = source;
        this.path = path;
    }
}
