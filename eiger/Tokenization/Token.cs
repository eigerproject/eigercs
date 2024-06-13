/*
 * EIGERLANG TOKEN CLASS
 * WRITTEN BY VARDAN PETROSYAN
*/

namespace EigerLang.Tokenization;

public class Token
{
    public TokenType type = TokenType.UNDEFINED; // the type of the token
    public dynamic? value = null; // the value of the token
    public int line = 1, pos = 1; // the position of the token

    public Token(int line, int pos, TokenType type)
    {
        this.line = line;
        this.pos = pos;
        this.type = type;
    }

    public Token(int line, int pos, TokenType type, dynamic value)
    {
        this.line = line;
        this.pos = pos;
        this.type = type;
        this.value = value;
    }

    public override string ToString()
    {
        if (value == null)
            return type.ToString();
        else
            return value.ToString();
    }

    // to string for debugging
    public string ToLongString()
    {
        if (value == null)
            return $"Token({type})";
        else
            return $"Token({type},`{value}`)";
    }
}