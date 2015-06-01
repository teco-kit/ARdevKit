﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ARdevKit.Model.Project.File
{
    /// <summary>
    /// A <see cref="HtmlGenericFile"/> is an <see cref="AbstractFile"/> which represents the htmlGeneric.js.
    /// </summary>
    public class HtmlGenericFile : AbstractFile
    {
        /// <summary>   Identifier for the htmlGeneric. </summary>
        private string htmlGenericID;

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Constructor. </summary>
        ///
        /// <remarks>   Imanuel, 23.01.2014. </remarks>
        ///
        /// <param name="projectPath">  The project path to write. </param>
        /// <param name="htmlGenericID">      Identifier for the htmlGeneric. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public HtmlGenericFile(string projectPath, string htmlGenericID)
        {
            this.htmlGenericID = htmlGenericID;
            string htmlGenericPath = Path.Combine(projectPath, "Assets", htmlGenericID);
            if (!Directory.Exists(htmlGenericPath))
                Directory.CreateDirectory(htmlGenericPath);
            filePath = Path.Combine(htmlGenericPath, "htmlGeneric.js");
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Saves the file to its <see cref="filePath"/>. </summary>
        ///
        /// <remarks>   Imanuel, 23.01.2014. </remarks>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public override void Save()
        {
            StreamWriter writer = new StreamWriter(filePath);
            if (blocks != null)
            {
                foreach (JavaScriptBlock jsBlock in blocks)
                {
                    jsBlock.Write(writer);
                }
            }
            writer.Close();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Saves the file to the using the passed <see cref="projectPath"/>. </summary>
        ///
        /// <remarks>   Imanuel, 23.01.2014. </remarks>
        ///
        /// <param name="projectPath">  The project path to write. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public override void Save(string projectPath)
        {
            string htmlGenericPath = Path.Combine(projectPath, "Assets", htmlGenericID);
            if (!Directory.Exists(htmlGenericPath))
                Directory.CreateDirectory(htmlGenericPath);
            filePath = Path.Combine(htmlGenericPath, "htmlGeneric.js");
            Save();
        }
    }
}
