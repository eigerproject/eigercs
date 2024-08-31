/*
 * EIGERLANG INCHAR FUNCTION
 * DESCRIPTION: GETS AND RETURNS SINGLE CHARACTER FROM INPUT STREAM
*/

using EigerLang.Execution.BuiltInTypes;

namespace EigerLang.Execution.BuiltInFunctions;

class IncharFunction : BuiltInFunction
{
    public IncharFunction() : base("inchar", []) { }

    public override ReturnResult Execute(List<Value> args, int line, int pos, string filepath)
    {
        CheckArgs(filepath, line, pos, args.Count);
        return new()
        {
            result = new BuiltInTypes.String(filepath, line, pos, Console.ReadKey().KeyChar.ToString())
        };
    }
}