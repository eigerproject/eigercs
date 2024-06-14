/*
 * EIGERLANG CLS FUNCTION
 * DESCRIPTION: CLEARS OUTPUT SCREEN
*/

namespace EigerLang.Execution.BuiltInFunctions;

class ClsFunction : BuiltInFunction
{
    public ClsFunction() : base("cls", []) { }

    public override (bool, dynamic?) Execute(List<dynamic> args,int line,int pos, string filepath)
    {
        CheckArgs(filepath, line, pos, args.Count);
        Console.Clear();
        return (false, null);
    }
}