/*
 * EIGERLANG INCHAR FUNCTION
 * DESCRIPTION: GETS AND RETURNS SINGLE CHARACTER FROM INPUT STREAM
*/

namespace EigerLang.Execution.BuiltInFunctions;

class IncharFunction : BuiltInFunction
{
    public IncharFunction() : base("inchar", []) { }

    public override (bool, dynamic?) Execute(List<dynamic> args, int line, int pos, string filepath)
    {
        CheckArgs(filepath, line, pos, args.Count);
        return (true, Console.ReadKey().KeyChar);
    }
}