/*
 * EIGERLANG ARRAY TYPE
*/

using EigerLang.Errors;
using EigerLang.Parsing;

namespace EigerLang.Execution.BuiltInTypes;

using System.Text;

class Array : Value
{
    public List<Value> array;

    public Array(string fn, int ln, int ps, List<Value> array) : base(fn, ln, ps)
    {
        this.array = array;
    }

    public override Value AddedTo(object other)
    {
        if (other is Array arr)
        {
            var newArr = new List<Value>(array.Count + arr.array.Count);
            newArr.AddRange(array);
            newArr.AddRange(arr.array);
            return new Array(filename, line, pos, newArr);
        }
        else
        {
            var newArr = new List<Value>(array.Count + 1);
            newArr.AddRange(array);
            newArr.Add((Value)other);
            return new Array(filename, line, pos, newArr);
        }
    }

    public override Boolean ComparisonEqeq(object other)
    {
        if (other is Array arr)
        {
            return new Boolean(filename, line, pos, arr.array.Count == array.Count && arr.array.SequenceEqual(array));
        }
        return new Boolean(filename, line, pos, false);
    }

    public override Boolean ComparisonNeqeq(object other)
    {
        if (other is Array arr)
        {
            return new Boolean(filename, line, pos, !arr.array.SequenceEqual(array));
        }
        return new Boolean(filename, line, pos, true);
    }

    private void ValidateIndex(int idx)
    {
        if (idx < 0 || idx >= array.Count)
            throw new EigerError(filename, line, pos, "Index outside of bounds", EigerError.ErrorType.IndexError);
    }

    public override Value GetIndex(int idx)
    {
        ValidateIndex(idx);
        return array[idx];
    }

    public override void SetIndex(int idx, Value val)
    {
        ValidateIndex(idx);
        val.modifiers = array[idx].modifiers;
        array[idx] = val;
    }

    public override Value GetLength()
    {
        return new Number(filename, line, pos, array.Count);
    }

    public override Value GetAttr(ASTNode attr)
    {
        if (attr.value == "type")
        {
            return new String(filename, line, pos, "array");
        }
        return base.GetAttr(attr);
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append('[');
        for (int i = 0; i < array.Count; i++)
        {
            sb.Append(array[i].ToString());
            if (i != array.Count - 1)
                sb.Append(", ");
        }
        sb.Append(']');
        return sb.ToString();
    }
}
