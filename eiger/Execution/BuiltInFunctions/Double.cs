/*
 * EIGERLANG IN FUNCTION
 * DESCRIPTION: RETURNS DOUBLE REPRESENTATION OF A VALUE
*/

using EigerLang.Execution.BuiltInTypes;

namespace EigerLang.Execution.BuiltInFunctions;

class DoubleFunction : BuiltInFunction
{
    public DoubleFunction() : base("double", ["val"]) { }

    public override ReturnResult Execute(List<Value> args, int line, int pos, string filepath)
    {
        CheckArgs(filepath, line, pos, args.Count);
        if (args[0] is Number n)
        {
            return new()
            {
                result = new Number(filepath, line, pos, n.value)
            };
        }
        else if (args[0] is BuiltInTypes.String s)
        {
            try
            {
                return new()
                {
                    result = new Number(filepath, line, pos, Convert.ToDouble(s.value))
                };
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