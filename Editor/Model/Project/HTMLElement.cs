using ARdevKit.Model.Project.Event;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ARdevKit.Model.Project
{
    [Serializable]
    [TypeConverterAttribute(typeof(ExpandableObjectConverter))]
    class HTMLElement : AbstractDynamic2DAugmentation
    {

        /// <summary>
        /// The id of the HTMLElement
        /// </summary>
        private string id;
        /// <summary>
        /// Gets or sets the id of the HTMLElement
        /// </summary>
        /// /// <value>
        /// The id.
        /// </value>
        public string ID
        {
            get { return id; }
            set { id = value; }
        }

        /// <summary>
        /// The positioning of the HTMLElement
        /// </summary>
        private ChartPositioning positioning;
        /// <summary>
        /// Gets or sets the positioning of the HTMLElement
        /// </summary>
        /// <value>
        /// The positioning.
        /// </value>
        [CategoryAttribute("Position"), ReadOnly(true)]
        public ChartPositioning Positioning
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
        /// Stump to remove it from propertyGrid because its not possible to use this functions on <see cref="HTMLElement"/>s.
        /// </summary>
        [Browsable(false)]
        public AbstractEvent OnTouchStarted
        {
            get { return onTouchStarted == null ? (onTouchStarted = new OnTouchStartedEvent(ID)) : onTouchStarted; }
            set { onTouchStarted = value; }
        }

        /// <summary>
        /// Stump to remove it from propertyGrid because its not possible to use this functions on <see cref="HTMLElement"/>s.
        /// </summary>
        [Browsable(false)]
        public AbstractEvent OnTouchEnded
        {
            get { return onTouchEnded == null ? (onTouchEnded = new OnTouchEndedEvent(ID)) : onTouchEnded; }
            set { onTouchEnded = value; }
        }

        /// <summary>
        /// Stump to remove it from propertyGrid because its not possible to use this functions on <see cref="HTMLElement"/>s.
        /// </summary>
        [Browsable(false)]
        public AbstractEvent OnVisible
        {
            get { return onVisible == null ? (onVisible = new OnVisibleEvent(ID)) : onVisible; }
            set { onVisible = value; }
        }

        /// <summary>
        /// Stump to remove it from propertyGrid because its not possible to use this functions on <see cref="HTMLElement"/>s.
        /// </summary>
        [Browsable(false)]
        public AbstractEvent OnInvisible
        {
            get { return onInvisible == null ? (onInvisible = new OnInvisibleEvent(ID)) : onInvisible; }
            set { onInvisible = value; }
        }

        /// <summary>
        /// Stump to remove it from propertyGrid because its not possible to use this functions on <see cref="HTMLElement"/>s.
        /// </summary>
        [Browsable(false)]
        public AbstractEvent OnLoaded
        {
            get { return onLoaded == null ? (onLoaded = new OnLoadedEvent(ID)) : onLoaded; }
            set { onLoaded = value; }
        }

        /// <summary>
        /// Stump to remove it from propertyGrid because its not possible to use this functions on <see cref="HTMLElement"/>s.
        /// </summary>
        [Browsable(false)]
        public AbstractEvent OnUnloaded
        {
            get { return onUnloaded == null ? (onUnloaded = new OnUnloadedEvent(ID)) : onUnloaded; }
            set { onUnloaded = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public HTMLElement()
        {
        }

        public HTMLElement(string id)
        {
            this.id = id;
        }

        public override System.Drawing.Bitmap getPreview(string projectPath)
        {
            return Properties.Resources.HTMLElement;
        }

        public override System.Drawing.Bitmap getIcon()
        {
            return Properties.Resources.HTMLElement;
        }

        public override void CleanUp()
        {
            throw new NotImplementedException();
        }
    }
}
