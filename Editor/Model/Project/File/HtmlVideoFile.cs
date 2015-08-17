using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ARdevKit.Model.Project.File
{
    /// <summary>
    /// A <see cref="HtmlVideoFile"/> is an <see cref="AbstractFile"/> which represents the htmlVideo.js.
    /// </summary>
    public class HtmlVideoFile : AbstractFile
    {
        /// <summary>   Identifier for the htmlVideo. </summary>
        private string htmlVideoID;

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Constructor. </summary>
        ///
        /// <remarks>   Imanuel, 23.01.2014. </remarks>
        ///
        /// <param name="projectPath">  The project path to write. </param>
        /// <param name="htmlVideoID">      Identifier for the htmlVideo. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public HtmlVideoFile(string projectPath, string htmlVideoID)
        {
            this.htmlVideoID = htmlVideoID;
            string htmlVideoPath = Path.Combine(projectPath, "Assets", htmlVideoID);
            if (!Directory.Exists(htmlVideoPath))
                Directory.CreateDirectory(htmlVideoPath);
            filePath = Path.Combine(htmlVideoPath, "htmlVideo.js");
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
            string htmlVideoPath = Path.Combine(projectPath, "Assets", htmlVideoID);
            if (!Directory.Exists(htmlVideoPath))
                Directory.CreateDirectory(htmlVideoPath);
            filePath = Path.Combine(htmlVideoPath, "htmlVideo.js");
            Save();
        }
    }
}
