/*
 * EIGERLANG NUMBER TYPE
*/

namespace EigerLang.Execution.BuiltInTypes;

class Number(string filename, int line, int pos, double value) : Value(filename, line, pos)
{
    public double value = value;

    public override dynamic AddedTo(dynamic other)
    {
        return new Number(filename,line,pos, value + other.value);
    }

    public override dynamic SubbedBy(dynamic other)
    {
        return new Number(filename, line, pos, value - other.value);
    }

    public override dynamic MultedBy(dynamic other)
    {
        return new Number(filename, line, pos, value * other.value);
    }

    public override dynamic DivedBy(dynamic other)
    {
        if (other.value == 0)
            throw new Errors.EigerError(filename, line, pos, "Zero Division");

        return new Number(filename, line, pos, value / other.value);
    }

    public override Boolean ComparisonEqeq(dynamic other)
    {
        return new Boolean(filename, line, pos, value == other.value);
    }

    public override Boolean ComparisonNeqeq(dynamic other)
    {
        return new Boolean(filename, line, pos, value != other.value);
    }

    public override Boolean ComparisonGT(dynamic other)
    {
        return new Boolean(filename, line, pos, value > other.value);
    }

    public override Boolean ComparisonLT(dynamic other)
    {
        return new Boolean(filename, line, pos, value < other.value);
    }

    public override Boolean ComparisonGTE(dynamic other)
    {
        return new Boolean(filename, line, pos, value >= other.value);
    }

    public override Boolean ComparisonLTE(dynamic other)
    {
        return new Boolean(filename, line, pos, value <= other.value);
    }

    public override string ToString()
    {
        return value.ToString();
    }
}