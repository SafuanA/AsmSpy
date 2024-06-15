namespace AsmSpy.Core.Tests
{
    public class NullLogger : ILogger
    {

        public NullLogger()
        {
        }

        public void LogError(string message)
        {
        }

        public void LogMessage(string message)
        {
        }

        public void LogWarning(string message)
        {
        }
    }
}
