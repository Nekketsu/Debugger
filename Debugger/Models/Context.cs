using Windows.Win32;
using Windows.Win32.Foundation;

namespace Debugger.Models
{
    public class Context
    {
        public bool ExpectStepException { get; set; }
        internal NTSTATUS ContinueStatus { get; set; }
        public bool ContinueExecution { get; set; }
        public bool Quit { get; set; }

        public CONTEXT64 ThreadContext { get; set; }
        public nint ThreadId { get; set; }
    }
}
