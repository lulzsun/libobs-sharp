using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using static LibObs.Obs;

namespace LibObs.example {
    class Program {
        [DllImport("libX11", EntryPoint = "XOpenDisplay")]
        public static extern IntPtr XOpenDisplay(IntPtr display);

        static void Main(string[] args) {
            Directory.SetCurrentDirectory(Path.Combine(AppContext.BaseDirectory));

            // STARTUP
            if (obs_initialized()) {
                throw new Exception("error: obs already initialized");
            }

#if !WINDOWS
            obs_set_nix_platform(obs_nix_platform_type.OBS_NIX_PLATFORM_X11_EGL);
            obs_set_nix_platform_display(XOpenDisplay(IntPtr.Zero));
#else
            // Not working on Linux. See: https://github.com/dotnet/runtime/issues/48796
            base_set_log_handler(new log_handler_t((lvl, msg, args, p) => {
                string formattedMsg = MarshalUtils.GetLogMessage(msg, args);
                Console.WriteLine(((LogErrorLevel)lvl).ToString() + ": " + formattedMsg);
            }), IntPtr.Zero);
#endif
            Console.WriteLine("libobs version: " + obs_get_version_string());
            if (!obs_startup("en-US", null, IntPtr.Zero)) {
                throw new Exception("error on libobs startup");
            }
            obs_add_data_path("./data/libobs/");
            obs_add_module_path("./obs-plugins/64bit/", "./data/obs-plugins/%module%/");

            obs_audio_info avi = new() {
                samples_per_sec = 44100,
                speakers = speaker_layout.SPEAKERS_STEREO
            };
            bool resetAudioCode = obs_reset_audio(ref avi);

            // scene rendering resolution
            int MainWidth = 1920;
            int MainHeight = 1080;

            obs_video_info ovi = new() {
                adapter = 0,
#if WINDOWS
                graphics_module = "libobs-d3d11",
#else
                graphics_module = "libobs-opengl",
#endif
                fps_num = 60,
                fps_den = 1,
                base_width = (uint)MainWidth,
                base_height = (uint)MainHeight,
                output_width = (uint)MainWidth,
                output_height = (uint)MainHeight,
                output_format = video_format.VIDEO_FORMAT_NV12,
                gpu_conversion = true,
                colorspace = video_colorspace.VIDEO_CS_DEFAULT,
                range = video_range_type.VIDEO_RANGE_DEFAULT,
                scale_type = obs_scale_type.OBS_SCALE_BILINEAR
            };
            int resetVideoCode = obs_reset_video(ref ovi);
            if (resetVideoCode != 0) {
                throw new Exception("error on libobs reset video: " + ((VideoResetError)resetVideoCode).ToString());
            }

            obs_load_all_modules();
            obs_log_loaded_modules();
            obs_post_load_modules();

            // SETUP NEW VIDEO SOURCE
#if WINDOWS
            IntPtr videoSource = obs_source_create("monitor_capture", "Screen Capture Source", IntPtr.Zero, IntPtr.Zero);
#else
            IntPtr videoSource = obs_source_create("xshm_input", "Screen Capture Source", IntPtr.Zero, IntPtr.Zero);
#endif
            obs_set_output_source(0, videoSource); //0 = VIDEO CHANNEL
            // SETUP NEW VIDEO ENCODER
            IntPtr videoEncoderSettings = obs_data_create();
            obs_data_set_bool(videoEncoderSettings, "use_bufsize", true);
            obs_data_set_string(videoEncoderSettings, "profile", "high");
            obs_data_set_string(videoEncoderSettings, "preset", "veryfast");
            obs_data_set_string(videoEncoderSettings, "rate_control", "CRF");
            obs_data_set_int(videoEncoderSettings, "crf", 20);
            IntPtr videoEncoder = obs_video_encoder_create("obs_x264", "simple_h264_recording", videoEncoderSettings, IntPtr.Zero);
            obs_encoder_set_video(videoEncoder, obs_get_video());
            obs_data_release(videoEncoderSettings);

            // SETUP NEW AUDIO SOURCE
#if WINDOWS
            IntPtr audioSource = obs_source_create("wasapi_output_capture", "Audio Capture Source", IntPtr.Zero, IntPtr.Zero);
#else
            IntPtr audioEncoderSettings = obs_data_create();
            obs_data_set_string(audioEncoderSettings, "device_id", "default");
            IntPtr audioSource = obs_source_create("pulse_output_capture", "Audio Capture Source", IntPtr.Zero, IntPtr.Zero);
            obs_data_release(audioEncoderSettings);
#endif
            obs_set_output_source(1, audioSource); //1 = AUDIO CHANNEL
            // SETUP NEW AUDIO ENCODER
            IntPtr audioEncoder = obs_audio_encoder_create("ffmpeg_aac", "simple_aac_recording", IntPtr.Zero, (UIntPtr)0, IntPtr.Zero);
            obs_encoder_set_audio(audioEncoder, obs_get_audio());

            // SETUP NEW RECORD OUTPUT
            IntPtr recordOutputSettings = obs_data_create();
            obs_data_set_string(recordOutputSettings, "path", "./record.mp4");
            IntPtr recordOutput = obs_output_create("ffmpeg_muxer", "simple_ffmpeg_output", recordOutputSettings, IntPtr.Zero);
            obs_data_release(recordOutputSettings);

            obs_output_set_video_encoder(recordOutput, videoEncoder);
            obs_output_set_audio_encoder(recordOutput, audioEncoder, (UIntPtr)0);

            // SETUP NEW BUFFER OUTPUT (OPTIONAL, just demonstrating it here in example that multiple outputs can be run)
            IntPtr bufferOutputSettings = obs_data_create();
            obs_data_set_string(bufferOutputSettings, "directory", "./");
            obs_data_set_string(bufferOutputSettings, "format", "%CCYY-%MM-%DD %hh-%mm-%ss");
            obs_data_set_string(bufferOutputSettings, "extension", "mp4");
            obs_data_set_int(bufferOutputSettings, "max_time_sec", 15);
            obs_data_set_int(bufferOutputSettings, "max_size_mb", 500);
            IntPtr bufferOutput = obs_output_create("replay_buffer", "replay_buffer_output", bufferOutputSettings, IntPtr.Zero);
            obs_data_release(bufferOutputSettings);

            obs_output_set_video_encoder(bufferOutput, videoEncoder);
            obs_output_set_audio_encoder(bufferOutput, audioEncoder, (UIntPtr)0);

            // START RECORD OUTPUT
            bool recordOutputStartSuccess = obs_output_start(recordOutput);
            Console.WriteLine("record output successful start: " + recordOutputStartSuccess);
            if (recordOutputStartSuccess != true) {
                Console.WriteLine("record output error: '" + obs_output_get_last_error(recordOutput) + "'");
            }

            // START BUFFER OUTPUT
            bool bufferOutputStartSuccess = obs_output_start(bufferOutput);
            Console.WriteLine("buffer output successful start: " + bufferOutputStartSuccess);
            if (bufferOutputStartSuccess != true) {
                Console.WriteLine("buffer output error: '" + obs_output_get_last_error(bufferOutput) + "'");
            }

            // SAVE REPLAY BUFFER
            Task.Run(async () => {
                await Task.Delay(5000); // Record for 5 seconds
                calldata_t cd = new();
                var ph = obs_output_get_proc_handler(bufferOutput);
                Console.WriteLine("buffer output successful save: " + proc_handler_call(ph, "save", cd));
            });

            Console.WriteLine("Record Output id is " + obs_output_get_id(recordOutput));
            Console.WriteLine("Buffer Output id is " + obs_output_get_id(bufferOutput));

            Console.ReadLine();
        }
    }
}
