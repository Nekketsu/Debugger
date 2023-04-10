using Debugger.Models;

namespace Debugger.Commands
{
    public class QuitCommand : Command
    {
        private readonly Context context;

        public QuitCommand(Context context)
        {
            this.context = context;
        }

        public override void Handle()
        {
            context.Quit = true;
        }
    }
}
