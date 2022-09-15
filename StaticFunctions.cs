using System.IO.Compression;
using static Helpers;

public static class StaticFunctions
{
    public static (string assignmentName, string assignmentPath) GetAssignment()
    {
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

        return (assignmentName, assignmentPath);
    }

    public static List<string> GetRequiredFiles(string assignmentPath)
    {
        List<string> requiredFiles = new List<string>();

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

            for (int j = 0; j < num; j++)
            {
                Console.Write($"Enter required file #{j + 1}'s name: ");
                requiredFiles.Add(Console.ReadLine() ?? "");
            }
        }
        else
        {
            // load required files from file
            FileStream loadConfig = File.OpenRead(assignmentPath + "/config.yml");
            StreamReader loadReader = new StreamReader(loadConfig);
            loadReader.ReadLine();
            loadReader.ReadLine();

            while (!loadReader.EndOfStream)
            {
                string line = loadReader.ReadLine() ?? "";
                if (line == "")
                    break;

                requiredFiles.Add(line.Replace("-", " ").Trim());
            }

            loadReader.Close();
            loadConfig.Close();
        }

        return requiredFiles;
    }

    public static int GetSubmissionLimit(string assignmentPath)
    {
        // get submission limit
        int submissionLimit = -1;
        Console.Write("Update submission limit? y/n: ");
        var updateLimit = Console.ReadLine() ?? "N";
        if (updateLimit.ToLower() == "y" || updateLimit.ToLower() == "yes")
        {
            Console.Write("Enter submission limit: ");
            do
            {
                try
                {
                    submissionLimit = int.Parse(Console.ReadLine() ?? "-1");
                    if(submissionLimit <= 0)
                        Console.Write($"Enter an integer greater than 0: ");

                }
                catch (Exception)
                {
                    submissionLimit = -1;
                    Console.Write($"Enter an integer greater than 0: ");
                }
            } while (submissionLimit <= 0);
        }
        else
        {
            FileStream loadConfig = File.OpenRead(assignmentPath + "/config.yml");
            StreamReader loadReader = new StreamReader(loadConfig);
            
            submissionLimit = int.Parse((loadReader.ReadLine() ?? "limit_submissions: 3").Split(":")[1]);
            
            loadReader.Close();
            loadConfig.Close();
        }

        return submissionLimit;
    }

    public static void WriteConfig(string assignmentPath, List<string> requiredFiles, int submissionLimit)
    {
        FileStream configWrite = File.Open("./src/config.yml", FileMode.Create);
        StreamWriter writer = new StreamWriter(configWrite);

        FileStream loadConfigWriter = File.Open(assignmentPath + "/config.yml", FileMode.Create);
        StreamWriter loadWriter = new StreamWriter(loadConfigWriter);

        writer.WriteLine($"limit_submissions: {submissionLimit}");
        writer.WriteLine($"required_files:");
        loadWriter.WriteLine($"limit_submissions: {submissionLimit}");
        loadWriter.WriteLine($"required_files:");

        foreach (var file in requiredFiles)
        {
            writer.WriteLine($"  - {file}");
            loadWriter.WriteLine($"  - {file}");
        }

        writer.Close();
        configWrite.Close();
        loadWriter.Close();
        loadConfigWriter.Close();
    }

    public static void WriteRunTests(string assignmentPath, List<string> requiredFiles)
    {
        var run_tests = Directory.GetFiles(assignmentPath, "run_test", SearchOption.AllDirectories).ToList();
        run_tests.Sort((a, b) => string.Compare(a,b));
        run_tests = run_tests.Select(x => x.Replace("\\", "/")).ToList();

        if (run_tests.Count == 0)
        {
            Console.WriteLine("No run_test script found");
            Console.ReadLine();
            return;
        }

        int testNum = 0;
        foreach (var run_test in run_tests)
        {
            string parentDirName = run_test.Split('/')[^2];
            string cpptext = File.ReadAllText(run_test.Replace("run_test", "test.cpp")).Replace(" ", "");
            string[] cppfiles = requiredFiles.Where(x => x.EndsWith(".cpp") && cpptext.Contains($"#include\"{x.Replace(".cpp", ".h")}\"")).ToArray();

            File.WriteAllText(run_test, String.Empty);
            FileStream run_testWrite = File.OpenWrite(run_test);
            StreamWriter run_testWriter = new StreamWriter(run_testWrite);
            run_testWriter.WriteLine("#!/usr/bin/env bash");
            run_testWriter.WriteLine("");
            
            if(cppfiles.Length > 0)
                run_testWriter.WriteLine($"g++ -O0 -std=c++17 test.cpp ../Check.cpp \"../{string.Join("\" \"../", cppfiles)}\" -I ../ -o testex.exe  2> \"../../../output" + parentDirName + ".txt\"");
            else
                run_testWriter.WriteLine($"g++ -O0 -std=c++17 test.cpp ../Check.cpp -I ../ -o testex.exe  2> \"../../../" + parentDirName + ".txt\"");
            
            run_testWriter.WriteLine("./testex.exe");
            run_testWriter.Close();
            run_testWrite.Close();
            testNum++;
        }
    }

    public static void PackageZip(string assignmentPath, string assignmentName)
    {
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
    }
}