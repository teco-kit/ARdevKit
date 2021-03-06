﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using ARdevKit;
using ARdevKit.Model.Project;

namespace Controller.EditorController
{
    /// <summary>
    /// The PropertyController contains events for the propertyGrid
    /// </summary>
    class PropertyController
    {
        /// <summary>
        /// Instance of the EditorWindow(Controller)
        /// </summary>
        private EditorWindow ew;

        /// <summary>
        /// Constructor of the class. It adds automatically all events which belongs to the propertyGrid.
        /// </summary>
        /// <param name="ew"></param>
        public PropertyController(EditorWindow ew)
        {
            this.ew = ew;
            ew.PropertyGrid1.PropertyValueChanged += new PropertyValueChangedEventHandler(changedProperty);
            //ew.PropertyGrid1.SelectedGridItemChanged += new SelectedGridItemChangedEventHandler(selectedGridItemChanged);
        }

        /*
        private void selectedGridItemChanged(object source, SelectedGridItemChangedEventArgs e)
        {
            if (ew.PropertyGrid1.SelectedGridItem.PropertyDescriptor.PropertyType.Name.Equals("Event"))
            {
                TextEditorForm tef;
                AbstractAugmentation a = (AbstractAugmentation)ew.PropertyGrid1.SelectedObject;
                Event selectedEvent;
                Event.Types type;
                switch (e.NewSelection.Label)
                {
                    case ("OnTouchStarted"):
                        if (a.OnTouchStarted == null)
                        {
                            a.createEvent(Event.Types.ONTOUCHSTARTED);
                            tef = new TextEditorForm(a.OnTouchStarted);
                            if (tef.ShowDialog() == DialogResult.OK)
                            {
                                a.OnTouchStarted = tef.SelectedEvent;
                            }
                            else
                                a.OnTouchStarted = null;
                        }
                        else
                        {
                            tef = new TextEditorForm(a.OnTouchStarted);
                            if (tef.ShowDialog() == DialogResult.OK)
                            {
                                a.OnTouchStarted = tef.SelectedEvent;
                            }
                        }
                        break;
                    case ("OnTouchEnded"):
                        if (a.OnTouchEnded == null)
                        {
                            a.createEvent(Event.Types.ONTOUCHENDED);
                            tef = new TextEditorForm(a.OnTouchEnded);
                            if (tef.ShowDialog() == DialogResult.OK)
                            {
                                a.OnTouchEnded = tef.SelectedEvent;
                            }
                            else
                                a.OnTouchEnded = null;
                        }
                        else
                        {
                            tef = new TextEditorForm(a.OnTouchEnded);
                            if (tef.ShowDialog() == DialogResult.OK)
                            {
                                a.OnTouchEnded = tef.SelectedEvent;
                            }
                        }
                        break;
                    case ("OnVisible"):
                        if (a.OnVisible == null)
                        {
                            a.createEvent(Event.Types.ONVISIBLE);
                            tef = new TextEditorForm(a.OnVisible);
                            if (tef.ShowDialog() == DialogResult.OK)
                            {
                                a.OnVisible = tef.SelectedEvent;
                            }
                            else
                                a.OnVisible = null;
                        }
                        else
                        {
                            tef = new TextEditorForm(a.OnVisible);
                            if (tef.ShowDialog() == DialogResult.OK)
                            {
                                a.OnVisible = tef.SelectedEvent;
                            }
                        }
                        break;
                    case ("OnInvisible"):
                        if (a.OnInvisible == null)
                        {
                            a.createEvent(Event.Types.ONINVISIBLE);
                            tef = new TextEditorForm(a.OnInvisible);
                            if (tef.ShowDialog() == DialogResult.OK)
                            {
                                a.OnInvisible = tef.SelectedEvent;
                            }
                            else
                                a.OnInvisible = null;
                        }
                        else
                        {
                            tef = new TextEditorForm(a.OnInvisible);
                            if (tef.ShowDialog() == DialogResult.OK)
                            {
                                a.OnInvisible = tef.SelectedEvent;
                            }
                        }
                        break;
                    case ("OnLoaded"):
                        if (a.OnLoaded == null)
                        {
                            a.createEvent(Event.Types.ONLOADED);
                            tef = new TextEditorForm(a.OnLoaded);
                            if (tef.ShowDialog() == DialogResult.OK)
                            {
                                a.OnLoaded = tef.SelectedEvent;
                            }
                            else
                                a.OnLoaded = null;
                        }
                        else
                        {
                            tef = new TextEditorForm(a.OnLoaded);
                            if (tef.ShowDialog() == DialogResult.OK)
                            {
                                a.OnLoaded = tef.SelectedEvent;
                            }
                        }
                        break;
                    case ("OnUnloaded"):
                        if (a.OnUnloaded == null)
                        {
                            a.createEvent(Event.Types.ONUNLOADED);
                            tef = new TextEditorForm(a.OnUnloaded);
                            if (tef.ShowDialog() == DialogResult.OK)
                            {
                                a.OnUnloaded = tef.SelectedEvent;
                            }
                            else
                                a.OnUnloaded = null;
                        }
                        else
                        {
                            tef = new TextEditorForm(a.OnUnloaded);
                            if (tef.ShowDialog() == DialogResult.OK)
                            {
                                a.OnUnloaded = tef.SelectedEvent;
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
        }*/

        /// <summary>
        /// See issue #13 for reason of these invalid methods etc.
        /// </summary>
        [Obsolete("addCustomUserEvent() : File is completly invalid, because with the original method signature it is impossible to implement.", true)]
        public /*File*/void addCustomUserEvent()
        { throw new NotImplementedException(); }

        /// <summary>
        /// See issue #13 for reason of these invalid methods etc.
        /// </summary>
        [Obsolete("editCustomUserEvent(customUserEvent : File) is completly invalid, because with the original method signature it is impossible to implement.", true)]
        public void editCustomUserEvent(/*File customUserEvent*/)
        { throw new NotImplementedException(); }

        /// <summary>
        /// Event which catches all changes of the propertyGrid, which should have an impact of the elements on the preview panel.
        /// </summary>
        /// <param name="sender">Sender ~...</param>
        /// <param name="e">Event argument</param>
        private void changedProperty(object sender, PropertyValueChangedEventArgs e)
        {
            /*==============================================================================*/
            // Changes which should be undone if set to null

            // Checks if a image/option path is set to null
            if (string.Equals(e.ChangedItem.Label.ToString(), "Options", StringComparison.Ordinal))
            {
                if (string.Equals((string)e.ChangedItem.Value, "", StringComparison.Ordinal))
                {
                    ((Chart)ew.CurrentElement).ResFilePath = e.OldValue.ToString();
                    return;
                }
                this.ew.PreviewController.reloadPreviewable((AbstractAugmentation)this.ew.CurrentElement);
                this.ew.PreviewController.setCurrentElement(this.ew.CurrentElement);
                //this replaces TODO: check if there are unexpected changes or if its even neede
                /* PictureBox temp = this.ew.PreviewController.findElement(this.ew.CurrentElement);
                temp.BorderStyle = BorderStyle.Fixed3D;
                temp.BringToFront();*/
            }

            // Checks if picturePath has been changed
            if (String.Equals(e.ChangedItem.Label.ToString(), "SensorCosID", StringComparison.Ordinal))
            {
                if (string.Equals((string)e.ChangedItem.Value, "", StringComparison.Ordinal))
                    ((Abstract2DTrackable)ew.CurrentElement).SensorCosID = e.OldValue.ToString();
                else
                {
                    string newName = e.ChangedItem.Value.ToString().Replace(" ", "_");
                    ((Abstract2DTrackable)ew.CurrentElement).SensorCosID = e.OldValue.ToString();
                    
                    foreach (AbstractTrackable track in ew.project.Trackables)
                    {
                        //check if the trackable has the same id
                        if (track != null && track is Abstract2DTrackable && ((Abstract2DTrackable)track).SensorCosID.Equals(newName))
                            return;
                        //check if any augmentation associated with this trackable has the same id
                        if (track != null && track.Augmentations.Exists(x => ((AbstractAugmentation)x).ID.Equals(newName)))
                            return;
                    }
                    ew.PreviewController.renamePreviewable(ew.CurrentElement, newName);
                    ew.Cmb_editor_properties_objectSelection.Items.Clear();
                    ew.Cmb_editor_properties_objectSelection.Items.Clear();
                    ew.PreviewController.updateElementCombobox((AbstractTrackable)ew.CurrentElement);
                    ew.Cmb_editor_properties_objectSelection.SelectedItem = ew.CurrentElement;
                }
                return;
            }

            if (String.Equals(e.ChangedItem.Label.ToString(), "ID", StringComparison.Ordinal))
            {
                if (string.Equals((string)e.ChangedItem.Value, "", StringComparison.Ordinal))
                    ((AbstractAugmentation)ew.CurrentElement).ID = e.OldValue.ToString();
                else
                {
                    string newName = e.ChangedItem.Value.ToString().Replace(" ", "_");
                    ((AbstractAugmentation)ew.CurrentElement).ID = e.OldValue.ToString();

                    foreach (AbstractTrackable track in ew.project.Trackables)
	                {
                        //check if the trackable has the same id
                        if (track != null && track is Abstract2DTrackable && ((Abstract2DTrackable)track).SensorCosID == newName)
                            return;
                        //check if any augmentation associated with this trackable has the same id
                        if (track != null && track.Augmentations.Exists(x => ((AbstractAugmentation)x).ID.Equals(newName)))
                            return;
	                }

                    ew.PreviewController.renamePreviewable(ew.CurrentElement, newName);
                    ew.Cmb_editor_properties_objectSelection.Items.Clear();
                    ew.PreviewController.updateElementCombobox(((AbstractAugmentation)ew.CurrentElement).Trackable);
                    ew.Cmb_editor_properties_objectSelection.SelectedItem = ew.CurrentElement;
                }
                return;
            }

            // Checks if picturePath has been changed
            if (String.Equals(e.ChangedItem.Label.ToString(), "PicturePath", StringComparison.Ordinal))
            {
                if (string.Equals((string)e.ChangedItem.Value, "", StringComparison.Ordinal))
                    ((PictureMarker)ew.CurrentElement).PicturePath = e.OldValue.ToString();
                else
                {
                    ((PictureMarker)ew.CurrentElement).PicturePath = e.ChangedItem.Value.ToString();
                    this.ew.PreviewController.reloadPreviewable(this.ew.CurrentElement);
                    this.ew.PreviewController.setCurrentElement(this.ew.CurrentElement);
                }

                return;
            }

            // Checks if imagePath has been changed (it's indifferent if it's an augmentation or trackable)
            if (String.Equals(e.ChangedItem.Label.ToString(), "ImagePath", StringComparison.Ordinal))
            {
                if (string.Equals((string)e.ChangedItem.Value, "", StringComparison.Ordinal))
                {
                    ((ImageTrackable)ew.CurrentElement).ImagePath = e.OldValue.ToString();
                }
                else
                {
                    ((ImageTrackable)ew.CurrentElement).ImagePath = e.ChangedItem.Value.ToString();
                    this.ew.PreviewController.reloadPreviewable(this.ew.CurrentElement);
                    this.ew.PreviewController.setCurrentElement(this.ew.CurrentElement);
                }
                return;
            }

            // Checks if resFilePath has been changed
            if (String.Equals(e.ChangedItem.Label.ToString(), "ResFilePath", StringComparison.Ordinal))
            {
                if (string.Equals((string)e.ChangedItem.Value, "", StringComparison.Ordinal))
                {
                    ((Abstract2DAugmentation)ew.CurrentElement).ResFilePath = e.OldValue.ToString();
                }
                else
                {
                    ((Abstract2DAugmentation)ew.CurrentElement).ResFilePath = e.ChangedItem.Value.ToString();
                    this.ew.PreviewController.reloadPreviewable((AbstractAugmentation)this.ew.CurrentElement);
                    this.ew.PreviewController.setCurrentElement(this.ew.CurrentElement);
                    //this replaces TODO: check if there are unexpected changes or if its even neede
                    /* PictureBox temp = this.ew.PreviewController.findElement(this.ew.CurrentElement);
                    temp.BorderStyle = BorderStyle.Fixed3D;
                    temp.BringToFront();*/
                }
            }


            // Checks if Data has been changed (only for FileSource for now)
            if (string.Equals(e.ChangedItem.Label.ToString(), "Data", StringComparison.Ordinal))
            {
                if (ew.CurrentElement is FileSource)
                {
                    if (string.Equals((string)e.ChangedItem.Value, "", StringComparison.Ordinal))
                    {
                        ((FileSource)ew.CurrentElement).Data = e.OldValue.ToString();
                        //this replaces TODO: check if there are unexpected changes or if its even neede
                        /* PictureBox temp = this.ew.PreviewController.findElement(this.ew.CurrentElement);
                        temp.BorderStyle = BorderStyle.Fixed3D;
                        temp.BringToFront();*/
                    }
                    else
                    {
                        ((FileSource)ew.CurrentElement).Data = e.ChangedItem.Value.ToString();
                        this.ew.PreviewController.reloadPreviewable(((FileSource)ew.CurrentElement).Augmentation);
                        this.ew.PreviewController.setCurrentElement(this.ew.CurrentElement);
                    }
                }
                return;
                if (((Chart)ew.CurrentElement).Source is FileSource)
                {
                    if (string.Equals((string)e.ChangedItem.Value, "", StringComparison.Ordinal))
                    {
                        ((FileSource)((Chart)ew.CurrentElement).Source).Data = e.OldValue.ToString();
                        //this replaces TODO: check if there are unexpected changes or if its even neede
                        /* PictureBox temp = this.ew.PreviewController.findElement(this.ew.CurrentElement);
                        temp.BorderStyle = BorderStyle.Fixed3D;
                        temp.BringToFront();*/
                    } 
                    else
                    {
                        ((FileSource)((Chart)ew.CurrentElement).Source).Data = e.ChangedItem.Value.ToString();
                        this.ew.PreviewController.reloadPreviewable((Chart)this.ew.CurrentElement);
                        this.ew.PreviewController.setCurrentElement(this.ew.CurrentElement);
                    }
                }
                return;
            }

            // Checks if Url has been changed (only for DbSource for now)
            if (string.Equals(e.ChangedItem.Label.ToString(), "Url", StringComparison.Ordinal))
            {
                if (ew.CurrentElement is DbSource)
                {
                    if (string.Equals((string)e.ChangedItem.Value, "", StringComparison.Ordinal))
                    {
                        ((DbSource)ew.CurrentElement).Url = e.OldValue.ToString();
                        //this replaces TODO: check if there are unexpected changes or if its even neede
                        /* PictureBox temp = this.ew.PreviewController.findElement(this.ew.CurrentElement);
                        temp.BorderStyle = BorderStyle.Fixed3D;
                        temp.BringToFront();*/
                    }
                    else
                    {
                        ((DbSource)ew.CurrentElement).Url = e.ChangedItem.Value.ToString();
                        this.ew.PreviewController.reloadPreviewable(((DbSource)ew.CurrentElement).Augmentation);
                        this.ew.PreviewController.setCurrentElement(ew.CurrentElement);
                    }
                }
                return;
                if (((Chart)ew.CurrentElement).Source is DbSource)
                {
                    if (string.Equals((string)e.ChangedItem.Value, "", StringComparison.Ordinal))
                    {
                        ((DbSource)((Chart)ew.CurrentElement).Source).Url = e.OldValue.ToString();
                        //this replaces TODO: check if there are unexpected changes or if its even neede
                        /* PictureBox temp = this.ew.PreviewController.findElement(this.ew.CurrentElement);
                        temp.BorderStyle = BorderStyle.Fixed3D;
                        temp.BringToFront();*/
                    }
                    else
                    {
                        ((DbSource)((Chart)ew.CurrentElement).Source).Url = e.ChangedItem.Value.ToString();
                        this.ew.PreviewController.reloadPreviewable((Chart)this.ew.CurrentElement);
                        this.ew.PreviewController.setCurrentElement(this.ew.CurrentElement);
                    }
                }
                return;
            }

            /*=================================================================================*/
            // Changes which enable/disables button if set to null or was null before the change

            // Checks if Query has been changed
            if (String.Equals(e.ChangedItem.Label.ToString(), "Query", StringComparison.Ordinal))
            {
                if (string.Equals((string)e.ChangedItem.Value, "", StringComparison.Ordinal))
                {
                    //attaches eventhandlers each time when the site is loaded, which does reloadPreviewable
                    //(ew.PreviewController.findElement(ew.CurrentElement).ContextMenu).MenuItems[5].Enabled = false;
                    this.ew.PreviewController.reloadPreviewable((Chart)this.ew.CurrentElement);
                    this.ew.PreviewController.setCurrentElement(this.ew.CurrentElement);
                    //this replaces TODO: check if there are unexpected changes or if its even neede
                    /* PictureBox temp = this.ew.PreviewController.findElement(this.ew.CurrentElement);
                    temp.BorderStyle = BorderStyle.Fixed3D;
                    temp.BringToFront();*/

                    return;
                }

                if (e.OldValue == null || string.Equals(e.OldValue.ToString(), "", StringComparison.Ordinal))
                {
                    //attaches eventhandlers each time when the site is loaded, which does reloadPreviewable
                    //(ew.PreviewController.findElement(ew.CurrentElement).ContextMenu).MenuItems[5].Enabled = true;
                    this.ew.PreviewController.reloadPreviewable((Chart)this.ew.CurrentElement);
                    this.ew.PreviewController.setCurrentElement(this.ew.CurrentElement);
                    //this replaces TODO: check if there are unexpected changes or if its even neede
                    /* PictureBox temp = this.ew.PreviewController.findElement(this.ew.CurrentElement);
                    temp.BorderStyle = BorderStyle.Fixed3D;
                    temp.BringToFront();*/

                    return;
                }
            }

            /*=================================================================================*/
            // Changes which changes things in the previewPanel

            // Checks if X/Y position has been changed
            if ((String.Equals(e.ChangedItem.Label.ToString(), "X", StringComparison.Ordinal)
                || String.Equals(e.ChangedItem.Label.ToString(), "Y", StringComparison.Ordinal))
                && !(String.Equals(e.ChangedItem.Parent.Label.ToString(), "Translation", StringComparison.Ordinal)))
            {
                if (this.ew.CurrentElement is AbstractAugmentation)
                {
                    AbstractAugmentation aug = (AbstractAugmentation)this.ew.CurrentElement;
                    this.ew.PreviewController.setAugOnNativeVector(aug, aug.Translation);
                    this.ew.PreviewController.reloadPreviewable((AbstractAugmentation)this.ew.CurrentElement);
                    this.ew.PreviewController.setCurrentElement(this.ew.CurrentElement);
                }
                return;
            }

            // Checks if height/width has been changed
            if (ew.CurrentElement is Abstract2DAugmentation)
            {
                if (string.Equals(e.ChangedItem.Label.ToString(), "Height", StringComparison.Ordinal)
                    || string.Equals(e.ChangedItem.Label.ToString(), "Width", StringComparison.Ordinal))
                {
                    this.ew.PreviewController.reloadPreviewable((AbstractAugmentation)this.ew.CurrentElement);
                    this.ew.PreviewController.setCurrentElement(this.ew.CurrentElement);
                    //this replaces TODO: check if there are unexpected changes or if its even neede
                    /* PictureBox temp = this.ew.PreviewController.findElement(this.ew.CurrentElement);
                    temp.BorderStyle = BorderStyle.Fixed3D;
                    temp.BringToFront();*/
                }
            }

            // Checks if the position of a htmlAUgmentation has been changed.
            if (string.Equals(e.ChangedItem.Label.ToString(), "Top", StringComparison.Ordinal)
                || string.Equals(e.ChangedItem.Label.ToString(), "Left", StringComparison.Ordinal))
            {
                this.ew.PreviewController.reloadPreviewable((AbstractAugmentation)this.ew.CurrentElement);
                this.ew.PreviewController.setCurrentElement(this.ew.CurrentElement);
            }

            // Checks if the size of a trackable has been changed.
            if (string.Equals(e.ChangedItem.Label.ToString(), "Size", StringComparison.Ordinal))
            {
                this.ew.PreviewController.reloadPreviewable(this.ew.CurrentElement);
                this.ew.PreviewController.setCurrentElement(this.ew.CurrentElement);
                return;
            }

            /*=================================================================================*/
            // Miscellaneous changes


            // Checks if the MatrixID has been changed. If changed, it checks, if the id is equal to another id in
            // the project to generate a new id. This should prevent having two identical id's. 
            if (string.Equals(e.ChangedItem.Label.ToString(), "MatrixID", StringComparison.Ordinal))
            {
                if (ew.project.existTrackable((int)e.ChangedItem.Value))
                {
                    IDMarker marker = (IDMarker)ew.CurrentElement;
                    marker.MatrixID = ew.project.nextID();
                }
                //ATTENTION could cause onforsighted problems
                //this.ew.PreviewController.findElement(temp).Image = this.ew.PreviewController.scaleIPreviewable(temp);
                //this.ew.PreviewController.reloadPreviewPanel(this.ew.PreviewController.Index);
                this.ew.PreviewController.reloadPreviewable(this.ew.CurrentElement);
                this.ew.PreviewController.setCurrentElement(this.ew.CurrentElement);
                return;
            }
        }
    }
}