/*
 * EIGERLANG IN FUNCTION
 * DESCRIPTION: GETS AND RETURNS INPUT FROM INPUT STREAM
*/

using EigerLang.Errors;

namespace EigerLang.Execution.BuiltInFunctions;

class InFunction : BuiltInFunction
{
    public InFunction() : base("in", []) { }

    public override dynamic Execute(List<dynamic> args)
    {
        if (args.Count != arg_n.Count)
        {
            throw new EigerError($"Function {name} takes {arg_n.Count} arguments, got {args.Count}");
        }
        return Console.ReadLine() ?? "";
    }
}