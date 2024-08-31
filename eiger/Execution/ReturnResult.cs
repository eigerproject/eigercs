using EigerLang.Execution.BuiltInTypes;

namespace EigerLang.Execution
{
    public class ReturnResult
    {
        public required Value result;
        public bool shouldReturn, shouldBreak, shouldContinue;
    }
}
