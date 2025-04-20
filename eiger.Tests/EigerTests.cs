using System.Text;

namespace EigerLang.Tests;

[TestClass]
public class EigerTests
{
    private readonly StringWriter _stringWriter = new();
    private readonly TextWriter _originalConsoleOut = Console.Out;
    private StringBuilder _stringBuilder = new();

    private static readonly string TestsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tests");

    [TestInitialize]
    public void Setup()
    {
        Console.SetOut(_stringWriter);
        _stringBuilder = _stringWriter.GetStringBuilder();
    }

    [TestCleanup]
    public void Teardown()
    {
        Console.SetOut(_originalConsoleOut);
        _stringWriter.Dispose();
    }

    private void TestCode(string code, string expected, bool printexprs = false)
    {
        _stringBuilder.Clear();
        Program.Reset();
        Program.Execute(code, "<stdin>", printexprs);
        string actual = _stringWriter.ToString().Trim().Replace("\r\n", "\r");
        Assert.AreEqual(expected.Trim(), actual.Trim());
    }

    public static IEnumerable<object[]> GetTestCases()
    {
        var eiFiles = Directory.GetFiles(TestsFolder, "*.ei");

        foreach (var eiFile in eiFiles)
        {
            var expectedFile = Path.ChangeExtension(eiFile, ".expected");

            if (!File.Exists(expectedFile))
                throw new FileNotFoundException($"Expected file not found for {eiFile}");

            string testName = Path.GetFileNameWithoutExtension(eiFile); // Used for display purposes
            yield return new object[] { testName, eiFile, expectedFile };
        }
    }

    [DataTestMethod]
    [DynamicData(nameof(GetTestCases), DynamicDataSourceType.Method)]
    public void FileBasedTests(string testName, string eiFilePath, string expectedFilePath)
    {
        string code = File.ReadAllText(eiFilePath);
        string expectedOutput = File.ReadAllText(expectedFilePath);

        _stringBuilder.Clear();
        Program.Reset();
        Program.Execute(code, "<stdin>", false);

        string actualOutput = _stringWriter.ToString().Trim().Replace("\r\n", "\r");
        string expectedCleaned = expectedOutput.Trim().Replace("\r\n", "\r");

        Assert.AreEqual(expectedCleaned, actualOutput, $"Test failed for {testName}");

        _stringWriter.GetStringBuilder().Clear();
    }
}
