﻿/*
 * EIGERLANG ASCII FUNCTION
 * DESCRIPTION: RETURNS ASCII VALUE OF CHAR
*/

using EigerLang.Execution.BuiltInTypes;
using String = EigerLang.Execution.BuiltInTypes.String;

namespace EigerLang.Execution.BuiltInFunctions;

class AsciiFunction : BuiltInFunction
{
    public AsciiFunction() : base("ascii", ["chr"]) { }

    public override ReturnResult Execute(List<Value> args, int line, int pos, string filepath)
    {
        CheckArgs(filepath, line, pos, args.Count);
        return new()
        {
            result = new Number(filepath, line, pos, ((String)args[0]).value[0])
        };
    }
}