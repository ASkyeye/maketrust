using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using static maketrust.InjectorDependencies;

namespace maketrust
{
    class DescriptionInjector
    {
        /// <summary>
        /// Read version info from a file and inject it into another
        /// </summary>
        public static void Inject(string from, string to)
        {
            IntPtr hSourceFile = LoadLibraryEx (from.NormalizePath(), IntPtr.Zero, LOAD_LIBRARY_AS_DATAFILE);//(from, IntPtr.Zero, RESOURCE_ONLY);
            Debug.Assert(hSourceFile != IntPtr.Zero, new Win32Exception(Marshal.GetLastWin32Error()).Message);

            IntPtr hVersionInfo = FindResource(hSourceFile, 1, RT_VERSION);
            Debug.Assert(hVersionInfo != IntPtr.Zero, new Win32Exception(Marshal.GetLastWin32Error()).Message);

            uint sizeOfResource = SizeofResource(hSourceFile, hVersionInfo);
            Debug.Assert(sizeOfResource != 0, new Win32Exception(Marshal.GetLastWin32Error()).Message);

            IntPtr hGlob = LoadResource(hSourceFile, hVersionInfo);
            Debug.Assert(hGlob != IntPtr.Zero, new Win32Exception(Marshal.GetLastWin32Error()).Message);

            IntPtr hLock = LockResource(hGlob);
            Debug.Assert(hLock != IntPtr.Zero, new Win32Exception(Marshal.GetLastWin32Error()).Message);

            IntPtr hUpdate = BeginUpdateResource(to.NormalizePath(), false);
            Debug.Assert(hUpdate != IntPtr.Zero, new Win32Exception(Marshal.GetLastWin32Error()).Message);

            UpdateResource(hUpdate, RT_VERSION, 1, ENGLISH_USA, hLock, sizeOfResource);
            EndUpdateResource(hUpdate, false);

            FreeResource(hGlob);
            FreeLibrary(hSourceFile);
        }
    }
}