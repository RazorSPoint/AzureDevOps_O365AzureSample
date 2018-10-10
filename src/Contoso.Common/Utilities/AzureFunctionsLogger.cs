using System;
using System.Collections.Generic;
using System.Text;
using GG.FA.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace GG.FA.Common.Utilities
{
    /// <summary>
    /// Azure Functions logger wrapper
    /// </summary>
    public class AzureFunctionLogger : IMyLogger
    {
        private static Microsoft.Extensions.Logging.ILogger _logger;

        public AzureFunctionLogger(ILogger logger)
        {
            _logger = logger;
        }

        public void Error(string message)
        {
            _logger.LogError(message);
        }

        public void Info(string message)
        {
            _logger.LogInformation(message);
        }

        public void Warning(string message)
        {
            _logger.LogWarning(message);
        }
    }
}
