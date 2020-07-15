using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace lucidcode.LucidScribe.Plugin.Halovision.VLC
{
    // This code is a slight refactoring/extension of:
    // http://www.helyar.net/2009/libvlc-media-player-in-c/
    // http://www.helyar.net/2009/libvlc-media-player-in-c-part-2/

    internal static class LibVlc
    {
        #region core
        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libvlc_new(int argc, [MarshalAs(UnmanagedType.LPArray,
          ArraySubType = UnmanagedType.LPStr)] string[] argv);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_release(IntPtr instance);
        #endregion

        #region media
        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libvlc_media_new_path(IntPtr p_instance, [MarshalAs(UnmanagedType.LPStr)] string psz_mrl);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libvlc_media_new_location(IntPtr p_instance, [MarshalAs(UnmanagedType.LPStr)] string psz_mrl);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_media_add_option(IntPtr libvlc_media_inst, [MarshalAs(UnmanagedType.LPArray)] byte[] ppsz_options);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_media_release(IntPtr p_meta_desc);
        #endregion

        #region media player
        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libvlc_media_player_new_from_media(IntPtr media);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_media_player_release(IntPtr player);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_media_player_set_hwnd(IntPtr player, IntPtr drawable);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libvlc_media_player_get_media(IntPtr player);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_media_player_set_media(IntPtr player, IntPtr media);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern int libvlc_media_player_play(IntPtr player);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_media_player_pause(IntPtr player);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_media_player_stop(IntPtr player);
        #endregion

        #region exception
        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_clearerr();

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libvlc_errmsg();
        #endregion
    }

    internal class VlcException : Exception
    {
        private string _err;

        public VlcException()
            : base()
        {
            IntPtr errorPointer = LibVlc.libvlc_errmsg();
            _err = errorPointer == IntPtr.Zero ? "VLC Exception"
                : Marshal.PtrToStringAuto(errorPointer);
        }

        public override string Message { get { return _err; } }
    }

    internal class VlcInstance : IDisposable
    {
        public IntPtr InstanceHandle { get; private set; }

        public VlcInstance(string pathToVlc)
        {
            if (string.IsNullOrEmpty(pathToVlc))
            {
                pathToVlc = @"C:\Program Files (x86)\VideoLAN\VLC\";
            }

            pathToVlc = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) + @"\VideoLAN\VLC\";
            if (!Directory.Exists(pathToVlc))
            {
                pathToVlc = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + @"\VideoLAN\VLC\";
            }
            if (!Directory.Exists(pathToVlc))
            {
                pathToVlc = @"C:\Program Files (x64)\VideoLAN\VLC\";
            }
            if (!Directory.Exists(pathToVlc))
            {
                pathToVlc = @"C:\Program Files\VideoLAN\VLC\";
            }

            string aCurrentDirectory = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(pathToVlc);

            try
            {
                InstanceHandle = LibVlc.libvlc_new(0, null);
                if (InstanceHandle == IntPtr.Zero) throw new VlcException();
            }
            finally
            {
                Directory.SetCurrentDirectory(aCurrentDirectory);
            }
        }

        public void Dispose()
        {
            LibVlc.libvlc_release(InstanceHandle);
        }
    }

    internal class VlcMedia : IDisposable
    {
        public IntPtr MediaHandle { get; private set; }

        public VlcMedia(VlcInstance instance, string url)
        {
            if (File.Exists(url))
            {
                MediaHandle = LibVlc.libvlc_media_new_path(instance.InstanceHandle, url);
            }
            else
            {
                MediaHandle = LibVlc.libvlc_media_new_location(instance.InstanceHandle, url);
            }
            if (MediaHandle == IntPtr.Zero) throw new VlcException();
        }

        public VlcMedia(IntPtr handle)
        {
            this.MediaHandle = handle;
        }

        public void Dispose()
        {
            LibVlc.libvlc_media_release(MediaHandle);
        }

        public void AddOption(string option)
        {
            LibVlc.libvlc_media_add_option(MediaHandle, Encoding.UTF8.GetBytes(option));
        }
    }

    internal class VlcMediaPlayer : IDisposable
    {
        private IntPtr drawable;
        private bool playing, paused;

        public IntPtr MediaPlayerHandle { get; private set; }

        public VlcMediaPlayer(VlcMedia media)
        {
            MediaPlayerHandle = LibVlc.libvlc_media_player_new_from_media(media.MediaHandle);
            if (MediaPlayerHandle == IntPtr.Zero) throw new VlcException();
        }

        public void Dispose()
        {
            LibVlc.libvlc_media_player_release(MediaPlayerHandle);
        }

        public IntPtr Drawable
        {
            get
            {
                return drawable;
            }
            set
            {
                LibVlc.libvlc_media_player_set_hwnd(MediaPlayerHandle, value);
                drawable = value;
            }
        }

        public VlcMedia Media
        {
            get
            {
                IntPtr media = LibVlc.libvlc_media_player_get_media(MediaPlayerHandle);
                if (media == IntPtr.Zero) return null;
                return new VlcMedia(media);
            }
            set
            {
                LibVlc.libvlc_media_player_set_media(MediaPlayerHandle, value.MediaHandle);
            }
        }

        public bool IsPlaying { get { return playing && !paused; } }

        public bool IsPaused { get { return playing && paused; } }

        public bool IsStopped { get { return !playing; } }

        public void Play()
        {
            int ret = LibVlc.libvlc_media_player_play(MediaPlayerHandle);
            if (ret == -1)
                throw new VlcException();

            playing = true;
            paused = false;
        }

        public void Pause()
        {
            LibVlc.libvlc_media_player_pause(MediaPlayerHandle);

            if (playing)
                paused ^= true;
        }

        public void Stop()
        {
            LibVlc.libvlc_media_player_stop(MediaPlayerHandle);

            playing = false;
            paused = false;
        }
    }
}
