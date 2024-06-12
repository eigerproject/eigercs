
using EigerLang.Errors;

namespace EigerLang.Tokenization;

public class Lexer
{
    string source;
    int ptr = 0;
    char current_char;
    int current_pos = 1;
    int current_line = 1;

    void Advance()
    {
        current_pos++;
        ptr++;
        if (ptr < source.Length)
            current_char = source[ptr];
    }

    void Reverse()
    {
        ptr--;
        current_pos--;
        if (ptr < source.Length)
            current_char = source[ptr];
    }

    Token MakeIdent()
    {
        string val = "";
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

    Token MakeNumber()
    {
        string strval = "";
        bool isFloat = false;

        do
        {
            if (current_char == '.')
                isFloat = true;
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

    Token MakeString()
    {
        string val = "";
        Advance();
        while (current_char != '"' && ptr < source.Length)
        {
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

    void SkipComment()
    {
        Advance();
        while(current_char != '\n' && ptr < source.Length)
        {
            Advance();
        }
    }

    public List<Token> Tokenize()
    {
        List<Token> result = new List<Token>();

        ptr = 0;
        current_char = source[ptr];
        current_pos = 1;
        current_line = 1;

        while (ptr < source.Length)
        {
            if (current_char == '\0') break;
            else if (current_char == ' ' || current_char == '\t')
            {
                Advance();
                continue;
            }
            else if (current_char == '~')
            {
                SkipComment();
            }
            else if (current_char == '\n')
            {
                current_line++;
                current_pos = 1;
                Advance();
                continue;
            }
            else if (char.IsLetter(current_char))
            {
                result.Add(MakeIdent());
            }
            else if (char.IsDigit(current_char))
            {
                result.Add(MakeNumber());
            }
            else if (current_char == '"')
            {
                result.Add(MakeString());
            }
            else if (current_char == '(')
            {
                result.Add(new Token(current_line, current_pos, TokenType.LPAREN, "("));
            }
            else if (current_char == ')')
            {
                result.Add(new Token(current_line, current_pos, TokenType.RPAREN, ")"));
            }
            else if (current_char == '+')
            {
                result.Add(new Token(current_line, current_pos, TokenType.PLUS, "+"));
            }
            else if (current_char == '-')
            {
                result.Add(new Token(current_line, current_pos, TokenType.MINUS, "-"));
            }
            else if (current_char == '*')
            {
                result.Add(new Token(current_line, current_pos, TokenType.MUL, "*"));
            }
            else if (current_char == '/')
            {
                result.Add(new Token(current_line, current_pos, TokenType.DIV, "/"));
            }
            else if (current_char == '=')
            {
                result.Add(new Token(current_line, current_pos, TokenType.EQ, "="));
            }
            else if (current_char == ',')
            {
                result.Add(new Token(current_line, current_pos, TokenType.COMMA, ","));
            }
            else if (current_char == '>')
            {
                Advance();
                if (current_char == '=')
                    result.Add(new Token(current_line, current_pos, TokenType.GTE, ">="));
                else
                {
                    result.Add(new Token(current_line, current_pos, TokenType.GT, ">"));
                    Reverse();
                }
            }
            else if (current_char == '<')
            {
                Advance();
                if (current_char == '=')
                    result.Add(new Token(current_line, current_pos, TokenType.LTE, "<="));
                else
                {
                    result.Add(new Token(current_line, current_pos, TokenType.LT, "<"));
                    Reverse();
                }
            }
            else if (current_char == '?')
            {
                Advance();
                if (current_char == '=')
                    result.Add(new Token(current_line, current_pos, TokenType.EQEQ, "?="));
                else
                    throw new EigerError("<stdin>", current_line, current_pos, $"Invalid Character: {current_char}");
            }
            else if (current_char == '!')
            {
                Advance();
                if (current_char == '=')
                    result.Add(new Token(current_line, current_pos, TokenType.NEQEQ, "!="));
                else
                    throw new EigerError("<stdin>", current_line, current_pos, $"Invalid Character: {current_char}");
            }
            else
            {
                throw new EigerError("<stdin>", current_line, current_pos, $"Invalid Character: {current_char}");
            }
            Advance();
        }
        return result;
    }

    public Lexer(string source)
    {
        this.source = source;
    }
}
