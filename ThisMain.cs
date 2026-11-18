using System.Text.Json;

namespace CodeScan;

public static class ThisMain
{
	public static readonly List<string> Errors = [];

	private static readonly HashSet<string> GoodFiles = [];

	private static List<string> _filesToMoveToManaged = new()
	{
		Capacity = 0
	};
	
	public static async Task Main(string[] args)
	{
		// Get the location of the executable.
		string executableDirectory = AppContext.BaseDirectory;
		string correctedWhitelistPath = Path.Combine(executableDirectory, "CodeScanList.corrected.json");
		AssemblyTypeChecker? dat = null;

		try
		{
			if (File.Exists(correctedWhitelistPath)) File.Delete(correctedWhitelistPath);

			bool memoryClearingMode = false;

			foreach (var arg in args)
			{
				if (arg.Contains('@'))
				{
					memoryClearingMode = true;
				}
			}

			using (StreamReader r = new StreamReader( Path.Combine(executableDirectory, "..", "FilesToMoveToManaged.json")))
			{
				string injson = await r.ReadToEndAsync();
				_filesToMoveToManaged = JsonSerializer.Deserialize(injson, CodeScanJsonContext.Default.ListString)
					?? throw new InvalidDataException("FilesToMoveToManaged.json is empty");
			}


			if (memoryClearingMode)
			{
			}
			else
			{
				try
				{
					Action<string> info = Console.WriteLine;
					Action<string> error = log => { Errors.Add(log); };

					string? goodPath = null;

					foreach (var arg in args)
					{
						if (arg.Contains('>'))
						{
							goodPath = arg.Replace(">", "");
							break;
						}
					}


					if (goodPath == null)
					{
						throw new ArgumentException("No build path given. Pass it as an argument prefixed with '>', e.g. \">/path/to/build\"");
					}

					List<string> multiAssemblyReference = new List<string>();
					dat = new AssemblyTypeChecker();

					var osText = "";
					var managedPath = "";

					if (OperatingSystem.IsWindows())
					{
						managedPath = @"Unitystation_Data/Managed";
						osText = "Windows";
					}

					if (OperatingSystem.IsLinux())
					{
						managedPath = @"Unitystation_Data/Managed";
						osText = "Linux";
					}

					if (OperatingSystem.IsMacOS())
					{
						managedPath = @"Mac.app/Contents/Resources/Data/Managed";
						osText = "Mac";
					}

					DirectoryInfo directory = new DirectoryInfo(Path.Combine(goodPath, managedPath));
					DirectoryInfo goodFilesPath = new DirectoryInfo(executableDirectory).Parent
						?? throw new DirectoryNotFoundException("Executable directory has no parent");
					goodFilesPath = goodFilesPath.CreateSubdirectory("MissingAssemblies");
					goodFilesPath = goodFilesPath.CreateSubdirectory(osText);

					// Get a list of files in the source directory
					var files = directory.GetFiles();

					// Copy files that match the specified patterns
					foreach (var file in files)
					{
						string fileName = file.Name;

						// Check if any pattern in the list matches the file name
						if (_filesToMoveToManaged.Any(Name => fileName.Contains(Name)))
						{
							string destinationFilePath = Path.Combine(goodFilesPath.ToString(), fileName);
							File.Copy(file.ToString(), destinationFilePath, true);
							Console.WriteLine($"Copied: {fileName}");
						}
					}


					GoodFiles.UnionWith(
						goodFilesPath.GetFiles().Select(x => x.Name).ToHashSet()
						);


					foreach (var file in directory.GetFiles())
					{
						if (GoodFiles.Contains(file.Name)) continue;
						if (file.Extension == ".pdb") continue;
						multiAssemblyReference.Add(Path.GetFileNameWithoutExtension(file.Name));
					}


					foreach (var file in directory.GetFiles())
					{
						var listy = multiAssemblyReference.ToList();
						if (GoodFiles.Contains(file.Name)) continue;
						if (file.Extension == ".pdb") continue;
						listy.Remove(Path.GetFileNameWithoutExtension(file.Name));
						if (dat.CheckAssembly(file, directory, goodFilesPath, listy, info, error) == false)
						{
							Errors.Add($"{file.Name} Failed scanning Cancelling");
						}
					}
				}
				catch (Exception e)
				{
					Errors.Add(e.ToString());
				}
			}
		}
		catch (Exception e)
		{
			Errors.Add(e.ToString());
		}



		if (dat != null && dat.HasWhitelistSuggestions &&
		    !args.Any(arg => string.Equals(arg, "-supressCorrectedWhitelist", StringComparison.OrdinalIgnoreCase)))
		{
			try
			{
				dat.WriteCorrectedWhitelist(correctedWhitelistPath);
			}
			catch (Exception e)
			{
				Errors.Add($"Unable to write corrected whitelist: {e}");
			}
		}

		// Construct the JSON file path next to the executable
		string jsonFilePath = Path.Combine(executableDirectory, "errors.json");

		// Serialize the data to JSON and write it to the file
		string json = JsonSerializer.Serialize(Errors, CodeScanJsonContext.Default.ListString);
		await File.WriteAllTextAsync(jsonFilePath, json);
	}
}
