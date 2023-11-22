using Newtonsoft.Json.Linq;
using System.Xml;

namespace ConfigUpdater
{
    public static class MainService
    {
        private static bool _hide;
        public static void Process(string[] args)
        {
            if (args == null || args.Length < 2)
            {
                ShowHelp();
                _hide = true;
            }
            else
            {
                if (args.Length > 2 && args[0].Contains("ConfigUpdater.exe"))
                {
                    args[0] = args[1];
                    args[1] = args[2];
                }

                try
                {
                    UpdateConfigFile(args[0], args[1]);
                    return;
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(Environment.NewLine + ex.Message);
                }
                finally
                {
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }

            string[] inputs = Console.ReadLine().Split(' ');
            inputs = inputs.Where(x => x != string.Empty).ToArray();
            Process(inputs);
        }

        private static void ShowHelp()
        {
            if (_hide) return;

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("-----------------------------------------");
            Console.WriteLine("------------- ConfigUpdater -------------");
            Console.WriteLine("-----------------------------------------");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($"ConfigUpdater.exe ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"<path1> ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"<path2>");
            Console.WriteLine(Environment.NewLine);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"<path1> = ");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($"diretório ou arquivo de origem");
            Console.Write(Environment.NewLine);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"<path2> = ");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($"diretório ou arquivo de destino");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(Environment.NewLine);
        }

        public static void UpdateConfigFile(string sourceFolderPath, string destinationFolderPath)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            sourceFolderPath = sourceFolderPath.Fix();
            destinationFolderPath = destinationFolderPath.Fix();

            Console.WriteLine($"\nBuscando arquivo de origem no diretório: {sourceFolderPath}");
            var sourceFilePath = GetConfigFilePath(sourceFolderPath);
            Console.WriteLine($"\nArquivo de origem: {sourceFilePath}");

            Console.WriteLine($"\nBuscando arquivo de destino no diretório: {destinationFolderPath}");
            var destinationFilePath = GetConfigFilePath(destinationFolderPath);
            Console.WriteLine($"\nArquivo de destino: {destinationFilePath}");

            if (sourceFilePath is null)
            {
                throw new ApplicationException("Erro: Arquivo de origem não encontrado!");
            }

            var file = File.ReadAllText(sourceFilePath);

            if (destinationFilePath is null)
            {
                if (Directory.Exists(destinationFolderPath))
                {
                    destinationFilePath = Path.Combine(destinationFolderPath, Path.GetFileName(sourceFilePath));
                }
                else if (destinationFolderPath.EndsWith(Path.GetFileName(sourceFilePath)))
                {
                    destinationFilePath = destinationFolderPath;
                }
                else
                {
                    throw new ApplicationException("Erro: Diretório de destino inválido!");
                }

                Console.WriteLine($"\nArquivo de destino não encontrado, portanto será criado em: {destinationFilePath}");
                File.WriteAllText(destinationFilePath, file);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\nArquivo criado com sucesso!");
                return;
            }

            if (Path.GetFileName(sourceFilePath) != Path.GetFileName(destinationFilePath))
            {
                throw new ApplicationException("Erro: Os nomes dos arquivos de origem e destino não são iguais!");
            }

            if (file.StartsWith("<"))
            {
                Console.WriteLine("\nAtualizando arquivo do tipo XML...");
                UpdateXmlFile(sourceFilePath, destinationFilePath);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\nArquivo XML atualizado com sucesso!");
            }
            else if (file.StartsWith("{"))
            {
                Console.WriteLine("\nAtualizando arquivo do tipo JSON...");
                UpdateJsonFile(sourceFilePath, destinationFilePath);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\nArquivo JSON atualizado com sucesso!");
            }
            else
            {
                throw new ApplicationException("Erro: Arquivo com formato desconhecido!");
            }
        }

        private static string? GetConfigFilePath(string folderPath, SearchOption option = SearchOption.TopDirectoryOnly)
        {
            var files = new List<string>();

            if (File.Exists(folderPath))
            {
                return folderPath;
            }

            if (!Directory.Exists(folderPath))
            {
                return null;
            }

            var execonfigs = Directory.GetFiles(folderPath, "*.exe.config", option);
            if (execonfigs.Any())
            {
                files.AddRange(execonfigs);
            }
            else
            {
                var appSettings = Directory.GetFiles(folderPath, "appsettings.json", option);
                if (appSettings.Any())
                {
                    files.AddRange(appSettings);
                }
                else
                {
                    var webconfigs = Directory.GetFiles(folderPath, "web.config", option);
                    if (webconfigs.Any())
                    {
                        files.AddRange(webconfigs);
                    }
                }
            }

            if (files.Count > 1)
            {
                throw new ApplicationException($"Mais de um arquivo de configuração foi encontrado no diretório: {folderPath}");
            }

            return files.FirstOrDefault();
        }

        private static void UpdateXmlFile(string sourceFilePath, string destinationFilePath)
        {
            var oldXml = new XmlDocument();
            oldXml.Load(destinationFilePath);

            var oldConfigurationNode = oldXml.DocumentElement.SelectSingleNode($"/configuration");
            var oldSettingsNodes = oldConfigurationNode.ChildNodes.ToList().Where(x => x.Name.ToLower().Contains("settings")).ToList();

            var newXml = new XmlDocument();
            newXml.Load(sourceFilePath);
            var newContent = newXml.DocumentElement.OuterXml;

            var newConfigurationNode = newXml.DocumentElement.SelectSingleNode($"/configuration");
            var newSettingsNodes = newConfigurationNode.ChildNodes.ToList().Where(x => x.Name.ToLower().Contains("settings")).ToList();

            foreach (var oldSettingsNode in oldSettingsNodes)
            {
                var oldSettings = oldSettingsNode.InnerXml;
                var oldAddKeyNodes = oldSettingsNode.ChildNodes.ToList().Where(x => x.Name == "add").ToList();
                var oldAddKeys = oldAddKeyNodes.Select(x => x.Attributes["key"].Value).ToList();

                var newSettingsNode = newSettingsNodes.FirstOrDefault(x => x.Name == oldSettingsNode.Name);

                var newSettings = newSettingsNode.InnerXml;
                var newAddKeyNodes = newSettingsNode.ChildNodes.ToList().Where(x => x.Name == "add").ToList();
                var newAddKeys = newAddKeyNodes.Select(x => x.Attributes["key"].Value).ToList();

                foreach (var newAddKey in newAddKeys)
                {
                    if (!oldAddKeys.Contains(newAddKey))
                    {
                        var nodeToAdd = newAddKeyNodes.FirstOrDefault(x => x.Attributes["key"].Value == newAddKey);
                        oldSettings += nodeToAdd.OuterXml;
                    }
                }

                newContent = newContent.Replace(newSettings, oldSettings);
            }

            File.WriteAllText(destinationFilePath, newContent.PrettifyXml());
        }

        private static void UpdateJsonFile(string sourceFilePath, string destinationFilePath)
        {
            var oldFile = File.ReadAllText(destinationFilePath);
            var oldJson = JObject.Parse(oldFile);

            var newFile = File.ReadAllText(sourceFilePath);
            var newJson = JObject.Parse(newFile);

            foreach (var node in newJson.Values())
            {
                MergeNodes(node, oldJson);
            }

            File.WriteAllText(destinationFilePath, oldJson.ToString());
        }

        private static void MergeNodes(JToken node, JObject oldJson)
        {
            var path = node.Path;
            var name = path.Split('.').LastOrDefault();
            var oldChildren = oldJson.SelectTokens(node.Path).ToList();
            if (!oldChildren.Any())
            {
                var parentObject = oldJson.SelectToken(node.Parent.Parent.Path).ToObject<JObject>();
                parentObject.Add(new JProperty(name, node));
                oldJson[node.Parent.Parent.Path] = parentObject;
            }
            else
            {
                if (node.HasValues)
                {
                    foreach (var subNode in node.Values())
                    {
                        MergeNodes(subNode, oldJson);
                    }
                }
            }
        }
    }
}
