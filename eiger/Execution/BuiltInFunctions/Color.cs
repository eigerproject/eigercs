/*
 * EIGERLANG COLOR FUNCTION
 * DESCRIPTION: SETS BACKGROUND AND FOREGROUND COLORS
*/

using EigerLang.Execution.BuiltInTypes;

namespace EigerLang.Execution.BuiltInFunctions;

class ColorFunction : BuiltInFunction
{
    public ColorFunction() : base("color", ["fg", "bg"]) { }

    public override (bool, bool, Value) Execute(List<Value> args, int line, int pos, string filepath)
    {
        CheckArgs(filepath, line, pos, args.Count);
        Console.ForegroundColor = (ConsoleColor)((Number)args[0]).value;
        Console.BackgroundColor = (ConsoleColor)((Number)args[1]).value;
        Interpreter.fgColor.value = (int)Console.ForegroundColor;
        Interpreter.bgColor.value = (int)Console.BackgroundColor;
        return (false, false, new Nix(filepath, line, pos));
    }
}