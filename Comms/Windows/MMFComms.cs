// SPDX-FileCopyrightText: 2026 milkydelta
// SPDX-License-Identifier: LicenseRef-AllRightsReserved
using System;
using System.IO.MemoryMappedFiles;

using System.Runtime.InteropServices;



namespace TapSuite.Comms.Windows
{
    public class MMFComms : AbComms
    {
        protected MemoryMappedFile mmf;
        protected MemoryMappedViewAccessor mmfView;

        public override bool Open(string targetName)
        {
            if (isOpen) { return false; }

            targetName =targetName+ ".v1.1";

            int size = (sizeof(float) * 8) + sizeof(int);
            size += sizeof(int) * 2;
            size += sizeof(float) * 3;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                mmf = MemoryMappedFile.CreateOrOpen(targetName, size);
            }
            else
            {
                // For the *very* unlikely situation of running on a real Unix .NET runtime,
                // where MMF is a supported feature, but non-persisted named maps are not.
                // Why? OSX. https://github.com/dotnet/runtime/issues/30878#issuecomment-1229530332
                // Technically, that doesn't matter for unity, since we're on Mono for that case, but this gets the compiler to be quiet.

                // It would probably be a better idea to have an else-if for Linux,
                // and use a file-backed MMF under /dev/shm
                mmf = MemoryMappedFile.CreateNew(null, size);
            }


            mmfView = mmf.CreateViewAccessor(0, size, MemoryMappedFileAccess.ReadWrite);

            isOpen = true;
            name = targetName;
            return true;
        }

        public override void WriteCamPos(float x, float y, float z)
        {
            if (!isOpen) { return; }
            mmfView.Write(0                , x);
            mmfView.Write(sizeof(float)    , y);
            mmfView.Write(sizeof(float) * 2, z);
        }

        public override void WriteCamRot(float w, float x, float y, float z)
        {
            if (!isOpen) { return; }
            mmfView.Write(sizeof(float) * 3, w);
            mmfView.Write(sizeof(float) * 4, x);
            mmfView.Write(sizeof(float) * 5, y);
            mmfView.Write(sizeof(float) * 6, z);
        }

        public override void WriteCamFov(float fov)
        {
            if (!isOpen) { return; }
            mmfView.Write(sizeof(float) * 7, fov);
        }

        protected override void WriteSettings(int settings)
        {
            if (!isOpen) { return; }
            mmfView.Write(sizeof(float) * 8, settings);
        }

        public override void WriteRes(int x, int y)
        {
            if (!isOpen) { return; }
            mmfView.Write(sizeof(float) * 9, x);
            mmfView.Write(sizeof(float) * 10, y);
        }

        public override void WriteClipPos(float x, float y, float z)
        {
            if (!isOpen) { return; }
            mmfView.Write(sizeof(float) * 11, x);
            mmfView.Write(sizeof(float) * 12, y);
            mmfView.Write(sizeof(float) * 13, z);
        }

        public override void Close()
        {
            if (!isOpen) { return; }

            isOpen = false;
            mmfView.Dispose();
            mmf.Dispose();
        }
    }
}