using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ARdevKit.Model.Project.File
{
    class SceneFile : AbstractFile
    {
        /// <summary>   Identifier for the scene. </summary>
        private string sceneID;

        /// <summary>
        /// DefineBlock, outer structure of the object literal, to be filled by the exportVisitor
        /// and here public for the augmentations to access it
        /// </summary>
        public JavaScriptBlock DefineBlock;

        /// <summary>
        /// CreateBlock, initialization of the object literal, to be filled by the exportVisitor
        /// and here public for the augmentations to access it
        /// </summary>
        public JavaScriptBlock CreateBlock;

        /// <summary>
        /// getSceneReadyBlock, initialization of the included Elements e.g. loading events,
        /// to be filled by the exportVisitor and here public for the augmentations to access it
        /// </summary>
        public JavaScriptBlock SceneReadyBlock;

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Constructor. </summary>
        ///
        /// <remarks>   Imanuel, 23.01.2014. </remarks>
        ///
        /// <param name="projectPath">  The project path to write. </param>
        /// <param name="sceneID">      Identifier for the scene. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public SceneFile(string projectPath, string sceneID)
        {
            this.sceneID = sceneID;
            string scenePath = Path.Combine(projectPath, "Assets");
            if (!Directory.Exists(scenePath))
                Directory.CreateDirectory(scenePath);
            filePath = Path.Combine(scenePath, sceneID+".js");
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
            string scenePath = Path.Combine(projectPath, "Assets");
            if (!Directory.Exists(scenePath))
                Directory.CreateDirectory(scenePath);
            filePath = Path.Combine(scenePath, sceneID + ".js");
            Save();
        }
    }
}
