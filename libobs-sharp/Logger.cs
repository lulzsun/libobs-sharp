using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace LibObs {
    public partial class Obs {
        public enum LogErrorLevel { error = 100, warning = 200, info = 300, debug = 400 };

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, SetLastError = true)]
        public delegate void log_handler_t(int lvl, string msg, IntPtr args, IntPtr p);

        [DllImport(importLibrary, CallingConvention = importCall)]
        public static extern void base_set_log_handler(log_handler_t handler, IntPtr param);
    }

    public static class MarshalUtils {
        internal readonly struct Native {
            [DllImport("libc", EntryPoint = "vsprintf", CallingConvention = CallingConvention.Cdecl)]
            public static extern int vsprintf_linux(IntPtr buffer, [In][MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(UTF8StringMarshaler))] string format, IntPtr args);

            [DllImport("msvcrt", EntryPoint = "vsprintf", CallingConvention = CallingConvention.Cdecl)]
            public static extern int vsprintf_windows(IntPtr buffer, [In][MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(UTF8StringMarshaler))] string format, IntPtr args);

            [DllImport("libc", EntryPoint = "vsnprintf", CallingConvention = CallingConvention.Cdecl)]
            public static extern int vsnprintf_linux(IntPtr buffer, UIntPtr size, [In][MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(UTF8StringMarshaler))] string format, IntPtr args);

            [DllImport("msvcrt", EntryPoint = "vsnprintf", CallingConvention = CallingConvention.Cdecl)]
            public static extern int vsnprintf_windows(IntPtr buffer, UIntPtr size, [In][MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(UTF8StringMarshaler))] string format, IntPtr args);
        }

        internal static T PtrToStructure<T>(IntPtr ptr) {
            return Marshal.PtrToStructure<T>(ptr)!;
        }

        public static string GetLogMessage(string format, IntPtr args) {
#if !WINDOWS
            // special marshalling is needed on Linux desktop 64 bits.
            var listStructure = PtrToStructure<VaListLinuxX64>(args);
            var byteLength = 0;
            UseStructurePointer(listStructure, listPointer => {
                byteLength = Native.vsnprintf_linux(IntPtr.Zero, UIntPtr.Zero, format, listPointer) + 1;
            });

            var utf8Buffer = IntPtr.Zero;
            try {
                utf8Buffer = Marshal.AllocHGlobal(byteLength);

                return UseStructurePointer(listStructure, listPointer => {
                    Native.vsprintf_linux(utf8Buffer, format, listPointer);
                    return utf8Buffer.FromUtf8();
                });
            }
            finally {
                Marshal.FreeHGlobal(utf8Buffer);
            }
#else
            var byteLength = Native.vsnprintf_windows(IntPtr.Zero, UIntPtr.Zero, format, args) + 1;
            if (byteLength <= 1)
                return string.Empty;

            var buffer = IntPtr.Zero;
            try {
                buffer = Marshal.AllocHGlobal(byteLength);
                Native.vsnprintf_windows(buffer, format, args);
                return buffer.FromUtf8()!;
            }
            finally {
                Marshal.FreeHGlobal(buffer);
            }
#endif
        }

        static string UseStructurePointer<T>(T structure, Func<IntPtr, string> action) where T: notnull {
            var structurePointer = IntPtr.Zero;
            try {
                structurePointer = Marshal.AllocHGlobal(Marshal.SizeOf(structure));
                Marshal.StructureToPtr(structure, structurePointer, false);
                return action(structurePointer);
            }
            finally {
                Marshal.FreeHGlobal(structurePointer);
            }
        }

        static void UseStructurePointer<T>(T structure, Action<IntPtr> action) where T : notnull {
            var structurePointer = IntPtr.Zero;
            try {
                structurePointer = Marshal.AllocHGlobal(Marshal.SizeOf(structure));
                Marshal.StructureToPtr(structure, structurePointer, false);
                action(structurePointer);
            }
            finally {
                Marshal.FreeHGlobal(structurePointer);
            }
        }

        internal static string? FromUtf8(this IntPtr nativeString) {
            if (nativeString == IntPtr.Zero)
                return null;

            var length = 0;

            while (Marshal.ReadByte(nativeString, length) != 0) {
                length++;
            }
            var buffer = new byte[length];
            Marshal.Copy(nativeString, buffer, 0, buffer.Length);
            // if (libvlcFree)
            //     MarshalUtils.LibVLCFree(ref nativeString);
            return Encoding.UTF8.GetString(buffer, 0, buffer.Length);
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        struct VaListLinuxX64 {
            uint gp_offset;
            uint fp_offset;
            IntPtr overflow_arg_area;
            IntPtr reg_save_area;
        }
    }
}