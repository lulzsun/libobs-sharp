﻿using System;
using System.Runtime.InteropServices;

namespace LibObs {
    using audio_t = IntPtr;
    using obs_data_t = IntPtr;
    using obs_output_t = IntPtr;
    using proc_handler_t = IntPtr;
    using signal_handler_t = IntPtr;
    using size_t = UIntPtr;
    using video_t = IntPtr;

    public partial class Obs {
        [DllImport(importLibrary, CallingConvention = importCall, CharSet = importCharSet)]
        public static extern obs_output_t obs_output_create(
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(UTF8StringMarshaler))] string id,
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(UTF8StringMarshaler))] string name,
            obs_data_t settings, obs_data_t hotkey_data);

        [DllImport(importLibrary, CallingConvention = importCall)]
        public static extern obs_output_t obs_output_get_ref(obs_output_t output);

        [DllImport(importLibrary, CallingConvention = importCall)]
        public static extern void obs_output_release(obs_output_t output);

        [DllImport(importLibrary, CallingConvention = importCall)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool obs_output_active(obs_output_t output);

        [DllImport(importLibrary, CallingConvention = importCall)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool obs_output_start(obs_output_t output);

        [DllImport(importLibrary, CallingConvention = importCall)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool obs_output_can_begin_data_capture(obs_output_t output, uint flags);

        [DllImport(importLibrary, CallingConvention = importCall)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool obs_output_initialize_encoders(obs_output_t output, uint flags);

        /// <summary>
        /// <para>https://obsproject.com/docs/reference-outputs.html?highlight=obs_output_stop#c.obs_output_stop</para>
        /// <para>Requests the output to stop. The output will wait until all data is sent up until the time the call was made, then when the output has successfully stopped, it will send the “stop” signal.</para>
        /// <para>See <see href="https://obsproject.com/docs/reference-outputs.html?highlight=obs_output_stop#output-signal-handler-reference">Output Signals</see> for more information on output signals.</para>
        /// </summary>
        /// <param name="output"></param>
        [DllImport(importLibrary, CallingConvention = importCall)]
        public static extern void obs_output_stop(obs_output_t output);

        /// <summary>
        /// <para>https://obsproject.com/docs/reference-outputs.html?highlight=obs_output_force_stop#c.obs_output_force_stop</para>
        /// <para>Attempts to get the output to stop immediately without waiting for data to send.</para>
        /// </summary>
        /// <param name="output"></param>
        [DllImport(importLibrary, CallingConvention = importCall)]
        public static extern void obs_output_force_stop(obs_output_t output);

        /// <summary>
        /// <para>https://docs.obsproject.com/reference-outputs#c.obs_output_end_data_capture</para>
        /// <para>Ends data capture of an output. This is typically when the output actually intentionally deactivates (stops). Video/audio data will stop being sent to the callbacks of the output. The output will trigger the “stop” signal with the OBS_OUTPUT_SUCCESS code to indicate that the output has stopped successfully.</para>
        /// </summary>
        /// <param name="output"></param>
        [DllImport(importLibrary, CallingConvention = importCall)]
        public static extern void obs_output_end_data_capture(obs_output_t output);

        [DllImport(importLibrary, CallingConvention = importCall)]
        public static extern signal_handler_t obs_output_get_signal_handler(obs_output_t output);

        /// <summary>
        /// https://obsproject.com/docs/reference-outputs.html#c.obs_output_get_last_error
        /// </summary>
        /// <returns>Gets the translated error message that is presented to a user in case of disconnection, inability to connect, etc.</returns>
        [DllImport(importLibrary, CallingConvention = importCall, CharSet = importCharSet)]
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(UTF8StringMarshaler))]
        public static extern string obs_output_get_last_error(obs_output_t output);

        /// <summary>
        /// <para>https://obsproject.com/docs/reference-outputs.html?highlight=obs_output_update#c.obs_output_update</para>
        /// <para>Updates the settings for this output context.</para>
        /// </summary>
        /// <param name="output"></param>
        /// <param name="settings"></param>
        [DllImport(importLibrary, CallingConvention = importCall)]
        public static extern void obs_output_update(obs_output_t output, obs_data_t settings);

        /// <summary>
        /// <para>https://obsproject.com/docs/reference-outputs.html?highlight=obs_output_update#c.obs_output_set_mixers</para>
        /// <para>Sets the current audio mixer for non-encoded outputs. For multi-track outputs, this would be the equivalent of setting the mask only for the specified mixer index.</para>
        /// </summary>
        /// <param name="output"></param>
        /// <param name="mixers"></param>
        [DllImport(importLibrary, CallingConvention = importCall)]
        public static extern void obs_output_set_mixer(obs_output_t output, size_t mixer_idx);

        /// <summary>
        /// <para>https://obsproject.com/docs/reference-outputs.html?highlight=obs_output_update#c.obs_output_set_mixers</para>
        /// <para>Sets the current audio mixers (via mask) for non-encoded multi-track outputs.</para>
        /// <para>If used with single-track outputs, the single-track output will use either the first set mixer track in the bitmask, or the first track if none is set in the bitmask.</para>
        /// </summary>
        /// <param name="output"></param>
        /// <param name="mixers"></param>
        [DllImport(importLibrary, CallingConvention = importCall)]
        public static extern void obs_output_set_mixers(obs_output_t output, size_t mixers);

        [DllImport(importLibrary, CallingConvention = importCall)]
        public static extern video_t obs_output_video(obs_output_t output);

        [DllImport(importLibrary, CallingConvention = importCall)]
        public static extern audio_t obs_output_audio(obs_output_t output);

        [DllImport(importLibrary, CallingConvention = importCall)]
        public static extern proc_handler_t obs_output_get_proc_handler(obs_output_t output);

        [DllImport(importLibrary, CallingConvention = importCall)]
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(UTF8StringMarshaler))]
        public static extern string obs_output_get_id(obs_output_t output);
    }
}