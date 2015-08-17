using ARdevKit.Controller.ProjectController;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ARdevKit.Model.Project
{
    [Serializable]
    [TypeConverterAttribute(typeof(ExpandableObjectConverter))]
    public class GenericHtml : AbstractHtmlElement
    {
        public string Tag { get; set; }
        /// <summary>
        /// default constructor
        /// </summary>
        public GenericHtml()
        {
            base.Positioning = new HtmlPositioning(HtmlPositioning.PositioningModes.RELATIVE);
            resFilePath = null;
            Scaling = new Vector3D(0, 0, 0);
        }

        /// <summary>
        /// Gets or sets the width.
        /// </summary>
        /// <value>
        /// The width, in mm.
        /// </value>
        [Browsable(true)]
        public new int Width
        {
            get { return base.Width; }
            set { base.Width = value; }
        }

        /// <summary>
        /// Gets or sets the height.
        /// </summary>
        /// <value>
        /// The height, in mm.
        /// </value>
        [Browsable(true)]
        public new int Height
        {
            get { return base.Height; }
            set { base.Height = value; }
        }

        /// <summary>
        /// An overwriting method, to accept a <see cref="AbstractProjectVisitor" />
        /// which must be implemented according to the visitor design pattern.
        /// </summary>
        /// <param name="visitor">the visitor which encapsulates the action
        /// which is performed on this element</param>
        public override void Accept(AbstractProjectVisitor visitor)
        {
            base.Accept(visitor);
            visitor.Visit(this);
        }

        public override System.Drawing.Bitmap getPreview(string projectPath)
        {
            string absolutePath = Path.Combine(projectPath == null ? "" : projectPath, resFilePath);
            if (System.IO.File.Exists(absolutePath))
                if (Height == 0 || Width == 0)
                    return new Bitmap(absolutePath);
                else
                    return new Bitmap(new Bitmap(absolutePath), Width, Height);
            else
                throw new ArgumentException("Projekt-Datei beschädigt");
        }

        public override System.Drawing.Bitmap getIcon()
        {
            return ARdevKit.Properties.Resources.HtmlGenericAugmention_small_;
        }

        public override void CleanUp()
        {
            throw new NotImplementedException();
        }
        public override object Clone()
        {
            return ObjectCopier.Clone<GenericHtml>(this);
        }

        /// <summary>
        /// This method is called by the previewController when a new instance of the element is added to the Scene. It sets "must-have" properties.
        /// </summary>
        /// <param name="ew">The ew.</param>
        /// <returns>
        /// true if it succeeds, false if it fails.
        /// </returns>
        public override bool initElement(EditorWindow ew)
        {
            
            if (ResFilePath == null)
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.InitialDirectory = Environment.CurrentDirectory + "\\res\\testFiles\\augmentations";
                openFileDialog.Title = "Wählen sie eine HTMLDatei";
                openFileDialog.Filter = "Supported files (*.html)|*.html";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    ResFilePath = openFileDialog.FileName;
                    return base.initElement(ew);
                }
                else
                {
                    return false;
                }
            }
            return base.initElement(ew);
        }

    }
}
