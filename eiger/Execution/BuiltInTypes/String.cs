﻿/*
 * EIGERLANG STRING TYPE
*/

using EigerLang.Parsing;
using System.Text;

namespace EigerLang.Execution.BuiltInTypes;

class String : Value
{
    public string value;

    public String(string filename, int line, int pos, string value) : base(filename, line, pos)
    {
        this.value = value;
    }

    public override Value GetIndex(int idx)
    {
        return new String(filename, line, pos, value[idx].ToString());
    }

    public override void SetIndex(int idx, Value val)
    {
        StringBuilder sb = new(value);
        sb[idx] = ((String)val ?? throw new Errors.EigerError(filename, line, pos, "Failed to set index (value is null)", Errors.EigerError.ErrorType.ParserError)).value[0];
        value = sb.ToString();
    }

    public override Value AddedTo(dynamic other)
    {
        return new String(filename, line, pos, value + other.value);
    }

    public override Value GetAttr(ASTNode attr)
    {
        if (attr.value == "type")
        {
            return new String(filename, line, pos, "string");
        }
        return base.GetAttr(attr);
    }

    public override Value GetLength()
    {
        return new Number(filename, line, pos, value.Length);
    }

    public override Value MultedBy(object other)
    {
        if (other is Number number)
        {
            return new String(filename, line, pos, string.Concat(Enumerable.Repeat(value, (int)number.value)));
        }
        else
            return base.MultedBy(other);
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