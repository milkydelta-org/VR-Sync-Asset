// SPDX-FileCopyrightText: 2026 milkydelta
// SPDX-License-Identifier: LicenseRef-AllRightsReserved
using System;
using System.IO;
using System.IO.MemoryMappedFiles;

// 2026-07-28 19:29 - 2026-07-29 02:08
// I am not the biggest fan of this class.
// While the shm_get function used in lincomm is a standard POSIX feature,
// /dev/shm is an Linux-specific implementation detail. It would be unlikely,
// but the implementation could change in the future, which would break this.
// This also requires the user to have / mounted as their Z drive. That is the
// default, and almost nobody ever changes it, but it is not guaranteed.

// 2026-09-13 13:22
// Wait. It's worse. /dev/shm is apparently a **glibc** implementation detail.

namespace TapSuite.Comms.Wine
{
    public class ZDevShmMMFComms : Windows.MMFComms
    {
        public override bool Open(string targetName)
        {
            if (isOpen) { return false; }
            targetName =targetName+ ".v1.1";

            int size = (sizeof(float) * 8) + sizeof(int);
            size += sizeof(int) * 2;
            size += sizeof(float) * 3;

            string pathName = "Z:\\dev\\shm\\" + targetName;

            if (!File.Exists(pathName))
            {
                using (var f = File.Create(pathName))
                {
                    byte[] b = new byte[size];
                    f.Write(b);
                }
            }

            mmf = MemoryMappedFile.CreateFromFile(pathName, System.IO.FileMode.Open, targetName, size, MemoryMappedFileAccess.ReadWrite);

            mmfView = mmf.CreateViewAccessor(0, size, MemoryMappedFileAccess.ReadWrite);

            isOpen = true;
            name = targetName;
            return true;
        }
    }
}