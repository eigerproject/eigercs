/*
 * EIGERLANG BOOLEAN TYPE
*/

namespace EigerLang.Execution.BuiltInTypes;

public class Boolean : Value
{
    public bool value;
    string filename;
    int line, pos;

    public Boolean(string filename, int line, int pos, bool value) : base(filename, line, pos)
    {
        this.filename = filename;
        this.value = value;
        this.line = line;
        this.pos = pos;
        this.value = value;
    }

    public override Boolean ComparisonEqeq(dynamic other)
    {
        return new Boolean(filename, line, pos, value == other.value);
    }

    public override Boolean ComparisonNeqeq(dynamic other)
    {
        return new Boolean(filename, line, pos, value != other.value);
    }

    public override string ToString() => value ? "true" : "false";
}