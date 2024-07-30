using EigerLang.Parsing;

namespace EigerLang.Execution.BuiltInTypes;

class Nix : Value
{
    public readonly dynamic? value = null;

    public Nix(string filename, int line, int pos) : base(filename, line, pos)
    {
        this.filename = filename;
        this.line = line;
        this.pos = pos;
    }

    public override string ToString()
    {
        return "nix";
    }

    public override Value GetAttr(ASTNode attr)
    {
        if (attr.value == "type")
        {
            return new String(filename, line, pos, "nix");
        }
        return base.GetAttr(attr);
    }

    public override Boolean ComparisonEqeq(object other)
    {
        return new Boolean(filename, line, pos, other is Nix);
    }

    public override Boolean ComparisonNeqeq(object other)
    {
        return new Boolean(filename, line, pos, other is not Nix);
    }
}