using System.Diagnostics.CodeAnalysis;
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
            { LogLevel.Info,    ConsoleColor.Gray       },
            { LogLevel.Debug,   ConsoleColor.DarkGray   },
            { LogLevel.Warning, ConsoleColor.DarkYellow },
            { LogLevel.Error,   ConsoleColor.DarkRed    },
        };

        private static readonly Dictionary<LogLevel, string> _logLevelPrefixes = new()
        {
            { LogLevel.Info,    $"$({_logLevelColors[LogLevel.Info]})[Info]$(reset)"        },
            { LogLevel.Debug,   $"$({_logLevelColors[LogLevel.Debug]})[Debug]$(reset)"      },
            { LogLevel.Warning, $"$({_logLevelColors[LogLevel.Warning]})[Warning]$(reset)"  },
            { LogLevel.Error,   $"$({_logLevelColors[LogLevel.Error]})[Error]$(reset)"      },
        };

        private static readonly ConsoleColor[] _allColors = (ConsoleColor[]) ConsoleColor.GetValues(typeof(ConsoleColor));

        private Options _optionsField;
        private ConsoleColor _timestampColor = ConsoleColor.DarkGray;

        [StringSyntax(StringSyntaxAttribute.DateTimeFormat)]
        private string _timestampFormat = "HH:mm:ss";

        [StringSyntax(StringSyntaxAttribute.DateTimeFormat)]
        public string TimestampFormat
        {
            get => _timestampFormat;
            set => _timestampFormat = value;
        }

        public Dictionary<LogLevel, ConsoleColor> LogLevelColors => _logLevelColors;

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
        public void Write<T>(T obj)
        {
            WriteF($"$(reset){obj}\n");
        }

        /// <summary>
        /// <para>
        /// Outputs a color formatted string to the console. To properly to change the color of the
        /// output, use $(COLOR). If no color is specified at the start, a default value
        /// will be used. Whitespace is preserved.
        /// </para>
        /// 
        /// <para>
        /// Example: The string <c>"I am red with $(red)anger$(white)!!!"</c> turns to "I am red with anger!!!"
        /// with "anger" being in red text, and everything else in white text.
        /// </para>
        /// 
        /// </summary>
        public void WriteF(string str)
        {
            // TODO: Make this function less janky and ugly lol
            Regex rx = new(@"\$\((.*?)\)");
            MatchCollection matches = rx.Matches(str);
            Queue<(string Message, ConsoleColor Color)> messages = new(matches.Count + 1);

            // Prepend timestamp (if enabled)
            if (_optionsField.HasFlag(Options.Timestamp))
            {
                messages.Enqueue(new($"[{DateTime.Now.ToString(_timestampFormat)}]", _timestampColor));
            }

            // On first match, see if there's anything at the beginning that wasn't formatted.
            if (matches.Count > 0 && matches.First().Index != 0)
            {
                messages.Enqueue(new(str.Substring(0, matches.First().Index), _defaultFgColor));
            }

            // For all the matches, grab their string contents and assign the respective color to them
            for (int i = 0; i < matches.Count; i++)
            {
                // Get color string from regex match
                string colorStr = matches[i].Groups.Values.Last().Value;

                string contentStr = (i < matches.Count - 1)
                    ? str.Substring(matches[i].Index + matches[i].Length, matches[i + 1].Index - (matches[i].Index + matches[i].Length))
                    : str.Substring(matches[i].Index + matches[i].Length);

                messages.Enqueue(new(contentStr, GetColor(colorStr)));
            }

            while (messages.Count > 0)
            {
                var coloredMessage = messages.Dequeue();

                // TODO: Save time not doing all the above for when Options.Color is disabled.
                if (_optionsField.HasFlag(Options.Color)) 
                    Console.ForegroundColor = coloredMessage.Color;
                Console.Write($"{coloredMessage.Message}");
            }
            Console.ResetColor();
        }

        public void Info<T>(T obj) => WriteF($"{_logLevelPrefixes[LogLevel.Info]} {obj}\n");
        public void Debug<T>(T obj) => WriteF($"{_logLevelPrefixes[LogLevel.Debug]} {obj}\n");
        public void Warning<T>(T obj) => WriteF($"{_logLevelPrefixes[LogLevel.Warning]} {obj}\n");
        public void Error<T>(T obj) => WriteF($"{_logLevelPrefixes[LogLevel.Error]} {obj}\n");


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

        public void SetLogLevelPrefix(LogLevel logLevel, string prefix)
        {
            _logLevelPrefixes[logLevel] = prefix;
        }

        #endregion

        // ----------------------------------
        //
        //      PRIVATE METHODS
        //
        // ----------------------------------
        #region Private Methods

        /// <summary>
        /// <para>
        /// Get the ConsoleColor from respective string value. If the string is not a valid color,
        /// then a default color will be returned.
        /// </para>
        /// 
        /// <para>
        /// Example: "red" returns ConsoleColor.Red.
        /// </para>
        /// </summary>
        private ConsoleColor GetColor(string str)
        {
            foreach (ConsoleColor color in _allColors)
            {
                if (str.Equals(color.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    return color;
                }
                else if (str.Equals("reset", StringComparison.OrdinalIgnoreCase))
                {
                    return _defaultFgColor;
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
