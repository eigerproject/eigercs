/*
 * EIGERLANG PARSER
 * WRITTEN BY VARDAN PETROSYAN
*/

using EigerLang.Errors;
using EigerLang.Tokenization;

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

    // parse the root
    public ASTNode Parse()
    {
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
        throw new EigerError("<stdin>", tokens[tokens.Count - 1].line, tokens[^1].pos, "Unexpected End of Input");
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
            throw new EigerError("<stdin>", Peek().line, Peek().pos, $"Unexpected token `{Peek().value}`");
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
            throw new EigerError("<stdin>", Peek().line, Peek().pos, $"Unexpected token `{Peek().value}`");
        }
    }

    // statement list
    ASTNode StatementList()
    {
        ASTNode root = new ASTNode(NodeType.Block, null);
        // add statements until the end of block (end or else)
        while (current < tokens.Count && Peek().value.ToString() != "end" && Peek().value.ToString() != "else")
        {
            root.AddChild(Statement());
        }
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
            case "while": return WhileStatement();
            case "func": return FuncDefStatement();
            case "ret": return RetStatement();
            default: return Expr();
        }
    }

    // function definition
    ASTNode FuncDefStatement()
    {
        // match func
        Match(TokenType.IDENTIFIER, "func");

        // get func name
        string funcName = Advance().value ?? "";

        // create func def node
        ASTNode node = new(NodeType.FuncDef, funcName);
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
        }

        // match right parenthasis
        Match(TokenType.RPAREN);

        // parse the body
        ASTNode rootNode = StatementList();

        // match end
        Match(TokenType.IDENTIFIER, "end");

        // add the root node first, then the argnames
        node.AddChild(rootNode);

        foreach(var arg in args)
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
        ASTNode node = new ASTNode(NodeType.Return, null);
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

        ASTNode node = new(NodeType.FuncCall, funcName.value);

        // if the function has no args, return node
        if (Peek().type == TokenType.RPAREN) { Match(TokenType.RPAREN); return node; }

        // get arguments similar way in FuncDefStatement
        do // add arguments until there are no more
        {
            if (Peek().type == TokenType.COMMA) Advance(); // advance through the comma

            node.AddChild(Expr()); // add the expression to the args

        } while (Peek().type == TokenType.COMMA);  // if there is a comma right after the last expression

        // match right parenthasis
        Match(TokenType.RPAREN);

        return node;
    }

    // while statement
    ASTNode WhileStatement()
    {
        // match while
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
        ASTNode whileNode = new(NodeType.While, null);
        whileNode.AddChild(condition);
        whileNode.AddChild(doBlock);
        return whileNode;
    }

    // if statement
    ASTNode IfStatement()
    {
        // natch if
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
        ASTNode ifNode = new ASTNode(NodeType.If, null);
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

            // if its an assignment operator
            if (assignOps.Contains(tokenType))
            {
                Token op = Advance(); // get operator
                ASTNode right = Expr(); // get right hand side
                ASTNode binOpNode = new ASTNode(NodeType.BinOp, op.value); // construct the node
                binOpNode.AddChild(node);
                binOpNode.AddChild(right);
                node = binOpNode;
            }
            // if its an expression operator
            else if (exprOps.Contains(tokenType))
            {
                Token op = Advance(); // get operator
                ASTNode right = Term(); // get right hand side
                ASTNode binOpNode = new ASTNode(NodeType.BinOp, op.value); // construct the node
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
            ASTNode binOpNode = new ASTNode(NodeType.BinOp, op.value); // construct the node
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
            return new ASTNode(NodeType.Literal, Advance().value);
        }
        // if it's an identifier
        else if (Peek().type == TokenType.IDENTIFIER)
        {
            // if the next token is a left parenthasis, then it's a function call
            if (PeekNext() != null && PeekNext().type == TokenType.LPAREN)
            {
                return FunctionCallStatement();
            }
            else
            {
                return new ASTNode(NodeType.Identifier, Advance().value);
            }
        }
        // if it's a string literal
        else if (Peek().type == TokenType.STRING)
        {
            return new ASTNode(NodeType.Literal, Advance().value);
        }
        // parenthasis in expressions
        else if (Peek().type == TokenType.LPAREN)
        {
            Match(TokenType.LPAREN);
            ASTNode node = Expr();
            Match(TokenType.RPAREN);
            return node;
        }
        else
        {
            // unexpected token
            throw new EigerError("<stdin>", Peek().line, Peek().pos, $"Unexpected Token: {Peek()}");
        }
    }
}