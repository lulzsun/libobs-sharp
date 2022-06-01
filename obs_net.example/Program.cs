using System;
using static obs_net.Obs;

namespace obs_net.example {
	class Program {
		static void Main(string[] args) {
			// STARTUP
			if(obs_initialized()) {
				throw new Exception("error: obs already initialized");
			}

			base_set_log_handler(new log_handler_t((lvl, msg, args, p) => {
				using (va_list arglist = new va_list(args))
				{
					object[] objs = arglist.GetObjectsByFormat(msg);
					string formattedMsg = Printf.sprintf(msg, objs);

					Console.WriteLine(((LogErrorLevel)lvl).ToString() + ": " + formattedMsg);
				}
			}), IntPtr.Zero);

			Console.WriteLine("libobs version: " + obs_get_version_string());
			if(!obs_startup("en-US", null, IntPtr.Zero)) {
				throw new Exception("error on libobs startup");
			}
			obs_add_data_path("./data/libobs/");
			obs_add_module_path("./obs-plugins/64bit/", "./data/obs-plugins/%module%/");
			obs_load_all_modules();
			obs_log_loaded_modules();

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
				graphics_module = "libobs-d3d11",
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

			obs_post_load_modules();

			// SETUP NEW VIDEO SOURCE
			IntPtr videoSource = obs_source_create("monitor_capture", "Monitor Capture Source", IntPtr.Zero, IntPtr.Zero);
			// SETUP NEW VIDEO ENCODER
			IntPtr videoEncoderSettings = obs_data_create();
			obs_data_set_bool(videoEncoderSettings, "use_bufsize", true);
			obs_data_set_string(videoEncoderSettings, "profile", "high");
			obs_data_set_string(videoEncoderSettings, "preset", "veryfast");
			obs_data_set_string(videoEncoderSettings, "rate_control", "CRF");
			obs_data_set_int(videoEncoderSettings, "crf", 20);
			IntPtr videoEncoder = obs_video_encoder_create("obs_x264", "simple_h264_recording", videoEncoderSettings, IntPtr.Zero);
			obs_data_release(videoEncoderSettings);

			// SETUP NEW AUDIO SOURCE
			IntPtr audioSource = obs_source_create("wasapi_output_capture", "Audio Capture Source", IntPtr.Zero, IntPtr.Zero);
			// SETUP NEW AUDIO ENCODER
			IntPtr audioEncoder = obs_audio_encoder_create("ffmpeg_aac", "simple_aac_recording", IntPtr.Zero, (UIntPtr)0, IntPtr.Zero);

			// SETUP NEW OUTPUT
			IntPtr outputSettings = obs_data_create();
			obs_data_set_string(outputSettings, "path", "./recording.mp4");
			IntPtr output = obs_output_create("ffmpeg_muxer", "simple_ffmpeg_output", outputSettings, IntPtr.Zero);
			obs_data_release(outputSettings);

			obs_encoder_set_video(videoEncoder, obs_get_video());
			obs_set_output_source(0, videoSource); //0 = VIDEO CHANNEL
			obs_output_set_video_encoder(output, videoEncoder);
			Console.WriteLine("video encoder active: " + (videoEncoder != IntPtr.Zero));

			obs_encoder_set_audio(audioEncoder, obs_get_audio());
			obs_set_output_source(1, audioSource); //1 = AUDIO CHANNEL
			obs_output_set_audio_encoder(output, audioEncoder, (UIntPtr)0);
			Console.WriteLine("audio encoder active: " + (audioEncoder != IntPtr.Zero));

			// START RECORDING
			bool outputStartSuccess = obs_output_start(output);
			Console.WriteLine("output successful: " + outputStartSuccess);
			if(outputStartSuccess != true) {
				Console.WriteLine("output error: '" + obs_output_get_last_error(output) + "'");
			}

			Console.ReadLine();
        }
    }
}
