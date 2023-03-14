
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Windows.Management.Deployment;

namespace Program
{
    class Program
    {
        public static void Main(string[] args)
        {
            MenuManager manager = new MenuManager();
            var task = manager.Process(args);
            task.Wait();
        }
    }

    class MenuItem
    {
        public string FileType { get; set; }
        public string Title { get; set; }
        public string Target { get; set; }
    };

    public class MenuManager
    {
        private Dictionary<string, string> AvaliableArgs = new Dictionary<string, string>
        {
            { "-t","Menu item title"},
            { "-p","Menu item target process"},
        };

        private void _printHelpMsg()
        {
            Console.WriteLine("Menu manager console args: menumanager.exe -t \"open\" -p \"c:\\windows\\test.exe\"");
            foreach (var item in AvaliableArgs)
            {
                Console.WriteLine($"\t  {item.Key} : {item.Value}");
            }
        }

        private async Task _addMenuItem(MenuItem item)
        {
            //0. Save item to menus.json
            await _updateManifest(item, true); //1. Add menu item in AppxManifest;
            _launchBuildScript();
            //2. Build & Sign Sparse Appx;
            //3. Install Sparse Appx
        }

        private async Task _updateManifest(MenuItem item, bool isAdd)
        {
            var manifestFile = System.IO.File.OpenRead(@"TemplateAppxManifest.xml");
            XmlReader reader = XmlReader.Create(manifestFile);
            var root = XDocument.Load(reader).Root;
            XmlNameTable nameTable = reader.NameTable;
            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(nameTable);
            namespaceManager.AddNamespace("desktop4", "http://schemas.microsoft.com/appx/manifest/desktop/windows10/4");
            namespaceManager.AddNamespace("desktop5", "http://schemas.microsoft.com/appx/manifest/desktop/windows10/5");
            namespaceManager.AddNamespace("com", "http://schemas.microsoft.com/appx/manifest/com/windows10");
            namespaceManager.AddNamespace("prefix", "http://schemas.microsoft.com/appx/manifest/foundation/windows10");

            var comClass = root.XPathSelectElement("//com:Class", namespaceManager);
            var clsid = comClass.Attribute("Id").Value;

            var contextMenus = root.XPathSelectElement("//desktop4:FileExplorerContextMenus", namespaceManager);

            XNamespace desktop5 = "http://schemas.microsoft.com/appx/manifest/desktop/windows10/5";
            var newItemElement = new XElement(desktop5 + "ItemType");
            
            newItemElement.SetAttributeValue("Type", item.FileType);
            var verbElement = new XElement(desktop5 + "Verb");
            verbElement.SetAttributeValue("Id", Guid.NewGuid().ToString());
            verbElement.SetAttributeValue("Clsid", clsid);

            newItemElement.Add(verbElement);
            contextMenus.Add(newItemElement);

            using (XmlTextWriter writer = new XmlTextWriter("AppxManifest.xml", System.Text.Encoding.UTF8))
            {
                writer.Formatting = Formatting.Indented;
                root.WriteTo(writer);
            }
            manifestFile?.Close();
        }

        private async Task _launchBuildScript()
        {
            Process proc = new Process();
            proc.StartInfo.WorkingDirectory = @"D:\Code\Repos\MenuManager";
            proc.StartInfo.FileName = @"D:\Code\Repos\MenuManager\generate_package.bat";
            proc.StartInfo.CreateNoWindow = false;
            proc.Start();
            proc.WaitForExit();
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

            var result = await packageManager.AddPackageByUriAsync(new Uri(sparsePkgPath),options);
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

        public async Task Process(string[] args)
        {
            var item = _processArgs(args);
            if(item == null)
            {
                return;
            }

            await _addMenuItem(item);

            if(!await _registerSparsePackage("",""))
            {
                return;
            }
        }

    }
}
