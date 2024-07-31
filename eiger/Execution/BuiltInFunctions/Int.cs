/*
 * EIGERLANG IN FUNCTION
 * DESCRIPTION: RETURNS INTEGER REPRESENTATION OF A VALUE
*/

using EigerLang.Execution.BuiltInTypes;

namespace EigerLang.Execution.BuiltInFunctions;

class IntFunction : BuiltInFunction
{
    public IntFunction() : base("int", ["val"]) { }

    public override (bool, bool, Value) Execute(List<Value> args, int line, int pos, string filepath)
    {
        CheckArgs(filepath, line, pos, args.Count);
        if (args[0] is Number n)
        {
            return (false, true, new Number(filepath, line, pos, Convert.ToInt32(n.value)));
        }
        else if (args[0] is BuiltInTypes.String s)
        {
            try
            {
                return (false, true, new Number(filepath, line, pos, Convert.ToInt32(Convert.ToDouble(s.value))));
            }
            catch (FormatException)
            {
                throw new Errors.EigerError(filepath, line, pos, "Failed to convert to double", Errors.EigerError.ErrorType.ArgumentError);
            }
        }
        else
        {
            throw new Errors.EigerError(filepath, line, pos, "Failed to convert to double", Errors.EigerError.ErrorType.ArgumentError);
        }
    }
}