namespace EigerLang.Tokenization;

public class Token
{
    public TokenType type = TokenType.UNDEFINED;
    public dynamic? value = null;
    public int line = 1, pos = 1;

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

    public string ToLongString()
    {
        if (value == null)
            return $"Token({type})";
        else
            return $"Token({type},`{value}`)";
    }
}