/*
 * EIGERLANG EMIT FUNCTION
 * DESCRIPTION: OUTPUTS GIVEN PARAMETER
*/

using EigerLang.Execution.BuiltInTypes;

namespace EigerLang.Execution.BuiltInFunctions;

class EmitFunction : BuiltInFunction
{
    public EmitFunction() : base("emit", ["value"]) { }

    public override (bool, Value) Execute(List<Value> args, int line, int pos, string filepath)
    {
        CheckArgs(filepath, line, pos, args.Count);
        Console.Write(args[0]);
        return (false, new Nix(filepath, line, pos));
    }
}