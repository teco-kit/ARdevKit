﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;
using System.ComponentModel;
using ARdevKit.Controller.ProjectController;
using ARdevKit.View;

namespace ARdevKit.Model.Project
{
    /// <summary>   
    /// An augmentation only described by an ImagePath.
    /// It is an <see cref="Abstract2DAugmentation"/>
    /// </summary>
    [Serializable]
    [TypeConverterAttribute(typeof(ExpandableObjectConverter))]
    public class ImageAugmentation : Abstract2DAugmentation
    {
        /// <summary>
        /// Full pathname of the image file.
        /// </summary>
        private string imagePath;
        /// <summary>
        /// Gets or sets the full pathname of the image file.
        /// </summary>
        /// <value>
        /// The full pathname of the image file.
        /// </value>
        [CategoryAttribute("General")]
        public string ImagePath
        {
            get { return imagePath; }
            set { imagePath = value; }
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ImageAugmentation() : base()
        {
            imagePath = null;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ImageAugmentation"/> class.
        /// </summary>
        /// <param name="imagePath">The image path.</param>
        public ImageAugmentation(string imagePath)
            : base()
        {
            this.imagePath = imagePath;
        }

        /// <summary>
        /// An overwriting method, to accept a <see cref="AbstractProjectVisitor" />
        /// which must be implemented according to the visitor design pattern.
        /// </summary>
        /// <param name="visitor">the visitor which encapsulates the action
        /// which is performed on this element</param>
        public override void Accept(AbstractProjectVisitor visitor)
        {
            visitor.Visit(this);
        }


        /// <summary>
        /// returns a <see cref="Bitmap" /> in order to be displayed
        /// on the PreviewPanel, implements <see cref="IPreviewable" />
        /// </summary>
        /// <returns>
        /// a representative Bitmap
        /// </returns>
        /// <exception cref="FileNotFoundException">Thrown when the requested File is
        /// not found in <see cref="ImagePath" />.</exception>
        public override Bitmap getPreview()
        {
            return new Bitmap(ImagePath);
        }
                
        /// <summary>
        /// returns a <see cref="Bitmap" /> in order to be displayed
        /// on the ElementSelectionPanel, implements <see cref="IPreviewable" />
        /// </summary>
        /// <returns>
        /// a representative iconized Bitmap
        /// </returns>
        /// <exception cref="FileNotFoundException">If ImagePath is bad</exception>
        public override Bitmap getIcon()
        {
            return Properties.Resources.ImageAugmentation_small_; 
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
            return ObjectCopier.Clone<ImageAugmentation>(this);
        }
    }
}
