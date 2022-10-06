using System.IO.Compression;
using static Helpers;

public static class StaticFunctions
{
    static YAMLEditor config;
    static YAMLEditor localConfig;


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

        localConfig = new YAMLEditor($"{assignmentPath}/config.yml");
        config = new YAMLEditor("./src/config.yml");

        return (assignmentName, assignmentPath);
    }

    public static List<string> GetRequiredFiles(string assignmentPath)
    {
        List<string> requiredFiles = new List<string>();

        Console.Write("Update required files list? y/n: ");
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
            requiredFiles = localConfig.ReadKeyList<string>("required_files");
        }

        return requiredFiles;
    }

    public static List<string> GetExtraBuiltFiles(string assignmentPath)
    {
        List<string> extraBuiltFiles = new List<string>();

        Console.Write("Update file names list to be built with g++ command? y/n: ");
        var update = Console.ReadLine() ?? "N";
        if (update.ToLower() == "y" || update.ToLower() == "yes")
        {
            Console.Write("Number of extra built files: ");

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
                Console.Write($"Enter extra built file #{j + 1}'s name: ");
                extraBuiltFiles.Add(Console.ReadLine() ?? "");
            }
        }
        else
        {
            // load extra built files from file
            extraBuiltFiles = localConfig.ReadKeyList<string>("extra_built_files");
        }

        return extraBuiltFiles;
    }

    public static List<string> GetFilesToRemoveMainFrom(string assignmentPath)
    {
        List<string> files_to_remove_main_from = new List<string>();

        Console.Write("Update files list that will have the main funtion removed from them if it exists? y/n: ");
        var update = Console.ReadLine() ?? "N";
        if (update.ToLower() == "y" || update.ToLower() == "yes")
        {
            Console.Write("Number of files to remove main from: ");

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
                Console.Write($"Enter file #{j + 1}'s name: ");
                files_to_remove_main_from.Add(Console.ReadLine() ?? "");
            }
        }
        else
        {
            // load files to remove main from from file
            files_to_remove_main_from = localConfig.ReadKeyList<string>("files_to_remove_main_from");
        }

        return files_to_remove_main_from;
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
            // load submission limit from file
            submissionLimit = localConfig.ReadKey<int>("limit_submissions");
        }

        return submissionLimit;
    }

    public static void WriteConfig(string assignmentPath, List<string> requiredFiles, List<string> extra_built_files, List<string> files_to_remove_main_from, int submissionLimit)
    {
        // write config data to files
        localConfig.Update("limit_submissions", submissionLimit);
        localConfig.Update("required_files", requiredFiles);
        localConfig.Update("extra_built_files", extra_built_files);
        localConfig.Update("files_to_remove_main_from", files_to_remove_main_from);

        config.Update("limit_submissions", submissionLimit);
        config.Update("required_files", requiredFiles);
        config.Update("files_to_remove_main_from", files_to_remove_main_from);
    }

    public static void WriteRunTests(string assignmentPath, List<string> requiredFiles, List<string> extra_built_files)
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

            // 'cppfiles' is the list of required and included files that is #included in this test's 'test.cpp' file
            List<string> cppfiles = requiredFiles;
            cppfiles.AddRange(extra_built_files);
            cppfiles = cppfiles.Where(x => (x.EndsWith(".cpp") || x.EndsWith(".h")) && (cpptext.Contains($"#include\"{x.Replace(".cpp", ".h")}\"") || cpptext.Contains($"#include\"{x}\"") || cpptext.Contains($"#include\"{x.Replace(".h", ".cpp")}\""))).ToList();
            cppfiles = cppfiles.Distinct().ToList();

            File.WriteAllText(run_test, String.Empty);
            FileStream run_testWrite = File.OpenWrite(run_test);
            StreamWriter run_testWriter = new StreamWriter(run_testWrite);
            run_testWriter.WriteLine("#!/usr/bin/env bash");
            run_testWriter.WriteLine("");
            
            if(cppfiles.Count > 0)
                run_testWriter.WriteLine($"g++ -O0 -std=c++17 test.cpp ../Check.cpp \"../{string.Join("\" \"../", cppfiles)}\" -I ../ -o testex.exe  2> \"../../../" + parentDirName + ".txt\"");
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