﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel;
using ARdevKit.Controller.ProjectController;
using ARdevKit.View;
using System.Drawing;
using System.Windows.Forms;

namespace ARdevKit.Model.Project
{
    /// <summary>
    /// Describes a Marker, which is somewhat in-between an id marker and a markerless.
    /// For more information look at the metaio developer forum or here:
    /// http://dev.metaio.com/sdk/tracking-config/optical-tracking/picture-marker/
    /// It inhertis form <see cref="Abstract2DTrackable"/>.
    /// </summary>
    [Serializable]
    [TypeConverterAttribute(typeof(ExpandableObjectConverter))]
    public class PictureMarker : Abstract2DTrackable
    {
        /// <summary>
        /// Full pathname of the picture file.
        /// </summary>
        protected string picturePath;

        /// <summary>
        /// A cached preview to prevent access problems.
        /// </summary>
        private Bitmap cachePreview = null;

        /// <summary>
        /// Gets or sets the full pathname of the picture file.
        /// </summary>
        /// <value>
        /// The full pathname of the picture file.
        /// </value>
        [CategoryAttribute("General"), Description("Full pathname of the image file"), EditorAttribute(typeof(FileSelectorTypeEditor),
            typeof(System.Drawing.Design.UITypeEditor))]
        public string PicturePath
        {
            get { return picturePath; }
            set
            {
                picturePath = value;
                pictureName = Path.GetFileName(value);
                
            }
        }

        /// <summary>
        /// Name of the picture.
        /// </summary>
        protected string pictureName;
        /// <summary>
        /// Gets or sets the name of the picture.
        /// </summary>
        /// <value>
        /// The name of the picture.
        /// </value>
        [CategoryAttribute("General"), Description("Name of the image"), ReadOnly(true)]
        public string PictureName
        {
            get { return pictureName; }
        }

        /// <summary>
        /// Gets or sets the size.
        /// </summary>
        [Browsable(false)]
        public new int Size
        {
            get { return base.Size; }
            set { base.Size = value; }
        }

        /// <summary>
        /// Default Constructor.
        /// </summary>
        public PictureMarker()
        {
            type = "PictureMarker";
            similarityThreshold = 0.7;
            vector = new Vector3D(0, 0, 0);
            translationVector = new Vector3D(0, 0, 0);
            rotationVector = new Vector3Di(0, 0, 0, 0);
            Augmentations = new List<AbstractAugmentation>();
            sensorCosID = IDFactory.CreateNewSensorCosID(this);
            fuser = new MarkerFuser();
            picturePath = null;
            pictureName = "";
            widthMM = 300;
            heightMM = 300;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="picturePath">The picture path.</param>
        public PictureMarker(string picturePath)
            : this()
        {
            widthMM = (int) Math.Round(new Bitmap(picturePath).PhysicalDimension.Width, 0);
            heightMM = new Bitmap(picturePath).Height;
            this.picturePath = picturePath;
            pictureName = Path.GetFileName(picturePath);
        }

        /// <summary>
        /// An overwriting method, to accept a <see cref="AbstractProjectVisitor" />
        /// which must be implemented according to the visitor design pattern.
        /// It lets the visitor visit every augmentation associated with it.
        /// </summary>
        /// <param name="visitor">the visitor which encapsulates the action
        /// which is performed on this element</param>
        public override void Accept(AbstractProjectVisitor visitor)
        {
            visitor.Visit(this);
            foreach (AbstractAugmentation augmentation in Augmentations)
            {
                augmentation.Accept(visitor);
            }
            fuser.Accept(visitor);
        }

        /// <summary>
        /// returns a <see cref="Bitmap" /> in order to be displayed
        /// on the PreviewPanel, implements <see cref="IPreviewable" />
        /// </summary>
        /// <returns>
        /// a representative Bitmap
        /// </returns>
        /// <exception cref="FileNotFoundException">If ImagePath is
        ///     not correct.</exception>
        public override Bitmap getPreview(string projectPath)
        {
            string absolutePath = Path.Combine(projectPath == null ? "" : projectPath, picturePath);
            if (System.IO.File.Exists(absolutePath))
                return (Bitmap)Bitmap.FromStream(new MemoryStream(System.IO.File.ReadAllBytes(absolutePath)));
            else
                throw new ArgumentException("Projekt-Datei beschädigt");            
        }


        /// <summary>
        /// returns a <see cref="Bitmap" /> in order to be displayed
        /// on the ElementSelectionPanel, implements <see cref="IPreviewable" />
        /// </summary>
        /// <returns>
        /// a representative iconized Bitmap
        /// </returns>
        public override System.Drawing.Bitmap getIcon()
        {
            return Properties.Resources.PictureMarker_small_;
        }

        /**
         * <summary>    Makes a deep copy of this object. </summary>
         *
         * <remarks>    Robin, 22.01.2014. </remarks>
         *
         * <returns>    A copy of this object. </returns>
         */

        public override object Clone()
        {
            PictureMarker n = ObjectCopier.Clone<PictureMarker>(this);
            n.sensorCosID = IDFactory.CreateNewSensorCosID(this);
            return n;
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
            if (base.initElement(ew))
            {
                bool isInitOk = true;
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.InitialDirectory = Environment.CurrentDirectory + "\\res\\testFiles\\trackables";
                openFileDialog.Title = "Wählen sie einen Marker";
                openFileDialog.Filter = "Supported image files (*.jpg, *.png, *.bmp, *.ppm, *.pgm)|*.jpg; *.png; *.bmp; *.ppm; *.pgm";
                isInitOk = openFileDialog.ShowDialog() == DialogResult.OK;
                if (isInitOk)
                {
                    string path = openFileDialog.FileName;
                    bool isClonedMarker = PicturePath != null;
                    picturePath = path;
                    pictureName = Path.GetFileName(picturePath);

                    Bitmap bmp = getPreview(picturePath);
                    int widthPX = bmp.Width;
                    float hPXperInch = bmp.HorizontalResolution;
                    widthMM = (int)Math.Round(widthPX / hPXperInch * 25.4, 0);

                    int heightPX = bmp.Height;
                    float vPXperInch = bmp.VerticalResolution;
                    heightMM = (int)Math.Round(heightPX / vPXperInch * 25.4, 0);

                    if (!ew.project.existTrackable(this))
                    {
                        ew.project.Sensor = new MarkerSensor();
                    }
                    else
                    {
                        if (!isClonedMarker)
                        {
                            MessageBox.Show("You can't use the same marker in different Scenes.");
                            PicturePath = null;
                        }
                        return initElement(ew);
                    }

                }
                return isInitOk;
            }
            else
            {
                return false;
            }
        }
    }
}
