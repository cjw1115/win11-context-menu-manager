using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Windows.ApplicationModel;
using Windows.Management.Deployment;

namespace Program
{
    public class MenuManager
    {
        public const string MENU_CONFIG_FILE = "menus_config.json";

        private Dictionary<string, string> AvaliableArgs = new Dictionary<string, string>
        {
            { "-title","Menu item title"},
            { "-target","Menu item target process"},
            { "-type","Target file extention" }
        };

        private List<MenuItem> _menuItems = new List<MenuItem>();

        public MenuManager()
        {
            _loadMenus();
        }

        private void _loadMenus()
        {
            FileStream configFile = null;
            try
            {
                configFile = File.OpenRead(MENU_CONFIG_FILE);
                _menuItems = System.Text.Json.JsonSerializer.Deserialize<List<MenuItem>>(configFile);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Open menu config file failed:{ex.Message}");
            }
            finally
            {
                configFile?.Close();
            }
        }

        private void _saveMenus()
        {
            FileStream configFile = null;
            try
            {
                if (File.Exists(MENU_CONFIG_FILE))
                {
                    File.Delete(MENU_CONFIG_FILE);
                }
                configFile = File.Open(MENU_CONFIG_FILE, FileMode.CreateNew);
                var content = System.Text.Json.JsonSerializer.Serialize(_menuItems);
                var configBytes = System.Text.Encoding.UTF8.GetBytes(content);
                configFile.Write(configBytes, 0, configBytes.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Save menu config file failed:{ex.Message}");
            }
            finally
            {
                configFile?.Close();
            }
        }

        public async Task Process(string[] args)
        {
            var item = _processArgs(args);
            if (item == null)
            {
                return;
            }

            var externalLocation = Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            var sparseAppxPath = await _addMenuItem(item);

            await _removeSparsePackage();
            if (!await _registerSparsePackage(externalLocation, sparseAppxPath))
            {
                return;
            }
        }

        private void _printHelpMsg()
        {
            Console.WriteLine("Menu manager console args: menumanager.exe -t \"open\" -p \"c:\\windows\\test.exe\"");
            foreach (var item in AvaliableArgs)
            {
                Console.WriteLine($"\t  {item.Key} : {item.Value}");
            }
        }

        private async Task<string> _addMenuItem(MenuItem item)
        {
            item.ComServer = Guid.NewGuid();
            _menuItems.Add(item);
            _saveMenus(); //0. Save item to menus.json
            await _updateManifest(_menuItems); //1. Add menu item in AppxManifest;
            return await _launchBuildScript(); //2. Build & Sign Sparse Appx;
        }

        private XElement _createSurrogateServerElement(MenuItem item, int index)
        {
            XNamespace com = "http://schemas.microsoft.com/appx/manifest/com/windows10";
            var surrogateServer = new XElement(com + "SurrogateServer");

            surrogateServer.SetAttributeValue("DisplayName", $"Context menu verb handler {index}");
            var comClass = new XElement(com + "Class");
            comClass.SetAttributeValue("Id", item.ComServer);
            comClass.SetAttributeValue("Path", "ContextMenus.dll");
            comClass.SetAttributeValue("ThreadingModel", "STA");

            surrogateServer.Add(comClass);
            return surrogateServer;
        }

        private XElement _createItemTypeElement(MenuItem item)
        {
            XNamespace desktop5 = "http://schemas.microsoft.com/appx/manifest/desktop/windows10/5";
            var newItemElement = new XElement(desktop5 + "ItemType");

            newItemElement.SetAttributeValue("Type", item.FileType);
            var verbElement = new XElement(desktop5 + "Verb");
            verbElement.SetAttributeValue("Id", Guid.NewGuid().ToString().Replace("-", ""));
            verbElement.SetAttributeValue("Clsid", item.ComServer);

            newItemElement.Add(verbElement);
            return newItemElement;
        }

        private async Task _updateManifest(List<MenuItem> menuItems)
        {
            var manifestFile = File.OpenRead(@"TemplateAppxManifest.xml");
            XmlReader reader = XmlReader.Create(manifestFile);
            var root = XDocument.Load(reader).Root;
            XmlNameTable nameTable = reader.NameTable;
            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(nameTable);
            namespaceManager.AddNamespace("desktop4", "http://schemas.microsoft.com/appx/manifest/desktop/windows10/4");
            namespaceManager.AddNamespace("desktop5", "http://schemas.microsoft.com/appx/manifest/desktop/windows10/5");
            namespaceManager.AddNamespace("com", "http://schemas.microsoft.com/appx/manifest/com/windows10");
            namespaceManager.AddNamespace("prefix", "http://schemas.microsoft.com/appx/manifest/foundation/windows10");


            var comServer = root.XPathSelectElement("//com:ComServer", namespaceManager);
            var contextMenus = root.XPathSelectElement("//desktop4:FileExplorerContextMenus", namespaceManager);
            int index = 0;
            foreach (var item in menuItems)
            {
                comServer.Add(_createSurrogateServerElement(item, index));
                contextMenus.Add(_createItemTypeElement(item));
                index++;
            }

            using (XmlTextWriter writer = new XmlTextWriter(@".\Appx\SparsePackage\AppxManifest.xml", System.Text.Encoding.UTF8))
            {
                writer.Formatting = Formatting.Indented;
                root.WriteTo(writer);
            }
            manifestFile?.Close();
        }

        private async Task<string> _launchBuildScript()
        {
            var workingDir = Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            Process proc = new Process();
            proc.StartInfo.WorkingDirectory = Path.Combine(workingDir, "Appx");
            proc.StartInfo.FileName = Path.Combine(workingDir, @"Appx\generate_package.bat");
            proc.StartInfo.CreateNoWindow = false;
            proc.Start();
            proc.WaitForExit();
            return Path.Combine(workingDir, @"Appx\ContextMenuySparseAppx.msix");
        }

        private MenuItem _processArgs(string[] args)
        {
            MenuItem item = new MenuItem();

            if (args.Length <= 0)
            {
                Console.WriteLine("Invalid args");
                _printHelpMsg();
                return null;
            }

            for (var i = 0; i < args.Length; i++)
            {
                if (args[i].ToLower() == "-title" && i + 1 < args.Length)
                {
                    item.Title = args[i + 1];
                    continue;
                }
                if (args[i].ToLower() == "-target" && i + 1 < args.Length)
                {
                    item.Target = args[i + 1];
                    continue;
                }
                if (args[i].ToLower() == "-type" && i + 1 < args.Length)
                {
                    item.FileType = args[i + 1];
                    continue;
                }
            }

            if (string.IsNullOrEmpty(item.Title)
                || string.IsNullOrEmpty(item.Target)
                || string.IsNullOrEmpty(item.FileType))
            {
                Console.WriteLine("Invalid args");
                _printHelpMsg();
                return null;
            }

            return item;
        }

        private async Task<bool> _registerSparsePackage(string externalLocation, string sparsePkgPath)
        {
            bool registration = false;

            Console.WriteLine($"exe Location {externalLocation }");
            Console.WriteLine($"msix Address {sparsePkgPath }");

            PackageManager packageManager = new PackageManager();

            //Declare use of an external location
            AddPackageOptions options = new AddPackageOptions();
            options.ExternalLocationUri = new Uri(externalLocation);

            var result = await packageManager.AddPackageByUriAsync(new Uri(sparsePkgPath), options);
            if (result.IsRegistered)
            {
                registration = true;
                Console.WriteLine("Package Registration succeeded!");
            }
            else
            {
                Console.WriteLine($"Installation Error: {result.ExtendedErrorCode}");
            }
            return registration;
        }

        private async Task<bool> _removeSparsePackage()
        {
            PackageManager packageManager = new PackageManager();
            string targetPackageName = "ContextMenuManager";
            Package targetPackage = null;

            foreach (var item in packageManager.FindPackagesForUser(string.Empty))
            {
                if (item.Id.FullName.Contains(targetPackageName))
                {
                    targetPackage = item;
                }
            }
            if (targetPackage != null)
            {
                var result = await packageManager.RemovePackageAsync(targetPackage.Id.FullName, RemovalOptions.None);
                Console.WriteLine($"Remove Package {targetPackage.Id.FullName} finished with message:{result.ErrorText}");
            }
            else
            {
                Console.WriteLine($"Can not find installed pacakge with name {targetPackageName}");
            }
            return true;
        }

    }
}
