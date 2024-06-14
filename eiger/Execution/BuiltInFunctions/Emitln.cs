/*
 * EIGERLANG EMITLN FUNCTION
 * DESCRIPTION: OUTPUTS GIVEN PARAMETER AND NEWLINE
*/

namespace EigerLang.Execution.BuiltInFunctions;

class EmitlnFunction : BuiltInFunction
{
    public EmitlnFunction() : base("emitln", ["value"]) { }

    public override (bool, dynamic?) Execute(List<dynamic> args,int line,int pos, string filepath)
    {
        CheckArgs(filepath, line, pos, args.Count);
        Console.WriteLine(args[0]);
        return (false,null);
    }
}