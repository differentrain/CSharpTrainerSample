using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CETT
{
    public partial class Form1 : Form
    {


        //只适配X64平台。

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern Boolean ReadProcessMemory(HandleRef hProcess, Int64 lpBaseAddress, [In, Out] Byte[] lpBuffer, Int64 nSize, Int64 lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern Boolean WriteProcessMemory(HandleRef hProcess, Int64 lpBaseAddress, Byte[] lpBuffer, Int64 nSize, Int64 lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern Boolean VirtualProtectEx(HandleRef hProcess, Int64 lpAddress, Int64 dwSize, UInt32 flNewProtect, out UInt32 lpflOldProtect);

        [DllImport("kernel32.dll")]
        private static extern Int64 VirtualQueryEx(HandleRef hProcess, Int64 lpAddress, out MEMORY_BASIC_INFORMATION64 lpBuffer, Int64 dwLength);

        //详见 https://msdn.microsoft.com/en-us/library/windows/desktop/aa366775(v=vs.85).aspx 备注
        [StructLayout(LayoutKind.Sequential, Pack = 16)]
        private struct MEMORY_BASIC_INFORMATION64
        {
            public Int64 BaseAddress;
            public Int64 AllocationBase;
            public UInt32 AllocationProtect;
            public UInt32 __alignment1;
            public Int64 RegionSize;
            public UInt32 State;
            public UInt32 Protect;
            public UInt32 Type;
            public UInt32 __alignment2;

        }

        private const Int64 MEMORY_BASIC_INFORMATION64_SIZE = 48;

        //https://msdn.microsoft.com/zh-cn/library/windows/desktop/aa366786(v=vs.85).aspx
        private const UInt32 PAGE_EXECUTE_READ = 0x20;
        private const UInt32 PAGE_EXECUTE_READWRITE = 0x40;

        // https://msdn.microsoft.com/en-us/library/windows/desktop/aa366775(v=vs.85).aspx
        private const UInt32 MEM_COMMIT = 0x1000;

        //全局变量，表示游戏进程
        private Process GameProcess = null;

        private Byte[] ReadBytes(Int64 address, Int64 length)
        {
            var buffer = new Byte[length];
            ReadProcessMemory(new HandleRef(GameProcess, GameProcess.Handle), address, buffer, length, 0);
            return buffer;
        }

        private void WriteBytes(Int64 address, Byte[] data)
        {
            WriteProcessMemory(new HandleRef(GameProcess, GameProcess.Handle), address, data, data.LongLength, 0);
        }

        private void WriteBytes(Int64 address, Byte[] data, Int32 length)
        {
            WriteProcessMemory(new HandleRef(GameProcess, GameProcess.Handle), address, data, length, 0);
        }

        private Int64 Aobscan(Byte[] data)
        {
            var address = GameProcess.MainModule.BaseAddress.ToInt64();
            var stopAddress = address + GameProcess.MainModule.ModuleMemorySize;
            var handle = new HandleRef(GameProcess, GameProcess.Handle);
            while (address < stopAddress)
            {
                //遍历内存页信息
                var infoLength = VirtualQueryEx(handle, address, out var memInfo, MEMORY_BASIC_INFORMATION64_SIZE);
                if (infoLength == 0)
                {
                    return 0;
                }
                //判断页面信息，如果State不是MEM_COMMIT，或Protect属性不是PAGE_EXECUTE_READ，则忽略
                //需要注意的是，这里我只比较了PAGE_EXECUTE_READ，实际上，如果写成通用的类，应该判断是否存在PAGE_GUARD位，这样才适用多种情况，具体见MSDN。
                if ((memInfo.State & MEM_COMMIT) != 0 && memInfo.Protect == PAGE_EXECUTE_READ)
                {
                    //读取整个内存页
                    var buffer = ReadBytes(memInfo.BaseAddress, memInfo.RegionSize);
                    var index = QSIndexOf(buffer, data); //查找，
                    if (index != -1)
                    {
                        //找到则修改Protect属性。这里只修改2字节即可。
                        var currentAddress = memInfo.BaseAddress + index;
                        VirtualProtectEx(handle, currentAddress, 2, PAGE_EXECUTE_READWRITE, out _);
                        return currentAddress;
                    }
                }
                address = memInfo.BaseAddress + memInfo.RegionSize;
            }
            return 0;
        }

        #region Sunday Quick-Search算法的C#实现

        private static Int32[] FlagBuffer = new Int32[256];

        private static Int32 QSIndexOf(Byte[] source, Byte[] pattern)
        {

            if (source.Length < pattern.Length)
            {
                return -1;
            }

            var sLength = source.Length;
            var pLength = pattern.Length;
            var pMaxIndex = pLength - 1;
            var startIndex = 0;
            var endPos = sLength - pLength;
            var badMov = pLength + 1;

            for (Int32 i = 0; i < 256; i++)
            {
                FlagBuffer[i] = badMov;

            }
            for (int i = 0; i <= pMaxIndex; i++)
            {
                FlagBuffer[pattern[i]] = pLength - i;
            }

            Int32 pIndex, step, result = -1;

            while (startIndex <= endPos)
            {
                for (pIndex = 0; pIndex <= pMaxIndex && source[startIndex + pIndex] == pattern[pIndex]; pIndex++)
                {
                    if (pIndex == pMaxIndex)
                    {
                        result = startIndex;
                    }
                }
                if (result > -1) break;
                step = startIndex + pLength;
                if (step >= sLength) break;
                startIndex += FlagBuffer[source[step]];
            }
            return result;
        }

        #endregion

        /* 
        Tutorial-x86_64.exe+2B423 - 75 38                 - jne Tutorial-x86_64.exe+2B45D
        Tutorial-x86_64.exe+2B425 - 48 8B 8B 60070000     - mov rcx,[rbx+00000760]
        */
        private static readonly Byte[] AsmArray = new Byte[] { 0x75, 0x38, 0x48, 0x8B, 0x8B, 0x60, 0x07, 0x00, 0x00 };

        private static readonly Byte[] NOPCode = new Byte[] { 0x90, 0x90 };

        private Int64 AsmAddress = 0;



        public Form1()
        {
            InitializeComponent();
        }



        //TimerListener.Tick事件
        private void TimerListener_Tick(object sender, EventArgs e)
        {
            if (GameProcess == null)
            {
                var processes = Process.GetProcessesByName("Tutorial-x86_64");
                if (processes.Length > 0)
                {

                    Thread.Sleep(1000);//等待进程加载完毕，1S足够了
                    foreach (var process in processes)
                    {
                        try
                        {
                            //一旦捕获过，就不再尝试了，直接释放
                            if (GameProcess == null && process.MainWindowTitle == "Tutorial-x86_64")
                            {
                                GameProcess = process;
                                AsmAddress = Aobscan(AsmArray);
                                if (AsmAddress == 0) //查找失败，说明不是正确的进程或不支持修改
                                {
                                    GameProcess = null;
                                    process.Dispose();
                                }
                                else
                                {
                                    //绑定退出事件
                                    GameProcess.EnableRaisingEvents = true;
                                    GameProcess.Exited += GameProcess_Exited;
                                    //激活Checkbox
                                    CheckBoxPassBy.Enabled = true;

                                }

                            }
                            else
                            {
                                process.Dispose();
                            }
                        }
                        catch
                        {
                            process.Dispose();
                            GameProcess = null;
                        }

                    }
                }
            }
        }

        private delegate void DelegateGameClose();

        private void GameClose()
        {
            CheckBoxPassBy.Enabled = false;
            CheckBoxPassBy.Checked = false;
        }
        private void GameProcess_Exited(object sender, EventArgs e)
        {
            this.Invoke(new DelegateGameClose(GameClose));
            if (GameProcess != null)
            {
                GameProcess.Exited -= GameProcess_Exited;
                GameProcess.Dispose();
                GameProcess = null;
            }
        }

        private void CheckBoxPassBy_CheckedChanged(object sender, EventArgs e)
        {
            //如果游戏已退出就不修改
            if (CheckBoxPassBy.Enabled == false) return;

            if (CheckBoxPassBy.Checked) 
            {
                WriteBytes(AsmAddress, NOPCode);
            }
            else
            {
                WriteBytes(AsmAddress, AsmArray, 2);
            }

        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (GameProcess != null && CheckBoxPassBy.Checked == true)
            {
                CheckBoxPassBy.Checked = false;
                GameProcess.Dispose();
            }
        }



    }



}
