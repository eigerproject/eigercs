/*
 * EIGERLANG PARSER
 * WRITTEN BY VARDAN PETROSYAN
*/

using EigerLang.Errors;
using EigerLang.Tokenization;
using System.ComponentModel.Design;
using System.Xml.Linq;

namespace EigerLang.Parsing;

public class Parser(List<Token> tokens)
{
    // stores the index of the current token
    int current;

    // term level operators
    HashSet<TokenType> termOps = new()
    {
        TokenType.MUL, TokenType.DIV, TokenType.EQEQ, TokenType.NEQEQ,
        TokenType.GT, TokenType.LT, TokenType.GTE, TokenType.LTE,
    };

    // expression level operators
    HashSet<TokenType> exprOps = new()
    {
        TokenType.PLUS, TokenType.MINUS
    };

    // assignment level operators
    HashSet<TokenType> assignOps = new()
    {
        TokenType.EQ, TokenType.PLUSEQ,TokenType.MINUSEQ,TokenType.MULEQ,TokenType.DIVEQ
    };

    string path = "<stdin>";

    // parse the root
    public ASTNode Parse(string p)
    {
        path = p;
        return StatementList();
    }

    // advance to next token
    Token Advance()
    {
        // if the pointer is inside the bounds of the token array
        if (current < tokens.Count)
        {
            // return the current token and advance
            return tokens[current++];
        }
        // if not, throw error
        throw new EigerError(path, tokens[^1].line, tokens[^1].pos, "Unexpected End of Input", EigerError.ErrorType.ParserError);
    }

    // return current token
    Token Peek()
    {
        // if the pointer is inside the bounds of the token array
        if (current < tokens.Count)
        {
            // return the current token
            return tokens[current];
        }

        // i don't know what to return here, this not a good solution
        return new Token(-1, -1, TokenType.UNDEFINED);
    }

    // return next token without advancing
    Token? PeekNext()
    {
        // if the pointer is inside the bounds of the token array
        if (current + 1 < tokens.Count)
        {
            // return the next token without advancing
            return tokens[current + 1];
        }
        return null;
    }

    // check if the current token matches the expected one and advance
    void Match(TokenType type)
    {
        if (Peek().type == type)
        {
            Advance();
        }
        else
        {
            throw new EigerError(path, Peek().line, Peek().pos, $"{Globals.UnexpectedTokenStr} `{Peek().value}`", EigerError.ErrorType.ParserError);
        }
    }

    // check if the current token matches the expected one and advance
    void Match(TokenType type, dynamic expected)
    {
        if (Peek().type == type && Peek().value == expected)
        {
            Advance();
        }
        else
        {
            throw new EigerError(path, Peek().line, Peek().pos, $"{Globals.UnexpectedTokenStr} `{Peek().value}`", EigerError.ErrorType.ParserError);
        }
    }

    // statement list
    ASTNode StatementList()
    {
        ASTNode root = new(NodeType.Block, null, 1, 0, path);
        // add statements until the end of block (end or else)
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        while (current < tokens.Count && Peek().value.ToString() != "end" && Peek().value.ToString() != "else")
        {
            root.AddChild(Statement());
        }
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        return root;
    }

    // single statement
    ASTNode Statement()
    {
        // store the peeked token
        Token peeked = Peek();
        switch (peeked.value)
        {
            // if it's a statement, parse a statement, else, parse an expression
            case "if": return IfStatement();
            case "for": return ForToStatement();
            case "while": return WhileStatement();
            case "func": return FuncDefStatement();
            case "ret": return RetStatement();
            case "class": return ClassStatement();
            case "dataclass": return DataclassStatement();
            case "include": return IncludeStatement();
            default: return Expr();
        }
    }

    // include statement
    ASTNode IncludeStatement()
    {
        Match(TokenType.IDENTIFIER, "include");
        Token pathToken = Advance();
        return new ASTNode(NodeType.Include, pathToken.value, pathToken.line, pathToken.pos, path);
    }

    // function definition
    ASTNode FuncDefStatement()
    {
        // match func
        Match(TokenType.IDENTIFIER, "func");

        // get func name
        Token funcTok = Peek();
        string funcName = Advance().value ?? "";

        // create func def node
        ASTNode node = new(NodeType.FuncDef, funcName, funcTok.line, funcTok.pos, path);
        // create list for args
        List<ASTNode> args = [];

        // match left parenthasis
        Match(TokenType.LPAREN);

        // if the function has no args (right parenthasis just after left parenthasis), just advance through it
        if (Peek().type == TokenType.RPAREN) { Match(TokenType.RPAREN); }
        else // if the function has args
        {
            do // add arguments until there are no more
            {
                if (Peek().type == TokenType.COMMA) Advance(); // advance through the comma

                args.Add(Factor()); // add the name to the args

            } while (Peek().type == TokenType.COMMA); // if there is a comma right after the argname

            // match right parenthasis
            Match(TokenType.RPAREN);
        }

        // parse the body
        ASTNode rootNode = StatementList();

        // match end
        Match(TokenType.IDENTIFIER, "end");

        // add the root node first, then the argnames
        node.AddChild(rootNode);

        foreach (var arg in args)
        {
            node.AddChild(arg);
        }

        return node;
    }

    // return statement
    ASTNode RetStatement()
    {
        // match ret
        Match(TokenType.IDENTIFIER, "ret");
        // create node
        ASTNode node = new(NodeType.Return, null, Peek().line, Peek().pos, path);
        // add the expression as the child
        node.AddChild(Expr());
        return node;
    }

    // function call statement
    ASTNode FunctionCallStatement()
    {
        // get the function name;
        Token funcName = Advance();

        // match left parenthasis
        Match(TokenType.LPAREN);

        ASTNode node = new(NodeType.FuncCall, funcName.value, funcName.line, funcName.pos, path);

        // if the function has no args, return node
        if (Peek().type == TokenType.RPAREN)
        {
            Match(TokenType.RPAREN);
            if (Peek().type == TokenType.DOT) // if it's an atrribute access of function call like Person("test").a
            {
                return ParseAttrAccess(node);
            }
            return node;
        }

        // get arguments similar way in FuncDefStatement
        do // add arguments until there are no more
        {
            if (Peek().type == TokenType.COMMA) Advance(); // advance through the comma

            node.AddChild(Expr()); // add the expression to the args

        } while (Peek().type == TokenType.COMMA);  // if there is a comma right after the last expression

        // match right parenthasis
        Match(TokenType.RPAREN);

        if (Peek().type == TokenType.DOT) // if it's an atrribute access of function call like Person("test").a
        {
            return ParseAttrAccess(node);
        }

        return node;
    }

    // for-to statement
    ASTNode ForToStatement()
    {
        // match for
        Token forToken = Peek();
        Match(TokenType.IDENTIFIER, "for");
        // match iterator name
        Token iteratorVar = Advance();
        // match equal sign
        Match(TokenType.EQ);
        // match initial value
        ASTNode value = Expr();
        // match to
        Match(TokenType.IDENTIFIER, "to");
        // match ending value
        ASTNode to = Expr();
        // match do
        Match(TokenType.IDENTIFIER, "do");
        // get for block
        ASTNode forBlock = StatementList();
        // match end
        Match(TokenType.IDENTIFIER, "end");

        // construct for node
        ASTNode forNode = new(NodeType.ForTo, null, forToken.line, forToken.pos, path);
        forNode.AddChild(new(NodeType.Identifier, iteratorVar.value, iteratorVar.line, iteratorVar.pos, path));
        forNode.AddChild(value);
        forNode.AddChild(to);
        forNode.AddChild(forBlock);

        return forNode;
    }

    // class definition
    ASTNode ClassStatement()
    {
        // match class
        Match(TokenType.IDENTIFIER, "class");
        // get class name
        string className = Advance().value ?? throw new EigerError(path, Peek().line, Peek().pos, "Expected class name", EigerError.ErrorType.ParserError);
        // get class body
        ASTNode body = StatementList();
        // match end
        Match(TokenType.IDENTIFIER, "end");
        // construct the node
        ASTNode node = new ASTNode(NodeType.Class, className, body.line, body.pos, path);

        node.AddChild(body);

        return node;
    }

    // dataclass definition
    ASTNode DataclassStatement()
    {
        // match class
        Match(TokenType.IDENTIFIER, "dataclass");
        // get class name
        string className = Advance().value ?? throw new EigerError(path, Peek().line, Peek().pos, "Expected dataclass name", EigerError.ErrorType.ParserError);
        // get class body
        ASTNode body = StatementList();
        // match end
        Match(TokenType.IDENTIFIER, "end");
        // construct the node
        ASTNode node = new ASTNode(NodeType.Dataclass, className, body.line, body.pos, path);

        node.AddChild(body);

        return node;
    }

    // while statement
    ASTNode WhileStatement()
    {
        // match while
        Token whileToken = Peek();
        Match(TokenType.IDENTIFIER, "while");
        // match condition (which is an expression)
        ASTNode condition = Expr();
        // match do
        Match(TokenType.IDENTIFIER, "do");
        // get while block
        ASTNode doBlock = StatementList();
        // match end
        Match(TokenType.IDENTIFIER, "end");
        // construct the node, first the condition, then the block
        ASTNode whileNode = new(NodeType.While, null, whileToken.line, whileToken.pos, path);
        whileNode.AddChild(condition);
        whileNode.AddChild(doBlock);
        return whileNode;
    }

    // if statement
    ASTNode IfStatement()
    {
        // natch if
        Token ifToken = Peek();
        Match(TokenType.IDENTIFIER, "if");
        // match condition (which is an expression)
        ASTNode condition = Expr();
        // match then
        Match(TokenType.IDENTIFIER, "then");
        // get if block
        ASTNode ifBlock = StatementList();
        // save space for the else block (if it exists)
        ASTNode? elseBranch = null;
        // if the current token is an else
        if (Peek().value == "else")
        {
            // match else
            Match(TokenType.IDENTIFIER, "else");
            // get else block
            elseBranch = StatementList();
        }
        // match end
        Match(TokenType.IDENTIFIER, "end");

        // construct the node, first the condition, then the if block, then the else block (if exists)
        ASTNode ifNode = new(NodeType.If, null, ifToken.line, ifToken.pos, path);
        ifNode.AddChild(condition);
        ifNode.AddChild(ifBlock);
        if (elseBranch != null)
        {
            ifNode.AddChild(elseBranch);
        }
        return ifNode;
    }

    // expression
    ASTNode Expr()
    {
        // get term first (higher precedence)
        ASTNode node = Term();
        while (true)
        {
            // get the token type
            TokenType tokenType = Peek().type;

            // if it's an expression operator
            if (assignOps.Contains(tokenType))
            {
                Token op = Advance(); // get operator
                ASTNode right = Expr(); // get right hand side
                ASTNode binOpNode = new(NodeType.BinOp, op.value, op.line, op.pos, path); // construct the node
                binOpNode.AddChild(node);
                binOpNode.AddChild(right);
                node = binOpNode;
            }
            else if (exprOps.Contains(tokenType))
            {
                Token op = Advance(); // get operator
                ASTNode right = Term(); // get right hand side
                ASTNode binOpNode = new ASTNode(NodeType.BinOp, op.value, op.line, op.pos, path); // construct the node
                binOpNode.AddChild(node);
                binOpNode.AddChild(right);
                node = binOpNode;
            }
            // done parsing expression
            else
            {
                break;
            }
        }
        return node;
    }

    // term
    ASTNode Term()
    {
        // get factor first (higher precedence)
        ASTNode node = Factor();

        // while it's a term operator
        while (termOps.Contains(Peek().type))
        {
            Token op = Advance(); // get operator
            ASTNode right = Factor(); // get right hand side
            ASTNode binOpNode = new(NodeType.BinOp, op.value, op.line, op.pos, path); // construct the node
            binOpNode.AddChild(node);
            binOpNode.AddChild(right);
            node = binOpNode;
        }
        return node;
    }

    // factor
    ASTNode Factor()
    {
        // if its a number literal
        if (Peek().type == TokenType.NUMBER)
        {
            Token numberToken = Advance();

            if (Peek().type == TokenType.DOT) // if it's an atrribute access of a literal like a.AsString()
            {
                return ParseAttrAccess(new ASTNode(NodeType.Literal, numberToken.value, numberToken.line, numberToken.pos, path));
            }

            return new ASTNode(NodeType.Literal, numberToken.value, numberToken.line, numberToken.pos, path);
        }
        // if it's an identifier
        else if (Peek().type == TokenType.IDENTIFIER)
        {
            Token identToken = Advance();
            // Check for function call or element access
            if (Peek().type == TokenType.LPAREN)
            {
                current--;
                return FunctionCallStatement();
            }
            else if (Peek().type == TokenType.DOT)
            {
                Token op = Advance(); // get operator (.)
                ASTNode right = Factor(); // get right hand side
                ASTNode attrAccessNode = new ASTNode(NodeType.AttrAccess, op.value, op.line, op.pos, path); // construct the node
                attrAccessNode.AddChild(new ASTNode(NodeType.Identifier, identToken.value, identToken.line, identToken.pos,path));
                attrAccessNode.AddChild(right);
                return attrAccessNode;
            }
            else if (Peek().type == TokenType.LSQUARE)
            {
                return ParseElementAccess(new ASTNode(NodeType.Identifier, identToken.value, identToken.line, identToken.pos, path));
            }
            else
            {
                if (identToken.value == "true" || identToken.value == "false" || identToken.value == "nix")
                    return new ASTNode(NodeType.Literal, identToken.value, identToken.line, identToken.pos, path);
                else
                    return new ASTNode(NodeType.Identifier, identToken.value, identToken.line, identToken.pos, path);
            }
        }
        // if it's a string literal
        else if (Peek().type == TokenType.STRING)
        {
            Token stringToken = Advance();
            ASTNode stringNode = new ASTNode(NodeType.Literal, stringToken.value, stringToken.line, stringToken.pos, path);
            // Check for element access
            if (Peek().type == TokenType.LSQUARE)
            {
                return ParseElementAccess(stringNode);
            }
            else if (Peek().type == TokenType.DOT) // if it's an atrribute access of a literal like a.AsString()
            {
                return ParseAttrAccess(stringNode);
            }
            return stringNode;
        }
        // parenthesis in expressions
        else if (Peek().type == TokenType.LPAREN)
        {
            Match(TokenType.LPAREN);
            ASTNode node = Expr();
            Match(TokenType.RPAREN);
            return node;
        }
        // if it's an array
        else if (Peek().type == TokenType.LSQUARE)
        {
            return ParseArrayLiteral();
        }
        else
        {
            // unexpected token
            throw new EigerError(path, Peek().line, Peek().pos, $"{Globals.UnexpectedTokenStr} `{Peek().value}`", EigerError.ErrorType.ParserError);
        }
    }

    // Parse array literal
    ASTNode ParseArrayLiteral()
    {
        Token lsquareToken = Peek();
        Match(TokenType.LSQUARE);
        ASTNode arrayNode = new ASTNode(NodeType.Array, null, lsquareToken.line, lsquareToken.pos, path);
        while (true)
        {
            if (Peek().type == TokenType.RSQUARE) { Match(TokenType.RSQUARE); break; }
            arrayNode.AddChild(Expr());
            if (Peek().type == TokenType.RSQUARE) { Match(TokenType.RSQUARE); break; }
            else if (Peek().type == TokenType.COMMA) { Match(TokenType.COMMA); continue; }
        }
        // Check for element access
        if (Peek().type == TokenType.LSQUARE)
        {
            return ParseElementAccess(arrayNode);
        }
        else if (Peek().type == TokenType.DOT) // if it's an atrribute access of a literal like a.AsString()
        {
            return ParseAttrAccess(arrayNode);
        }
        return arrayNode;
    }

    ASTNode ParseAttrAccess(ASTNode target)
    {
        Token op = Advance(); // get operator (.)
        ASTNode right = Factor(); // get right hand side
        ASTNode attrAccessNode = new ASTNode(NodeType.AttrAccess, op.value, op.line, op.pos, path); // construct the node
        attrAccessNode.AddChild(target);
        attrAccessNode.AddChild(right);
        return attrAccessNode;
    }

    // Parse element access
    ASTNode ParseElementAccess(ASTNode target)
    {
        while (Peek().type == TokenType.LSQUARE)
        {
            Token lsquareToken = Peek();
            Match(TokenType.LSQUARE);
            ASTNode indexNode = Expr();
            Match(TokenType.RSQUARE);
            target = new ASTNode(NodeType.ElementAccess, null, lsquareToken.line, lsquareToken.pos, path)
            {
                children = { target, indexNode }
            };
        }
        return target;
    }
}