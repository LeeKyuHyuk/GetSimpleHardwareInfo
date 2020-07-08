using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Management;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;

namespace GetSimpleHardwareInfo
{
    class Program
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);
        static void Main(string[] args)
        {
            Wmi wmi = new Wmi();
            wmi.GetComputerSystemInfo();
            wmi.GetOperatingSystemInfo();
            wmi.GetSecureBoot();
            wmi.GetModernStanby();
            wmi.GetProcessorInfo();
            wmi.GetMemoryInfo();
            wmi.GetBiosInfo();
            wmi.GetVideoInfo();
            wmi.GetDiskInfo();
            wmi.GetPartitionInfo();
            GenerateWallpaper generateWallpaper = new GenerateWallpaper(wmi);
            generateWallpaper.Run();
            SystemParametersInfo(0x0014, 0, generateWallpaper.GetFileName(), 0x0001);
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
    }

    /* Windows Management Instrumentation */
    class Wmi
    {
        public string computerVendor, computerVersion, computerName;
        public string osCaption, osArchitecture, osVersion;
        public string secureBoot, modernStanby;
        public string cpuName, cpuNumberOfCores, cpuNumberOfLogicalProcessors;
        public string memoryCapacity;
        public string biosVersion, ecMajor, ecMinor;
        public List<string> videoName = new List<string>(), videoDriverVersion = new List<string>();
        public List<string> diskModel = new List<string>(), diskFirmware = new List<string>();
        public List<string> partitionId = new List<string>(), partitionSize = new List<string>(), partitionFileSystem = new List<string>();
        public string GetSecureBoot()
        {
            int result = 0;
            string key = @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\SecureBoot\State";
            string subkey = @"UEFISecureBootEnabled";
            try
            {
                object value = Registry.GetValue(key, subkey, result);
                if (value != null)
                    result = (int)value;
            }
            catch (Exception e)
            {
                return e.Message;
            }
            if (result >= 1)
                secureBoot = "Secure Boot : On";
            else
                secureBoot = "Secure Boot : Off";
            Console.WriteLine(secureBoot);
            return secureBoot;
        }
        public string GetModernStanby()
        {
            int result = 0;
            string key = @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\ModernSleep";
            string subkey = @"EnabledActions";
            try
            {
                object value = Registry.GetValue(key, subkey, result);
                if (value != null)
                    result = (int)value;
            }
            catch (Exception e)
            {
                return e.Message;
            }
            if (result == 7)
                modernStanby = "ModernStanby : On";
            else
                modernStanby = "ModernStanby : Off";
            Console.WriteLine(modernStanby);
            return modernStanby;
        }
        public void GetComputerSystemInfo()
        {
            ManagementObjectSearcher objects = new ManagementObjectSearcher("SELECT Vendor,Version,Name FROM Win32_ComputerSystemProduct");
            ManagementObjectCollection coll = objects.Get();
            foreach (ManagementObject obj in coll)
            {
                if (obj["Vendor"] != null)
                    computerVendor = obj["Vendor"].ToString();
                if (obj["Version"] != null)
                    computerVersion = obj["Version"].ToString();
                if (obj["Name"] != null)
                    computerName = obj["Name"].ToString();
            }
            Console.WriteLine(computerVendor + " - " + computerVersion + " (" + computerName + ")");
        }
        public void GetOperatingSystemInfo()
        {
            ManagementObjectSearcher objects = new ManagementObjectSearcher("SELECT Caption,OSArchitecture,Version FROM Win32_OperatingSystem");
            ManagementObjectCollection coll = objects.Get();
            foreach (ManagementObject obj in coll)
            {
                if (obj["Caption"] != null)
                    osCaption = obj["Caption"].ToString();
                if (obj["OSArchitecture"] != null)
                    osArchitecture = obj["OSArchitecture"].ToString();
                if (obj["Version"] != null)
                    osVersion = obj["Version"].ToString();
            }
            Console.WriteLine(osCaption + " " + osArchitecture + ", " + osVersion);
        }
        public void GetProcessorInfo()
        {
            ManagementObjectSearcher objects = new ManagementObjectSearcher("SELECT Name,NumberOfCores,NumberOfLogicalProcessors FROM Win32_Processor");
            ManagementObjectCollection coll = objects.Get();
            Console.WriteLine("[+] Platform Information");
            foreach (ManagementObject obj in coll)
            {
                if (obj["Name"] != null)
                    cpuName = obj["Name"].ToString();
                if (obj["NumberOfCores"] != null)
                    cpuNumberOfCores = obj["NumberOfCores"].ToString();
                if (obj["NumberOfLogicalProcessors"] != null)
                    cpuNumberOfLogicalProcessors = obj["NumberOfLogicalProcessors"].ToString();
            }
            Console.WriteLine("   - CPU : " + cpuName);
            Console.WriteLine("   - Physical Core : " + cpuNumberOfCores);
            Console.WriteLine("   - Logicial Core : " + cpuNumberOfLogicalProcessors);
        }
        public void GetMemoryInfo()
        {
            UInt64 capacity = 0;
            ManagementObjectSearcher objects = new ManagementObjectSearcher("SELECT Capacity FROM Win32_PhysicalMemory");
            ManagementObjectCollection coll = objects.Get();
            foreach (ManagementObject obj in coll)
            {
                if (obj["Capacity"] != null)
                    capacity += (UInt64)obj["Capacity"] / 1048576;
            }
            memoryCapacity = capacity / 1024 + "GB";
            Console.WriteLine("   - Memory : " + memoryCapacity);
        }
        public void GetBiosInfo()
        {
            string biosDate = "";
            string key = @"HKEY_LOCAL_MACHINE\HARDWARE\DESCRIPTION\System\BIOS";
            string subkey = @"BIOSReleaseDate";
            try
            {
                object value = Registry.GetValue(key, subkey, biosDate);
                if (value != null)
                    biosDate = (string)value;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            ManagementObjectSearcher objects = new ManagementObjectSearcher("SELECT Version,EmbeddedControllerMajorVersion,EmbeddedControllerMinorVersion FROM Win32_BIOS");
            ManagementObjectCollection coll = objects.Get();
            foreach (ManagementObject obj in coll)
            {
                if (obj["Version"] != null)
                    biosVersion = obj["Version"].ToString() + ", " + biosDate;
                if (obj["EmbeddedControllerMajorVersion"] != null)
                    ecMajor = obj["EmbeddedControllerMajorVersion"].ToString();
                if (obj["EmbeddedControllerMinorVersion"] != null)
                    ecMinor = obj["EmbeddedControllerMinorVersion"].ToString();
            }
            Console.WriteLine("   - BIOS : " + biosVersion);
            Console.WriteLine("   - Embedded Controller Version : " + ecMajor + "." + ecMinor);
        }
        public void GetVideoInfo()
        {
            ManagementObjectSearcher objects = new ManagementObjectSearcher("SELECT VideoProcessor,DriverVersion FROM Win32_VideoController");
            ManagementObjectCollection coll = objects.Get();
            foreach (ManagementObject obj in coll)
            {
                if (obj["VideoProcessor"] != null)
                    videoName.Add(obj["VideoProcessor"].ToString());
                if (obj["DriverVersion"] != null)
                    videoDriverVersion.Add(obj["DriverVersion"].ToString());
            }
            Console.WriteLine("   - Video Name : " + videoName[0]);
            for (int index = 1; index < videoName.Count; index++)
                Console.WriteLine("\t\t" + videoName[index]);
            Console.WriteLine("   - Video Driver Version : " + videoDriverVersion[0]);
            for (int index = 1; index < videoDriverVersion.Count; index++)
                Console.WriteLine("\t\t" + videoDriverVersion[index]);
        }
        public void GetDiskInfo()
        {
            ManagementObjectSearcher objects = new ManagementObjectSearcher("SELECT Model,FirmwareRevision FROM Win32_DiskDrive");
            ManagementObjectCollection coll = objects.Get();
            foreach (ManagementObject obj in coll)
            {
                if (obj["Model"] != null)
                    diskModel.Add(obj["Model"].ToString());
                if (obj["FirmwareRevision"] != null)
                    diskFirmware.Add(obj["FirmwareRevision"].ToString());
            }
            Console.WriteLine("[+] Device Information");
            Console.WriteLine("   - Device Name : " + diskModel[0]);
            for (int index = 1; index < diskModel.Count; index++)
                Console.WriteLine("\t\t" + diskModel[index]);
            Console.WriteLine("   - Device FW : " + diskFirmware[0]);
            for (int index = 1; index < diskFirmware.Count; index++)
                Console.WriteLine("\t\t" + diskFirmware[index]);
        }
        public void GetPartitionInfo()
        {
            ManagementObjectSearcher objects = new ManagementObjectSearcher("SELECT DeviceID,Size,FileSystem FROM Win32_LogicalDisk");
            ManagementObjectCollection coll = objects.Get();
            foreach (ManagementObject obj in coll)
            {
                if (obj["DeviceID"] != null)
                    partitionId.Add(obj["DeviceID"].ToString());
                if (obj["Size"] != null)
                    partitionSize.Add(obj["Size"].ToString());
                if (obj["FileSystem"] != null)
                    partitionFileSystem.Add(obj["FileSystem"].ToString());
            }
            Console.WriteLine("   - Device Density : " + partitionId[0] + "\\ " + Math.Round((Convert.ToDouble(partitionSize[0])/1024/1024/1024), 3) + "GB " + partitionFileSystem[0]);
            for (int index = 1; index < partitionSize.Count; index++)
                Console.WriteLine("\t\t" + partitionId[index] + "\\ " + Math.Round((Convert.ToDouble(partitionSize[index]) / 1024 / 1024 / 1024), 3) + "GB " + partitionFileSystem[index]);
        }
    }

    class GenerateWallpaper
    {
        private Wmi wmi;
        public GenerateWallpaper(Wmi wmi)
        {
            this.wmi = wmi;
        }
        public string GetFileName()
        {
            return Path.Combine(Path.GetTempPath(), "GetSimpleHardwareInfo.bmp");
        }
        public void Run()
        {
            Rectangle resolution = Screen.PrimaryScreen.Bounds;
            int xPosition = resolution.Width - 700, yPosition = 20;
            Bitmap bitmap = new Bitmap(resolution.Width, resolution.Height);
            Graphics graphics = Graphics.FromImage(bitmap);
            graphics.Clear(Color.Black);
            DrawString(graphics, wmi.computerVendor + " - " + wmi.computerVersion + " (" + wmi.computerName + ")", 30, FontStyle.Bold, Color.Yellow, xPosition, yPosition, 40, out yPosition);
            DrawString(graphics, wmi.osCaption + " " + wmi.osArchitecture + ", " + wmi.osVersion, 20, FontStyle.Regular, Color.White, xPosition, yPosition, 30, out yPosition);
            DrawString(graphics, wmi.secureBoot, 20, FontStyle.Regular, Color.White, xPosition, yPosition, 30, out yPosition);
            DrawString(graphics, wmi.modernStanby, 20, FontStyle.Regular, Color.White, xPosition, yPosition, 30, out yPosition);
            DrawString(graphics, "Platform Information", 25, FontStyle.Regular, Color.YellowGreen, xPosition, yPosition, 40, out yPosition);
            DrawString(graphics, "CPU : \t" + wmi.cpuName, 20, FontStyle.Regular, Color.White, xPosition, yPosition, 30, out yPosition);
            DrawString(graphics, "Physical Core : \t" + wmi.cpuNumberOfCores, 20, FontStyle.Regular, Color.White, xPosition, yPosition, 30, out yPosition);
            DrawString(graphics, "Logicial Core : \t" + wmi.cpuNumberOfLogicalProcessors, 20, FontStyle.Regular, Color.White, xPosition, yPosition, 30, out yPosition);
            DrawString(graphics, "Memory : \t" + wmi.memoryCapacity, 20, FontStyle.Regular, Color.White, xPosition, yPosition, 30, out yPosition);
            DrawString(graphics, "BIOS : \t" + wmi.biosVersion, 20, FontStyle.Regular, Color.White, xPosition, yPosition, 30, out yPosition);
            DrawString(graphics, "Embedded Controller Version : \t" + wmi.ecMajor + "." + wmi.ecMinor, 20, FontStyle.Regular, Color.White, xPosition, yPosition, 30, out yPosition);
            DrawString(graphics, "Video Name : \t" + wmi.videoName[0], 20, FontStyle.Regular, Color.White, xPosition, yPosition, 30, out yPosition);
            for (int index = 1; index < wmi.videoName.Count; index++)
                DrawString(graphics, "\t\t" + wmi.videoName[index], 20, FontStyle.Regular, Color.White, xPosition, yPosition, 30, out yPosition);
            DrawString(graphics, "Video Driver Version : \t" + wmi.videoDriverVersion[0], 20, FontStyle.Regular, Color.White, xPosition, yPosition, 30, out yPosition);
            for (int index = 1; index < wmi.videoDriverVersion.Count; index++)
                DrawString(graphics, "\t\t" + wmi.videoDriverVersion[index], 20, FontStyle.Regular, Color.White, xPosition, yPosition, 30, out yPosition);
            DrawString(graphics, "Device Information", 25, FontStyle.Regular, Color.YellowGreen, xPosition, yPosition, 40, out yPosition);
            DrawString(graphics, "Device Name : \t" + wmi.diskModel[0], 20, FontStyle.Regular, Color.White, xPosition, yPosition, 30, out yPosition);
            for (int index = 1; index < wmi.diskModel.Count; index++)
                DrawString(graphics, "\t\t" + wmi.diskModel[index], 20, FontStyle.Regular, Color.White, xPosition, yPosition, 30, out yPosition);
            DrawString(graphics, "Device FW : \t" + wmi.diskFirmware[0], 20, FontStyle.Regular, Color.White, xPosition, yPosition, 30, out yPosition);
            for (int index = 1; index < wmi.diskFirmware.Count; index++)
                DrawString(graphics, "\t\t" + wmi.diskFirmware[index], 20, FontStyle.Regular, Color.White, xPosition, yPosition, 30, out yPosition);
            DrawString(graphics, "Device Density : \t" + wmi.partitionId[0] + "\\ " + Math.Round((Convert.ToDouble(wmi.partitionSize[0]) / 1024 / 1024 / 1024), 3) + "GB " + wmi.partitionFileSystem[0], 20, FontStyle.Regular, Color.White, xPosition, yPosition, 30, out yPosition);
            for (int index = 1; index < wmi.partitionSize.Count; index++)
                DrawString(graphics, "\t\t" + wmi.partitionId[index] + "\\ " + Math.Round((Convert.ToDouble(wmi.partitionSize[index]) / 1024 / 1024 / 1024), 3) + "GB " + wmi.partitionFileSystem[index], 20, FontStyle.Regular, Color.White, xPosition, yPosition, 30, out yPosition);
            graphics.DrawImage(bitmap, new Point(10, 10));
            bitmap.Save(GetFileName());
        }
        private void DrawString(Graphics graphics, string text, int size, FontStyle fontStyle, Color fontColor, int xPosition, int yPosition, int yMargin, out int outYPosition)
        {
            graphics.DrawString(text, new Font(new FontFamily("Tahoma"), size, fontStyle, GraphicsUnit.Pixel), new SolidBrush(fontColor), new PointF(xPosition, yPosition));
            outYPosition = yPosition + yMargin;
        }
    }
}
