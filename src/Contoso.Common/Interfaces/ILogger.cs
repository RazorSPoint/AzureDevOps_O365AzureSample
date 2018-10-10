namespace Contoso.Common.Interfaces
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
