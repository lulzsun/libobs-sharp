using System;
using System.Runtime.InteropServices;

namespace obs_net {
    using proc_handler_t = IntPtr;

    public partial class Obs {
        [StructLayout(LayoutKind.Sequential)]
        public struct calldata_t {
            public IntPtr stack;
            public UIntPtr size;        /* size of the stack, in bytes */
            public UIntPtr capacity;    /* capacity of the stack, in bytes */
            public bool fixedSize;      /* fixed size (using call stack) */
        };

        [DllImport(importLibrary, CallingConvention = importCall, CharSet = importCharSet)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool proc_handler_call(proc_handler_t handler, string name, calldata_t _params);

        [DllImport(importLibrary, CallingConvention = importCall, CharSet = importCharSet)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool calldata_get_string(calldata_t data, string name, out string str);
    }
}
