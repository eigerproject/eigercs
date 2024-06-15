/*
 * EIGERLANG ARRAY TYPE
*/

namespace EigerLang.Execution.BuiltInTypes;

class Array(string filename,int line,int pos,Value[] array) : Value(filename,line,pos)
{
    public Value[] array = array;

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