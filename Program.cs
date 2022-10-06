using static StaticFunctions;

// Ask user for which assignment they want to package
(string assignmentName, string assignmentPath) = GetAssignment();

Console.Write("\n-----------------------\nThese next parameters are optional, press enter to skip. \nThey can also be edited from the  \"tests/Assignment Name/config.yml\"  file. \nBut be sure to repackage after editing. \n-----------------------\n\n");

// Retreive either user entered or stored required files
List<string> requiredFiles = GetRequiredFiles(assignmentPath);

List<string> extra_built_files = GetExtraBuiltFiles(assignmentPath);

List<string> files_to_remove_main_from = GetFilesToRemoveMainFrom(assignmentPath); 

// Retreive either user entered or stored submission limit
int submissionLimit = GetSubmissionLimit(assignmentPath);

// Write config data to files
WriteConfig(assignmentPath, requiredFiles, extra_built_files, files_to_remove_main_from, submissionLimit);

// Write the run_test scripts
WriteRunTests(assignmentPath, requiredFiles, extra_built_files);

// Create a zip of all the necessary files
PackageZip(assignmentPath, assignmentName);

Console.WriteLine("\nZip succesfully packaged!");




