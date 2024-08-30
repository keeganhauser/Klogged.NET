using System.Text;
using System.Text.RegularExpressions;

namespace Klogged
{
    public class ConsoleLogger : IDisposable
    {
        private static readonly ConsoleColor _defaultFgColor = ConsoleColor.White;
        private static readonly ConsoleColor _defaultBgColor = ConsoleColor.Black;

        private static readonly Dictionary<LogLevel, ConsoleColor> _logLevelColors = new()
        {
            { LogLevel.Info,    ConsoleColor.DarkGray   },
            { LogLevel.Debug,   ConsoleColor.White      },
            { LogLevel.Warning, ConsoleColor.DarkYellow },
            { LogLevel.Error,   ConsoleColor.DarkRed    },
        };

        private static readonly ConsoleColor[] _allColors = (ConsoleColor[]) ConsoleColor.GetValues(typeof(ConsoleColor));

        private Options _optionsField;

        [Flags]
        public enum Options
        {
            None = 0, // Note: all options should be a power of 2.
            Color = 1,
            Timestamp = 2,
        }

        public enum LogLevel
        {
            Info = 0,
            Debug,
            Warning,
            Error,
        }

        // ----------------------------------
        //
        //      CONSTRUCTORS
        //
        // ----------------------------------
        #region Constructors
        public ConsoleLogger() : this(Options.None) { }

        public ConsoleLogger(Options options)
        {
            _optionsField = options;
        }
        #endregion

        // ----------------------------------
        //
        //      PUBLIC METHODS
        //
        // ----------------------------------
        #region Public Methods
        public void Write<T>(T obj, LogLevel? logLevel = null)
        {
            StringBuilder sb = new StringBuilder();

            if (_optionsField.HasFlag(Options.Color) && logLevel.HasValue)
            {
                Console.ForegroundColor = _logLevelColors[logLevel.Value];

                // TODO: Be able to modify background color
                Console.BackgroundColor = _defaultBgColor;
            }

            if (_optionsField.HasFlag(Options.Timestamp))
            {
                sb.Append($"[{DateTime.Now.ToString("HH:mm:ss")}]");
            }

            sb.Append(obj);

            Console.Write(sb);
            Console.ResetColor();
        }

        /// <summary>
        /// Outputs a color formatted string to the console. To properly to change the color of the
        /// output, use: $[COLOR]:[STRING]. If no color is specified at the start, a default value
        /// will be used. Whitespace is preserved.
        /// 
        /// Example: I am red with $red:anger$white:!!!
        /// </summary>
        /// <param name="str"></param>
        public void WriteF(string str)
        {
            // TODO: Make this function less janky and ugly lol
            List<KeyValuePair<string, ConsoleColor>> messages = new();

            Regex rx = new(@"\$(.*?):");
            MatchCollection matches = rx.Matches(str);
            
            // On first match, see if there's anything at the beginning that wasn't formatted.
            if (matches.Count > 0 && matches.First().Index != 0)
            {
                messages.Add(new(str.Substring(0, matches.First().Index), _defaultFgColor));

            }

            // For all the matches, grab their string contents and assign the respective color to them
            for (int i = 0; i < matches.Count; i++)
            {
                // Get color string from regex match
                string colorStr = matches[i].Groups.Values.Last().Value;

                string contentStr = (i < matches.Count - 1)
                    ? str.Substring(matches[i].Index + matches[i].Length, matches[i + 1].Index - (matches[i].Index + matches[i].Length))
                    : str.Substring(matches[i].Index + matches[i].Length);

                messages.Add(new(contentStr, GetColor(colorStr)));
            }

            foreach (var message in messages)
            {
                Console.ForegroundColor = message.Value;
                Console.Write($"{message.Key}");
            }
            Console.Write("\n");
            Console.ResetColor();
        }

        public void Info<T>(T obj) => WriteF($"${_logLevelColors[LogLevel.Info]}:[Info] $white:{obj}\n");
        public void Debug<T>(T obj) => WriteF($"${_logLevelColors[LogLevel.Debug]}:[Debug] $white:{obj}\n");
        public void Warning<T>(T obj) => WriteF($"${_logLevelColors[LogLevel.Warning]}:[Warning] $white:{obj}\n");
        public void Error<T>(T obj) => WriteF($"${_logLevelColors[LogLevel.Error]}:[Error] $white:{obj}\n");


        public void EnableOptions(Options options)
        {
            _optionsField |= options;
        }

        public void DisableOptions(Options options) 
        {
            _optionsField &= ~options;
        }

        public void SetLogLevelColor(LogLevel logLevel, ConsoleColor color)
        {
            _logLevelColors[logLevel] = color;
        }

        #endregion

        // ----------------------------------
        //
        //      PRIVATE METHODS
        //
        // ----------------------------------
        #region Private Methods
        private ConsoleColor GetColor(string str)
        {
            for (int i = 0; i < _allColors.Length; i++)
            {
                if (str.Equals(_allColors[i].ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    return _allColors[i];
                }
            }

            return _defaultFgColor;
        }

        #endregion

        #region IDisposable
        /// <summary>
        /// IDisposable implementation
        /// </summary>
        public void Dispose() { }
        #endregion
    }
}
