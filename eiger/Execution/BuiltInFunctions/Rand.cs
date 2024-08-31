/*
 * EIGERLANG RAND FUNCTION
 * DESCRIPTION: RETURNS RANDOM DOUBLE FROM 0 TO 1
*/

using EigerLang.Execution.BuiltInTypes;

namespace EigerLang.Execution.BuiltInFunctions;

class RandFunction : BuiltInFunction
{
    public RandFunction() : base("rand", []) { }
    readonly Random rand = new();

    public override ReturnResult Execute(List<Value> args, int line, int pos, string filepath)
    {
        CheckArgs(filepath, line, pos, args.Count);
        return new()
        {
            result = new Number(filepath, line, pos, rand.NextDouble())
        };
    }
}