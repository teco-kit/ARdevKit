﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Dieser Code wurde von einem Tool generiert.
//     Wenn der Code neu generiert wird, gehen alle Änderungen an dieser Datei verloren
// </auto-generated>
//------------------------------------------------------------------------------
namespace Editor.Controller.ProjectController
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>   An abstract project visitor. </summary>
    ///
    /// <remarks>   Geht, 18.12.2013. </remarks>
    ////////////////////////////////////////////////////////////////////////////////////////////////////

	public abstract class AbstractProjectVisitor
	{
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Visits the given identifier marker. </summary>
        ///
        /// <remarks>   Geht, 18.12.2013. </remarks>
        ///
        /// <exception cref="NotImplementedException">  Thrown when the requested operation is
        ///                                             unimplemented. </exception>
        ///
        /// <param name="barGraph"> The bar graph. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

		public virtual void visit(BarGraph barGraph)
		{
			throw new System.NotImplementedException();
		}

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Visits the given identifier marker. </summary>
        ///
        /// <remarks>   Geht, 18.12.2013. </remarks>
        ///
        /// <exception cref="NotImplementedException">  Thrown when the requested operation is
        ///                                             unimplemented. </exception>
        ///
        /// <param name="dbSource"> The database source. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

		public virtual void visit(DbSource dbSource)
		{
			throw new System.NotImplementedException();
		}

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Visits the given identifier marker. </summary>
        ///
        /// <remarks>   Geht, 18.12.2013. </remarks>
        ///
        /// <exception cref="NotImplementedException">  Thrown when the requested operation is
        ///                                             unimplemented. </exception>
        ///
        /// <param name="pictureMarker">    The picture marker. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

		public virtual void visit(PictureMarker pictureMarker)
		{
			throw new System.NotImplementedException();
		}

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Visits the given identifier marker. </summary>
        ///
        /// <remarks>   Geht, 18.12.2013. </remarks>
        ///
        /// <exception cref="NotImplementedException">  Thrown when the requested operation is
        ///                                             unimplemented. </exception>
        ///
        /// <param name="idMarker"> The identifier marker. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

		public virtual void visit(IDMarker idMarker)
		{
			throw new System.NotImplementedException();
		}

	}
}

