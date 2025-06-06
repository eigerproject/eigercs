/*
 * EIGERLANG MAP FUNCTION
 * DESCRIPTION: APPLIES FUNCTION TO ALL ELEMENTS OF AN ARRAY
*/

using EigerLang.Execution.BuiltInTypes;

namespace EigerLang.Execution.BuiltInFunctions;

class MapFunction : BuiltInFunction
{
    public MapFunction() : base("map", ["array", "callback"]) { }

    public override ReturnResult Execute(List<Value> args, int line, int pos, string filepath)
    {
        CheckArgs(filepath, line, pos, args.Count);

        if(args[0] is not EigerLang.Execution.BuiltInTypes.Array) 
              throw new Errors.EigerError(filepath, line, pos, $"{Globals.ArgumentErrorStr}: map array is not an array", Errors.EigerError.ErrorType.ArgumentError);

        if(args[1] is not BaseFunction) 
              throw new Errors.EigerError(filepath, line, pos, $"{Globals.ArgumentErrorStr}: map callback is not a function", Errors.EigerError.ErrorType.ArgumentError);

        List<Value> newArray = new();
        BaseFunction b = (args[1] as BaseFunction)!;

        foreach(Value v in (args[0] as EigerLang.Execution.BuiltInTypes.Array)!.array)
             newArray.Add(b.Execute([v],line,pos,filepath).result);

        return new()
        {
            result = new EigerLang.Execution.BuiltInTypes.Array(filepath, line, pos, newArray)
        };
    }
}