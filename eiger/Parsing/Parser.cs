/*
 * EIGERLANG PARSER
*/

using EigerLang.Errors;
using EigerLang.Tokenization;

namespace EigerLang.Parsing;

public class Parser(List<Token> tokens)
{
    // stores the index of the current token
    int current;

    // term level operators
    readonly HashSet<TokenType> termOps = new()
    {
        TokenType.MUL, TokenType.DIV,
        TokenType.PERC, TokenType.CARET
    };

    // expression level operators
    readonly HashSet<TokenType> exprOps = new()
    {
        TokenType.PLUS, TokenType.MINUS
    };

    // comparison or assignment level operators
    readonly HashSet<TokenType> comparOps = new()
    {
        TokenType.EQ, TokenType.PLUSEQ,TokenType.MINUSEQ,TokenType.MULEQ,TokenType.DIVEQ,TokenType.EQEQ, TokenType.NEQEQ,TokenType.GT, TokenType.LT, TokenType.GTE, TokenType.LTE, TokenType.AT,
    };

    readonly List<string> varModifiers = new()
    {
        "readonly", "private"
    };

    string path = "<stdin>";

    // parse the root
    public ASTNode Parse(string p)
    {
        path = p;
        return StatementList(true);
    }

    // advance to next token
    Token Advance()
    {
        // if the pointer is inside the bounds of the token array
        if (current < tokens.Count)
            // return the current token and advance
            return tokens[current++];

        // if not, throw error
        throw new EigerError(path, tokens[^1].line, tokens[^1].pos, "Unexpected End of Input", EigerError.ErrorType.ParserError);
    }

    // return current token
    Token Peek()
    {
        // if the pointer is inside the bounds of the token array
        if (current < tokens.Count)
            // return the current token
            return tokens[current];

        // i don't know what to return here, this not a good solution
        return new Token(-1, -1, TokenType.UNDEFINED);
    }

    // return next token without advancing
    Token? PeekNext()
    {
        // if the pointer is inside the bounds of the token array
        if (current + 1 < tokens.Count)
            // return the next token without advancing
            return tokens[current + 1];
        return null;
    }

    // check if the current token matches the expected one and advance
    void Match(TokenType type)
    {
        if (Peek().type == TokenType.UNDEFINED)
            throw new EigerError(path, tokens[^1].line, tokens[^1].pos, $"Unexpected end of Input", EigerError.ErrorType.ParserError);

        if (Peek().type == type)
            Advance();
        else
            throw new EigerError(path, Peek().line, Peek().pos, $"{Globals.UnexpectedTokenStr} `{Peek().value}`, expected a {type}", EigerError.ErrorType.ParserError);
    }

    // check if the current token matches the expected one and advance
    void Match(TokenType type, dynamic expected)
    {
        if (Peek().type == TokenType.UNDEFINED)
            throw new EigerError(path, tokens[^1].line, tokens[^1].pos, $"Unexpected end of Input", EigerError.ErrorType.ParserError);

        if (Peek().type == type && Peek().value == expected)
            Advance();
        else
            throw new EigerError(path, Peek().line, Peek().pos, $"{Globals.UnexpectedTokenStr} `{Peek().value}`, expected {expected}", EigerError.ErrorType.ParserError);
    }

    // statement list
    ASTNode StatementList(bool isRoot = false)
    {
      if(current < tokens.Count && Peek().type == TokenType.LBRACE && !isRoot) Advance();
        ASTNode root = new(NodeType.Block, null, 1, 0, path);
        // add statements until the end of block (end or else)
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        while (current < tokens.Count && Peek().type != TokenType.RBRACE)
            root.AddChild(Statement());
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        if(current < tokens.Count && Peek().type == TokenType.RBRACE) Advance();
        return root;
    }

    // single statement
    ASTNode Statement()
    {
        // store the peeked token
        Token peeked = Peek();
        if(peeked.type == TokenType.LBRACE) return StatementList();
        return peeked.value switch
        {
            // if it's a statement, parse a statement, else, parse an expression
            "let" => LetStatement(),
            "if" => IfStatement(),
            "for" => ForToStatement(),
            "while" => WhileStatement(),
            "func" => FuncDefStatement(),
            "ret" => RetStatement(),
            "brk" => BreakStatement(),
            "cont" => ContinueStatement(),
            "class" => ClassStatement(),
            "namespace" => NamespaceStatement(),
            "include" => IncludeStatement(),
            _ => Expr(),
        };
    }

    // include statement
    ASTNode IncludeStatement()
    {
        Match(TokenType.IDENTIFIER, "include");
        ASTNode pathToken = Factor();
        ASTNode includeNode = new ASTNode(NodeType.Include, null, pathToken.line, pathToken.pos, path);
        includeNode.AddChild(pathToken);
        return includeNode;
    }

    // function definition
    ASTNode FuncDefStatement(bool includeName = true)
    {
        // match func
        Token funcTok = Peek();
        Match(TokenType.IDENTIFIER, "func");

        List<string> modifiers = [];

        while (Peek().type == TokenType.IDENTIFIER && varModifiers.Contains(Peek().value))
        {
            string modifier = Peek().value ?? throw new EigerError(path, Peek().line, Peek().pos, $"Modifier has no value", EigerError.ErrorType.RuntimeError);

            if (modifiers.Contains(modifier))
                throw new EigerError(path, Peek().line, Peek().pos, $"Double modifier {modifier}", EigerError.ErrorType.RuntimeError);
            else
                modifiers.Add(modifier);

            Advance();
        }

        // get func name
        string funcName = "";

        if (includeName)
            funcName = Advance().value ?? "";

        // create func def node
        ASTNode node = new(NodeType.FuncDef, new dynamic?[2] { modifiers, funcName }, funcTok.line, funcTok.pos, path);
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
                if (Peek().type == TokenType.PLUS)  // variadic arguments
                {
                    args.Add(new ASTNode(NodeType.Literal, '+', funcTok.line, funcTok.pos, path));
                    Advance();
                    args.Add(Factor()); // add the name to the args
                    break;
                }

                args.Add(Factor()); // add the name to the args

            } while (Peek().type == TokenType.COMMA); // if there is a comma right after the argname

            // match right parenthasis
            Match(TokenType.RPAREN);
        }

        if (Peek().type == TokenType.GT)
        {
            node.type = NodeType.FuncDefInline;

            // match GTE
            Match(TokenType.GT);

            ASTNode expr = Expr();

            node.AddChild(expr);
        }
        else
        {
            // parse the body
            ASTNode rootNode = StatementList();

            // add the root node first, then the argnames
            node.AddChild(rootNode);
        }

        foreach (var arg in args)
            node.AddChild(arg);

        return node;
    }

    // continue statement
    ASTNode ContinueStatement()
    {
        // match cont
        Token breakToken = Peek();
        Match(TokenType.IDENTIFIER, "cont");
        // create node
        ASTNode node = new(NodeType.Continue, null, breakToken.line, breakToken.pos, path);
        return node;
    }

    // return statement
    ASTNode BreakStatement()
    {
        // match brk
        Token breakToken = Peek();
        Match(TokenType.IDENTIFIER, "brk");
        // create node
        ASTNode node = new(NodeType.Break, null, breakToken.line, breakToken.pos, path);
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
    ASTNode FunctionCallStatement(ASTNode nameNode)
    {
        // match left parenthasis
        Match(TokenType.LPAREN);

        ASTNode node = new(NodeType.FuncCall, null, nameNode.line, nameNode.pos, path);
        node.AddChild(nameNode);

        // if the function has no args, return node
        if (Peek().type == TokenType.RPAREN)
        {
            Match(TokenType.RPAREN);
            if (Peek().type == TokenType.DOT) // if it's an atrribute access of function call like Person("test").a
                return ParseAttrAccess(node);
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
        // get for block
        ASTNode forBlock = Statement();

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
        // construct the node
        ASTNode node = new ASTNode(NodeType.Class, className, body.line, body.pos, path);

        node.AddChild(body);

        return node;
    }

    // namespace definition
    ASTNode NamespaceStatement()
    {
        // match class
        Match(TokenType.IDENTIFIER, "namespace");
        // get class name
        string className = Advance().value ?? throw new EigerError(path, Peek().line, Peek().pos, "Expected namesapce name", EigerError.ErrorType.ParserError);
        // get class body
        ASTNode body = StatementList();
        // construct the node
        ASTNode node = new ASTNode(NodeType.Namespace, className, body.line, body.pos, path);

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
        // get while block
        ASTNode doBlock = Statement();
        // construct the node, first the condition, then the block
        ASTNode whileNode = new(NodeType.While, null, whileToken.line, whileToken.pos, path);
        whileNode.AddChild(condition);
        whileNode.AddChild(doBlock);
        return whileNode;
    }

    // let statement
    ASTNode LetStatement()
    {
        // match let
        Token letToken = Peek();
        Match(TokenType.IDENTIFIER, "let");

        List<string> modifiers = [];

        while (Peek().type == TokenType.IDENTIFIER && varModifiers.Contains(Peek().value))
        {
            string modifier = Peek().value ?? throw new EigerError(path, Peek().line, Peek().pos, $"Modifier has no value", EigerError.ErrorType.RuntimeError);

            if (modifiers.Contains(modifier))
                throw new EigerError(path, Peek().line, Peek().pos, $"Double modifier {modifier}", EigerError.ErrorType.RuntimeError);
            else
                modifiers.Add(modifier);

            Advance();
        }

        Token variableName = Advance();

        ASTNode letNode = new(NodeType.Let, new dynamic?[2] { modifiers, variableName.value }, letToken.line, letToken.pos, path);

        // if the variable has an initial value (let x = 10)
        if (Peek().type == TokenType.EQ)
        {
            Match(TokenType.EQ);
            letNode.AddChild(Expr());
        }

        return letNode;
    }

    // if statement
    ASTNode IfStatement()
    {
        // match if
        Token ifToken = Peek();
        Match(TokenType.IDENTIFIER, "if");
        // match condition (which is an expression)
        ASTNode condition = Expr();

        // get if block
        ASTNode ifBlock = Statement();

        // list to store else if branches
        List<ASTNode> elseIfBranches = new List<ASTNode>();

        // save space for the else block (if it exists)
        ASTNode? elseBranch = null;

        // check for else if statements
        while (Peek().value == "elif")
        {
            // match else if
            Match(TokenType.IDENTIFIER, "elif");
            // match condition (which is an expression)
            ASTNode elseIfCondition = Expr();
            // get else if block
            ASTNode elseIfBlock = Statement();

            // construct the else if node
            ASTNode elseIfNode = new(NodeType.If, null, ifToken.line, ifToken.pos, path);
            elseIfNode.AddChild(elseIfCondition);
            elseIfNode.AddChild(elseIfBlock);
            elseIfBranches.Add(elseIfNode);
        }

        // if the current token is else
        if (Peek().value == "else")
        {
            // match else
            Match(TokenType.IDENTIFIER, "else");
            // get else block
            elseBranch = Statement();
        }

        // construct the node, first the condition, then the if block, then the else if blocks, then the else block (if exists)
        ASTNode ifNode = new(NodeType.If, null, ifToken.line, ifToken.pos, path);
        ifNode.AddChild(condition);
        ifNode.AddChild(ifBlock);
        foreach (var elseIfNode in elseIfBranches)
            ifNode.AddChild(elseIfNode);
        if (elseBranch != null)
            ifNode.AddChild(elseBranch);
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
            if (comparOps.Contains(tokenType))
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
                break;
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

    ASTNode CreateLiteralNode(Token token)
    {
        return new ASTNode(NodeType.Literal, token.value, token.line, token.pos, path);
    }

    ASTNode HandleUnaryOp(Token op)
    {
        ASTNode fact = Factor();
        ASTNode unaryOpNode = new(NodeType.UnaryOp, op.value, op.line, op.pos, path);
        unaryOpNode.AddChild(fact);

        return (Peek().type == TokenType.DOT) ? ParseAttrAccess(unaryOpNode) : unaryOpNode;
    }

    ASTNode HandleIdentifier(Token identToken, bool checkFuncCall)
    {
        if (identToken.value == "func")
        {
            return FuncDefStatement(false);
        }
        else
        {
            Advance();
            ASTNode identNode = new ASTNode(NodeType.Identifier, identToken.value, identToken.line, identToken.pos, path);

            if (checkFuncCall)
            {
                if (Peek().type == TokenType.DOT) return ParseAttrAccess(identNode);
                if (Peek().type == TokenType.LSQUARE) return ParseElementAccess(identNode);
                if (Peek().type == TokenType.LPAREN) return FunctionCallStatement(identNode);
            }

            return (identToken.value == "true" || identToken.value == "false" || identToken.value == "nix") ? CreateLiteralNode(identToken) : identNode;
        }
    }

    ASTNode HandleStringLiteral(Token stringToken)
    {
        ASTNode stringNode = CreateLiteralNode(stringToken);
        if (Peek().type == TokenType.LSQUARE) return ParseElementAccess(stringNode);
        if (Peek().type == TokenType.DOT) return ParseAttrAccess(stringNode);
        return stringNode;
    }

    ASTNode HandleParenthesisExpression()
    {
        Match(TokenType.LPAREN);
        ASTNode node = Expr();
        Match(TokenType.RPAREN);

        if (Peek().type == TokenType.LSQUARE) return ParseElementAccess(node);
        if (Peek().type == TokenType.DOT) return ParseAttrAccess(node);
        return node;
    }

    ASTNode Factor(bool checkFuncCall = true)
    {
        switch (Peek().type)
        {
            case TokenType.NUMBER:
                Token numberToken = Advance();
                return (Peek().type == TokenType.DOT) ? ParseAttrAccess(CreateLiteralNode(numberToken)) : CreateLiteralNode(numberToken);
            case TokenType.MINUS:
            case TokenType.IDENTIFIER when Peek().value == "not":
                return HandleUnaryOp(Advance());
            case TokenType.IDENTIFIER:
                return HandleIdentifier(Peek(), checkFuncCall);
            case TokenType.STRING:
                return HandleStringLiteral(Advance());
            case TokenType.LPAREN:
                return HandleParenthesisExpression();
            case TokenType.LSQUARE:
                return ParseArrayLiteral();
            case TokenType.UNDEFINED:
                throw new EigerError(path, tokens[^1].line, tokens[^1].pos, $"Unexpected end of Input", EigerError.ErrorType.ParserError);

            default:
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
            return ParseElementAccess(arrayNode);
        else if (Peek().type == TokenType.DOT) // if it's an atrribute access of a literal like a.AsString()
            return ParseAttrAccess(arrayNode);
        return arrayNode;
    }

    ASTNode ParseAttrAccess(ASTNode target)
    {
        while (true)
        {
            if (Peek().type == TokenType.DOT)
            {
                Token op = Advance(); // get operator (.)
                ASTNode right = Factor(false); // get right hand side
                ASTNode attrAccessNode = new ASTNode(NodeType.AttrAccess, null, op.line, op.pos, path); // construct the node
                attrAccessNode.AddChild(target);
                attrAccessNode.AddChild(right);
                target = attrAccessNode;
            }
            else if (Peek().type == TokenType.LSQUARE)
                target = ParseElementAccess(target);
            else if (Peek().type == TokenType.LPAREN)
                target = FunctionCallStatement(target);
            else
                break;
        }
        return target;
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

        if (Peek().type == TokenType.DOT) // if it's an atrribute access of a literal like a.AsString()
            return ParseAttrAccess(target);

        return target;
    }
}
