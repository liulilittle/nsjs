namespace nsjsdotnet.Core.IO
{
    using System;
    using System.IO;

    public static class DirectoryAuxiliary
    {
        public static bool Copy(string sourceDirName, string destDirName)
        {
            return CopyDirectory(sourceDirName, destDirName, true);
        }

        private static bool CopyDirectory(string sourceDirName, string destDirName, bool doRootDir)
        {
            if (string.IsNullOrEmpty(sourceDirName) || string.IsNullOrEmpty(destDirName))
            {
                return false;
            }
            if (!Directory.Exists(sourceDirName))
            {
                return false;
            }
            if (sourceDirName == destDirName)
            {
                return false;
            }
            if (!Directory.Exists(destDirName))
            {
                try
                {
                    Directory.CreateDirectory(destDirName);
                }
                catch
                {
                    return false;
                }
            }
            string folderName = doRootDir ? string.Empty : sourceDirName.Substring(sourceDirName.LastIndexOf("\\") + 1);
            string destFolderPath = destDirName + (doRootDir ? string.Empty : "\\" + folderName);
            if (destDirName.LastIndexOf("\\") == (destDirName.Length - 1))
            {
                destFolderPath = destDirName + folderName;
            }
            string[] strFileNames = Directory.GetFileSystemEntries(sourceDirName);
            foreach (string strFileName in strFileNames)
            {
                if (Directory.Exists(strFileName))
                {
                    string currentDirectoryPath = destFolderPath + "\\" + strFileName.Substring(strFileName.LastIndexOf("\\") + 1);
                    if (!Directory.Exists(currentDirectoryPath))
                    {
                        try
                        {
                            Directory.CreateDirectory(currentDirectoryPath);
                        }
                        catch (Exception)
                        {
                            return false;
                        }
                    }
                    DirectoryAuxiliary.CopyDirectory(strFileName, destFolderPath, false);
                }
                else
                {
                    string srcFileName = strFileName.Substring(strFileName.LastIndexOf("\\") + 1);
                    srcFileName = destFolderPath + "\\" + srcFileName;
                    if (!Directory.Exists(destFolderPath))
                    {
                        Directory.CreateDirectory(destFolderPath);
                    }
                    try
                    {
                        File.Copy(strFileName, srcFileName);
                    }
                    catch (Exception) { }
                }
            }
            return true;
        }
    }
}
