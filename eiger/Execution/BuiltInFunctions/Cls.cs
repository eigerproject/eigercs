/*
 * EIGERLANG CLS FUNCTION
 * DESCRIPTION: CLEARS OUTPUT SCREEN
*/

using EigerLang.Execution.BuiltInTypes;

namespace EigerLang.Execution.BuiltInFunctions;

class ClsFunction : BuiltInFunction
{
    public ClsFunction() : base("cls", []) { }

    public override ReturnResult Execute(List<Value> args, int line, int pos, string filepath)
    {
        CheckArgs(filepath, line, pos, args.Count);
        Console.Clear();
        return new()
        {
            result = new Nix(filepath, line, pos)
        };
    }
}