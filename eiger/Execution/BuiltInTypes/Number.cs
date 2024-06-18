/*
 * EIGERLANG NUMBER TYPE
*/

using EigerLang.Parsing;

namespace EigerLang.Execution.BuiltInTypes;

class Number: Value
{
    public double value;
    string filename;
    int line, pos;

    public Number(string filename, int line, int pos, double value) : base(filename,line,pos)
    {
        this.filename = filename;
        this.line = line;
        this.pos = pos;
        this.value = value;
    }

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

    public override dynamic ModdedBy(dynamic other)
    {
        return new Number(filename, line, pos, value % other.value);
    }

    public override dynamic ExpedBy(dynamic other)
    {
        return new Number(filename, line, pos, Math.Pow(value, other.value));
    }

    public override dynamic DivedBy(dynamic other)
    {
        if (other.value == 0)
            throw new Errors.EigerError(filename, line, pos, "Zero Division",Errors.EigerError.ErrorType.ZeroDivisionError);

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

    public override Value GetAttr(ASTNode attr)
    {
        if (attr.value == "type")
        {
            return new String(filename, line, pos, "[type Number]");
        }
        return base.GetAttr(attr);
    }

    public override string ToString()
    {
        return value.ToString();
    }
}