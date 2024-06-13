/*
 * EIGERLANG CLS FUNCTION
 * DESCRIPTION: CLEARS OUTPUT SCREEN
*/

using EigerLang.Errors;

namespace EigerLang.Execution.BuiltInFunctions;

class ClsFunction : BuiltInFunction
{
    public ClsFunction() : base("cls", []) { }

    public override (bool, dynamic?) Execute(List<dynamic> args)
    {
        if (args.Count != arg_n.Count)
        {
            throw new EigerError($"Function {name} takes {arg_n.Count} arguments, got {args.Count}");
        }
        Console.Clear();
        return (false, null);
    }
}