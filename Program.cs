using static StaticFunctions;

// Ask user for which assignment they want to package
(string assignmentName, string assignmentPath) = GetAssignment();

// Retreive either user entered or stored required files
List<string> requiredFiles = GetRequiredFiles(assignmentPath);

// Retreive either user entered or stored submission limit
int submissionLimit = GetSubmissionLimit(assignmentPath);

// Write config data to files
WriteConfig(assignmentPath, requiredFiles, submissionLimit);

// Write the run_test scripts
WriteRunTests(assignmentPath, requiredFiles);

// Create a zip of all the necessary files
PackageZip(assignmentPath, assignmentName);

Console.WriteLine("\nZip succesfully packaged!");
