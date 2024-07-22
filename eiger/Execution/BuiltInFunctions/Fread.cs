/*
 * EIGERLANG EMITLN FUNCTION
 * DESCRIPTION: READS FILE CONTENTS
*/

using EigerLang.Execution.BuiltInTypes;
using String = EigerLang.Execution.BuiltInTypes.String;

namespace EigerLang.Execution.BuiltInFunctions;

class FreadFunction : BuiltInFunction
{
    public FreadFunction() : base("fread", ["path"]) { }

    public override (bool, Value) Execute(List<Value> args, int line, int pos, string filepath)
    {
        CheckArgs(filepath, line, pos, args.Count);
        if (args[0] is not String)
            throw new Errors.EigerError(filepath, line, pos, Globals.ArgumentErrorStr, Errors.EigerError.ErrorType.ArgumentError);
        else if (args[0] is String path)
        {
            string pathStr = path.value;
            if (File.Exists(pathStr))
                return (false, new String(filepath, line, pos, File.ReadAllText(pathStr)));
            else
                throw new Errors.EigerError(filepath, line, pos, "File doesn't exist", Errors.EigerError.ErrorType.IOError);

        }
        throw new Errors.EigerError(filepath, line, pos, "File doesn't exist", Errors.EigerError.ErrorType.IOError);
    }
}