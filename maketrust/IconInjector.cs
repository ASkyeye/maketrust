using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using static maketrust.InjectorDependencies;

namespace maketrust
{
    class IconInjector
    {
        private static bool  EnumRes(
          IntPtr hModule,
          IntPtr lpszType,
          IntPtr lpszName,
          IntPtr lParam)
        {
            groupName = (uint)lpszName;
            return true;
        }

        private static uint groupName;

        /// <summary>
        /// Take icon from one .exe and put in another. This is done the dirty way and not the proper way
        /// </summary>
        public static void Inject(string from, string to)
        {
            IntPtr hSourceFile = LoadLibraryEx(from.NormalizePath(), IntPtr.Zero, LOAD_LIBRARY_AS_DATAFILE);//(from, IntPtr.Zero, RESOURCE_ONLY);
            Debug.Assert(hSourceFile != IntPtr.Zero, new Win32Exception(Marshal.GetLastWin32Error()).Message);

            if (!EnumResourceNamesWithID(hSourceFile, RT_GROUP_ICON, new EnumResNameDelegate(EnumRes), IntPtr.Zero))
                return;

            List<Tuple<IntPtr, uint>> resources = new List<Tuple<IntPtr, uint>>();
            IntPtr groupIcon = IntPtr.Zero;

            IntPtr hGroupIcon = FindResource(hSourceFile, groupName, RT_GROUP_ICON);
            Debug.Assert( hGroupIcon !=  IntPtr.Zero, new Win32Exception(Marshal.GetLastWin32Error()).Message);

            uint hGroupSize = SizeofResource(hSourceFile, hGroupIcon);
            Debug.Assert(hGroupSize != 0u, new Win32Exception(Marshal.GetLastWin32Error()).Message);

            IntPtr hGroupRes = LoadResource(hSourceFile, hGroupIcon);
            Debug.Assert(hGroupRes  != IntPtr.Zero, new Win32Exception(Marshal.GetLastWin32Error()).Message);

            IntPtr hGroupLock = LockResource(hGroupRes);
            Debug.Assert(hGroupLock != IntPtr.Zero, new Win32Exception(Marshal.GetLastWin32Error()).Message);


            for (uint i = 1; i < 100; i++)
            {
                IntPtr hResource = FindResource(hSourceFile, i , RT_ICON);
               // Debug.Assert(hResource != IntPtr.Zero, new Win32Exception(Marshal.GetLastWin32Error()).Message);
                if (hResource == IntPtr.Zero) break;

                uint sizeOfResource = SizeofResource(hSourceFile, hResource);
                Debug.Assert(sizeOfResource != 0, new Win32Exception(Marshal.GetLastWin32Error()).Message);

                IntPtr hGlob = LoadResource(hSourceFile, hResource);
                Debug.Assert(hGlob != IntPtr.Zero, new Win32Exception(Marshal.GetLastWin32Error()).Message);

                IntPtr hLock = LockResource(hGlob);
                Debug.Assert(hLock != IntPtr.Zero, new Win32Exception(Marshal.GetLastWin32Error()).Message);

                resources.Add(new Tuple<IntPtr, uint>(hLock, sizeOfResource));
            }

            IntPtr hUpdate = BeginUpdateResource(to.NormalizePath(), false);
            Debug.Assert(hUpdate != IntPtr.Zero, new Win32Exception(Marshal.GetLastWin32Error()).Message);

            UpdateResource(hUpdate, RT_GROUP_ICON, groupName, ENGLISH_USA, hGroupLock, hGroupSize);
            FreeResource(hGroupLock);
            FreeResource(hGroupRes );

            for (int i = 0; i < resources.Count; i++)
            {
                UpdateResource(hUpdate, RT_ICON, (uint)i, ENGLISH_USA, resources[i].Item1, resources[i].Item2);
                FreeResource(resources[i].Item1);
            }

            EndUpdateResource(hUpdate, false);
            FreeLibrary(hSourceFile);
        }
    }
}
