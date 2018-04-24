using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace FF12TZAPCPatcher
{
    public static unsafe class FastCompare
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Equal(byte[] a, byte[] b)
        {
            return a.Length == b.Length && memcmp(a, b, a.Length) == 0;
        }

        #pragma warning disable IDE1006 // Naming Styles
        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int memcmp(byte[] b1, byte[] b2, long count);

        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int memcmp(void* b1, void* b2, long count);
        #pragma warning restore IDE1006 // Naming Styles
    }
}