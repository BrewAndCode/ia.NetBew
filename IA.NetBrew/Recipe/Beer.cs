using System;
using System.IO;
using System.Threading;

using Microsoft.SPOT;
using Microsoft.SPOT.IO;
using Microsoft.SPOT.Hardware;

using GHIElectronics.NETMF.FEZ;
using GHIElectronics.NETMF.IO;




namespace IA.NetBrew.Recipe
{
    public class Beer
    {
        public String Filename { get; set; }

        public static void Load()
        {
            var sdPS = new PersistentStorage("SD");
            sdPS.MountFileSystem();

            Debug.Print("Getting all files and Folders on SD:");
            if (VolumeInfo.GetVolumes()[0].IsFormatted)
            {
                string rootDirectory = VolumeInfo.GetVolumes()[0].RootDirectory;
         
                string[] files = Directory.GetFiles(rootDirectory);
                string[] folders = Directory.GetDirectories(rootDirectory);
 
                Debug.Print("Files available on " + rootDirectory + ":");
                for (int i = 0; i < files.Length; i++)
                    Debug.Print(files[i]);
 
                Debug.Print("Folders available on " + rootDirectory + ":");
                for (int i = 0; i < folders.Length; i++)
                    Debug.Print(folders[i]);
            }
            else
            {
                Debug.Print("Storage is not formatted. Format on PC with FAT32/FAT16 first.");
            }
 
            // Unmount
            sdPS.UnmountFileSystem();


        }
    }
}
