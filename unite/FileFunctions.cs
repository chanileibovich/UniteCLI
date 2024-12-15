using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace unite
{
    public static class FileFunctions
    {
        public static void RemoveEmptyLines(List<string> filePaths)
        {

            foreach (var file in filePaths)
            {
                try
                {
                    if (!File.Exists(file))
                    {
                        Console.WriteLine($"The file {file} was not found.");
                        continue;
                    }
                    var lines = File.ReadAllLines(file);
                    var nonEmptyLines = lines.Where(line => !string.IsNullOrWhiteSpace(line)).ToArray();
                    File.WriteAllLines(file, nonEmptyLines);
                    Console.WriteLine($"Empty lines successfully deleted from file: {file}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing file {file}: {ex.Message}");
                }
            }
        }
        
        public static string GetDirectory(FileInfo selectedoutput)
        {
            string outputPath = selectedoutput.FullName;
            if (Path.IsPathRooted(outputPath))
                return Path.GetDirectoryName(outputPath);

            else
                return Directory.GetCurrentDirectory();
        }
    }
}
