/*
 * EIGERLANG STRING TYPE
*/

namespace EigerLang.Execution.BuiltInTypes;

class String : Value
{
    public string value;
    string filename;
    int line;
    int pos;

    public String(string filename, int line, int pos,string value) : base(filename, line,pos)
    {
        this.value = value;
        this.filename = filename;
        this.line = line;
        this.pos = pos;
    }

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