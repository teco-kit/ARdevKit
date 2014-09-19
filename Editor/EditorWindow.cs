﻿////////////////////////////////////////////////////////////////////////////////////////////////////
// file:	editorwindow.cs
//
// summary:	Implements the editorwindow class
// This is the main window of the entire program, as well as the main entrance point.
// The Editor Window controlls the main tasks and delegates complex task to the other dedicated controllers.
// It also controls the communication between the model and the view, meaning it builds the view and saves/deletes/changes data in the model.
// 
////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using ARdevKit.Model.Project;
using Controller.EditorController;
using ARdevKit.Controller.ProjectController;
using ARdevKit.Controller.EditorController;
using ARdevKit.Controller.Connections.DeviceConnection;
using ARdevKit.Controller.TestController;
using ARdevKit.View;
using System.IO;
using ARdevKit.Model.Project.File;
using System.Drawing.Printing;
using System.Security.Cryptography;

namespace ARdevKit
{
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Form for viewing the editor. This is the main form of the program.
    /// </summary>
    ////////////////////////////////////////////////////////////////////////////////////////////////////

    public partial class EditorWindow : Form
    {
        /// <summary>
        /// A callback function used to enable a menu item
        /// </summary>
        private delegate void SetEnabledCallback();

        /// <summary>
        /// The checksum of the project. It is needed to determine whether there has been made a change to the project or not.
        /// </summary>
        /// <remarks>geht 20.02.2014 13:06</remarks>
        private string checksum;

        /// <summary>
        /// The debug window, which is used by the DeviceConnectionController
        /// </summary>
        private DebugWindow debugWindow;

        /// <summary>
        /// Gets the debug window.
        /// </summary>
        /// <value>
        /// The debug window.
        /// </value>
        public DebugWindow DebugWindow
        {
            get { return debugWindow; }
        }

        /// <summary>
        /// The minscreenwidht
        /// </summary>
        /// <remarks>geht 28.01.2014 15:12</remarks>
        private const uint MINSCREENWIDHT = 320;

        /// <summary>
        /// The minscreenheight
        /// </summary>
        /// <remarks>geht 28.01.2014 15:12</remarks>
        private const uint MINSCREENHEIGHT = 240;

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Categories the element belongs to.
        /// List of scene element categories. 
        /// </summary>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        private List<SceneElementCategory> elementCategories;

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Gets or sets the categories the element belongs to.
        /// </summary>
        ///
        /// <value>
        /// The element categories.
        /// </value>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        internal List<SceneElementCategory> ElementCategories
        {
            get { return elementCategories; }
            set { elementCategories = value; }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// The element selection controller.
        /// </summary>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        private ElementSelectionController elementSelectionController;

        /// <summary>
        /// Gets the element selection controller.
        /// </summary>
        /// <value>
        /// The element selection controller.
        /// </value>
        /// <remarks>geht 19.01.2014 23:06</remarks>

        internal ElementSelectionController ElementSelectionController
        {
            get { return elementSelectionController; }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// The preview controller.
        /// </summary>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        private PreviewController previewController;

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets or sets the previewController. </summary>
        ///
        /// <value> The previewController. </value>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public PreviewController PreviewController
        {
            get { return previewController; }
            set { previewController = value; }
        }

        private HTMLPreviewController htmlPreviewController;

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets or sets the HTMLPreviewController. </summary>
        ///
        /// <value> The htmlPreviewController. </value>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public HTMLPreviewController HTMLPreviewController
        {
            get { return htmlPreviewController; }
            set { htmlPreviewController = value; }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets or sets the project. </summary>
        ///
        /// <value> The project. </value>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public Project project { get; set; }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// The property controller.
        /// </summary>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        private PropertyController propertyController;

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// The device connection controller.
        /// </summary>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        private DeviceConnectionController deviceConnectionController;

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// The export visitor.
        /// </summary>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        private ExportVisitor exportVisitor;

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// The current element.
        /// </summary>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        private IPreviewable currentElement;

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Gets or sets the current element.
        /// </summary>
        ///
        /// <value>
        /// The current element.
        /// </value>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        internal IPreviewable CurrentElement
        {
            get { return currentElement; }
            set { currentElement = value; }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Default constructor. initializes components on startup.
        /// </summary>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public EditorWindow()
        {
            InitializeComponent();
            createNewProject("");
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Event handler. Called by tsm_editor_menu_file_new for click events.
        /// </summary>
        ///
        /// <exception cref="NotImplementedException">  Thrown when the requested operation is
        ///                                             unimplemented. </exception>
        ///
        /// <param name="sender">   Source of the event. </param>
        /// <param name="e">        Event information. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        private void tsm_editor_menu_file_new_Click(object sender, EventArgs e)
        {
            if (ProjectChanged())
            {
                if (MessageBox.Show("Möchten Sie das aktuelle Projekt abspeichern, bevor ein neues angelegt wird?", "Projekt speichern?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    try
                    {
                        this.ExportProject(true);
                    }
                    catch (ArgumentNullException ae)
                    {
                        Debug.WriteLine(ae.StackTrace);
                    }
                }
            }

            createNewProject("");
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Event handler. Called by tsm_editor_menu_file_exit for click events. exits the program.
        /// </summary>
        ///
        /// <param name="sender">   Source of the event. </param>
        /// <param name="e">        Event information. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        private void tsm_editor_menu_file_exit_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Application.Exit();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Event handler. Called by tsm_editor_menu_test_startWithImage for click events. Starts the test
        /// mode for images.
        /// </summary>
        ///
        /// <param name="sender">   Source of the event. </param>
        /// <param name="e">        Event information. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        private void tsm_editor_menu_test_startImage_Click(object sender, EventArgs e)
        {
            if (project.Trackables != null && project.Trackables.Count > 0 && project.Trackables[0] != null)
            {
                try
                {
                    TestController.StartPlayer(this, project, TestController.IMAGE, (int)project.Screensize.Width, (int)project.Screensize.Height, tsm_editor_menu_test_togleDebug.Checked);
                }
                catch (OperationCanceledException oae)
                {
                    MessageBox.Show("Vorgang wurde abgebrochen");
                }
            }
            else
                MessageBox.Show("Keine Szene zum Testen vorhanden");
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Event handler. Called by tsm_editor_menu_test_startWithVideo for click events. Starts the test
        /// mode for images.
        /// </summary>
        ///
        /// <param name="sender">   Source of the event. </param>
        /// <param name="e">        Event information. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        private void tsm_editor_menu_test_startVideo_Click(object sender, EventArgs e)
        {
            if (project.Trackables != null && project.Trackables.Count > 0 && project.Trackables[0] != null)
                TestController.StartPlayer(this, project, TestController.VIDEO, (int)project.Screensize.Width, (int)project.Screensize.Height, tsm_editor_menu_test_togleDebug.Checked);
            else
                MessageBox.Show("Keine Szene zum Testen vorhanden");
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Event handler. Called by tsm_editor_menu_test_startWithVirtualCamera for click events. Starts the test
        /// mode using a virtual camera.
        /// </summary>
        ///
        /// <param name="sender">   Source of the event. </param>
        /// <param name="e">        Event information. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        private void tsm_editor_menu_test_startWithVirtualCamera_Click(object sender, EventArgs e)
        {
            if (project.Trackables != null && project.Trackables.Count > 0 && project.Trackables[0] != null)
                TestController.StartPlayer(this, project, TestController.CAMERA, (int)project.Screensize.Width, (int)project.Screensize.Height, tsm_editor_menu_test_togleDebug.Checked);
            else
                MessageBox.Show("Keine Szene zum Testen vorhanden");
        }

        /// <summary>
        /// This method is used to tell the editorWindow that the player was started.
        /// </summary>
        public void PlayerStarted()
        {
            this.tsm_editor_menu_file.Enabled = false;
        }

        /// <summary>
        /// This method is used to tell the editorWindow that the player has been closed.
        /// </summary>
        public void PlayerClosed()
        {
            if (this.tsm_editor_menu_file.GetCurrentParent().InvokeRequired)
            {
                SetEnabledCallback d = new SetEnabledCallback(PlayerClosed);
                this.Invoke(d, new object[] { });
            }
            else
                this.tsm_editor_menu_file.Enabled = true;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        ///     Event handler. This eventHandler is to change the choosen scene from the
        ///     SceneSelectionPanel. The handler will load an existent scene, which was created in the
        ///     past. If you change the scene from a new created scene, which is empty this scene will be
        ///     delete.
        /// </summary>
        ///
        /// <remarks>   Lizzard, 1/16/2014. </remarks>
        ///
        /// <param name="sender">   Source of the event. </param>
        /// <param name="e">        Event information. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        private void btn_editor_scene_scene_change(object sender, EventArgs e)
        {
            int temp = Convert.ToInt32(((Button)sender).Text);
            if (this.project.Trackables.Count > 1)
            {
                this.htmlPreviewController.reloadPreviewPanel(temp - 1);
                this.PropertyGrid1.SelectedObject = null;

            }
            else
            {
                this.htmlPreviewController.reloadPreviewPanel(0);
                this.PropertyGrid1.SelectedObject = null;
            }

            this.resetButton();
            this.setButton(((Button)sender).Text);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        ///     Event handler. This eventHandler is to add a scene to the SceneSelectionPanel. This
        ///     funktion adds a new Button to the SceneSelectionPanel and set a new Scene to the
        ///     PreviewPanel.
        /// </summary>
        ///
        /// <remarks>   Lizzard, 1/16/2014. </remarks>
        ///
        /// <param name="sender">   Source of the event. </param>
        /// <param name="e">        Event information. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        private void btn_editor_scene_scene_new(object sender, EventArgs e)
        {
            if (this.project.Trackables.Count < 10)
            {
                Button tempButton = new Button();
                tempButton.Location = new System.Drawing.Point(54 + (52 * project.Trackables.Count), 4);
                tempButton.Name = "btn_editor_scene_scene_" + (this.project.Trackables.Count + 1);
                tempButton.Size = new System.Drawing.Size(46, 45);
                tempButton.TabIndex = 1;
                tempButton.Text = Convert.ToString(this.project.Trackables.Count + 1);
                tempButton.UseVisualStyleBackColor = true;
                tempButton.Click += new System.EventHandler(this.btn_editor_scene_scene_change);
                tempButton.ContextMenu = new ContextMenu();
                tempButton.ContextMenu.Tag = tempButton;
                tempButton.ContextMenu.MenuItems.Add("Duplicate", new EventHandler(this.pnl_editor_scene_duplicate));

                this.pnl_editor_scenes.Controls.Add(tempButton);
                this.htmlPreviewController.reloadPreviewPanel(this.project.Trackables.Count);
                this.PropertyGrid1.SelectedObject = null;
                this.resetButton();
                this.setButton(tempButton.Text);
            }
            else
            {
                MessageBox.Show("Sie können nicht mehr als 10 Szenen pro Project haben.");
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        ///     Event handler. This eventHandler is to remove a scene from the SceneSelectionPanel. This
        ///     Functions clean the scene, if there is only one scene, else the funktion removes the
        ///     panel and set scene 1 to the current scene.
        /// </summary>
        ///
        /// <remarks>   Lizzard, 1/16/2014. </remarks>
        ///
        /// <param name="sender">   Source of the event. </param>
        /// <param name="e">        Event information. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        private void btn_editor_scene_scene_remove(object sender, EventArgs e)
        {
            if (this.project.Trackables.Count > 1)
            {
                this.project.Trackables.Remove(this.htmlPreviewController.trackable);
                this.htmlPreviewController.trackable = this.project.Trackables[0];
                this.reloadSelectionPanel();
                this.htmlPreviewController.index = -1;
                this.htmlPreviewController.reloadPreviewPanel(0);
                if (!this.project.hasTrackable())
                {
                    this.ElementSelectionController.setElementEnable(typeof(PictureMarker), true);
                    this.ElementSelectionController.setElementEnable(typeof(IDMarker), true);
                }
            }
            else
            {
                if (this.project.Trackables[0] != null)
                {
                    this.project.Trackables[0] = null;
                    this.htmlPreviewController.currentMetaCategory = MetaCategory.Trackable;
                    this.htmlPreviewController.removePreviewable(this.htmlPreviewController.trackable);
                    if (!this.project.hasTrackable())
                    {
                        this.ElementSelectionController.setElementEnable(typeof(PictureMarker), true);
                        this.ElementSelectionController.setElementEnable(typeof(IDMarker), true);
                    }
                }
                
            }
            this.resetButton();
            this.setButton(Convert.ToString("1"));
            this.PropertyGrid1.SelectedObject = null;
        }

        /// <summary>
        /// Creates the new project. Initialized with the given name.
        /// </summary>
        /// <param name="name">Name of the new project.</param>
        public void createNewProject(String name)
        {
            this.initializeEmptyProject(name);
            this.initializeControllers();
            this.updatePanels();
            this.checksum = project.getChecksum();
        }

        /// <summary>
        /// Exports the project to the project path using <see cref="ExportVisitor" />.
        /// </summary>
        /// <param name="save">if set to <c>true</c> [save].</param>
        /// <returns>true, if export is valid</returns>
        /// <remarks>
        /// geht 19.01.2014 22:10
        /// </remarks>
        public bool Export(bool save)
        {
            try
            {
                try
                {
                    exportVisitor = new ExportVisitor();
                    project.Accept(exportVisitor);
                    foreach (AbstractFile file in exportVisitor.Files)
                    {
                        file.Save();
                    }
                }
                catch (DirectoryNotFoundException de)
                {
                    Debug.WriteLine(de.StackTrace);
                    return false;
                }
                catch (OperationCanceledException oce)
                {
                    MessageBox.Show("Exportvorgang abgebrochen");
                    return false;
                }
                catch (NullReferenceException ne)
                {
                    Debug.WriteLine(ne.StackTrace);
                    return false;
                }
                if (!save)
                    MessageBox.Show("Projekt wurde exportiert!", "Export");
                return exportVisitor.ExportIsValid;
            }
            catch (ArgumentNullException ae)
            {
                Debug.WriteLine(ae.StackTrace);
                return false;
            }
        }

        /// <summary>
        /// Loads the project. Opens a file dialog to select a saved project.
        /// </summary>
        /// <remarks>geht 19.01.2014 17:55</remarks>
        public void loadProject()
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "ARdevkit Projektdatei|*.ardev";
            openFileDialog1.Title = "Projekt öffnen";
            openFileDialog1.ShowDialog();
            try
            {
                initializeLoadedProject(SaveLoadController.loadProject(openFileDialog1.FileName));
                this.initializeControllers();
                this.updatePanels();
                htmlPreviewController.index = -1;
                htmlPreviewController.reloadPreviewPanel(0);
                this.updateSceneSelectionPanel();
                this.updateScreenSize();
                this.checksum = project.getChecksum();
            }
            catch (System.ArgumentException a)
            {
                
                if (a.Message.Equals("Projekt-Datei beschädigt"))
                {
                    MessageBox.Show(a.Message, "Error");
                    this.createNewProject("");
                }
            }
            catch (System.Runtime.Serialization.SerializationException se)
            {
                MessageBox.Show("Die Ausgewählte Datei ist nicht erkennbar binär kodiert", "Error");
            }
        }

        /**
         * <summary>    Registers all SceneElements that are available. </summary>
         *
         * <remarks>    Robin, 14.01.2014. </remarks>
         */

        public void registerElements()
        {
            SceneElementCategory sources = new SceneElementCategory(MetaCategory.Source, "Sources");
            sources.addElement(new SceneElement("Datasource", new DbSource(), this));
            sources.addElement(new SceneElement("File Source", new FileSource(""), this));
            SceneElementCategory augmentations = new SceneElementCategory(MetaCategory.Augmentation, "Augmentations");
            augmentations.addElement(new SceneElement("Chart", new Chart(), this));
            augmentations.addElement(new SceneElement("Image", new ImageAugmentation(), this));
            augmentations.addElement(new SceneElement("Video", new VideoAugmentation(), this));
            SceneElementCategory trackables = new SceneElementCategory(MetaCategory.Trackable, "Trackables");
            trackables.addElement(new SceneElement("Picture Marker", new PictureMarker(), this));
            trackables.addElement(new SceneElement("IDMarker", new IDMarker(1), this));
            trackables.addElement(new SceneElement("Image Trackable", new ImageTrackable(), this));
            addCategory(trackables);
            addCategory(augmentations);
            addCategory(sources);
            IDFactory.Reset();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Exports the project. Opens file save dialog if project Path isn't set yet. calls save(String path)
        /// and Export(bool save).
        /// </summary>
        /// <param name="save">True if an *.ardev file should be generated</param>
        /// <returns>true, if export is valid</returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <remarks>
        /// geht, 17.01.2014.
        /// </remarks>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public bool ExportProject(bool save)
        {
            this.project.OldProjectPath = this.project.ProjectPath;
            bool isValid = true;
            if (project.Sensor == null)
            {
                MessageBox.Show("Sie müssen mindestens ein Trackable hinzufügen!", "Achtung", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                Debug.WriteLine("You have to add at least one trackable first!");
                throw new ArgumentNullException();
            }
            else
            {
                if (save && project.Name.Equals(""))
                {
                    SaveFileDialog saveFileDialog = new SaveFileDialog();
                    if (project.ProjectPath == null)
                        saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    else
                        saveFileDialog.InitialDirectory = project.ProjectPath;
                    saveFileDialog.Filter = "ARdevkit Projektdatei|*.ardev";
                    saveFileDialog.Title = "Projekt speichern";
                    saveFileDialog.ShowDialog();
                    try
                    {
                        project.ProjectPath = Path.GetDirectoryName(saveFileDialog.FileName);
                        project.Name = Path.GetFileNameWithoutExtension(saveFileDialog.FileName);
                        isValid = this.Export(save);
                        if (isValid)
                            this.Save(project.ProjectPath);
                    }
                    catch (System.ArgumentException)
                    {
                        project.ProjectPath = null;
                    }
                }
                else if (!save && project.ProjectPath == null)
                {
                    FolderBrowserDialog exportDialog = new FolderBrowserDialog();
                    exportDialog.RootFolder = Environment.SpecialFolder.MyDocuments;
                    DialogResult exportDialogResult = DialogResult.OK;
                    try
                    {
                        if ((exportDialogResult = exportDialog.ShowDialog()) == DialogResult.OK)
                        {
                            project.ProjectPath = exportDialog.SelectedPath;
                            isValid = this.Export(save);
                        }
                    }
                    catch (System.ArgumentException)
                    {
                        project.ProjectPath = null;
                    }
                }
                else if (ProjectChanged())
                {
                    isValid = this.Export(save);
                    if (isValid && save)
                        this.Save(project.ProjectPath);
                }
            }
            if (!isValid)
                MessageBox.Show("Beim " + (save ? "Speichern" : "Export") + " ist ein Fehler aufgetreten. Das Projekt wird möglicherweise nicht richtig funktionieren.", "Error!");
            return isValid;
        }

        /// <summary>
        /// Saves project at the specified path (*.ardev file).
        /// </summary>
        /// <param name="path">The path.</param>
        private void Save(String path)
        {
            SaveLoadController.saveProject(this.project);
            checksum = this.project.getChecksum();
        }

        /// <summary>
        /// Updates the element selection panel.
        /// (Refreshes the View)
        /// </summary>
        public void updateElementSelectionPanel()
        {
            this.Cmb_editor_selection_toolSelection.Items.Clear();
            this.elementSelectionController.populateComboBox();
            this.elementSelectionController.updateElementSelectionPanel();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        ///     This functions Updates the scene PreviewPanel. Alle elements will be removed and
        ///     all current elements will add again to the panel.
        /// </summary>
        ///
        /// <remarks>   Lizzard, 1/16/2014. </remarks>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public void updatePreviewPanel()
        {
            this.htmlPreviewController.updatePreviewPanel();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        ///     This functions Updates the scene SceneSelectionPanel. Alle elements will be removed and
        ///     all current elements will add again to the panel.
        /// </summary>
        ///
        /// <remarks>   Lizzard, 1/16/2014. </remarks>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public void updateSceneSelectionPanel()
        {
            for (int i = 0; i < this.project.Trackables.Count; i++)
            {
                if (this.project.Trackables[i] == null)
                {
                    this.project.Trackables.Remove(this.project.Trackables[i]);
                }
            }
            this.pnl_editor_scenes.Controls.Clear();
            this.pnl_editor_scenes.Controls.Add(this.btn_editor_scene_new);
            this.pnl_editor_scenes.Controls.Add(this.btn_editor_scene_delete);

            for (int i = 0; i < this.project.Trackables.Count; i++)
            {
                Button tempButton = new Button();
                tempButton.Location = new System.Drawing.Point(54 + (i * 52), 4);
                tempButton.Name = "btn_editor_scene_scene_" + (this.project.Trackables.Count + 1);
                tempButton.Size = new System.Drawing.Size(46, 45);
                tempButton.Text = Convert.ToString(i + 1);
                tempButton.UseVisualStyleBackColor = true;
                tempButton.Click += new System.EventHandler(this.btn_editor_scene_scene_change);
                tempButton.ContextMenu = new ContextMenu();
                tempButton.ContextMenu.Tag = tempButton;
                tempButton.ContextMenu.MenuItems.Add("Duplicate", new EventHandler(this.pnl_editor_scene_duplicate));

                this.pnl_editor_scenes.Controls.Add(tempButton);
            }
            this.setButton("1");
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Reload selection panel. </summary>
        ///
        /// <remarks>   Lizzard, 1/19/2014. </remarks>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        private void reloadSelectionPanel()
        {
            this.pnl_editor_scenes.Controls.Clear();
            this.pnl_editor_scenes.Controls.Add(this.btn_editor_scene_new);
            this.pnl_editor_scenes.Controls.Add(this.btn_editor_scene_delete);

            for (int i = 0; i < this.project.Trackables.Count; i++)
            {
                Button tempButton = new Button();
                tempButton.Location = new System.Drawing.Point(54 + (i * 52), 4);
                tempButton.Name = "btn_editor_scene_scene_" + (this.project.Trackables.Count + 1);
                tempButton.Size = new System.Drawing.Size(46, 45);
                tempButton.Text = Convert.ToString(i + 1);
                tempButton.UseVisualStyleBackColor = true;
                tempButton.Click += new System.EventHandler(this.btn_editor_scene_scene_change);
                tempButton.ContextMenu = new ContextMenu();
                tempButton.ContextMenu.Tag = tempButton;
                tempButton.ContextMenu.MenuItems.Add("Duplicate", new EventHandler(this.pnl_editor_scene_duplicate));

                this.pnl_editor_scenes.Controls.Add(tempButton);
            }
        }

        /**
         * <summary>    Adds a category to the element categories. </summary>
         *
         * <remarks>    Robin, 18.01.2014. </remarks>
         *
         * <param name="category">  The category. </param>
         */

        private void addCategory(SceneElementCategory category)
        {
            elementCategories.Add(category);
        }

        /**
         * <summary>
         *  Event handler. Called by cmb_editor_selection_toolSelection for selected index changed
         *  events.
         * </summary>
         *
         * <remarks>    Robin, 18.01.2014. </remarks>
         *
         * <param name="sender">    Source of the event. </param>
         * <param name="e">         Event information. </param>
         */

        private void cmb_editor_selection_toolSelection_SelectedIndexChanged(object sender, EventArgs e)
        {
            elementSelectionController.updateElementSelectionPanel();
            htmlPreviewController.currentMetaCategory = ((SceneElementCategoryPanel)cmb_editor_selection_toolSelection.SelectedItem).Category.Category;
        }

        /**
         * <summary>    Event handler. Called by pnl_editor_preview for drag enter events. </summary>
         *
         * <remarks>    Robin, 18.01.2014. </remarks>
         *
         * <param name="sender">    Source of the event. </param>
         * <param name="e">         Drag event information. </param>
         */

        private void pnl_editor_preview_DragEnter(object sender, DragEventArgs e)
        {
            if (htmlPreviewController.currentMetaCategory != MetaCategory.Source)
            {
                e.Effect = DragDropEffects.Move;
            }
        }

        /**
         * <summary>    Event handler. Called by pnl_editor_preview for drag drop events when an element is droped on the preview. </summary>
         *
         * <remarks>    Robin, 18.01.2014. </remarks>
         *
         * <param name="sender">    Source of the event. </param>
         * <param name="e">         Drag event information. </param>
         */

        private void pnl_editor_preview_DragDrop(object sender, DragEventArgs e)
        {
            if (htmlPreviewController.currentMetaCategory != MetaCategory.Source)
            {
                if (((ElementIcon)e.Data.GetData(typeof(ElementIcon)) != null))
                {
                    ElementIcon icon = (ElementIcon)e.Data.GetData(typeof(ElementIcon));
                    Point p = pnl_editor_preview.PointToClient(Cursor.Position);

                    IPreviewable element = (IPreviewable)icon.Element.Prototype.Clone();
                    icon.EditorWindow.PreviewController.addPreviewable(element, new Vector3D(p.X, p.Y, 0));
                }
            }
        }

        /// <summary>
        /// Initializes the controllers.
        /// </summary>
        private void initializeControllers()
        {
            this.elementSelectionController = new ElementSelectionController(this);
            this.previewController = new PreviewController(this);
            this.htmlPreviewController = new HTMLPreviewController(this);
            this.propertyController = new PropertyController(this);
            this.propertyGrid1.SelectedObject = null;
            this.deviceConnectionController = new DeviceConnectionController(this);
        }

        /// <summary>
        /// Initializes the empty project.
        /// </summary>
        /// <param name="projectname">The projectname.</param>
        private void initializeEmptyProject(String projectname)
        {
            this.project = new Project(projectname);
            this.project.ProjectPath = null;
            this.elementCategories = new List<SceneElementCategory>();
            this.exportVisitor = new ExportVisitor();
            this.currentElement = null;
            this.project.Screensize = new ScreenSize();
            this.project.Screensize.Height = Convert.ToUInt32(pnl_editor_preview.Size.Height);
            this.project.Screensize.Width = Convert.ToUInt32(pnl_editor_preview.Size.Width);
            this.project.Screensize.SizeChanged += new System.EventHandler(this.pnl_editor_preview_SizeChanged);
            registerElements();
        }

        /// <summary>
        /// Initializes the loaded project.
        /// </summary>
        /// <param name="p">The p.</param>
        private void initializeLoadedProject(Project p)
        {
            this.project = p;
            this.elementCategories = new List<SceneElementCategory>();
            this.exportVisitor = new ExportVisitor();
            this.currentElement = null;
            registerElements();
        }

        /// <summary>
        /// Updates the panels.
        /// </summary>
        private void updatePanels()
        {
            this.updateElementSelectionPanel();
            this.updatePreviewPanel();
            this.updateSceneSelectionPanel();
        }

        /// <summary>
        /// Updates the size of the screen.
        /// </summary>
        /// <remarks>geht 26.01.2014 20:20</remarks>
        private void updateScreenSize()
        {
            if (project.Screensize.Width < MINSCREENWIDHT)
            {
                this.project.Screensize.Width = MINSCREENWIDHT;
                this.pnl_editor_preview.Size = new Size((int)project.Screensize.Width, (int)project.Screensize.Height);
                this.htmlPreviewController.rescalePreviewPanel();
            }
            else if (project.Screensize.Height < MINSCREENHEIGHT)
            {
                this.project.Screensize.Height = MINSCREENHEIGHT;
                this.pnl_editor_preview.Size = new Size((int)project.Screensize.Width, (int)project.Screensize.Height);
                this.htmlPreviewController.rescalePreviewPanel();
            }
            else
            {
                this.pnl_editor_preview.Size = new Size((int)project.Screensize.Width, (int)project.Screensize.Height);
                this.htmlPreviewController.rescalePreviewPanel();
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Event handler. Called by tsm_editor_menu_file_save for click events.
        /// Click on "Speichern" dialog.
        /// </summary>
        ///
        /// <remarks>
        /// geht, 17.01.2014.
        /// </remarks>
        ///
        /// <param name="sender">   Source of the event. </param>
        /// <param name="e">        Event information. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        private void tsm_editor_menu_file_save_Click(object sender, EventArgs e)
        {
            try
            {
                this.ExportProject(true);
            }
            catch (ArgumentNullException ae)
            {
                Debug.WriteLine(ae.StackTrace);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Event handler. Called by tsm_editor_menu_file_saveAs for click events.
        /// Click on "Speichern unter" dialog.
        /// </summary>
        ///
        /// <remarks>
        /// geht, 17.01.2014.
        /// </remarks>
        ///
        /// <param name="sender">   Source of the event. </param>
        /// <param name="e">        Event information. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        private void tsm_editor_menu_file_saveAs_Click(object sender, EventArgs e)
        {
            this.project.Name = "";
            try
            {
                this.ExportProject(true);
            }
            catch (ArgumentNullException ae)
            {
                Debug.WriteLine(ae.StackTrace);
            }
        }

        /// <summary>
        /// Handles the open Click event of the tsm_editor_menu_file_open_Click control.
        /// Gets called when the save button is clicked.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void tsm_editor_menu_file_open_Click_1(object sender, EventArgs e)
        {
            if (ProjectChanged())
            {
                if (MessageBox.Show("Möchten Sie das aktuelle Projekt abspeichern, bevor ein anderes geöffnet wird?", "Projekt speichern?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    try
                    {
                        this.ExportProject(true);
                    }
                    catch (ArgumentNullException ae)
                    {
                        Debug.WriteLine(ae.StackTrace);
                    }
                }
            }

            this.loadProject();
        }

        /// <summary>
        /// Handles the Click event of the tsm_editor_menu_file_export control.
        /// Gets called when the export button is clicked.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void tsm_editor_menu_file_export_Click(object sender, EventArgs e)
        {
            this.ExportProject(false);
        }

        /// <summary>
        /// Handles the Click event of the tsm_editor_menu_help_info control.
        /// Gets called when the info button is clicked
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void tsm_editor_menu_help_info_Click(object sender, EventArgs e)
        {
            MessageBox.Show("ARdevKit Version 0.3 alpha \n\n Rüdiger Heres \n Jonas Lachowitzer \n Robin Lamberti \n Tuong-Vu Mai \n Imanuel Richter\n Marwin Rieger \n\n", "Info");
        }


        /// <summary>
        /// Sets the current scene button to the Color ControlDark.
        /// </summary>
        /// <param name="text">The text.</param>
        private void setButton(string text)
        {
            foreach (Control comp in pnl_editor_scenes.Controls)
            {
                if (comp.Text == text)
                {
                    ((Button)comp).BackColor = SystemColors.ControlDark;
                }
            }
        }

        /// <summary>
        /// Resets the SceneSelectionsButtons to normal Color.
        /// </summary>
        private void resetButton()
        {
            foreach (Control comp in pnl_editor_scenes.Controls)
            {
                if (((Button)comp).BackColor == SystemColors.ControlDark)
                {
                    ((Button)comp).BackColor = SystemColors.Control;
                }
            }
        }

        /// <summary>
        /// Sets the PasteButton enabled.
        /// </summary>
        public void setPasteButtonEnabled()
        {
            this.tsm_editor_menu_edit_paste.Enabled = true;
            this.pnl_editor_preview.ContextMenu.MenuItems[0].Enabled = true;
        }

        /// <summary>
        /// Handles the Click event of the pnl_editor_preview control.
        /// Used for changing the screensize.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <remarks>geht 26.01.2014 20:21</remarks>
        private void pnl_editor_preview_Click(object sender, EventArgs e)
        {
            propertyGrid1.SelectedObject = project.Screensize;
            propertyGrid1.PropertySort = PropertySort.NoSort;
            this.previewController.setCurrentElement(null);
            this.tsm_editor_menu_edit_delete.Enabled = false;
        }

        /// <summary>
        /// Handles the SizeChanged event of the pnl_editor_preview control.
        /// Is called when the screensize has been changed.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <remarks>geht 26.01.2014 20:21</remarks>
        private void pnl_editor_preview_SizeChanged(object sender, EventArgs e)
        {
            Debug.WriteLine("ScreenSize has been changed!");
            this.updateScreenSize();
        }

        /// <summary>
        /// Handles the duplicate event of the pnl_editor_scene control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void pnl_editor_scene_duplicate(object sender, EventArgs e)
        {
            int temp = Convert.ToInt32(((Button)((ContextMenu)((MenuItem)sender).Parent).Tag).Text);
            AbstractTrackable tempTrack;

            if (this.project.Trackables[temp - 1] != null)
            {
                tempTrack = (AbstractTrackable)this.project.Trackables[temp - 1].Clone();
                if (tempTrack.initElement(this))
                {
                    for (int i = 0; i < tempTrack.Augmentations.Count; i++)
                    {
                        tempTrack.Augmentations[i] = (AbstractAugmentation)tempTrack.Augmentations[i].Clone();
                    }
                    if (!this.project.existTrackable(tempTrack))
                    {
                        tempTrack.vector = new Vector3D(this.pnl_editor_preview.Size.Width / 2, this.pnl_editor_preview.Size.Height / 2, 0);
                        this.project.Trackables.Add(tempTrack);
                        foreach (AbstractAugmentation a in tempTrack.Augmentations)
                        {
                            a.initElement(this);
                        }
                        this.updateSceneSelectionPanel();
                    }
                }
            }
        }

        /// <summary>
        /// counts the trackables for printing purposes.
        /// </summary>
        /// <remarks>geht 01.03.2014 16:33</remarks>
        private int trackablePCounter = 0;

        /// <summary>
        /// Handles the Click event of the trackableDruckenToolStripMenuItem control.
        /// Gets called when the trackable Drucken button is clicked.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <remarks>geht 04.02.2014 15:14</remarks>
        private void trackableDruckenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (project.hasTrackable())
            {
                Debug.WriteLine("printing out trackables");

                trackablePCounter = 0;
                PrintDocument pd = new PrintDocument();
                pd.PrintPage += new PrintPageEventHandler(Print_Page);

                PrintDialog printd = new PrintDialog();
                printd.Document = pd;

                if (printd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    PrintPreviewDialog dlg = new PrintPreviewDialog();
                    dlg.Document = printd.Document;

                    if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        pd.Print();
                }
            }
            else
            {
                Debug.WriteLine("there are no trackables to print out...");
            }
        }

        /// <summary>
        /// Handles the Page event of the Print control.
        /// prints one page for each trackable.
        /// </summary>
        /// <param name="o">The source of the event.</param>
        /// <param name="e">The <see cref="PrintPageEventArgs"/> instance containing the event data.</param>
        /// <remarks>geht 04.02.2014 15:14</remarks>
        private void Print_Page(object o, PrintPageEventArgs e)
        {
            float x = e.MarginBounds.Left;
            float y = e.MarginBounds.Top;

            if (project.Trackables[trackablePCounter] != null)
            {
                if (project.Trackables[trackablePCounter] is IDMarker)
                {
                    IDMarker temp = (IDMarker)project.Trackables[trackablePCounter];
                    int dpi = (int)(Math.Sqrt(Math.Pow(e.PageSettings.PrinterResolution.X, 2) + Math.Pow(e.PageSettings.PrinterResolution.Y, 2)));
                    e.Graphics.DrawImage(htmlPreviewController.scaleBitmap(temp.getPreview(project.ProjectPath), (int)((dpi * temp.Size) / 254), (int)((dpi * temp.Size) / 254)), x, y);
                }
                else
                    e.Graphics.DrawImage(project.Trackables[trackablePCounter].getPreview(project.ProjectPath), x, y);
            }

            if (project.Trackables[trackablePCounter] != project.Trackables.Last())
            {
                trackablePCounter++;
                e.HasMorePages = true;
                return;
            }

            e.HasMorePages = false;
            trackablePCounter = 0;
        }

        /// <summary>
        /// Handles the FormClosing event of the EditorWindow control.
        /// Displays a save dialog.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="FormClosingEventArgs"/> instance containing the event data.</param>
        /// <remarks>geht 04.02.2014 15:15</remarks>
        private void EditorWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!ProjectChanged())
                return;

            DialogResult dlg = MessageBox.Show("Möchten Sie das aktuelle Projekt abspeichern, bevor ARdevKit beendet wird?", "Projekt speichern?", MessageBoxButtons.YesNoCancel);
            if (dlg == DialogResult.Yes)
            {
                e.Cancel = true;
                try
                {
                    this.ExportProject(true);
                    e.Cancel = false;
                }
                catch (ArgumentNullException ae)
                {
                    Debug.WriteLine(ae.StackTrace);
                }
            }
            else if (dlg == DialogResult.No)
            {
                e.Cancel = false;
            }
            else
            {
                e.Cancel = true;
                return;
            }
            DialogResult result = DialogResult.Retry;
            while (Directory.Exists("tmp") && result == System.Windows.Forms.DialogResult.Retry)
            {
                try
                {
                    Directory.Delete("tmp", true);
                } catch (IOException ioe)
                {
                   result = MessageBox.Show("Could not delete tmp folder.\n" + ioe.Message, "Error!", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
                } catch (UnauthorizedAccessException uae)
                {
                    MessageBox.Show("Could not delete tmp folder.\n" + uae.Message, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// reports whether the project has been changed or not after the last saving.
        /// returns false when the project hasn't been changed.
        /// returns true when the project has been changed.
        /// </summary>
        /// <returns></returns>
        /// <remarks>geht 20.02.2014 14:15</remarks>
        private bool ProjectChanged()
        {
            if (checksum.Equals(this.project.getChecksum()))
                return false;
            else
                return true;
        }

        /// <summary>
        /// Handles the Click event of the tsm_editor_menu_help_help control.
        /// Gets called when the help button is clicked
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void tsm_editor_menu_help_help_Click(object sender, EventArgs e)
        {
            Help.ShowHelp(this, Environment.CurrentDirectory + "\\Documentation.chm", HelpNavigator.TableOfContents);
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the cmb_editor_properties_objectSelection control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void cmb_editor_properties_objectSelection_SelectedIndexChanged(object sender, EventArgs e)
        {
            previewController.setCurrentElement((IPreviewable)cmb_editor_properties_objectSelection.SelectedItem);
        }

        /// <summary>
        /// Reloads the device list.
        /// </summary>
        private void reloadDeviceList()
        {
            DeviceList.Items.Clear();
            try
            {
                deviceConnectionController.refresh();
            }
            catch (System.Net.Sockets.SocketException)
            {
                MessageBox.Show("Es gab ein Problem mit der Netzwerkverbindung, stellen sie sicher, dass kein anderes Programm den benötigten Port belegt");
            }
            List<string> devices = deviceConnectionController.getReportedDevices();
            foreach (string device in devices)
            {
                DeviceList.Items.Add(device);
            }
            if (DeviceList.Items.Count > 0)
            {
                DeviceList.SelectedItem = devices[0];
            }
        }

        /// <summary>
        /// Handles the Click event of the tsm_editor_menu_file control.
        /// Gets called when the refresh devices button is pushed.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void tsm_editor_menu_file_Click(object sender, EventArgs e)
        {
            reloadDeviceList();
        }

        /// <summary>
        /// Handles the Click event of the refreshDeviceList control.
        /// Gets called when the refresh devices button is pushed.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void refreshDeviceList_Click(object sender, EventArgs e)
        {
            reloadDeviceList();
        }

        /// <summary>
        /// Handles the Click event of the sendProject control.
        /// Gets called when the send Project button is clicked.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void sendProject_Click(object sender, EventArgs e)
        {
            if (project.Trackables != null && project.Trackables.Count > 0 && project.Trackables[0] != null)
            {
                if (DeviceList.Items.Count != 0)
                {
                    if (DeviceList.SelectedItem != null && DeviceList.SelectedIndex >= 0)
                    {
                        try
                        {
                            if (deviceConnectionController.sendProject(DeviceList.SelectedIndex))
                            {
                                MessageBox.Show("Das Projekt wurde versand.");
                            }
                            else
                            {
                                MessageBox.Show("Das Projekt wurde nicht versand.");
                            }
                        }
                        catch (System.Net.Sockets.SocketException)
                        {
                            MessageBox.Show("Es gab ein Verbindungsproblem. Bitte überprüfen sie ihre Netzwerkeinstellungen.");
                        }
                        catch (System.IO.IOException)
                        {
                            MessageBox.Show("Es gab ein Verbindungsproblem. Anscheinend wurde die Verbindung gelöst.");
                        }
                        catch (System.ArgumentException)
                        {
                            MessageBox.Show("Das Projekt muss zuerst exportiert werden.");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Es ist kein Gerät ausgewählt, wählen sie es in der Liste aus");
                    }
                }
                else
                {
                    MessageBox.Show("Es ist kein Gerät verfügbar, nutzen sie die Aktualisierungsfunktion und stellen sie sicher, dass die Geräte mit dem netzwerk verbunden sind");
                }
            }
            else
            {
                MessageBox.Show("Es ist kein Projekt zum versenden erstellt worden");
            }
        }

        /// <summary>
        /// Handles the Click event of the DeviceDebug control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void DeviceDebug_Click(object sender, EventArgs e)
        {
            if (DeviceList.Items.Count != 0)
            {
                if (DeviceList.SelectedItem != null && DeviceList.SelectedIndex >= 0)
                {
                    int index = DeviceList.SelectedIndex;
                    debugWindow = new DebugWindow(deviceConnectionController);
                    debugWindow.Show();
                    PlayerStarted();
                    Task.Factory.StartNew(() => deviceConnectionController.sendDebug(index));
                }
                else
                {
                    MessageBox.Show("Es ist kein Gerät ausgewählt, wählen sie es in der Liste aus");
                }
            }
            else
            {
                MessageBox.Show("Es ist kein Gerät verfügbar, nutzen sie die Aktualisierungsfunktion und stellen sie sicher, dass die Geräte mit dem netzwerk verbunden sind");
            }
        }

    }
}
