using System;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

internal static class Program
{
    private static int Main(string[] args)
    {
        string root = Path.GetFullPath(args.Length == 0 ? "." : args[0]);
        string assets = Path.Combine(root, "Assets");
        if (!Directory.Exists(assets))
        {
            Console.Error.WriteLine("Assets directory not found: " + assets);
            return 1;
        }
        int files = 0;
        int errors = 0;
        var options = new CSharpParseOptions(LanguageVersion.CSharp9);
        foreach (string path in Directory.EnumerateFiles(assets, "*.cs", SearchOption.AllDirectories))
        {
            files++;
            SyntaxTree tree = CSharpSyntaxTree.ParseText(File.ReadAllText(path), options, path);
            foreach (Diagnostic diagnostic in tree.GetDiagnostics())
                if (diagnostic.Severity == DiagnosticSeverity.Error)
                {
                    Console.Error.WriteLine(diagnostic);
                    errors++;
                }
        }
        Console.WriteLine("C# syntax only: " + files + " asset sources parsed; " + errors + " errors.");
        Console.WriteLine("No Unity API/type resolution, Editor compilation, or scene execution performed.");
        return errors == 0 && files > 0 ? 0 : 1;
    }
}
