using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace CSharp.NFC
{
    public class WindowsLogger : INFCLogger
    {
        private string _folderName = "NFCTicketValidatorLogs";
        private string _logFileName = "NFCTicketValidatorLog.txt";
        private StorageFolder _folder;
        private StorageFile _logFile;

        // I will manage big log files later
        public WindowsLogger(string filePath)
        {
            EnsureFile();
        }

        public WindowsLogger() : this("NFCLog.txt") { }

        private async void EnsureFile()
        {
            try
            {
                try
                {
                    _folder = await ApplicationData.Current.LocalFolder.GetFolderAsync(_folderName);
                }
                catch (FileNotFoundException)
                {
                    _folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(_folderName);
                }
                try
                {
                    _logFile = await _folder.GetFileAsync(_logFileName);
                }
                catch (FileNotFoundException)
                {
                    _logFile = await _folder.CreateFileAsync(_logFileName);
                }
            }            
            catch (Exception)
            {
                return;
            }
        }

        public async void Log(string message)
        {
            if(_logFile != null)
            {
                await FileIO.AppendTextAsync(_logFile, $"[{DateTime.Now.ToString("G")}] Information: {message}{Environment.NewLine}");
            }            
        }

        public async void ManageException(Exception ex)
        {
            await FileIO.AppendTextAsync(_logFile, $"[{DateTime.Now.ToString("G")}] Exception: {ex.Message}{Environment.NewLine}");
        }
    }
}
