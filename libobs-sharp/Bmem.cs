using System.Runtime.InteropServices;

namespace LibObs {
    public partial class Obs {
        [DllImport(importLibrary, CallingConvention = importCall)]
        public static extern long bnum_allocs();
    }
}
