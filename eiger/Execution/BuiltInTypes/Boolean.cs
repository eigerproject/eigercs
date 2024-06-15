/*
 * EIGERLANG BOOLEAN TYPE
*/

namespace EigerLang.Execution.BuiltInTypes;

public class Boolean(string filename, int line, int pos, bool value) : Value(filename, line, pos)
{
    public bool value = value;

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