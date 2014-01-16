﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ARdevKit.Model.Project
{
    public class PictureMarker : AbstractMarker
    {
        private string imagePath;

        private string imageName;
        public string ImageName
        {
            get { return imageName; }
            set { imageName = value; }
        }

        private double similarityThreshhold = 0.7;
        public double SimilarityThreshhold
        {
            get { return similarityThreshhold; }
            set { similarityThreshhold = value; }
        }

        private PictureMarkerSensor pictureMarkerTrackingSensor;
        public PictureMarkerSensor PictureMarkerTrackingSensor
        {
            get { return pictureMarkerTrackingSensor; }
            set { pictureMarkerTrackingSensor = value; }
        }

        public PictureMarker(string imagePath)
        {
            this.imagePath = imagePath;
            imageName = Path.GetFileName(imagePath);
            type = "PictureMarker";
            pictureMarkerTrackingSensor = new PictureMarkerSensor();
            sensor = pictureMarkerTrackingSensor;
            MarkerFuser = new Fuser();
            sensorCosID = IDFactory.getSensorCosID(this);
        }

        public override void Accept(Controller.ProjectController.AbstractProjectVisitor visitor)
        {
            visitor.visit(this);
            foreach (AbstractAugmentation augmentation in augmentations)
            {
                augmentation.Accept(visitor);
            }
        }

        public override List<View.AbstractProperty> getPropertyList()
        {
            throw new NotImplementedException();
        }

        public override System.Drawing.Bitmap getPreview()
        {
           return Properties.Resources.ARMarker_normal_;
        }

        public override System.Drawing.Bitmap getIcon()
        {
            return Properties.Resources.ARMarker_small_;
        }
    }
}
