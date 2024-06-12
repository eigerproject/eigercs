/*
 * EIGERLANG EMITLN FUNCTION
 * DESCRIPTION: OUTPUTS GIVEN PARAMETER AND NEWLINE
*/

using EigerLang.Errors;

namespace EigerLang.Execution.BuiltInFunctions;

class EmitlnFunction : BuiltInFunction
{
    public EmitlnFunction() : base("emitln", ["value"]) { }

    public override dynamic Execute(List<dynamic> args)
    {
        if (args.Count != arg_n.Count)
        {
            throw new EigerError($"Function {name} takes {arg_n.Count} arguments, got {args.Count}");
        }
        Console.WriteLine(args[0]);
        return 0;
    }
}