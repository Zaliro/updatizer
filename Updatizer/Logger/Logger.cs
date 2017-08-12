using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Updatizer.Shared.Reflection;

namespace Updatizer
{
    public class Logger : Singleton<Logger>
    {
        private Action<LogLevel, string> bindedOutput;

        private Logger()
        {
            this.bindedOutput = this.output;
        }

        private void output(LogLevel logLevel, string message)
        {
            switch (logLevel)
            {
                case LogLevel.Debug:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    break;

                case LogLevel.Information:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;

                case LogLevel.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;

                case LogLevel.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
            }

            Console.Write(string.Format("[{0}] ", logLevel.ToString()));
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(string.Format("{0}{1}", message, Environment.NewLine));
        }

        public void ChangeOutputHandler(Action<LogLevel, string> outputHandler)
        {
            this.bindedOutput = outputHandler;
        }

        public bool Debug(string message)
        {
            this.bindedOutput(LogLevel.Debug, message);
            return true;
        }

        public void Info(string message)
        {
            this.bindedOutput(LogLevel.Information, message);
        }

        public void Warning(string message)
        {
            this.bindedOutput(LogLevel.Warning, message);
        }

        public void Error(string message)
        {
            this.bindedOutput(LogLevel.Error, message);
        }
    }
}
