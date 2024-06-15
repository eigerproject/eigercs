/*
 * EIGERLANG STRING TYPE
*/

namespace EigerLang.Execution.BuiltInTypes;

class String(string filename, int line, int pos, string value) : Value(filename, line, pos)
{
    public string value = value;

    public override Value GetIndex(int idx)
    {
        return new String(filename,line,pos, value[idx].ToString());
    }

    public override dynamic AddedTo(dynamic other)
    {
        return new String(filename, line, pos, value + other.value);
    }

    public override Boolean ComparisonEqeq(dynamic other)
    {
        return new Boolean(filename, line, pos, value == other.value);
    }

    public override Boolean ComparisonNeqeq(dynamic other)
    {
        return new Boolean(filename, line, pos, value != other.value);
    }

    public override string ToString()
    {
        return value;
    }
}