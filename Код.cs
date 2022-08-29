using System;
using System.IO;
using Newtonsoft.Json;

namespace BackUpApp
{
    class Program
    {
        public class ConfigFolders
        {
            public string SourceFolderPath { get; set; }
            public string TargetFolderPath { get; set; }
        }
        static void Main(string[] args)
        {
            ConfigFolders returnedConfig = GetConfig();
            string sourceFolderPath = returnedConfig.SourceFolderPath;
            string targetFolderPath = returnedConfig.TargetFolderPath;
            string logFileFolder = GetLogFile();

            BackUpp(sourceFolderPath, targetFolderPath, logFileFolder);   

            Console.WriteLine("Программа завершилась без ошибок.");
            Console.WriteLine("Журнал событий находится по пути: " + logFileFolder);
            Console.WriteLine("Папка с резервными копиями находится по пути: " + targetFolderPath);
            Console.ReadLine();
        }

        static ConfigFolders GetConfig()
        {
            string getPathToExe = Environment.CurrentDirectory.ToString();
            string pathToConfig = getPathToExe + "\\config.json";
            string configText = File.ReadAllText(pathToConfig).Replace("\\", "\\\\");
            ConfigFolders returnedConfig = JsonConvert.DeserializeObject<ConfigFolders>(configText);

            return returnedConfig;
        }

        static string GetDate()
        {
            string GetDate = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss").Replace(":", "_");
            return GetDate;
        }

        static string GetLogFile()
        {
            string getPathToExe = Environment.CurrentDirectory.ToString();
            string path = getPathToExe + "\\Logs" + "\\Log" + GetDate() + ".txt";
            var Logfile = File.Create(path);
            Logfile.Close();

            File.AppendAllText(path, GetDate() + " [Info] Приложение запущено\n");
            return path;
        }

        static void BackUpp(string sourceFolderPath, string targetFolderPath, string logFileFolder)
        {
            try
            {
                bool TestOpenBackUpFolder = Directory.Exists(targetFolderPath);
                if (TestOpenBackUpFolder == false) { throw new Exception(); }
                bool TestOpenSourceFolder = Directory.Exists(sourceFolderPath);
                if (TestOpenSourceFolder == false) { throw new Exception(); }
            }
            catch
            {
                Console.WriteLine("Программа завершилась c ошибкой 1: Не удалось создать резервную папку. Проверьте пути к исходной и целевой папкам в файле настроек.");
                File.AppendAllText(logFileFolder, GetDate() + " [Error] Программа завершилась c ошибкой 1\n");
                Console.ReadLine();
                return;
            }
            string BackUpFolderPath = targetFolderPath + "\\" + GetDate() + "\\";
            Directory.CreateDirectory(BackUpFolderPath);

            try
            {
                string destFile;
                string fileName;
                string[] sourceFolderFiles = Directory.GetFiles(sourceFolderPath);
                foreach (string file in sourceFolderFiles)
                {
                    fileName = Path.GetFileName(file);
                    destFile = Path.Combine(BackUpFolderPath, fileName);

                    try
                    {
                        File.AppendAllText(logFileFolder, GetDate() + " [Info] Начинается копирование " + fileName + " в целевую папку\n");
                        File.Copy(file, destFile, true);
                        File.AppendAllText(logFileFolder, GetDate() + " [Complete] Файл " + fileName + " успешно скопирован по пути " + BackUpFolderPath + "\n");
                    }
                    catch (System.UnauthorizedAccessException)
                    {
                        File.AppendAllText(logFileFolder, GetDate() + " [AccessError] Не удалось cкопировать файл " + fileName + ", т.к. отсутствуют права доступа\n");
                    }
                    catch
                    {
                        File.AppendAllText(logFileFolder, GetDate() + " [Error] Не удалось cкопировать файл " + fileName + " из-за внезапной ошибки. Возможно, целевая папка указана не верно.\n");
                    }
                }
                File.AppendAllText(logFileFolder, GetDate() + " [Info] Программа успешно завершила свою работу\n");
            }
            catch
            {
                Console.WriteLine("Программа завершилась c ошибкой 2: Не удалось найти исходную папку. Проверьте пути к исходной и целевой папкам в файле настроек.");
                File.AppendAllText(logFileFolder, GetDate() + " [Error] Программа завершилась c ошибкой 2\n");
                Console.ReadLine();
                return;
            }
        }
    }
}