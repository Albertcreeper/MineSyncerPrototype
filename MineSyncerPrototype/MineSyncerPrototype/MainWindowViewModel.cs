using Prism.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace MineSyncerPrototype
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private string _LocalPath = @"C:\Users\asmidt\Documents\Minecraft\Local";
        private string _RemotePath = @"C:\Users\asmidt\Documents\Minecraft\Remote";

        #region Properties
        public string LocalPath
        {
            get { return _LocalPath; }
            set
            {
                if (_LocalPath == value)
                {
                    return;
                }

                _LocalPath = value;
                OnPropertyChanged("LocalPath");
            }
        }

        public string RemotePath
        {
            get { return _RemotePath; }
            set
            {
                if (_RemotePath == value)
                {
                    return;
                }

                _RemotePath = value;
                OnPropertyChanged("RemotePath");
            }
        }

        #endregion

        #region Commands
        private ICommand _SyncCommand;
        public ICommand SyncCommand
        {
            get
            {
                if (_SyncCommand == null)
                {
                    _SyncCommand = new DelegateCommand(HandleSyncClick);
                }

                return _SyncCommand;
            }
            set
            {
                if (_SyncCommand == value)
                {
                    return;
                }

                _SyncCommand = value;
                OnPropertyChanged("SyncCommand");
            }
        }
        #endregion


        public void HandleSyncClick()
        {
            //check if local path is null or empty
            if (String.IsNullOrEmpty(_LocalPath))
            {
                MessageBox.Show("Bitte einen lokalen Speicherort angeben", "Lokaler Speicherort fehlt", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            //check if remote path is null or empty
            if (String.IsNullOrEmpty(_RemotePath))
            {
                MessageBox.Show("Bitte einen remote Speicherort angeben", "Remote Speicherort fehlt", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            //check if local path exists
            if (!Directory.Exists(_LocalPath))
            {
                MessageBox.Show($"Der lokale Speicherort '{_LocalPath}' existiert nicht!", "Lokaler Speicherort existiert nicht", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            //check if remote path exists
            if (!Directory.Exists(_RemotePath))
            {
                MessageBox.Show($"Der remote Speicherort '{_RemotePath}' existiert nicht!", "Remote Speicherort existiert nicht", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                DateTime localeLastChange = GetLastChangeOfDirectory(_LocalPath);
                DateTime remoteLastChange = GetLastChangeOfDirectory(_RemotePath);
                int result = DateTime.Compare(localeLastChange, remoteLastChange);

                if (result > 0)
                {
                    //Sync PC -> Cloud
                    CopyDirectory(_LocalPath, _RemotePath);

                }
                else if (result < 0)
                {
                    //Sync Cloud -> PC
                    CopyDirectory(_RemotePath, _LocalPath);
                }

                MessageBox.Show("Synchronisation abgeschlossen.", "Synchronisation abgeschlossen", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Bei der Synchronisation ist ein Fehler aufgetreten." + Environment.NewLine + Environment.NewLine + ex.ToString());
            }
        }

        private static DateTime GetLastChangeOfDirectory(string dirName)
        {
            DirectoryInfo dir = new DirectoryInfo(dirName);

            //check if dir exists
            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException($"Speicherort '{dirName}' existiert nicht!");
            }

            DateTime lastTimeStamp = DateTime.MinValue;

            //get last write time of files
            foreach(FileInfo fileInfo in dir.GetFiles())
            {
                if(fileInfo.LastWriteTimeUtc.CompareTo(lastTimeStamp) > 0)
                {
                    lastTimeStamp = fileInfo.LastWriteTimeUtc;
                }
            }

            //get last write time of directories
            foreach(DirectoryInfo directoryInfo in dir.GetDirectories())
            {
                DateTime result = GetLastChangeOfDirectory(directoryInfo.FullName);

                if(result.CompareTo(lastTimeStamp) > 0)
                {
                    lastTimeStamp = result;
                }
            }

            return lastTimeStamp;
        }

        private static void CopyDirectory(string sourceDirName, string destDirName)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
         
            //check if sourceDir exists
            if(!dir.Exists)
            {
                throw new DirectoryNotFoundException($"Speicherort '{sourceDirName}' existiert nicht!");
            }

            //create directory if destDir not exists
            Directory.CreateDirectory(destDirName);

            //copy files to target path
            foreach (FileInfo file in dir.GetFiles())
            {
                string tempPath = Path.Combine(destDirName, file.Name);
                file.CopyTo(tempPath, true);
            }

            //copy subdirectories
            foreach(DirectoryInfo subDir in dir.GetDirectories())
            {
                string tempPath = Path.Combine(destDirName, subDir.Name);
                CopyDirectory(subDir.FullName, tempPath);
            }
        }

        #region INotfiyPropertyChanged Members
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

    }
}
