﻿using ARdevKit.Model.Project;
using ARdevKit.Model.Project.File;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ARdevKit.Controller.ProjectController
{
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>   An abstract project visitor. </summary>
    ///
    /// <remarks>   Imanuel, 17.01.2014. </remarks>
    ////////////////////////////////////////////////////////////////////////////////////////////////////

    public abstract class AbstractProjectVisitor
    {
        /// <summary>
        /// Visits the given <see cref="Event"/>.
        /// </summary>
        /// <param name="eventFile"> The custom user event. </param>
        public abstract void Visit(EventFile eventFile);

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Visits the given <see cref="Chart"/>. </summary>
        ///
        /// <remarks>   Imanuel, 17.01.2014. </remarks>
        ///
        /// <param name="chart">    The chart. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public abstract void Visit(Chart chart);

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Visits the given <see cref="ImageAugmentation"/>. </summary>
        ///
        /// <remarks>   Imanuel, 17.01.2014. </remarks>
        ///
        /// <param name="image">    The image. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public abstract void Visit(ImageAugmentation image);

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Visits the given <see cref="VideoAugmentation"/>. </summary>
        ///
        /// <remarks>   Imanuel, 29.01.2014. </remarks>
        ///
        /// <param name="video">    The video. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public abstract void Visit(VideoAugmentation video);

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Visits the given <see cref="DbSource"/>. </summary>
        ///
        /// <remarks>   Imanuel, 17.01.2014. </remarks>
        ///
        /// <param name="source">   Source for the. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public abstract void Visit(DbSource source);

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Visits the given <see cref="FileSource"/>. </summary>
        ///
        /// <remarks>   Imanuel, 23.01.2014. </remarks>
        ///
        /// <param name="source">   Source for the <see cref="AbstractDynamic2DAugmentation"/>. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public abstract void Visit(FileSource source);

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Visits the given <see cref="MarkerlessFuser"/>. </summary>
        ///
        /// <remarks>   Imanuel, 17.01.2014. </remarks>
        ///
        /// <param name="markerlessFuser">  The markerless fuser. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public abstract void Visit(MarkerlessFuser markerlessFuser);

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Visits the given <see cref="MarkerFuser"/>. </summary>
        ///
        /// <remarks>   Imanuel, 17.01.2014. </remarks>
        ///
        /// <param name="markerFuser">  The marker fuser. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public abstract void Visit(MarkerFuser markerFuser);

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Visits the given <see cref="MarkerlessSensor"/>. </summary>
        ///
        /// <remarks>   Imanuel, 17.01.2014. </remarks>
        ///
        /// <param name="MarkerlessSensor"> The markerless sensor. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public abstract void Visit(MarkerlessSensor MarkerlessSensor);

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Visits the given <see cref="ImageTrackable"/>. </summary>
        ///
        /// <remarks>   Imanuel, 26.01.2014. </remarks>
        ///
        /// <param name="image">    The image. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public abstract void Visit(ImageTrackable image);

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Visits the given <see cref="PictureMarkerSensor"/>. </summary>
        ///
        /// <remarks>   Imanuel, 17.01.2014. </remarks>
        ///
        /// <param name="pictureMarkerSensor">  The picture marker sensor. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public abstract void Visit(PictureMarkerSensor pictureMarkerSensor);

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Visits the given <see cref="PictureMarker"/>. </summary>
        ///
        /// <remarks>   Imanuel, 17.01.2014. </remarks>
        ///
        /// <param name="pictureMarker">    The picture marker. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public abstract void Visit(PictureMarker pictureMarker);

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Visits the given <see cref="MarkerSensor"/>. </summary>
        ///
        /// <remarks>   Imanuel, 17.01.2014. </remarks>
        ///
        /// <param name="idMarkerSensor">   The identifier marker sensor. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public abstract void Visit(MarkerSensor idMarkerSensor);

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Visits the given <see cref="IDMarker"/>. </summary>
        ///
        /// <remarks>   Imanuel, 17.01.2014. </remarks>
        ///
        /// <param name="idMarker"> The identifier marker. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public abstract void Visit(IDMarker idMarker);

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Visits the given <see cref="HtmlImage"/>. </summary>
        ///
        /// <remarks>   Imanuel, 17.01.2014. </remarks>
        ///
        /// <param name="HtmlImage"> The HtmlImage. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public abstract void Visit(HtmlImage htmlImage);
        
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Visits the given <see cref="HtmlVideo"/>. </summary>
        ///
        /// <remarks>   Imanuel, 17.01.2014. </remarks>
        ///
        /// <param name="HtmlVideo"> The identifier Html Video. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public abstract void Visit(HtmlVideo htmlVideo);

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Visits the given <see cref="Project"/>. </summary>
        ///
        /// <remarks>   Imanuel, 17.01.2014. </remarks>
        ///
        /// <param name="project">  The project. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public abstract void Visit(Project project);
    }
}
