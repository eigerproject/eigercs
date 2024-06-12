using EigerLang;
using EigerLang.Errors;
using EigerLang.Tokenization;
using EigerLang.Parsing;
using EigerLang.Execution;
using System.Transactions;

public class Program
{
    static void Main(string[] args)
    {
        if(args.Length == 0)
        {
            // Shell
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
                        Console.WriteLine(Interpreter.VisitNode(statement, Interpreter.globalSymbolTable));
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
        else if(args.Length == 1)
        {
            string filepath = args[1];
            if(Path.GetExtension(filepath) != ".el")
            {
                Console.WriteLine("Not an .el file!");
                return;
            }
            string content = "";
            try
            {
                content = File.ReadAllText(filepath);
            }
            catch(IOException e)
            {
                Console.WriteLine("Failed to read file");
                return;
            }

            Lexer lex = new(content);
            List<Token> tokens = lex.Tokenize();
            Parser parser = new(tokens);
            ASTNode root = parser.Parse();
            foreach (var statement in root.children)
            {
                Interpreter.VisitNode(statement, Interpreter.globalSymbolTable);
            }
        }
        else
        {
            Console.WriteLine("[USAGE] eiger <source_path (optional)>");
        }
        
    }
}
