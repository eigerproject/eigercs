/*
 * EIGERLANG ARRAY TYPE
*/

using EigerLang.Errors;
using EigerLang.Parsing;

namespace EigerLang.Execution.BuiltInTypes;

class Array : Value
{
    public Value[] array;
    string filename;
    int line, pos;

    public Array(string fn, int ln, int ps, Value[] array) : base(fn, ln, ps)
    {
        this.array = array;
        this.filename = fn;
        this.line = ln;
        this.pos = ps;
    }

    public override Value AddedTo(object other)
    {
        if (other is Array)
        {
            return new Array(filename, line, pos, [.. array, ..((Array)other).array]);
        }
        else
        {
            return new Array(filename, line, pos, [.. array, (Value)other]);
        }
        
    }

    public override Value GetIndex(int idx)
    {
        if(idx < 0 || idx >= array.Length)
            throw new EigerError(filename,line, pos,"Index outside of bounds",EigerError.ErrorType.IndexError);
        return array[idx];
    }

    public override Value GetAttr(ASTNode attr)
    {
        if (attr.value == "type")
        {
            return new String(filename, line, pos, "[type Array]");
        }
        return base.GetAttr(attr);
    }

    public override void SetIndex(int idx, Value val)
    {
        if (idx < 0 || idx >= array.Length)
            throw new EigerError(filename, line, pos, "Index outside of bounds", EigerError.ErrorType.IndexError);
        array[idx] = val;
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
        strep+= "]";
        return strep;
    }
}