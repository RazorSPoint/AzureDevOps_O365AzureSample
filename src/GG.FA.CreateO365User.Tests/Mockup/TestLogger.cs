using GG.FA.Common.Interfaces;

namespace GG.FA.CreateO365User.Tests.Mockup
{
    using System;

    /// <summary>
    /// Logging class as a mackup wrapper for the logging during testing.
    /// </summary>
    class TestLogger: ILogger
    {
        public void Error(string message)
        {
            Console.Write(message);
        }

        public void Info(string message)
        {
            Console.Write(message);
        }

        public void Warning(string message)
        {
            Console.Write(message);
        }

    }
}
