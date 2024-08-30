namespace Klogged
{
    public class ConsoleLogger : IDisposable
    {
        private static readonly ConsoleColor _defaultFgColor = ConsoleColor.Cyan;
        private static readonly ConsoleColor _defaultBgColor = ConsoleColor.Black;

        private ConsoleColor _fgColor, _bgColor;
        private Options _optionsField;

        [Flags]
        public enum Options
        {
            None = 0,
            ColorEnabled = 1,
        }

        public enum LogLevel
        {
            Info = 0,
            Debug,
            Warning,
            Error,
        }

        public ConsoleLogger()
        {
            _optionsField = 0;
            _fgColor = _defaultFgColor;
            _bgColor = _defaultBgColor;
        }

        public void Write(string str)
        {
            if (_optionsField.HasFlag(Options.ColorEnabled))
            {
                Console.ForegroundColor = _fgColor;
                Console.BackgroundColor = _bgColor;
            }

            Console.Write(str);
            Console.ResetColor();
        }

        public void WriteLine(string str)
        {
            Write(str + "\n");
        }

        public void EnableOptions(Options options)
        {
            _optionsField |= options;
        }

        public void DisableOptions(Options options) 
        {
            _optionsField &= ~options;
        }

        /// <summary>
        /// IDisposable implementation
        /// </summary>
        public void Dispose() { }
    }
}
