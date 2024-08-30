using Klogged;

namespace Klogged.Tests
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //BasicTest();
            ColorFormatTest();
        }

        static void BasicTest()
        {
            using (ConsoleLogger logger = new ConsoleLogger())
            {
                logger.Write("No colors :(\n");
                logger.EnableOptions(ConsoleLogger.Options.Color);
                logger.Write("Yay colors!\n");
                logger.DisableOptions(ConsoleLogger.Options.Color);
                logger.Write("No more colors :(\n");

                logger.EnableOptions(ConsoleLogger.Options.Color);
                logger.Info("This is an informative message :)");
                logger.Debug("I'm a debug message!");
                logger.Warning("I'm warning you of something...");
                logger.Error("Impending doom approaches...");

                logger.Write("\n");
                logger.EnableOptions(ConsoleLogger.Options.Timestamp);
                logger.Info("Well would ya look at the time!");
            }
        }

        static void ColorFormatTest()
        {
            ConsoleLogger logger = new ConsoleLogger();

            //logger.WriteF("$green:hello!");

            foreach (ConsoleColor color in (ConsoleColor[])ConsoleColor.GetValues(typeof(ConsoleColor)))
            {
                if (color == Console.BackgroundColor) continue;
                logger.WriteF($"I've turned ${color}:{color}$white:!");
            }
        }
    }
}
