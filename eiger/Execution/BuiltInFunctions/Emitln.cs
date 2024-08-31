/*
 * EIGERLANG EMITLN FUNCTION
 * DESCRIPTION: OUTPUTS GIVEN PARAMETER AND NEWLINE
*/

using EigerLang.Execution.BuiltInTypes;

namespace EigerLang.Execution.BuiltInFunctions;

class EmitlnFunction : BuiltInFunction
{
    public EmitlnFunction() : base("emitln", ["value"]) { }

    public override ReturnResult Execute(List<Value> args, int line, int pos, string filepath)
    {
        CheckArgs(filepath, line, pos, args.Count);
        Console.WriteLine(args[0]);
        return new()
        {
            result = new Nix(filepath, line, pos)
        };
    }
}