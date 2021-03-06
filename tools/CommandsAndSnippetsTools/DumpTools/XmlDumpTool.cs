using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using CommandsAndSnippetsAPI.Data;

namespace CommandsAndSnippetsTools.DumpTools
{
    public class XmlDumpTool
    {
        private readonly ApiDataContext _dbContext;

        public XmlDumpTool()
        {
            _dbContext = new DbAccess().DbContext;
        }

        public void DumpCommands(string path)
        {
            Console.WriteLine("Dumping all commands into an Xml file...");

            var commands = _dbContext.CommandItems.ToList();

            var commandDump = commands.ToXml();
            
            SaveXmlToFile(path, commandDump, "CommandsDumpedLinq.xml");
        }

        public void DumpCommandsWithEfPlatform(string path)
        {
            Console.WriteLine("Dumping commands with EF Platform into an Xml file...");
            
            var commands = _dbContext.CommandItems.ToList();
            
            var commandDump = commands.ToXmlWhere(c => c.Platform == "Entity Framework CLI");
            
            SaveXmlToFile(path, commandDump, "CommandsWithEFPlatform.xml");
        }

        
        private static void SaveXmlToFile(string path, XDocument commandDump, string fileName)
        {
            if (string.IsNullOrEmpty(path))
            {
                var projectPath = Environment.CurrentDirectory;
                var defaultPath = $@"{projectPath}/XmlDumps";
                var filePath = $"{defaultPath}/{fileName}";

                // @"Commands.xml" Will be saved to bin/Commands.xml
                if (Directory.Exists(defaultPath))
                {
                    commandDump.Save(filePath);
                }
                else
                {
                    Directory.CreateDirectory(defaultPath);
                    commandDump.Save(filePath);
                }

                Console.WriteLine($"[+] Xml dumped to: {filePath}");
                return;
            }

            // When a path was provided

            string givenPathFile = $@"{path}/Commands.xml";
            commandDump.Save(givenPathFile);
            Console.WriteLine($"[+] Xml dumped to: {givenPathFile}");
        }
    }
}