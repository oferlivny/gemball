using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.IO;

namespace GemSDK.Unity
{
    internal class CustomNamedPipe
    {
        private string name;
        private bool isConnected;
        private bool isMessageComplete;
        private IntPtr pipe;

        public string Name { get { return name; } }

        public bool IsConnected { get { return isConnected; } }

        public bool IsMessageComplete { get { return isMessageComplete; } }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CreateFile([MarshalAs(UnmanagedType.LPTStr)] string filename, [MarshalAs(UnmanagedType.U4)] FileAccess access, [MarshalAs(UnmanagedType.U4)] FileShare share, IntPtr securityAttributes,
         [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition, int flagsAndAttributes, IntPtr templateFile);

        [DllImport("kernel32.dll")]
        static extern bool SetNamedPipeHandleState(IntPtr hNamedPipe, ref uint lpMode, IntPtr lpMaxCollectionCount, IntPtr lpCollectDataTimeout);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool ReadFile(IntPtr hFile, [Out] byte[] lpBuffer, int nNumberOfBytesToRead, out int lpNumberOfBytesRead, IntPtr lpOverlapped);

        public CustomNamedPipe(string name)
        {
            this.name = "\\\\.\\pipe\\" + name;
            isConnected = false;
        }

        public void Connect()
        {
            if (isConnected)
                throw new InvalidOperationException("The client is already connected");

            pipe = CreateFile(
                name,
                FileAccess.ReadWrite,
                FileShare.None,
                IntPtr.Zero,
                FileMode.Open,
                0,
                IntPtr.Zero);

            if (pipe == new IntPtr(-1)) // INVALID_HANDLE_VALUE
                throw new Win32Exception(Marshal.GetLastWin32Error());

            uint mode = (uint)2;
            if (!SetNamedPipeHandleState(pipe, ref mode, IntPtr.Zero, IntPtr.Zero))
            {
                int error = Marshal.GetLastWin32Error();

                if(error != 0)
                    throw new Win32Exception(error);
            }
                

            isConnected = true;
        }

        public void Close()
        {
            if (!CloseHandle(pipe))
                throw new Win32Exception(Marshal.GetLastWin32Error());

            isConnected = false;
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            if (!isConnected)
                throw new InvalidOperationException("The pipe is disconnected, waiting to connect, or the handle has not been set");

            int read = 0;

            if (!ReadFile(pipe, buffer, count, out read, IntPtr.Zero))
            {
                if (Marshal.GetLastWin32Error() == 234) //ERROR_MORE_DATA
                    isMessageComplete = false;
                else
                    throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            else
            {
                isMessageComplete = true;
            }

            return read;
        }
    }
}