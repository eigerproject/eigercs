/*
 * EIGERLANG EXIT FUNCTION
 * DESCRIPTION: EXITS PROGRAM WITH EXIT CODE
*/

using EigerLang.Execution.BuiltInTypes;

namespace EigerLang.Execution.BuiltInFunctions;

class ExitFunction : BuiltInFunction
{
    public ExitFunction() : base("exit", ["code"]) { }

    public override (bool, bool, Value) Execute(List<Value> args, int line, int pos, string filepath)
    {
        CheckArgs(filepath, line, pos, args.Count);
        System.Environment.Exit(Convert.ToInt32(((Number)args[0]).value));
        return (false, false, new Nix(filepath, line, pos));
    }
}