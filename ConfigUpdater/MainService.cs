using Newtonsoft.Json.Linq;
using System.Xml;

namespace ConfigUpdater
{
    public static class MainService
    {
        private static bool _interactiveMode;
        public static void Process(string[] args)
        {
            if (args == null || args.Length < 2)
            {
                ShowHelp();
                _interactiveMode = true;
            }
            else
            {
                if (args[0].Contains("ConfigUpdater.exe"))
                {
                    for (int i = 0; i < args.Length - 1; i++)
                    {
                        args[i] = args[i + 1];
                    }
                }

                try
                {
                    switch (args.Length)
                    {
                        case 2:
                            {
                                UpdateConfigFile(args[0], args[1]);
                                break;
                            }
                        case 3:
                            {
                                EditConfigFile(args[0], args[1], args[2]);
                                break;
                            }
                    }
                    if (!_interactiveMode)
                    {
                        return;
                    }
                }
                catch (Exception ex)
                {
                    if (!_interactiveMode)
                    {
                        throw;
                    }
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
            if (_interactiveMode) return;

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("-----------------------------------------------");
            Console.WriteLine("---------------- ConfigUpdater ----------------");
            Console.WriteLine("-----------------------------------------------");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"Uso: ConfigUpdater.exe [options]");

            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("-----------------------------------------------");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($"ConfigUpdater.exe ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"<path1> <path2>");
            Console.Write(Environment.NewLine);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"<path1> = ");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($"diretório ou arquivo de origem");
            Console.Write(Environment.NewLine);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"<path2> = ");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($"diretório ou arquivo de destino");

            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("-----------------------------------------------");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($"ConfigUpdater.exe ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"<path> <oldValue> <newValue>");
            Console.Write(Environment.NewLine);
            Console.Write($"<path> = ");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($"caminho do arquivo a ser modificado");
            Console.Write(Environment.NewLine);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"<oldValue> = ");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($"valor antigo (procurado)");
            Console.Write(Environment.NewLine);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"<newValue> = ");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($"valor novo (substituto)");

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(Environment.NewLine);
        }

        private static void UpdateConfigFile(string sourceFolderPath, string targetFolderPath)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            sourceFolderPath = sourceFolderPath.Fix();
            targetFolderPath = targetFolderPath.Fix();

            Console.WriteLine($"\nBuscando arquivo de origem no diretório: {sourceFolderPath}");
            var sourceFilePath = GetConfigFilePath(sourceFolderPath);
            Console.WriteLine($"\nArquivo de origem: {sourceFilePath}");

            Console.WriteLine($"\nBuscando arquivo de destino no diretório: {targetFolderPath}");
            var targetFilePath = GetConfigFilePath(targetFolderPath);
            Console.WriteLine($"\nArquivo de destino: {targetFilePath}");

            if (sourceFilePath is null)
            {
                throw new ApplicationException("Erro: Arquivo de origem não encontrado!");
            }

            if (targetFilePath is null)
            {
                var file = File.ReadAllText(sourceFilePath);

                if (Directory.Exists(targetFolderPath))
                {
                    targetFilePath = Path.Combine(targetFolderPath, Path.GetFileName(sourceFilePath));
                }
                else if (targetFolderPath.EndsWith(Path.GetFileName(sourceFilePath)))
                {
                    targetFilePath = targetFolderPath;
                }
                else
                {
                    throw new ApplicationException("Erro: Diretório de destino inválido!");
                }

                Console.WriteLine($"\nArquivo de destino não encontrado, portanto será criado em: {targetFilePath}");
                File.WriteAllText(targetFilePath, file);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\nArquivo criado com sucesso!");
                return;
            }

            if (sourceFilePath.EndsWith("config"))
            {
                Console.WriteLine("\nAtualizando arquivo do tipo XML...");
                UpdateXmlFile(sourceFilePath, targetFilePath);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\nArquivo XML atualizado com sucesso!");
            }
            else if (sourceFilePath.EndsWith("json"))
            {
                Console.WriteLine("\nAtualizando arquivo do tipo JSON...");
                UpdateJsonFile(sourceFilePath, targetFilePath);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\nArquivo JSON atualizado com sucesso!");
            }
            else
            {
                throw new ApplicationException("Erro: Arquivo com formato desconhecido!");
            }
        }

        private static void EditConfigFile(string filePath, string oldValue, string newValue)
        {
            filePath = filePath.Fix();
            oldValue = oldValue.Fix();
            newValue = newValue.Fix();

            if (!File.Exists(filePath))
            {
                throw new ApplicationException($"Erro: Arquivo não encontrado! Caminho: {filePath}");
            }

            if (oldValue is null || newValue is null)
            {
                throw new ApplicationException("Erro: Para editar arquivos, informe 3 parâmetros!" +
                    "\n- Caminho do arquivo" +
                    "\n- Valor antigo" +
                    "\n- Valor novo");
            }

            Console.WriteLine($"\nEditando arquivo: substituindo o valor '{oldValue}' por '{newValue}'...");
            EditFile(filePath, oldValue, newValue);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\nEdição finalizada com sucesso!");
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

        private static void UpdateXmlFile(string sourceFilePath, string targetFilePath)
        {
            var oldXml = new XmlDocument();
            oldXml.Load(targetFilePath);

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

            File.WriteAllText(targetFilePath, newContent.PrettifyXml());
        }

        private static void UpdateJsonFile(string sourceFilePath, string targetFilePath)
        {
            var oldFile = File.ReadAllText(targetFilePath);
            var oldJson = JObject.Parse(oldFile);

            var newFile = File.ReadAllText(sourceFilePath);
            var newJson = JObject.Parse(newFile);

            foreach (var node in newJson.Values())
            {
                MergeNodes(node, oldJson);
            }

            File.WriteAllText(targetFilePath, oldJson.ToString());
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

        private static void EditFile(string filePath, string oldValue, string newValue)
        {
            var file = File.ReadAllText(filePath);

            file = file.Replace(oldValue, newValue);

            File.WriteAllText(filePath, file);
        }
    }
}
