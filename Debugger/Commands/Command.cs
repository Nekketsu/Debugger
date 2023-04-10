using Debugger.Models;

namespace Debugger.Commands
{
    public abstract class Command
    {
        public abstract void Handle(); 

        public static Command? ReadCommand(Context context)
        {
            Console.Write("> ");
            var input = Console.ReadLine()?.Trim();
            if (!string.IsNullOrEmpty(input))
            {
                return input switch
                {
                    "t" => new StepIntoCommand(context),
                    "g" => new GoCommand(context),
                    "r" => new DisplayRegistersCommand(context),
                    "q" => new QuitCommand(context),
                    _ => null
                };
            }

            return null;
        }
    }
}
