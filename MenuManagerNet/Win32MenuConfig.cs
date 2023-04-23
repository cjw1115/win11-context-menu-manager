using Program;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MenuManagerNet
{
    public class Win32MenuConfig : IMenuConfig
    {
        public string MENU_CONFIG_FILE_NAME = "menus_config.json";

        private List<MenuItem> _menuItems;

        public Task LoadAsync()
        {
            return Task.Run(() =>
            {
                FileStream configFile = null;
                _menuItems = new List<MenuItem>(0);
                try
                {
                    configFile = File.OpenRead(MENU_CONFIG_FILE_NAME);
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
            });
        }

        public Task SaveAsync()
        {
            return Task.Run(() =>
            {
                FileStream configFile = null;
                try
                {
                    if (File.Exists(MENU_CONFIG_FILE_NAME))
                    {
                        File.Delete(MENU_CONFIG_FILE_NAME);
                    }
                    configFile = File.Open(MENU_CONFIG_FILE_NAME, FileMode.CreateNew);
                    var content = System.Text.Json.JsonSerializer.Serialize(_menuItems);
                    var configBytes = Encoding.UTF8.GetBytes(content);
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
            });
        }

        public async Task Add(MenuItem newItem)
        {
            if (newItem == null)
            {
                throw new NullReferenceException("Added menu item is empty");
            }
            if (newItem.ComServer == Guid.Empty)
            {
                newItem.ComServer = Guid.NewGuid();
            }
            try
            {
                var target = await Get(newItem.ComServer);
                if (target == null)
                {
                    _menuItems.Add(newItem);
                }
                else
                {
                    throw new DuplicatedException($"Added menu item with ComServerP{newItem.ComServer} already exist");
                }
            }
            catch
            {
                throw;
            }
        }

        public Task<MenuItem> Get(Guid comServer)
        {
            return Task.FromResult(_menuItems.FirstOrDefault(x => x.ComServer == comServer));
        }

        public async Task Remove(MenuItem item)
        {
            try
            {
                var oldItem = await Get(item.ComServer);
                if (oldItem != null)
                {
                    _menuItems.Remove(oldItem);
                }
            }
            catch
            {
                throw;
            }
        }

        public async Task Update(MenuItem newItem)
        {
            try
            {
                var oldItem = await Get(newItem.ComServer);
                if (oldItem != null)
                {
                    oldItem.Title = newItem.Title;
                    oldItem.Target = newItem.Target;
                    oldItem.FileType = newItem.FileType;
                }
                else
                {
                    throw new NotExistException($"Updated item with ComServer {newItem.ComServer} is not exist");
                }
            }
            catch
            {
                throw;
            }
        }

        public Task<List<MenuItem>> GetAll()
        {
            return Task.FromResult(_menuItems.ToList());
        }
    }
}
