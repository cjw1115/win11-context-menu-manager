using System;

namespace Program
{
    class Program
    {
        public static void Main(string[] args)
        {
            MenuManager manager = new MenuManager();
            var task = manager.Process(args);
            task.Wait();
            Console.ReadLine();
        }
    }
}
