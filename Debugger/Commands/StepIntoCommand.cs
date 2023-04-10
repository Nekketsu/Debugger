using Debugger.Models;
using Windows.UI.Text.Core;
using Windows.Win32;

namespace Debugger.Commands
{
    public class StepIntoCommand : Command
    {
        private readonly Context context;

        public StepIntoCommand(Context context)
        {
            this.context = context;
        }

        public override void Handle()
        {
            const uint TRAP_FLAG = 1 << 8;

            var threadContext = context.ThreadContext;
            threadContext.EFlags |= TRAP_FLAG;
            context.ThreadContext = threadContext;
            var result = PInvoke.GetThreadContext(context.ThreadId, ref threadContext);
            if (!result)
            {
                throw new Exception("SetThreadContext failed");
            }

            context.ExpectStepException = true;
            context.ContinueExecution = true;
        }
    }
}
