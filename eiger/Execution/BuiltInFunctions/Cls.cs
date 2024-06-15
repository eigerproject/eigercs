/*
 * EIGERLANG CLS FUNCTION
 * DESCRIPTION: CLEARS OUTPUT SCREEN
*/

using EigerLang.Execution.BuiltInTypes;

namespace EigerLang.Execution.BuiltInFunctions;

class ClsFunction : BuiltInFunction
{
    public ClsFunction() : base("cls", []) { }

    public override (bool, Value) Execute(List<Value> args,int line,int pos, string filepath)
    {
        CheckArgs(filepath, line, pos, args.Count);
        Console.Clear();
        return (false, null);
    }
}