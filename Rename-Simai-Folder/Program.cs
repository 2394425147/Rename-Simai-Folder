using System.Text;
using SimaiSharp;

var workingDirectory   = Environment.CurrentDirectory;
var levelDirectoryMode = false;
var forceOverwriteMode = false;

foreach (var arg in args)
{
	switch (arg)
	{
		case "-l":
		case "--level-directory":
		{
			// this indicates that we're using the level directory mode
			levelDirectoryMode = true;
		}
			break;
		case "-f":
		case "--force-overwrite":
		{
			// this indicates that we're using the force overwrite mode
			forceOverwriteMode = true;
		}
			break;
		case "-h":
		case "--help":
		{
			var sb = new StringBuilder()
			         .AppendLine($"Usage: Rename-Simai-Folder [OPTIONS]")
			         .AppendLine()
			         .AppendLine("Options:")
			         .AppendLine("\t-l, --level-directory")
			         .AppendLine("\t\tUse the level directory mode: Rename the working directory to the title specified by the maidata.txt file under it.")
			         .AppendLine("\t-f, --force-overwrite")
			         .AppendLine("\t\tForce overwrite existing directories without confirmation.")
			         .AppendLine("\t-h, --help")
			         .AppendLine("\t\tDisplay this help message.");

			Console.WriteLine(sb.ToString());
		}
			return;
		default:
		{
			var trimmed = arg.Trim('"');
			workingDirectory = trimmed;
		}
			break;
	}
}

if (string.IsNullOrEmpty(workingDirectory))
{
	Console.WriteLine("Please specify a working directory.");
	return;
}

if (!Directory.Exists(workingDirectory))
{
	Console.WriteLine($"The directory {workingDirectory} does not exist.");
	return;
}

var workingDirectoryInfo = new DirectoryInfo(workingDirectory);

if (levelDirectoryMode)
{
	RenameDirectory(workingDirectoryInfo);
	return;
}

// we now rename all folders in the working directory
foreach (var directory in workingDirectoryInfo.GetDirectories())
{
	RenameDirectory(directory);
}

void RenameDirectory(DirectoryInfo directory)
{
	// check if .\maidata.txt exists in the current folder (case insensitive)
	var maidataFiles = directory.GetFiles("maidata.txt", SearchOption.TopDirectoryOnly);

	if (!maidataFiles.Any())
	{
		Console.WriteLine($"The directory {directory.Name} does not contain a maidata.txt file.");
		return;
	}

	var simaiFile = new SimaiFile(maidataFiles.First().FullName);
	var title     = simaiFile.GetValue("title");

	// rename the directory (replace the directoryName to title, since title isn't the full name)
	var newDirectoryPath = Path.Combine(directory.Parent?.FullName ?? string.Empty, title);

	if (Directory.Exists(newDirectoryPath))
	{
		if (!forceOverwriteMode)
		{
			Console.WriteLine($"The directory {newDirectoryPath} already exists. Would you like to overwrite it? (y/n)");
			var userResponse = Console.ReadKey().KeyChar;

			if (userResponse != 'y')
				return;
		}

		Directory.Delete(newDirectoryPath, true);
	}
	else if (!forceOverwriteMode)
	{
		// alert the user before renaming the directory
		Console.WriteLine($"Rename {directory.Name} to {title}? (y/n)");
		var userResponse = Console.ReadKey().KeyChar;

		if (userResponse != 'y')
			return;
	}

	// rename the current directory to the new directory
	Directory.Move(directory.FullName, newDirectoryPath);
}