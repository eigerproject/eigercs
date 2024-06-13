/*
 * EIGERLANG EMIT FUNCTION
 * DESCRIPTION: OUTPUTS GIVEN PARAMETER
*/

using EigerLang.Errors;

namespace EigerLang.Execution.BuiltInFunctions;

class EmitFunction : BuiltInFunction
{
    public EmitFunction() : base("emit", ["value"]) { }

    public override (bool, dynamic?) Execute(List<dynamic> args)
    {
        if (args.Count != arg_n.Count)
        {
            throw new EigerError($"Function {name} takes {arg_n.Count} arguments, got {args.Count}");
        }
        Console.Write(args[0]);
        return (false,null);
    }
}