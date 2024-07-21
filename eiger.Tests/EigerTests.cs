using System.Text;

namespace EigerLang.Tests
{
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
            Program.Execute(code, "<stdin>", printexprs);
            string actual = _stringWriter.ToString().Trim().Replace("\r\n","\r");
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
                "x = 0\nwhile x != 3 do\n emit(x)\n x += 1\nend",
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
                "x = 5\ny = x + 2\nemitln(y)",
                "7"
            );
        }

        [TestMethod]
        public void FunctionTests()
        {
            // Function definition and call
            TestCode(
                "func add(a, b)\n result = a + b\n ret result\nend\nemitln(add(2, 3))",
                "5"
            );

            // Function with no parameters
            TestCode(
                "func greet()\n emitln(\"Hello, world!\")\nend\ngreet()",
                "Hello, world!"
            );
        }

        [TestMethod]
        public void OOPClassTests()
        {
            // Test class instantiation and method calls
            TestCode(
                "class Person\n" +
                "    func Person(name, surname)\n" +
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
                "px = Person(\"Name1\", \"Surname1\")\n" +
                "py = Person(\"Name2\", \"Surname2\")\n" +
                "px.Introduction()\n" +
                "py.Introduction()\n" +
                "px.Greet(py)",
                "I am Name1 Surname1!I am Name2 Surname2!Name1 greeted Name2!"
            );

            // Test creating and using a class instance inline
            TestCode(
                "class Person\n" +
                "    func Person(name, surname)\n" +
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
    }
}
