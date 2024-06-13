/*
 * EIGERLANG MAIN
 * WRITTEN BY VARDAN PETROSYAN
*/

using EigerLang;
using EigerLang.Errors;
using EigerLang.Tokenization;
using EigerLang.Parsing;
using EigerLang.Execution;

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
                try
                {
                    Console.Write("#-> ");
                    string inp = Console.ReadLine() ?? "";

                    if (inp == "") continue;

                    Lexer lex = new(inp);
                    List<Token> tokens = lex.Tokenize();

                    /*
                    foreach(Token token in tokens)
                    {
                        Console.WriteLine(token.ToLongString());
                    }
                    */

                    Parser parser = new(tokens);
                    ASTNode root = parser.Parse();

                    foreach (var statement in root.children)
                    {
                        (bool didReturn, dynamic? val) = Interpreter.VisitNode(statement, Interpreter.globalSymbolTable);
                        Console.WriteLine((string)Convert.ToString(val));
                    }
                }
                catch (EigerError e)
                {
                    Console.WriteLine(e.Message);
                }
                catch (OverflowException e)
                {
                    Console.WriteLine(e.Message);
                }
                catch (Exception e) { Console.WriteLine(e); }
            }
        }
        // if given filepath
        else if (args.Length == 1)
        {
            string filepath = args[0];
            if (Path.GetExtension(filepath) != Globals.fileExtension) // check extension
            {
                Console.WriteLine($"Not an {Globals.fileExtension} file!");
                return;
            }
            string content = "";
            try
            {
                content = File.ReadAllText(filepath); // try reading the file
            }
            catch (IOException)
            {
                Console.WriteLine("Failed to read file");
                return;
            }

            Lexer lex = new(content);
            List<Token> tokens = lex.Tokenize();
            Parser parser = new(tokens);
            ASTNode root = parser.Parse();

            try
            {
                foreach (var statement in root.children)
                {
                    (bool didReturn, dynamic? val) = Interpreter.VisitNode(statement, Interpreter.globalSymbolTable);
                    if (didReturn) { Console.WriteLine((string)Convert.ToString(val)); }
                }
            }
            catch (EigerError e)
            {
                Console.WriteLine(e.Message);
            }
            catch (OverflowException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (Exception e) { Console.WriteLine(e); }
        }
        // invalid syntax, print usage
        else
        {
            Console.WriteLine("[USAGE] eiger <source_path (optional)>");
        }

    }
}
