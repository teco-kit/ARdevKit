﻿using ARdevKit.Controller.ProjectController;
using ARdevKit.View;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ARdevKit.Model.Project
{
    public abstract class AbstractAugmentation : ISerializable, IPreviewable
    {
        private int coordinatesystemid;
        public int Coordinatesystemid
        {
            get { return coordinatesystemid; }
            set { coordinatesystemid = value; }
        }

        private string augmentationPath;
        public string AugmentationPath
        {
            get { return augmentationPath; }
            set { augmentationPath = value; }
        }

        /// <summary>
        /// A list of all customUserEvents the current selected Element has.
        /// </summary>
        private List<CustomUserEvent> customUserEvent;

        /// <summary>
        /// ToDo
        /// </summary>
        private bool isVisible;
        public bool IsVisible
        {
            get { return isVisible; }
            set { isVisible = value; }
        }

        /// <summary>
        /// Vector to know the Position on the PreviewPanel.
        /// </summar>
        public Vector3D vector { get; set; }

        /// <summary>
        /// New Variable which is for link a Source with this Augmentation
        /// </summary>
        public AbstractSource source { get; set; }

        /// <summary>
        /// Gets the list of all customUserEvent of this augmentation. (Only readable)
        /// </summary>
        public List<CustomUserEvent> CustomUserEventList
        {
            get { return customUserEvent; }
        }

        protected AbstractTrackable trackable;
        public AbstractTrackable Trackable
        {
            get { return trackable; }
            set { trackable = value; }
        }

        public abstract void Accept(AbstractProjectVisitor visitor);

        abstract public Bitmap getPreview();

        abstract public Bitmap getIcon();

        public abstract List<AbstractProperty> getPropertyList();

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new NotImplementedException();
        }
    }
}

