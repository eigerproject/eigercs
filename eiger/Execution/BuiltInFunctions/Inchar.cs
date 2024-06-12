/*
 * EIGERLANG INCHAR FUNCTION
 * DESCRIPTION: GETS AND RETURNS SINGLE CHARACTER FROM INPUT STREAM
*/

using EigerLang.Errors;

namespace EigerLang.Execution.BuiltInFunctions;

class IncharFunction : BuiltInFunction
{
    public IncharFunction() : base("inchar", []) { }

    public override dynamic Execute(List<dynamic> args)
    {
        if (args.Count != arg_n.Count)
        {
            throw new EigerError($"Function {name} takes {arg_n.Count} arguments, got {args.Count}");
        }
        return Console.ReadKey().KeyChar;
    }
}