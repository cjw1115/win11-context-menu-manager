using System;

namespace MenuManagerNet
{
    public class MenuToolException : Exception 
    {
        public MenuToolException(string msg) : base(msg) { }
    };

    public class DuplicatedException : MenuToolException 
    {
        public DuplicatedException(string msg) : base(msg) { }
    };
    public class NotExistException : MenuToolException
    {
        public NotExistException(string msg) : base(msg) { }
    };
}
