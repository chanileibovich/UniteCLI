//unite bundle --language all --output "bundle.txt" --note --author chani --sort name --remove-empty-lines
//unite bun --l java --o "bundle4.txt" --n --a chani --s name --rel
using System.CommandLine;
using unite;

Dictionary<string, string> language= new Dictionary<string, string>
{
    { "csharp", ".cs" }, { "python", ".py" }, { "java", ".java" }, { "js", ".js" },
    { "html", ".html" }, { "css", ".css" }, { "all", "all" } 
};

List<string> extensionPatterns = new List<string>();
string extensionPattern = string.Empty;

var languageOption = new Option<string>(
    aliases: new[] { "--language","--l" }, 
    description: "Select a programming language.") 
    { IsRequired = true };

var outputFileOption = new Option<FileInfo>(
    aliases: new[] { "--output", "--o" },
    description: "Specify the output file name.")
    { IsRequired = true };

var noteOption = new Option<bool>(
    aliases: new[] { "--note", "--n" },
    description: "Writing a path in the file?");

var authorOption = new Option<string>(
    aliases: new[] { "--author", "--a" },
    description: "Writing an author name in the file?");

var sortOption = new Option<string>(
    aliases: new[] { "--sort", "--s" },
    description: "Sort files by name or type.");

var removeOption = new Option<bool>(
    aliases: new[] { "--remove-empty-lines", "--rel" },
    description: "Remove empty lines from source code before bundling?");

languageOption.AddValidator(result =>
{
    if (result.GetValueOrDefault<string>() is null)
    {
        result.ErrorMessage = "Value cannot be null.";
    }
    if (!string.IsNullOrEmpty(result.GetValueOrDefault<string>()) &&
                !language.ContainsKey(result.GetValueOrDefault<string>()))
    {
        result.ErrorMessage = $"Invalid language. Valid options are: {string.Join(", ", language.Keys)}";
    }
});

sortOption.AddValidator(result =>
{
    var sortValue = result.GetValueOrDefault<string>();
    if (sortValue != "name" && sortValue != "type")
    {
        result.ErrorMessage = "Invalid sort option. Valid options are 'name' or 'type'.";
    }

});

var bundleCommand = new Command("bundle", "Bundle code files to a single file.");
bundleCommand.AddAlias("bun");

bundleCommand.AddOption(languageOption);
bundleCommand.AddOption(outputFileOption);
bundleCommand.AddOption(noteOption);
bundleCommand.AddOption(authorOption);
bundleCommand.AddOption(sortOption);
bundleCommand.AddOption(removeOption);

bundleCommand.SetHandler((string selectedLanguage, FileInfo selectedOutput, bool selectednote, string? selectedauthor, string? selectedsort, bool selectedremove) =>
{

    try
    {
        if (selectedLanguage == "all")
        {
            extensionPatterns.AddRange(language.Values.Where(ext => ext != "all"));
        }

        else if (language.TryGetValue(selectedLanguage, out string extension))
        {
            extensionPatterns.Add(extension);
        }

        else
        {
            Console.WriteLine("Invalid language selected.");
            return;
        }

        List<string> files = new List<string>();
        string directory = FileFunctions.GetDirectory(selectedOutput);
        foreach (var ext in extensionPatterns)
        {
            files.AddRange(Directory.GetFiles(directory, $"*{ext}"));
        }

        if (files.Count == 0)
        {
            Console.WriteLine($"No files found in the specified directory for the selected languages.");
            return;
        }

        if (selectedremove){ FileFunctions.RemoveEmptyLines(files); }

        if (selectedsort == "type"){ files = files.OrderBy(f => Path.GetExtension(f)).ToList(); }

        else{ files = files.OrderBy(f => Path.GetFileName(f)).ToList(); }

        // אם הקובץ החדש כבר קיים, נמחק אותו כדי להתחיל מחדש
        if (File.Exists(selectedOutput.FullName)){ File.Delete(selectedOutput.FullName); }

        string outputPath = selectedOutput.FullName;
        string fileName = Path.GetFileName(outputPath);
        string exte = Path.GetExtension(fileName);
        if (string.IsNullOrEmpty(exte))
        {
            string newFilePath = Path.ChangeExtension(selectedOutput.FullName, ".txt");
            selectedOutput = new FileInfo(newFilePath);
        }

        // יוצר וסוגר את הקובץ
        File.Create(selectedOutput.FullName).Close();
        if (!string.IsNullOrEmpty(selectedauthor))
        {
            File.WriteAllText(selectedOutput.FullName, "// " + selectedauthor + "\n"); // מוסיף לקובץ
        }

        if (selectednote){ File.AppendAllText(selectedOutput.FullName, "// " + selectedOutput.FullName + "\n"); }

        Console.WriteLine($"File created: {fileName} in path {directory}");

        foreach (var file in files)
        {
            string content = File.ReadAllText(file);
            File.AppendAllText(selectedOutput.FullName, content + Environment.NewLine); // הוסף תוכן לקובץ
        }
        Console.WriteLine($"Bundle created successfully at {selectedOutput.FullName}");

    }

    catch (DirectoryNotFoundException ex)
    {
        Console.WriteLine("File path is invalid");
    }

    catch (IOException ex)
    {
        Console.WriteLine($"I/O error in the files: {ex.Message}");
    }
    catch (UnauthorizedAccessException ex)
    {
        Console.WriteLine($"No access permissions for the files: {ex.Message}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Unexpected error in the files : {ex.Message}");
    }

}, languageOption, outputFileOption, noteOption, authorOption, sortOption, removeOption);

var rspCommand = new Command("create-rsp", "create a response file with a pre-configured command.");
rspCommand.SetHandler(() =>
{
    Console.WriteLine("Let's create a response file for the bundle command.");

    Console.WriteLine("Enter a language (csharp, python, java, js, html, css)\r\nOr enter all to select all languages");
    string languageInput = Console.ReadLine() ?? "all";

    Console.WriteLine("Enter the output file name and/or file path.\r\nThe default file path will be the current location.");
    string outputFileName = Console.ReadLine() ?? "bundle";

    Console.WriteLine("Include note in the file? (yes/no):");
    string includeNoteInput = Console.ReadLine()?.Trim().ToLower() ?? "no";
    bool includeNote = includeNoteInput == "yes";

    Console.WriteLine("Enter author name (or leave blank): ");
    string authorName = Console.ReadLine() ?? string.Empty;

    Console.WriteLine("Sort files by (name/type): ");
    string sortOptionInput = Console.ReadLine()?.Trim().ToLower() ?? "name";

    Console.WriteLine("Remove empty lines from files? (yes/no): ");
    string removeEmptyLinesInput = Console.ReadLine()?.Trim().ToLower() ?? "no";
    bool removeEmptyLines = removeEmptyLinesInput == "yes";

    string commandLine = $"bundle --language {languageInput} --output \"{outputFileName}.txt\"";

   if( includeNote ){ commandLine += " --note"; }

    if (!string.IsNullOrEmpty(authorName)){ commandLine += $" --author {authorName}"; }

    commandLine+=$" --sort {sortOptionInput}";

    if (removeEmptyLines){ commandLine += " --remove-empty-lines"; }

    Console.WriteLine("Enter response file name: ");
    string responseFileName = Console.ReadLine() ?? "command.rsp";
    
    try
    {
        File.WriteAllText(responseFileName, commandLine);
        Console.WriteLine($"Response file created successfully: {responseFileName}");
        Console.WriteLine($"You can now run the command using: unite @{responseFileName}");
    }

    catch (Exception ex) 
    {
        Console.WriteLine($"Error creating response file: {ex.Message}");
    }

});

var rootComand = new RootCommand("Root comand for file Bundle CLI");

rootComand.AddCommand(bundleCommand);

rootComand.AddCommand(rspCommand);

await rootComand.InvokeAsync(args);



