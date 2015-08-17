using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ARdevKit.Model.Project
{
    [Serializable]
    [TypeConverterAttribute(typeof(ExpandableObjectConverter))]
    abstract public class AbstractHtmlElement : Abstract2DAugmentation
    {
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
    }
}
