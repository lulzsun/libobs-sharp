using System;
using System.Runtime.InteropServices;


namespace obs_net;

public partial class Obs {

    [DllImport(Obs.importLibrary, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    //public static extern void obs_set_nix_platform([NativeTypeName("enum obs_nix_platform_type")] obs_nix_platform_type platform);
    
    public static extern void obs_set_nix_platform(obs_nix_platform_type platform);

    [DllImport(Obs.importLibrary, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    //[return: NativeTypeName("enum obs_nix_platform_type")]
    public static extern obs_nix_platform_type obs_get_nix_platform();

    [DllImport(Obs.importLibrary, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void obs_set_nix_platform_display(IntPtr display);

    [DllImport(Obs.importLibrary, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern IntPtr obs_get_nix_platform_display();

}

public enum obs_nix_platform_type {
    OBS_NIX_PLATFORM_X11_GLX,
    OBS_NIX_PLATFORM_X11_EGL,
    OBS_NIX_PLATFORM_WAYLAND
}