/*
 * EIGERLANG IN FUNCTION
 * DESCRIPTION: GETS AND RETURNS INPUT FROM INPUT STREAM
*/

namespace EigerLang.Execution.BuiltInFunctions;

class InFunction : BuiltInFunction
{
    public InFunction() : base("in", []) { }

    public override (bool, dynamic?) Execute(List<dynamic> args,int line, int pos,string filepath)
    {
        CheckArgs(filepath, line, pos, args.Count);
        return (true, Console.ReadLine());
    }
}