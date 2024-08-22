/*
 * EIGERLANG ARRAY TYPE
*/

using EigerLang.Errors;
using EigerLang.Parsing;

namespace EigerLang.Execution.BuiltInTypes;

class Array : Value
{
    public Value[] array;

    public Array(string fn, int ln, int ps, Value[] array) : base(fn, ln, ps)
    {
        this.array = array;
        this.filename = fn;
        this.line = ln;
        this.pos = ps;
    }

    public override Value AddedTo(object other)
    {
        if (other is Array arr)
        {
            return new Array(filename, line, pos, [.. array, .. arr.array]);
        }
        else
        {
            return new Array(filename, line, pos, [.. array, (Value)other]);
        }
    }

    public override Boolean ComparisonEqeq(object other)
    {
        if (other is Array arr)
        {
            return new Boolean(filename, line, pos, arr.array.Length == array.Length && arr.array.SequenceEqual(array));
        }
        else
        {
            return new Boolean(filename, line, pos, false);
        }
    }

    public override Boolean ComparisonNeqeq(object other)
    {
        if (other is Array arr)
        {
            return new Boolean(filename, line, pos, !arr.array.SequenceEqual(array));
        }
        else
        {
            return new Boolean(filename, line, pos, true);
        }
    }

    public override Value GetIndex(int idx)
    {
        if (idx < 0 || idx >= array.Length)
            throw new EigerError(filename, line, pos, "Index outside of bounds", EigerError.ErrorType.IndexError);
        return array[idx];
    }

    public override Value GetAttr(ASTNode attr)
    {
        if (attr.value == "type")
        {
            return new String(filename, line, pos, "array");
        }
        return base.GetAttr(attr);
    }

    public override void SetIndex(int idx, Value val)
    {
        if (idx < 0 || idx >= array.Length)
            throw new EigerError(filename, line, pos, "Index outside of bounds", EigerError.ErrorType.IndexError);
        val.modifiers = array[idx].modifiers;
        array[idx] = val;
    }

    public override Value GetLength()
    {
        return new Number(filename, line, pos, array.Length);
    }

    public override string ToString()
    {
        string strep = "[";
        for (int i = 0; i < array.Length; i++)
        {
            strep += array[i].ToString();
            if (i != array.Length - 1)
            {
                strep += ", ";
            }
        }
        strep += "]";
        return strep;
    }
}