using System;
using System.IO;
using System.Linq;

namespace eTimeTrack.ProjectImportPreparation.Cli
{
    class Program
    {
        static void Main(string[] args)
        {
            //Get the import directory
            if (args.Length < 2)
            {
                Console.WriteLine("USAGE: importDirectory outputDirectory [--employeesOnly projectId]");
                return;
            }

            string importDirectory = args[0];
            if (!Directory.Exists(importDirectory))
            {
                Console.WriteLine("Import directory does not exist: " + importDirectory);
                return;
            }

            string exportDirectory = args[1];
            if (!Directory.Exists(exportDirectory))
            {
                Console.WriteLine("Export directory does not exist: " + exportDirectory);
                return;
            }

            if (args.Contains("--employeesOnly"))
            {
                int projectId;
                if (!int.TryParse(args.Last(), out projectId))
                {
                    Console.WriteLine("Project Id not provided as final parameter.");
                    return;
                }

                ProcessEmployee pe = new ProcessEmployee(importDirectory, exportDirectory, projectId);
                pe.Process();
            }
            else
            {
                ProcessProject pp = new ProcessProject(importDirectory, exportDirectory);
                pp.Process();
            }

            Console.ReadKey();
        }
    }
}
