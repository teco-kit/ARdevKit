﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Dieser Code wurde von einem Tool generiert.
//     Wenn der Code neu generiert wird, gehen alle Änderungen an dieser Datei verloren
// </auto-generated>
//------------------------------------------------------------------------------
namespace Editor.Model.Project
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>   An abstract trackable. </summary>
    ///
    /// <remarks>   Geht, 18.12.2013. </remarks>
    ////////////////////////////////////////////////////////////////////////////////////////////////////

	public abstract class AbstractTrackable : Serializable, IPreviewable
	{
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets or sets the identifier of the sensor. </summary>
        ///
        /// <value> The identifier of the sensor. </value>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

		private string sensorID
		{
			get;
			set;
		}

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets or sets the type of the sensor. </summary>
        ///
        /// <value> The type of the sensor. </value>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

		private string sensorType
		{
			get;
			set;
		}

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets or sets the type of the sensor sub. </summary>
        ///
        /// <value> The type of the sensor sub. </value>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

		private string sensorSubType
		{
			get;
			set;
		}

		public virtual {
			get;
			set;
		}

		public virtual void accept(ProjectVisitor visitor)
		{
			throw new System.NotImplementedException();
		}

		public virtual Bitmap getPreview()
		{
			throw new System.NotImplementedException();
		}

		public abstract List<AbstractProperty> getPropertyList();

	}
}

