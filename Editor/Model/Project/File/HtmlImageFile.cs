using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ARdevKit.Model.Project.File
{
    /// <summary>
    /// A <see cref="HtmlImageFile"/> is an <see cref="AbstractFile"/> which represents the htmlImage.js.
    /// </summary>
    public class HtmlImageFile : AbstractFile
    {
        /// <summary>   Identifier for the htmlImage. </summary>
        private string htmlImageID;

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Constructor. </summary>
        ///
        /// <remarks>   Imanuel, 23.01.2014. </remarks>
        ///
        /// <param name="projectPath">  The project path to write. </param>
        /// <param name="htmlImageID">      Identifier for the htmlImage. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public HtmlImageFile(string projectPath, string htmlImageID)
        {
            this.htmlImageID = htmlImageID;
            string htmlImagePath = Path.Combine(projectPath, "Assets", htmlImageID);
            if (!Directory.Exists(htmlImagePath))
                Directory.CreateDirectory(htmlImagePath);
            filePath = Path.Combine(htmlImagePath, "htmlImage.js");
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
            string htmlImagePath = Path.Combine(projectPath, "Assets", htmlImageID);
            if (!Directory.Exists(htmlImagePath))
                Directory.CreateDirectory(htmlImagePath);
            filePath = Path.Combine(htmlImagePath, "htmlImage.js");
            Save();
        }
    }
}
