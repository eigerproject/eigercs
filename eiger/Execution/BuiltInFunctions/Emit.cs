/*
 * EIGERLANG EMIT FUNCTION
 * DESCRIPTION: OUTPUTS GIVEN PARAMETER
*/

namespace EigerLang.Execution.BuiltInFunctions;

class EmitFunction : BuiltInFunction
{
    public EmitFunction() : base("emit", ["value"]) { }

    public override (bool, dynamic?) Execute(List<dynamic> args,int line,int pos, string filepath)
    {
        CheckArgs(filepath, line, pos, args.Count);
        Console.Write(args[0]);
        return (false,null);
    }
}