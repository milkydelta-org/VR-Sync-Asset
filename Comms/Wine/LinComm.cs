// SPDX-FileCopyrightText: 2026 milkydelta
// SPDX-License-Identifier: LicenseRef-AllRightsReserved
using System;
using System.Runtime.InteropServices;

namespace TapSuite.Comms.Wine
{
    public class LinComm : AbComms
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct nativeDataBlock
        {
            public string name;
            public int length;
            public IntPtr data;
            public int fd;
        };

        [DllImport("lincomm.dll", EntryPoint = "open")]
        private static extern int nativeOpen(ref nativeDataBlock dst);

        [DllImport("lincomm.dll", EntryPoint = "close")]
        private static extern int nativeClose(ref nativeDataBlock dst);

        nativeDataBlock shm;
        public LinComm()
        {
            shm = new nativeDataBlock();
        }

        public override bool Open(string targetName)
        {
            if (isOpen) { return false; }
            targetName =targetName+ ".v1.1";

            if (shm.fd != 0 || shm.data != IntPtr.Zero) { return false; }

            shm.name = "/" + targetName;
            shm.length = (sizeof(float) * 8) + sizeof(int);
            shm.length += sizeof(int) * 2;
            shm.length += sizeof(float) * 3;

            if (nativeOpen(ref shm) == 0)
            {
                isOpen = true;
                name = targetName;
                return true;
            }
            return false;
        }

        public override void WriteCamPos(float x, float y, float z)
        {
            if (!isOpen) { return; }
            float[] a = { x, y, z };
            Marshal.Copy(a, 0, shm.data, 3);
        }

        public override void WriteCamRot(float w, float x, float y, float z)
        {
            if (!isOpen) { return; }
            float[] a = { w, x, y, z };
            Marshal.Copy(a, 0, shm.data + sizeof(float) * 3, 4);
        }

        public override void WriteCamFov(float fov)
        {
            if (!isOpen) { return; }
            float[] a = { fov };
            Marshal.Copy(a, 0, shm.data + sizeof(float) * 7, 1);
        }

        protected override void WriteSettings(int settings)
        {
            if (!isOpen) { return; }
            Marshal.WriteInt32(shm.data, sizeof(float) * 8, settings);
        }

        public override void WriteRes(int x, int y)
        {
            if (!isOpen) { return; }
            Marshal.WriteInt32(shm.data, sizeof(float) * 9, x);
            Marshal.WriteInt32(shm.data, sizeof(float) * 10, y);
        }

        public override void WriteClipPos(float x, float y, float z)
        {
            if (!isOpen) { return; }
            float[] a = { x, y, z };
            Marshal.Copy(a, 0, shm.data + sizeof(float) * 11, 3);
        }

        public override void Close()
        {
            if (!isOpen) { return; }
            isOpen = false;

            nativeClose(ref shm);
            shm = new nativeDataBlock();

            return;
        }
    }
}