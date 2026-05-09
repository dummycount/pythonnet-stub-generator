using System.CommandLine;

namespace PythonNetStubGenerator.Tool;

static class Program
{
    static int Main(string[] args)
    {
		Argument<FileInfo[]> targetDlls = new("dll")
		{
			Description = "Target DLLs",
			Arity = ArgumentArity.OneOrMore,
		};

		Option<DirectoryInfo> outPathArg = new("--out", "-o")
        {
            Description = "Path to save the stubs to [default: ./stubs]",
        };

        Option<DirectoryInfo[]> searchPaths = new("--search", "-s")
        {
            Description = "Paths to search for referenced assemblies",
            Arity = ArgumentArity.ZeroOrMore,
        };

        Option<bool> forceLfOption = new("--force-lf")
        {
            Description = "Force LF line endings for stubs [default: false]"
        };

        RootCommand rootCommand = new("Creates stubs for Python.Net")
        {
            targetDlls,
            outPathArg,
            searchPaths,
        };

        rootCommand.SetAction((result) => WriteStubs(
            destPath: result.GetValue(outPathArg), 
            targetDlls: result.GetValue(targetDlls), 
            searchPaths: result.GetValue(searchPaths), 
            forceLf: result.GetValue(forceLfOption)
        ));

        var result = rootCommand.Parse(args);
        return result.Invoke();
    }


    static int WriteStubs(
        DirectoryInfo? destPath,
        FileInfo[]? targetDlls,
        DirectoryInfo[]? searchPaths = null,
        bool forceLf = false
        )
    {
        destPath ??= new DirectoryInfo("stubs");

        if (targetDlls is null)
        {
            Console.WriteLine("error: requires at least one DLL path");
            return -1;
        }

        if (searchPaths is not null)
        {
            foreach (var searchPath in searchPaths)
            {
                Console.WriteLine($"search path {searchPath}");
            }
        }

        foreach (var path in targetDlls)
        {
            if (!path.Exists)
            {
                Console.WriteLine($"error: can not find {path}");
                return -1;
            }
        }

        Console.WriteLine($"building stubs...");

        try
        {
            var dest = StubBuilder.BuildAssemblyStubs(destPath, targetDlls, searchPaths, forceLf);
            Console.WriteLine($"stubs saved to {dest}");
            return 0;
        }
        catch (Exception sgEx)
        {
            Console.WriteLine($"error: failed generating stubs | {sgEx.Message}");
            throw;
        }
    }
}
