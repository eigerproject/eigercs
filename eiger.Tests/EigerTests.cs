using System.Text;

namespace EigerLang.Tests;

[TestClass]
public class EigerTests
{
    private readonly StringWriter _stringWriter = new();
    private readonly TextWriter _originalConsoleOut = Console.Out;
    private StringBuilder _stringBuilder = new();

    [TestInitialize]
    public void Setup()
    {
        Console.SetOut(_stringWriter);
        _stringBuilder = _stringWriter.GetStringBuilder();
    }

    [TestCleanup]
    public void Teardown()
    {
        // Restore the original Console.Out
        Console.SetOut(_originalConsoleOut);
        _stringWriter.Dispose();
    }

    private void TestCode(string code, string expected, bool printexprs = false)
    {
        _stringBuilder.Clear(); // Clear the StringBuilder
        Program.Reset();
        Program.Execute(code, "<stdin>", printexprs);
        string actual = _stringWriter.ToString().Trim().Replace("\r\n", "\r");
        Assert.AreEqual(expected.Trim(), actual.Trim());
    }

    [TestMethod]
    public void BuiltInFunctionTests()
    {
        // emitln
        TestCode("emitln(\"Hello, world!\")", "Hello, world!");

        // double
        TestCode("double(\"3.1415\")", "3.1415", true);

        // int
        TestCode("int(\"3.1415\")", "3", true);
        TestCode("int(\"3.765\")", "4", true);
        TestCode("int(3.765)", "4", true);
        TestCode("int(3.1415)", "3", true);
    }

    [TestMethod]
    public void EmptyCodeTest()
    {
        TestCode("", "", true);
    }

    [TestMethod]
    public void ExpressionTests()
    {
        // Basic arithmetic
        TestCode("emitln(5 + 3 * 2)", "11");
        TestCode("emitln(10 / 2 - 1)", "4");
        TestCode("emitln(7 * (5 - 3))", "14");

        // String concatenation
        TestCode("emitln(\"Hello \" + \"world!\")", "Hello world!");
    }

    [TestMethod]
    public void IfElseTests()
    {
        // if
        TestCode("if 1 ?= 1 then emitln(\"True\") end", "True");

        // if-else
        TestCode("if 1 ?= 0 then emitln(\"False\") else emitln(\"True\") end", "True");

        // if-elif-else
        TestCode(
            "if 1 ?= 0 then emitln(\"First\") elif 1 ?= 1 then emitln(\"Second\") else emitln(\"Third\") end",
            "Second"
        );
    }

    [TestMethod]
    public void WhileLoopTests()
    {
        // Basic while loop
        TestCode(
            "let x = 0\nwhile x != 3 do\n emit(x)\n x += 1\nend",
            "012"
        );
    }

    [TestMethod]
    public void ForLoopTests()
    {
        // Basic for loop
        TestCode(
            "for i = 1 to 10 do\n emit(i)\nend",
            "123456789"
        );
    }

    [TestMethod]
    public void VariableTests()
    {
        // Variable assignment and usage
        TestCode(
            "let x = 5\nlet y = x + 2\nemitln(y)",
            "7"
        );
    }

    [TestMethod]
    public void FunctionTests()
    {
        // Function definition and call
        TestCode(
            "func add(a, b)\n let result = a + b\n ret result\nend\nemitln(add(2, 3))",
            "5"
        );

        // Function with no parameters
        TestCode(
            "func greet()\n emitln(\"Hello, world!\")\nend\ngreet()",
            "Hello, world!"
        );
    }

    [TestMethod]
    public void OOPInlineTest()
    {
        // Test creating and using a class instance inline
        TestCode(
            "class Person\n" +
            "    let name" +
            "    let surname" +
            "    func new(name, surname)\n" +
            "        this.name = name\n" +
            "        this.surname = surname\n" +
            "    end\n" +
            "    func Introduction()\n" +
            "        emitln(\"I am \" + this.name + \" \" + this.surname + \"!\")\n" +
            "    end\n" +
            "end\n" +
            "Person(\"a\", \"b\").Introduction()",
            "I am a b!"
        );
    }

    [TestMethod]
    public void OOPClassTests()
    {
        // Test class instantiation and method calls
        TestCode(
            "class Person\n" +
            "    let name" +
            "    let surname" +
            "    func new(name, surname)\n" +
            "        this.name = name\n" +
            "        this.surname = surname\n" +
            "    end\n" +
            "    func Introduction()\n" +
            "        emit(\"I am \" + this.name + \" \" + this.surname + \"!\")\n" +
            "    end\n" +
            "    func Greet(other)\n" +
            "        emit(this.name + \" greeted \" + other.name + \"!\")\n" +
            "    end\n" +
            "end\n" +
            "let px = Person(\"Name1\", \"Surname1\")\n" +
            "let py = Person(\"Name2\", \"Surname2\")\n" +
            "px.Introduction()\n" +
            "py.Introduction()\n" +
            "px.Greet(py)",
            "I am Name1 Surname1!I am Name2 Surname2!Name1 greeted Name2!"
        );
    }

    [TestMethod]
    public void BlockScopeTests()
    {
        TestCode(
            "let x = 10\nif x ?= 10 then\n let y = 20\n emit(y)\nend\nemit(x)",
            "2010"
        );

        TestCode(
            "let x = 10\nif x ?= 10 then\n x = 20\n emit(x)\nend\nemit(x)",
            "2020"
        );

        TestCode(
            "let x = 5\nfunc example()\n let y = x + 2\n emit(y)\nend\nexample()",
            "7"
        );

        TestCode(
            "let x = 5\nfunc example()\n x = 10\n emit(x)\nend\nexample()\nemit(x)",
            "1010"
        );
    }

    [TestMethod]
    public void NestedBlocksTests()
    {
        // Nested blocks and scopes
        TestCode(
            "let x = 5 let y = 0\nif x ?= 5 then\n y = 10\n if y ?= 10 then\n let z = 15\n emit(z)\n end\n end\nemit(y)\nemit(x)",
            "15105"
        );

        // Variables defined in inner blocks not affecting outer blocks
        TestCode(
            "let x = 5\nif x ?= 5 then\n let y = 10\n if y ?= 10 then\n x = 20\n emit(x)\n end\n emit(y)\nend\nemit(x)",
            "201020"
        );
    }

    [TestMethod]
    public void InlineFunctionTests()
    {
        // Inline function tests
        TestCode(
            "let f = func(a, b) > a + b emitln(f(2, 3))",
            "5"
        );

        TestCode(
            "let f = func(a) > a * a emitln(f(4))",
            "16"
        );
    }

    [TestMethod]
    public void AnonymousFunctionTests()
    {
        // Anonymous function tests
        TestCode(
            "let anonymous = func(a, b) ret a + b end\nemitln(anonymous(2, 3))",
            "5"
        );

        TestCode(
            "let anonymous = func(a) ret a * a end\nemitln(anonymous(4))",
            "16"
        );
    }

    [TestMethod]
    public void CombinedFunctionTests()
    {
        // Combined inline and anonymous function tests
        TestCode(
            "let anonymous_and_inline = func(a, b) > a + b\nemitln(anonymous_and_inline(2, 3))",
            "5"
        );

        TestCode(
            "let anonymous_and_inline = func(a) > a * a\nemitln(anonymous_and_inline(4))",
            "16"
        );
    }

    [TestMethod]
    public void BasicBreakInWhileLoopTest()
    {
        // This code should break out of the loop when x == 2, so the output should be "012"
        TestCode(
            "let x = 0\nwhile x != 5 do\n emit(x)\n if x ?= 2 then brk end\n x += 1\nend",
            "012"
        );
    }

    [TestMethod]
    public void NestedLoopsWithBreakTest()
    {
        // This code should break out of the inner loop when j == 1, so the output should be "01"
        TestCode(
            "for i = 0 to 2 do\n for j = 0 to 2 do\n emit(j)\n if j ?= 1 then brk end\n end\nend",
            "01"
        );
    }

    [TestMethod]
    public void BreakWithNestedConditionsTest()
    {
        // This code will break from the loop when x == 2, so the output should be "012"
        TestCode(
            "let x = 0\nwhile x != 5 do\n emit(x)\n if x ?= 2 then\n brk\n end\n x += 1\nend",
            "012"
        );
    }

    [TestMethod]
    public void BreakStatementTests()
    {
        // Test `brk` in a while loop
        TestCode(
            "let x = 0\nwhile x < 5 do\n if x ?= 3 then brk end\n emit(x)\n x += 1\nend",
            "012"
        );

        // Test `brk` in a for loop
        TestCode(
            "for i = 0 to 5 do\n if i ?= 3 then brk end\n emit(i)\nend",
            "012"
        );
    }


    [TestMethod]
    public void BasicReturnValueTest()
    {
        // This code will return the value 10 from the function and print it
        TestCode(
            "func return_ten()\n ret 10\nend\nemitln(return_ten())",
            "10"
        );
    }

    [TestMethod]
    public void ReturnStatementTests()
    {
        // Simple return statement
        TestCode(
            "func returnFive()\n ret 5\nend\nemitln(returnFive())",
            "5"
        );

        // Return result of calculation
        TestCode(
            "func add(a, b)\n ret a + b\nend\nemitln(add(4, 6))",
            "10"
        );

        // Return value based on condition
        TestCode(
            "func conditionalReturn(x)\n if x ?= 1 then ret 10\n else ret 20\nend\nend\nemitln(conditionalReturn(1))",
            "10"
        );

        TestCode(
            "func conditionalReturn(x)\n if x ?= 1 then ret 10\n else ret 20\nend\nend\nemitln(conditionalReturn(2))",
            "20"
        );

        // Test return in nested function calls
        TestCode(
            "func multiply(a, b)\n ret a * b\nend\nfunc addAndMultiply(a, b, c)\n let sum = a + b\n ret multiply(sum, c)\nend\nemitln(addAndMultiply(2, 3, 4))",
            "20"
        );

        // Test return with early exit
        TestCode(
            "func earlyReturn(x)\n if x ?= 1 then ret 10 \n emitln(\"This should not print\")\n ret 20\nend end\nemitln(earlyReturn(1))",
            "10"
        );

        // Test nested return statements in functions
        TestCode(
            "func outer()\n func inner(x)\n if x ?= 1 then ret 5 end\n ret 6\nend\n ret inner(1)\nend\nemitln(outer())",
            "5"
        );

        TestCode(
            "func outer()\n func inner(x)\n if x ?= 1 then ret 5 end\n ret 6\nend\n ret inner(2)\nend\nemitln(outer())",
            "6"
        );
    }

    [TestMethod]
    public void ReturnInsideLoopsTest()
    {
        // This code will return 3 immediately from the function, thus only printing "3"
        TestCode(
            "func find_number()\n let x = 0\n while x != 5 do\n if x ?= 3 then ret x end\n x += 1\n end\nend\nemitln(find_number())",
            "3"
        );
    }

    [TestMethod]
    public void EventLibTest()
    {
        // Subscribing to event and invoking event
        TestCode(
            "include event\nlet ev = Event()\nev.Subscribe(func(msg) emitln(\"First Subscriber: \" + msg) end)\nev.Subscribe(func(msg) emitln(\"Second Subscriber: \" + msg) end)\nev.Invoke(\"Hello!\")",
            "First Subscriber: Hello!\nSecond Subscriber: Hello!\n"
        );

        // Unsubscribing from event
        TestCode(
            "include event\nfunc functionOne(args) emitln(\"This is function one\") end\nfunc functionTwo(args) emitln(\"This is function two\") end\nlet ev = Event()\nev.Subscribe(functionTwo)\nev.Subscribe(functionOne)\nev.Unsubscribe(functionTwo)\nev.Invoke(nix)",
            "This is function one"
        );
    }

    [TestMethod]
    public void MathLibTest()
    {
        TestCode(
            "include math\nemitln(math.abs(-128))",
            "128"
        );

        TestCode(
            "include math\nemitln(math.abs(128))",
            "128"
        );

        TestCode(
            "include math\nemitln(math.sqrt(100))",
            "10"
        );

        TestCode(
            "include math\nemitln(math.sqrt(-100))",
            "NaN"
        );

        TestCode(
            "include math\nemitln(math.pow(2, -1))",
            "0.5"
        );

        TestCode(
            "include math\nemitln(math.pow(2, 10))",
            "1024"
        );

        TestCode(
            "include math\nemitln(math.mod(2, 2))",
            "0"
        );

        TestCode(
            "include math\nemitln(math.mod(37, 3))",
            "1"
        );

        TestCode(
            "include math\nemitln(math.factorial(5))",
            "120"
        );

        TestCode(
            "include math\nemitln(math.factorial(0))",
            "1"
        );

        TestCode(
            "include math\nemitln(math.sin(0))",
            "0"
        );
    }
}
