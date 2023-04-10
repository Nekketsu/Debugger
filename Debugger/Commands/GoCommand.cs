using Debugger.Models;

namespace Debugger.Commands
{
    public class GoCommand : Command
    {
        private readonly Context context;

        public GoCommand(Context context)
        {
            this.context = context;
        }

        public override void Handle()
        {
            context.ContinueExecution = true;
        }
    }
}
