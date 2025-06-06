/*
 * EIGERLANG FILTER FUNCTION
 * DESCRIPTION: FILTERS ARRAY WITH CALLBACK FUNCTION
*/

using EigerLang.Execution.BuiltInTypes;

namespace EigerLang.Execution.BuiltInFunctions;

class FilterFunction : BuiltInFunction
{
    public FilterFunction() : base("filter", ["array", "callback"]) { }

    public override ReturnResult Execute(List<Value> args, int line, int pos, string filepath)
    {
        CheckArgs(filepath, line, pos, args.Count);

        if(args[0] is not EigerLang.Execution.BuiltInTypes.Array) 
              throw new Errors.EigerError(filepath, line, pos, $"{Globals.ArgumentErrorStr}: filter array is not an array", Errors.EigerError.ErrorType.ArgumentError);

        if(args[1] is not BaseFunction) 
              throw new Errors.EigerError(filepath, line, pos, $"{Globals.ArgumentErrorStr}: filter callback is not a function", Errors.EigerError.ErrorType.ArgumentError);

        List<Value> newArray = new();
        BaseFunction b = (args[1] as BaseFunction)!;

        foreach(Value v in (args[0] as EigerLang.Execution.BuiltInTypes.Array)!.array) {
            Value result = b.Execute([v],line,pos,filepath).result;
            if(result is EigerLang.Execution.BuiltInTypes.Boolean bl && bl.value) newArray.Add(v);
        }

        return new()
        {
            result = new EigerLang.Execution.BuiltInTypes.Array(filepath, line, pos, newArray)
        };
    }
}