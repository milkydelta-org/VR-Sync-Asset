// SPDX-FileCopyrightText: 2026 milkydelta
// SPDX-License-Identifier: LicenseRef-AllRightsReserved
using System;
using System.Runtime.InteropServices;

// Comms classes are based primarily on the writing classes from Static-OAT, with some help from the main OAT reading classes.
namespace TapSuite.Comms
{
    [Flags]
    public enum LIVnyan_cfg : int
    {
        None = 0b0000_0000,
        CAM_ON = 0b0000_0001,
        LOG_ON = 0b0000_0010,
        LOGSPM = 0b0000_0100,
        OAT_READCLIP = 0b0000_1000
    }

    enum Platform
    {
        Wine,
        Windows,
        Linux,
        OtherUnix
    }

    public abstract class AbComms
    {
        internal bool isOpen = false;
        public string name { get; protected set; }
        internal LIVnyan_cfg set = LIVnyan_cfg.None;

        abstract public bool Open(string targetName);
        abstract public void Close();
        abstract public void WriteCamPos(float x, float y, float z);
        abstract public void WriteCamRot(float w, float x, float y, float z);
        abstract public void WriteCamFov(float fov);
        abstract protected void WriteSettings(int settings);
        abstract public void WriteRes(int x, int y);
        abstract public void WriteClipPos(float x, float y, float z);


        [DllImport("ntdll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr wine_get_version();


        private static Platform GetPlatform()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                try
                {
                    wine_get_version();
                    return Platform.Wine;
                }
                catch (EntryPointNotFoundException)
                {
                    return Platform.Windows;
                }
                catch
                {
                    UnityEngine.Debug.Log("TapSuite: Something very odd has happened.");
                }
            } else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return Platform.Linux;
            }
            return Platform.OtherUnix;
        }

        public static AbComms New()
        {
            if (GetPlatform() == Platform.Wine)
            {
                return new Wine.ZDevShmMMFComms();
            }
            return new Windows.MMFComms();
        }

        public void WriteCamPos(UnityEngine.Vector3 vec)
        {
            WriteCamPos(vec.x, vec.y, vec.z);
        }
        public void WriteCamRot(UnityEngine.Quaternion quat)
        {
            WriteCamRot(quat.w, quat.x, quat.y, quat.z);
        }
        public void WriteClipPos(UnityEngine.Vector3 vec)
        {
            WriteClipPos(vec.x, vec.y, vec.z);
        }

        public void WriteCfg(LIVnyan_cfg settings)
        {
            set = settings;
            WriteSettings((int)settings);
        }

        public bool SetCfg(LIVnyan_cfg setting, bool value)
        {
            if (value == false)
            {
                WriteCfg(set & ~setting);
                return false;
            }
            else
            {
                WriteCfg(set | setting);
                return true;
            }
        }
    }
}