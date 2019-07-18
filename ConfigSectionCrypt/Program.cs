using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Reflection;
using System.Web.Configuration;

namespace ConfigSectionCrypt
{
    class Program
    {
        static void Main(string[] args)
        {
            ShowTitle();

            if(args.Length < 3)
            {
                ShowUsage();
                return;
            }

            string operation = args[0].ToLower();
            string configFileName = args[1];
            string configSectionName = args[2];
            
            if(operation == "-e" || operation == "/e" || 
                operation == "-enc" || operation == "/enc" || 
                operation == "-encrypt" || operation == "/encrypt")
            {
                EncryptSection(configFileName, configSectionName);
            }
            else if (operation == "-d" || operation == "/d" ||
                    operation == "-dec" || operation == "/dec" ||
                    operation == "-decrypt" || operation == "/decrypt")
            {
                DecryptSection(configFileName, configSectionName);
            }
            else
            {
                Console.WriteLine("ERROR: unknown operation ({0}) specified", operation);
            }
        }


        private static void ShowUsage()
        {
            Console.WriteLine("USAGE:  ConfigSectionCrypt (-e / -d) filename section");
            Console.WriteLine("           -e/-encrypt    Encrypt the specified section in the given file");
            Console.WriteLine("           -d/-decrypt    Decrypt the specified section in the given file");
        }

        private static void ShowTitle()
        {
            Console.WriteLine(".NET Config Cryptographer, v{0}, (c) 2010 by Bluenose Software", Assembly.GetExecutingAssembly().GetName().Version.ToString(3));
            Console.WriteLine();
        }

        private static void DecryptSection(string configFileName, string sectionName)
        {
            Console.WriteLine("About to decrypt section '{0}' in '{1}", sectionName, configFileName);

            try
            {
                Configuration config = OpenConfiguration(configFileName);

                ConfigurationSection configSection = config.GetSection(sectionName);

                if (configSection != null)
                {
                    configSection.SectionInformation.UnprotectSection();
                    config.Save();
                    
                    Console.WriteLine("Successfully decrypted section '{0}' in '{1}", sectionName, configFileName);
                }
                else
                {
                    Console.WriteLine("ERROR: cannot load the configuration section '{0}' from file '{1}' - section not found or invalid", sectionName, configFileName);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: {0}: {1}", ex.GetType().FullName, ex.Message);
            }
        }

        private static void EncryptSection(string configFileName, string sectionName)
        {
            Console.WriteLine("About to encrypt section '{0}' in '{1}", sectionName, configFileName);

            try
            {
                Configuration config = OpenConfiguration(configFileName);

                ConfigurationSection configSection = config.GetSection(sectionName);

                if (configSection != null)
                {
                    configSection.SectionInformation.ProtectSection("DataProtectionConfigurationProvider");
                    config.Save();

                    Console.WriteLine("Successfully encrypted section '{0}' in '{1}", sectionName, configFileName);
                }
                else
                {
                    Console.WriteLine("ERROR: cannot load the configuration section '{0}' from file '{1}'", sectionName, configFileName);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: {0}: {1}", ex.GetType().FullName, ex.Message);
            }
        }

        private static Configuration OpenConfiguration(string filename)
        {
            if(filename.Trim().ToLower() == "web.config")
            {
                return WebConfigurationManager.OpenWebConfiguration(filename);
            }
            else
            {
                ExeConfigurationFileMap map = new ExeConfigurationFileMap();
                map.ExeConfigFilename = filename;

                return ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
            }
        }
    }
}
