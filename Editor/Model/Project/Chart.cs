﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel;
using System.IO;
using System.Drawing.Design;
using System.Windows.Forms;
using ARdevKit.Model.Project.File;
using ARdevKit.Model.Project.Event;

namespace ARdevKit.Model.Project
{

    /// <summary>
    /// Describes a <see cref="Chart"/> which is programmed in JavaScript and display a set of 
    /// data which can either be from a file or a database.
    /// It inherits from <see cref="AbstractDynamic2DAugmentation"/>.
    /// </summary>
    [Serializable]
    [TypeConverterAttribute(typeof(ExpandableObjectConverter))]
    public class Chart : AbstractDynamic2DAugmentation
    {
        /// <summary>
        /// True if the chart should be recalculated after tracking is lost.
        /// </summary>
        private bool forceRecalculation;
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Chart" /> should be recalculated after tracking is lost.
        /// </summary>
        /// <value>
        ///   <c>true</c> if recalculate; otherwise, <c>false</c>.
        /// </value>
        [CategoryAttribute("General")]
        public bool ForceRecalculation { get { return forceRecalculation; } set { forceRecalculation = value; } }

        /// <summary>
        /// The positioning of the Chart
        /// </summary>
        private HtmlPositioning positioning;
        /// <summary>
        /// Gets or sets the positioning of the Chart
        /// </summary>
        /// <value>
        /// The positioning.
        /// </value>
        [CategoryAttribute("Position"), ReadOnly(true)]
        public HtmlPositioning Positioning
        {
            get { return positioning; }
            set { positioning = value; }
        }

        /// <summary>
        /// gets or sets the Vector
        /// </summary>
        [Browsable(false)]
        public Vector3D Rotation
        {
            get { return base.Rotation; }
            set { base.Rotation = value; }
        }

        /// <summary>   Gets or sets the scaling. </summary>
        ///
        /// <value> The scaling. </value>
        [Browsable(false)]
        public Vector3D Scaling
        {
            get { return base.Scaling; }
            set { base.Scaling = value; }
        }

        /// <summary>
        /// Stump to remove it from propertyGrid because its not possible to use this functions on <see cref="Chart"/>s.
        /// </summary>
        [Browsable(false)]
        public AbstractEvent OnTouchStarted
        {
            get { return onTouchStarted == null ? (onTouchStarted = new OnTouchStartedEvent(ID)) : onTouchStarted; }
            set { onTouchStarted = value; }
        }

        /// <summary>
        /// Stump to remove it from propertyGrid because its not possible to use this functions on <see cref="Chart"/>s.
        /// </summary>
        [Browsable(false)]
        public AbstractEvent OnTouchEnded
        {
            get { return onTouchEnded == null ? (onTouchEnded = new OnTouchEndedEvent(ID)) : onTouchEnded; }
            set { onTouchEnded = value; }
        }

        /// <summary>
        /// Stump to remove it from propertyGrid because its not possible to use this functions on <see cref="Chart"/>s.
        /// </summary>
        [Browsable(false)]
        public AbstractEvent OnVisible
        {
            get { return onVisible == null ? (onVisible = new OnVisibleEvent(ID)) : onVisible; }
            set { onVisible = value; }
        }

        /// <summary>
        /// Stump to remove it from propertyGrid because its not possible to use this functions on <see cref="Chart"/>s.
        /// </summary>
        [Browsable(false)]
        public AbstractEvent OnInvisible
        {
            get { return onInvisible == null ? (onInvisible = new OnInvisibleEvent(ID)) : onInvisible; }
            set { onInvisible = value; }
        }

        /// <summary>
        /// Stump to remove it from propertyGrid because its not possible to use this functions on <see cref="Chart"/>s.
        /// </summary>
        [Browsable(false)]
        public AbstractEvent OnLoaded
        {
            get { return onLoaded == null ? (onLoaded = new OnLoadedEvent(ID)) : onLoaded; }
            set { onLoaded = value; }
        }

        /// <summary>
        /// Stump to remove it from propertyGrid because its not possible to use this functions on <see cref="Chart"/>s.
        /// </summary>
        [Browsable(false)]
        public AbstractEvent OnUnloaded
        {
            get { return onUnloaded == null ? (onUnloaded = new OnUnloadedEvent(ID)) : onUnloaded; }
            set { onUnloaded = value; }
        }

        /// <summary>   Default constructor. </summary>
        public Chart()
        {
            Positioning = new HtmlPositioning(HtmlPositioning.PositioningModes.RELATIVE);
            resFilePath = null;
            Width = 200;
            Height = 200;
            Scaling = new Vector3D(0, 0, 0);
        }

        /// <summary>
        /// An overwriting method, to accept a <see cref="AbstractProjectVisitor" />
        /// which must be implemented according to the visitor design pattern.
        /// </summary>
        /// <param name="visitor">the visitor which encapsulates the action
        ///     which is performed on this <see cref="Chart"/></param>
        public override void Accept(Controller.ProjectController.AbstractProjectVisitor visitor)
        {
            base.Accept(visitor);
            visitor.Visit(this);
            if (Source != null)
                Source.Accept(visitor);
        }


        /// <summary>
        /// returns a <see cref="Bitmap" /> in order to be displayed
        /// on the PreviewPanel, implements <see cref="IPreviewable" />
        /// </summary>
        /// <returns>
        /// a representative Bitmap
        /// </returns>
        public override Bitmap getPreview(string projectPath)
        {
            return Properties.Resources.highcharts_normal_;
        }


        /// <summary>
        /// returns a <see cref="Bitmap" /> in order to be displayed
        /// on the ElementSelectionPanel, implements <see cref="IPreviewable" />
        /// </summary>
        /// <returns>
        /// a representative iconized Bitmap
        /// </returns>
        public override Bitmap getIcon()
        {
            return Properties.Resources.highcharts_normal_;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Clean up (remove created/copied files and directories). </summary>
        ///
        /// <remarks>   Imanuel, 31.01.2014. </remarks>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public override void CleanUp()
        {
            string dir = Path.GetDirectoryName(resFilePath);
            if (Directory.Exists(dir) && System.IO.File.Exists(Path.Combine(dir, "chart.js")))
                Directory.Delete(dir, true);
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
            bool result = base.initElement(ew);
            string newPath = Path.Combine(Environment.CurrentDirectory, "tmp", id);
            if (ResFilePath == null)
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.InitialDirectory = Environment.CurrentDirectory + "\\res\\highcharts";
                openFileDialog.Filter = "js (*.js)|*.js";
                openFileDialog.Title = "Wählen sie eine Options Datei";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    Helper.Copy(openFileDialog.FileName, newPath, "options.js");
                    ResFilePath = Path.Combine(newPath, "options.js");
                }
                else
                {
                    return false;
                }
            }
            else
            {
                Helper.Copy(ResFilePath, newPath, "options.js");
                ResFilePath = Path.Combine(newPath, "options.js");
            }
            string res = Path.Combine(Environment.CurrentDirectory, "res");
            Helper.Copy(Path.Combine(res, "jquery", "jquery-1.11.1.js"), newPath);
            Helper.Copy(Path.Combine(res, "highcharts", "highcharts.js"), newPath);
            Helper.Copy(Path.Combine(res, "templates", "chart.html"), newPath);
            return result;
        }
    }
}