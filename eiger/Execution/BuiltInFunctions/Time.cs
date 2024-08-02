/*
 * EIGERLANG TIME FUNCTION
 * DESCRIPTION: RETURNS TIME AS A DOUBLE
*/

using EigerLang.Execution.BuiltInTypes;

namespace EigerLang.Execution.BuiltInFunctions;

class TimeFunction : BuiltInFunction
{
    public TimeFunction() : base("time", []) { }

    public override (bool, bool, Value) Execute(List<Value> args, int line, int pos, string filepath)
    {
        CheckArgs(filepath, line, pos, args.Count);
        DateTimeOffset now = DateTimeOffset.Now;
        DateTimeOffset epoch = new(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
        return (false, false, new Number(filepath, line, pos, (now - epoch).TotalSeconds));
    }
}