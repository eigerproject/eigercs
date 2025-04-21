/*
 * EIGERLANG NUMBER TYPE
*/

using EigerLang.Parsing;

namespace EigerLang.Execution.BuiltInTypes;

class Number : Value
{
    public double value;

    public Number(string filename, int line, int pos, double value) : base(filename, line, pos)
    {
        this.value = value;
    }

    public override Value AddedTo(dynamic other)
    {
        return new Number(filename, line, pos, value + other.value);
    }

    public override Value SubbedBy(dynamic other)
    {
        return new Number(filename, line, pos, value - other.value);
    }

    public override Value MultedBy(dynamic other)
    {
        return new Number(filename, line, pos, value * other.value);
    }

    public override Value ModdedBy(dynamic other)
    {
        return new Number(filename, line, pos, value % other.value);
    }

    public override Value ExpedBy(dynamic other)
    {
        return new Number(filename, line, pos, Math.Pow(value, other.value));
    }

    public override Value Negative()
    {
        return new Number(filename, line, pos, -value);
    }

    public override Value DivedBy(dynamic other)
    {
        if (other.value == 0)
            throw new Errors.EigerError(filename, line, pos, "Zero Division", Errors.EigerError.ErrorType.ZeroDivisionError);

        return new Number(filename, line, pos, value / other.value);
    }

    public override Value CreateArray(object other)
    {
        List<Value> vals = [];

        for (int i = 0; i < (int)value; i++)
            vals.Add((Value)other);

        return new Array(filename, line, pos, [.. vals]);
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
            return new String(filename, line, pos, "number");
        }
        return base.GetAttr(attr);
    }

    public override string ToString()
    {
        return value.ToString();
    }
}