using System.IO.Compression;

static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
{
    var dir = new DirectoryInfo(sourceDir);

    if (!dir.Exists)
        throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

    // Cache directories before we start copying
    DirectoryInfo[] dirs = dir.GetDirectories();

    Directory.CreateDirectory(destinationDir);

    foreach (FileInfo file in dir.GetFiles())
    {
        string targetFilePath = Path.Combine(destinationDir, file.Name);
        file.CopyTo(targetFilePath);
    }

    // If recursive and copying subdirectories, recursively call this method
    if (recursive)
    {
        foreach (DirectoryInfo subDir in dirs)
        {
            string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
            CopyDirectory(subDir.FullName, newDestinationDir, true);
        }
    }
}

var assignments = Directory.EnumerateDirectories("./tests/").ToList();

int i = 0;
foreach (var assignment in assignments)
{
    Console.WriteLine($"{i++}.  {assignment.Split('/').Last()}");
}

Console.Write("\nEnter assignment number: ");
int assignmentNum = -1;
do 
{
    try
    {
        assignmentNum = int.Parse(Console.ReadLine() ?? "-1");
    }
    catch (Exception)
    {
        assignmentNum = -1;
        Console.Write($"Enter number between 0 and {assignments.Count - 1}: ");
    }

} while (assignmentNum < 0 || assignmentNum >= assignments.Count);

string assignmentPath = assignments[assignmentNum];
string assignmentName = assignmentPath.Split('/').Last();

// get required files
Console.Write("Update required files? y/n: ");
var update = Console.ReadLine() ?? "N";
if (update.ToLower() == "y" || update.ToLower() == "yes")
{
    Console.Write("Number of required files: ");

    int num = -1;
    do 
    {
        try
        {
            num = int.Parse(Console.ReadLine() ?? "-1");
        }
        catch (Exception)
        {
            num = -1;
            Console.Write($"Enter number between 0 and 9: ");
        }

    } while (num < 0 || num >= 9);

    List<string> requiredFiles = new List<string>();

    for (int j = 0; j < num; j++)
    {
        Console.Write($"Enter required file #{j + 1}'s name: ");
        requiredFiles.Add(Console.ReadLine() ?? "");
    }

    FileStream config = File.OpenRead("./src/config.yml");
    StreamReader reader = new StreamReader(config);
    
    int submissionLimit = int.Parse((reader.ReadLine() ?? "limit_submissions: 3").Split(":")[1]);

    reader.Close();
    config.Close();

    FileStream configWrite = File.OpenWrite("./src/config.yml");
    StreamWriter writer = new StreamWriter(configWrite);

    writer.WriteLine($"limit_submissions: {submissionLimit}");
    writer.WriteLine($"required_files:");
    foreach (var file in requiredFiles)
    {
        writer.WriteLine($"  - {file}");
    }

    writer.Close();
    configWrite.Close();

    //write run_test script
    var run_tests = Directory.GetFiles(assignmentPath, "run_test", SearchOption.AllDirectories).ToList();
    run_tests.Sort((a, b) => string.Compare(a,b));
    
    if (run_tests.Count == 0)
    {
        Console.WriteLine("No run_test script found");
        Console.ReadLine();
        return;
    }

    int testNum = 0;
    foreach (var run_test in run_tests)
    {
        string cpptext = File.ReadAllText(run_test.Replace("run_test", "test.cpp")).Replace(" ", "");
        string[] cppfiles = requiredFiles.Where(x => x.EndsWith(".cpp") && cpptext.Contains($"#include\"{x.Replace(".cpp", ".h")}\"")).ToArray();

        File.WriteAllText(run_test, String.Empty);
        FileStream run_testWrite = File.OpenWrite(run_test);
        StreamWriter run_testWriter = new StreamWriter(run_testWrite);
        run_testWriter.WriteLine("#!/usr/bin/env bash");
        run_testWriter.WriteLine("");
        run_testWriter.WriteLine($"g++ -O0 -std=c++17 test.cpp ../Check.cpp ../{string.Join(" ../", cppfiles)} -I ../ -o testex.exe  2> ../../../output" + testNum + ".txt");
        run_testWriter.WriteLine("./testex.exe");
        run_testWriter.Close();
        run_testWrite.Close();
        testNum++;
    }
}


// get submission limit
Console.Write("Update submission limit? y/n: ");
var updateLimit = Console.ReadLine() ?? "N";
if (updateLimit.ToLower() == "y" || updateLimit.ToLower() == "yes")
{
    Console.Write("Enter submission limit: ");
    int limit = -1;
    do 
    {
        try
        {
            limit = int.Parse(Console.ReadLine() ?? "-1");
        }
        catch (Exception)
        {
            limit = -1;
            Console.Write($"Enter an integer greater than 0: ");
        }
    } while (limit <= 0);

    string[] arrLine = File.ReadAllLines("./src/config.yml");
    arrLine[0] = "limit_submissions: " + limit;
    File.WriteAllLines("./src/config.yml", arrLine);
}

// Create temporary directory for zip file
CopyDirectory(assignmentPath, "./temp_package/tests", true);
CopyDirectory("./src/", "./temp_package/", true);

// Add other files to zip file
string zipName = assignmentName + ".zip";

try
{
    if(File.Exists("./zip/" + zipName)) File.Delete("./zip/" + zipName);
}
catch (Exception)
{
    zipName = assignmentName + new Random().Next(0, 1000).ToString() + ".zip";
    Console.WriteLine("Could not delete existing zip file, exporting as " + zipName);
}

ZipFile.CreateFromDirectory("./temp_package", "./zip/" + zipName);

//delete temp_package
Directory.Delete("./temp_package", true);

Console.WriteLine("\nZip succesfully packaged!");



