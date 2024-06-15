/*
 * EIGERLANG ARRAY TYPE
*/

namespace EigerLang.Execution.BuiltInTypes;

class Array : Value
{
    public Value[] array;
    string fn;
    int ln, ps;

    public Array(string fn, int ln, int ps, Value[] array) : base(fn, ln, ps)
    {
        this.array = [];
        this.fn = fn;
        this.ln = ln;
        this.ps = ps;
    }

    public override Value AddedTo(object other)
    {
        if (other is Array)
        {
            return new Array(fn, ln, ps, [.. array, ..((Array)other).array]);
        }
        else
        {
            return new Array(fn, ln, ps, [.. array, (Value)other]);
        }
        
    }

    public override Value GetIndex(int idx)
    {
        return array[idx];
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