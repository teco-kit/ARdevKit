using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ARdevKit.Model.Project;
using System.Collections;
using ARdevKit;
using ARdevKit.Controller.EditorController;
using ARdevKit.View;
using ARdevKit.Properties;
using System.Drawing.Drawing2D;
using System.Runtime.CompilerServices;
using System.IO;
using System.Diagnostics;

/// <summary>
/// The class PreviewController manages all things which are in contact with the PreviewhtmlView. Here are all methods, who influence the PreviewhtmlView.
/// </summary>
public class HTMLPreviewController
{
    #region fields with getters and setters
    private EditorWindow editorWindow;
    
    private WebBrowser htmlPreview;

    private double scale;
    
    public AbstractTrackable trackable { get; set; }

    public MetaCategory currentMetaCategory { get; set; }

    public int index { get; set; }

    #endregion

    public HTMLPreviewController(EditorWindow editorWindow)
    {
        // TODO: Complete member initialization
        this.editorWindow = editorWindow;
        this.htmlPreview = this.editorWindow.HTMLPreview;
        this.currentMetaCategory = new MetaCategory();
        this.index = 0;
        this.trackable = null;
        this.editorWindow.project.Trackables.Add(trackable);
        this.editorWindow.Tsm_editor_menu_edit_paste.Click += new System.EventHandler(this.paste_augmentation_center);
        this.editorWindow.Tsm_editor_menu_edit_copie.Click += new System.EventHandler(this.copy_augmentation);
        this.editorWindow.Tsm_editor_menu_edit_delete.Click += new System.EventHandler(this.delete_current_element);
    }

    #region Eventhandler
    private void delete_current_element(object sender, EventArgs e)
    {
 	    throw new NotImplementedException();
    }

    private void copy_augmentation(object sender, EventArgs e)
    {
 	    throw new NotImplementedException();
    }

    private void paste_augmentation_center(object sender, EventArgs e)
    {
 	    throw new NotImplementedException();
    }

    private void paste_augmentation(object sender, EventArgs e)
    {
        throw new NotImplementedException();
    }
    #endregion
    internal void updatePreviewPanel()
    {
        //throw new NotImplementedException();
        this.htmlPreview.DocumentText="";
        this.editorWindow.project.Trackables.Add(trackable);
        updateElementCombobox(trackable);
        //ContextMenu cm = new ContextMenu();
        //cm.MenuItems.Add("einfügen", new EventHandler(this.paste_augmentation));
        //cm.MenuItems[0].Enabled = false;
        //this.htmlPreview.ContextMenu = cm;
    }


    private void updateElementCombobox(AbstractTrackable trackable)
    {
        //throw new NotImplementedException();
        if (trackable != null)
        {
            if (editorWindow.Cmb_editor_properties_objectSelection.Items.Count != 1 + trackable.Augmentations.Count + editorWindow.project.Sources.Count)
            {
                editorWindow.Cmb_editor_properties_objectSelection.Items.Clear();
                editorWindow.Cmb_editor_properties_objectSelection.Items.Add(trackable);
                foreach (AbstractAugmentation a in trackable.Augmentations)
                {
                    editorWindow.Cmb_editor_properties_objectSelection.Items.Add(a);
                }
                foreach (AbstractSource s in editorWindow.project.Sources)
                {
                    editorWindow.Cmb_editor_properties_objectSelection.Items.Add(s);
                }
            }
        }
        else
        {
            editorWindow.Cmb_editor_properties_objectSelection.Items.Clear();
        }
    }

    internal void reloadPreviewPanel(int p)
    {
        throw new NotImplementedException();
    }

    internal void removePreviewable(AbstractTrackable abstractTrackable)
    {
        throw new NotImplementedException();
    }

    internal Image scaleBitmap(Bitmap bitmap, int p1, int p2)
    {
        throw new NotImplementedException();
    }

    internal void rescalePreviewPanel()
    {
        throw new NotImplementedException();
    }

    internal void addPictureBox(IPreviewable previewable, Vector3D vector3D)
    {
        throw new NotImplementedException();
    }

    internal PictureBox findBox(IPreviewable previewable)
    {
        throw new NotImplementedException();
    }

    internal void reloadPreviewable(AbstractAugmentation abstractAugmentation)
    {
        throw new NotImplementedException();
    }

    internal void rotateAugmentation(IPreviewable previewable)
    {
        throw new NotImplementedException();
    }

    internal void setCurrentElement(IPreviewable temp)
    {
        throw new NotImplementedException();
    }

    internal Image scaleIPreviewable(IPreviewable temp)
    {
        throw new NotImplementedException();
    }
}