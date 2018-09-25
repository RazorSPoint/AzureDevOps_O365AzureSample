using System;
using System.Collections.Generic;
using System.Text;

namespace GG.FA.Common.Interfaces
{
    /// <summary>
    /// Generic logging interface for portability 
    /// </summary>
    public interface IMyLogger
	{
        void Error(string message);
        void Info(string message);
        void Warning(string message);
    }
}
