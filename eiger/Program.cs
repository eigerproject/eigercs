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

                foreach(var statement in root.children)
                {
                    Console.WriteLine(Interpreter.VisitNode(statement,Interpreter.globalSymbolTable));
                }

            }
            catch(EigerError e)
            {
                Console.WriteLine(e.Message);
            }
            catch(OverflowException e)
            {
                Console.WriteLine(e.Message);
            }
            catch(Exception e) { Console.WriteLine(e); }
        }
    }
}
