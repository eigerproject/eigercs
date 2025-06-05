/*
 * EIGERLANG MAIN
*/

using EigerLang.Errors;
using EigerLang.Execution;
using EigerLang.Execution.BuiltInTypes;
using EigerLang.Parsing;
using EigerLang.Tokenization;

namespace EigerLang;
public class Program
{
    static void Main(string[] args)
    {
        // if no args are passed
        if (args.Length == 0)
        {
            // start shell
            Console.WriteLine($"{Globals.langName} {Globals.langVer}\nDocumentation: {Globals.docUrl}\n");
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("% ");
                Console.ResetColor();
                string inp = Console.ReadLine() ?? "";

                if (inp == "") continue;

                Execute(inp, "<stdin>", true);
            }
        }
        // if given filepath
        else if (args.Length == 1)
        {
            string filepath = args[0];
            if (Path.GetExtension(filepath) != Globals.fileExtension) // check extension
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Not an {Globals.fileExtension} file!");
                return;
            }
            string content;
            try
            {
                content = File.ReadAllText(filepath); // try reading the file
            }
            catch (IOException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[EIGER] Failed to read file");
                return;
            }

            Execute(content, filepath, false);
        }
        // invalid syntax, print usage
        else
        {
            Console.WriteLine("[USAGE] eiger <source_path (optional)>");
        }
    }

    // Reset the interpreter
    public static void Reset()
    {
        Interpreter.ResetSymbolTable();
    }

    public static void Execute(string src, string fn, bool printExprs)
    {
        try
        {
            Lexer lex = new(src, fn);
            List<Token> tokens = lex.Tokenize();

            Parser parser = new(tokens);
            ASTNode root = parser.Parse(fn);

            foreach (var statement in root.children)
            {
                Value val = Interpreter.VisitNode(statement, Interpreter.globalSymbolTable).result;
                if (printExprs)
                {
                    string v = Convert.ToString(val) ?? "";

                    if (val is Number || val is EigerLang.Execution.BuiltInTypes.Boolean)
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                    }
                    else if (val is Nix)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                    }
                    else if (val is EigerLang.Execution.BuiltInTypes.String)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow; v = $"\"{v}\"";
                    }
                    else if (val is BaseFunction || val is Class || val is Dataclass || val is Instance)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                    }
                    else
                    {
                        Console.ResetColor();
                    }
                    Console.WriteLine(v);
                    Console.ResetColor();
                }
            }
        }
        catch (EigerError e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(e.Message);
            Console.ResetColor();
        }
        catch (OverflowException e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[EIGER] Overflow: {e.Message}");
            Console.ResetColor();
        }
        catch (Exception e) { Console.ForegroundColor = ConsoleColor.Red; Console.WriteLine(e); Console.ResetColor(); }
    }
}
