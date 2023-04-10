using Debugger.Models;

namespace Debugger.Commands
{
    public class DisplayRegistersCommand : Command
    {
        private readonly Context context;

        public DisplayRegistersCommand(Context context)
        {
            this.context = context;
        }

        public override void Handle()
        {
            var context = this.context.ThreadContext;

            Console.WriteLine($"0x{context.Rax:x16} 0x{context.Rbx:x16} 0x{context.Rcx:x16}");
            Console.WriteLine($"0x{context.Rdx:x16} 0x{context.Rsi:x16} 0x{context.Rdi:x16}");
            Console.WriteLine($"0x{context.Rip:x16} 0x{context.Rsp:x16} 0x{context.Rbp:x16}");
            Console.WriteLine($"0x{context.R8:x16} 0x{context.R9:x16} 0x{context.R10:x16}");
            Console.WriteLine($"0x{context.R11:x16} 0x{context.R12:x16} 0x{context.R13:x16}");
            Console.WriteLine($"0x{context.R14:x16} 0x{context.R15:x16} 0x{context.EFlags:x8}");
        }
    }
}
