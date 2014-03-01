﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace ARdevKit.Model.Project.File
{
    /// <summary>
    /// A static Helper class which contains some I/O methods.
    /// </summary>
    static class Helper
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Copies a passed file to the passed directory. </summary>
        ///
        /// <remarks>   Imanuel, 19.01.2014. </remarks>
        ///
        /// <param name="srcFile">          Source file. </param>
        /// <param name="destDirectory">    Pathname of the destination directory. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static void Copy(string srcFile, string destDirectory)
        {
            Copy(srcFile, destDirectory, Path.GetFileName(srcFile));
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Copies a passed file to the passed directory and renames it to the passed name. </summary>
        ///
        /// <remarks>   Imanuel, 27.01.2014. </remarks>
        ///
        /// <param name="srcFile">          Source file. </param>
        /// <param name="destDirectory">    Pathname of the destination directory. </param>
        /// <param name="newFileName">          Name of the new file. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static void Copy(string srcFile, string destDirectory, string newFileName)
        {
            Directory.CreateDirectory(destDirectory);

            string destFile = Path.Combine(destDirectory, newFileName);
            if (!System.IO.File.Equals(srcFile, destFile))
            {
                try
                {
                    System.IO.File.Copy(srcFile, destFile, true);
                }
                catch (Exception e)
                {
                    System.Windows.Forms.MessageBox.Show(e.Message, "Error!",
                        System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                }

            }
        }

        /// <summary>
        /// Checks if a file is in a specific directory or its subdirectory
        /// </summary>
        /// <param name="rootPath">Path of the directory which should be checked</param>
        /// <param name="filePath">Path of the file</param>
        /// <returns>Returns true if the file is in the directory of the rootPath or its subdirectory</returns>
        public static bool FileExists(string rootPath, string filePath)
        {
            if (System.IO.File.Exists(Path.Combine(rootPath, Path.GetFileName(filePath))))
                return true;

            foreach (string subDir in Directory.GetDirectories(rootPath))
            {
                if (FileExists(subDir, filePath) && System.IO.File.Equals(subDir, filePath))
                    return true;
            }

            return false;
        }
    }
}
