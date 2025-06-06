/*
 * EIGERLANG FMT FUNCTION
 * DESCRIPTION: FORMATS GIVEN STRING WITH ARGUMENTS
*/

using EigerLang.Execution.BuiltInTypes;
using String = EigerLang.Execution.BuiltInTypes.String;

namespace EigerLang.Execution.BuiltInFunctions;



class FmtFunction : BuiltInFunction
{
    public FmtFunction() : base("fmt", ["fmt", "args"], true) { }

    public override ReturnResult Execute(List<Value> args, int line, int pos, string filepath)
    {
        CheckArgs(filepath, line, pos, args.Count);

        if(args[0] is not String) 
              throw new Errors.EigerError(filepath, line, pos, $"{Globals.ArgumentErrorStr}: format is not string", Errors.EigerError.ErrorType.ArgumentError);

        string fmt = args[0].ToString()!;
        string result = "";

        List<Value> fargs = (args[1] as EigerLang.Execution.BuiltInTypes.Array)!.array;

        for (int i = 0; i < fmt.Length; ++i)
        {
            if (fmt[i] == '%')
            {
                if (i + 1 < fmt.Length && fmt[i + 1] == '%')
                {
                    result += "%";
                    ++i;
                    continue;
                }

                if (i + 1 < fmt.Length && char.IsDigit(fmt[i + 1]))
                {
                    int index = fmt[i + 1] - '0'; 
                    if (index > 0 && index <= fargs.Count)
                        result += fargs[index - 1].ToString();
                    else
                        result += $"%{fmt[i + 1]}";

                    ++i;
                    continue;
                }
            }
            result += fmt[i];
        }


        return new()
        {
            result = new String(filepath, line, pos, result)
        };
    }
}