using Klogged;

namespace Klogged.Tests
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using (ConsoleLogger logger = new ConsoleLogger())
            {
                logger.Write("No colors :(\n");
                logger.EnableOptions(ConsoleLogger.Options.ColorEnabled);
                logger.Write("Yay colors!\n");
                logger.DisableOptions(ConsoleLogger.Options.ColorEnabled);
                logger.Write("No more colors :(\n");
            }
        }
    }
}
