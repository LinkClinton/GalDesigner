using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalEngine
{
    public delegate string LogKeyMapProcesser(LogKey logKey);
    public delegate void LogPrintProcesser(Log log);

    /// <summary>
    /// Log Element: Sub-Log 
    /// </summary>
    public class LogElement
    {
        public string Text { get; }
        public LogColor Color { get; }

        public LogElement(string text, LogColor color = LogColor.White)
        {
            Text = text;
            Color = color;
        }
    }

    /// <summary>
    /// Log
    /// </summary>
    public class Log
    {
        public LogElement[] Elements { get; }
        public string Text { get; }

        public Log(LogElement[] elements)
        {
            Elements = elements;

            //init last log text
            Text = "";
            foreach (var element in Elements) Text = Text + element.Text;
        }
    }

    /// <summary>
    /// Log Key: key -> string 
    /// </summary>
    public class LogKey
    {
        private LogKeyMapProcesser mProcesser;

        public bool FormatEnable { get; set; }
        public LogColor Color { get; set; }

        public string GetLogString()
        {
            //return log string
            //if enable format, we return [log string]
            //else we return "log string"
            if (FormatEnable is true) return "[" + mProcesser(this) + "]";
            return mProcesser(this);
        }

        public LogElement GetLogElement()
        {
            return new LogElement(GetLogString(), Color);
        }

        public LogKey(LogKeyMapProcesser processer, bool formatEnable = true, LogColor color = LogColor.White)
        {
            //processer is a function that map the log key to string
            mProcesser = processer;

            FormatEnable = formatEnable;
            Color = color;
        }
    }

    /// <summary>
    /// Log Format: Log: [Key0] [Key1] [Key2] ... {0} ... {1}...
    /// We replace [key] to value with log key
    /// we replace {number} to value with parameter 
    /// </summary>
    public class LogFormat
    {
        private Dictionary<string, LogKey> mLogKeyMap;

        public LogColor Color { get; set; }

        private static string SetParamsToString(string text, params object[] context)
        {
            //get result
            string result = text;

            int count = 0;

            //find {x}
            while (text.Contains("{" + count + "}") is true)
            {
                //replace {x} to value
                result = result.Replace("{" + count + "}", context[count].ToString());

                //next {}
                count++;
            }

            return result;
        }

        private static LogElement SetLogKeyString(LogFormat logFormat, string key)
        {
            //if log format contains the key, we use the log key to get the log element
            //else we only return the [raw_key]
            if (logFormat.mLogKeyMap.ContainsKey(key) is false) return new LogElement("[" + key + "]", logFormat.Color);
            return logFormat.mLogKeyMap[key].GetLogElement();
        }

        public LogFormat()
        {
            mLogKeyMap = new Dictionary<string, LogKey>();

            //set default color
            Color = LogColor.White;
        }

        public void AddLogKey(string key, LogKey logKey)
        {
            mLogKeyMap.Add(key, logKey);
        }

        public void RemoveLogKey(string key)
        {
            mLogKeyMap.Remove(key);
        }

        public Log ApplyLog(string rawLogText, params object[] context)
        {
            //replace {x} to context
            rawLogText = SetParamsToString(rawLogText, context);

            var elements = new List<LogElement>();

            var rawString = "";

            //for all character in the raw log text
            foreach (var character in rawLogText)
            {
                //for some case
                switch (character)
                {
                    //find map key word "[", we can add current raw string to log's elements
                    //and clear up the raw string to record the key
                    case '[': elements.Add(new LogElement(rawString, Color)); rawString = ""; break;
                    //find map key word "]", we can try to find LogKey if it is existed
                    //if it is existed, we map the key to string else we keep it
                    case ']': elements.Add(SetLogKeyString(this, rawString)); rawString = ""; break;
                    //keep raw string, we only add character to raw string
                    default: rawString += character; break;
                }
            }

            //add remain raw string to log's elements
            elements.Add(new LogElement(rawString, Color));

            //return 
            return new Log(elements.ToArray());
        }
    }

    /// <summary>
    /// Log Print: print log
    /// </summary>
    public class LogPrint
    {
        private LogPrintProcesser mLogPrintProcesser;

        public LogPrint(LogPrintProcesser printProcesser)
        {
            mLogPrintProcesser = printProcesser;
        }

        public void Print(Log log)
        {
            mLogPrintProcesser(log);
        }
    }

    public static class LogLevelConvert
    {
        private static Dictionary<LogLevel, string> mStringMap;
        private static Dictionary<LogLevel, LogColor> mLogColorMap;

        static LogLevelConvert()
        {
            mStringMap = new Dictionary<LogLevel, string>();
            mLogColorMap = new Dictionary<LogLevel, LogColor>();

            mStringMap.Add(LogLevel.Information, "Information");
            mStringMap.Add(LogLevel.Warning, "__Warning__");
            mStringMap.Add(LogLevel.Error, "___Error___");

            mLogColorMap.Add(LogLevel.Information, LogColor.Blue);
            mLogColorMap.Add(LogLevel.Warning, LogColor.Green);
            mLogColorMap.Add(LogLevel.Error, LogColor.Red);
        }

        public static string ToString(LogLevel level)
        {
            return mStringMap[level];
        }

        public static LogColor ToColor(LogLevel level)
        {
            return mLogColorMap[level];
        }
    }

    public static class LogEmitter
    {
        private static bool[] mLogLevelEnable;

        public static LogPrint LogPrint { get; }
        public static LogFormat LogFormat { get; }

        static LogEmitter()
        {
            LogPrint = new LogPrint((Log log) =>
            {
                //for all element
                foreach (var logElement in log.Elements)
                {
                    //change write color and write, at least reset color
                    Console.ForegroundColor = EnumerationConvert.ToConsoleColor(logElement.Color);
                    Console.Write(logElement.Text);
                    Console.ResetColor();
                }
                Console.WriteLine();
            });

            //init default log format
            LogFormat = new LogFormat();
            LogFormat.Color = LogColor.Gray;

            //set the log key [time] means current date time (green)
            LogFormat.AddLogKey("time", new LogKey((LogKey logKey) => DateTime.Now.ToString() , true, LogColor.Green));

            //set the log key ["LogLevel"] means log level (with color)
            for (LogLevel logLevel = 0; logLevel < LogLevel.Count; logLevel++)
            {
                //get color and string
                var logLevelColor = LogLevelConvert.ToColor(logLevel);
                var logLevelString = LogLevelConvert.ToString(logLevel);

                //set string and color to log key
                LogFormat.AddLogKey(logLevelString, new LogKey((LogKey logKey) => logLevelString, true, logLevelColor));
            }

            //create enable array and set it to true
            mLogLevelEnable = new bool[(int)LogLevel.Count];
            for (int i = 0; i < mLogLevelEnable.Length; i++) mLogLevelEnable[i] = true;
        }

        public static void Apply(LogLevel level, string logText, params object[] context)
        {
            //disable level, we do not apply this log
            if (mLogLevelEnable[(int)level] == false) return;

            var logString = "[" + LogLevelConvert.ToString(level) + "] " + StringProperty.Log + logText;

            LogPrint.Print(LogFormat.ApplyLog(logString, context));
        }

        public static void Assert(bool condition, LogLevel level, string logText, params object[] context)
        {
            if (condition == false) Apply(level, logText, context);
        }

        public static void EnableLevel(LogLevel level, bool state)
        {
            //set level enable state
            //true means we can print the log of level
            //false means we cna not print the log of level
            mLogLevelEnable[(int)level] = state;
        }
    }
}
