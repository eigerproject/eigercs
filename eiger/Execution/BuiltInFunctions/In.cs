/*
 * EIGERLANG IN FUNCTION
 * DESCRIPTION: GETS AND RETURNS INPUT FROM INPUT STREAM
*/

using EigerLang.Execution.BuiltInTypes;

namespace EigerLang.Execution.BuiltInFunctions;

class InFunction : BuiltInFunction
{
    public InFunction() : base("in", []) { }

    public override (bool, Value) Execute(List<Value> args,int line, int pos,string filepath)
    {
        CheckArgs(filepath, line, pos, args.Count);
        return (true, new BuiltInTypes.String(filepath,line,pos,Console.ReadLine() ?? ""));
    }
}