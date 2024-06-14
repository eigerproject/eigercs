/*
 * EIGERLANG ARRAY TYPE
*/

namespace EigerLang.Execution.BuiltInTypes;

class Array(dynamic?[] array)
{
    public dynamic?[] array = array;

    public override string ToString()
    {
        return "[" + string.Join(", ", array) + "]";
    }

    public static Array operator +(Array a, Array b)
    {
        return new Array([.. a.array, .. b.array]);
    }

    public static bool operator ==(Array a, Array b)
    {
        return a.array.SequenceEqual(b.array);
    }

    public static bool operator !=(Array a, Array b)
    {
        return !a.array.SequenceEqual(b.array);
    }

    public override bool Equals(object obj)
    {
        if (obj is Array other)
        {
            return this == other;
        }
        return false;
    }
}