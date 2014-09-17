﻿using System.Drawing;
using System.Windows.Forms;
namespace ARdevKit
{
    partial class EditorWindow
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.pnl_editor_selection = new System.Windows.Forms.Panel();
            this.cmb_editor_selection_toolSelection = new System.Windows.Forms.ComboBox();
            this.pnl_editor_status = new System.Windows.Forms.Panel();
            this.lbl_version = new System.Windows.Forms.Label();
            this.btn_editor_scene_new = new System.Windows.Forms.Button();
            this.btn_editor_scene_delete = new System.Windows.Forms.Button();
            this.pnl_editor_scenes = new System.Windows.Forms.Panel();
            this.pnl_editor_properties = new System.Windows.Forms.Panel();
            this.cmb_editor_properties_objectSelection = new System.Windows.Forms.ComboBox();
            this.propertyGrid1 = new System.Windows.Forms.PropertyGrid();
            this.mst_editor_menu = new System.Windows.Forms.MenuStrip();
            this.tsm_editor_menu_file = new System.Windows.Forms.ToolStripMenuItem();
            this.tsm_editor_menu_file_new = new System.Windows.Forms.ToolStripMenuItem();
            this.tsm_editor_menu_file_open = new System.Windows.Forms.ToolStripMenuItem();
            this.tss_editor_menu_file_opnen_save = new System.Windows.Forms.ToolStripSeparator();
            this.tsm_editor_menu_file_save = new System.Windows.Forms.ToolStripMenuItem();
            this.tsm_editor_menu_file_saveAs = new System.Windows.Forms.ToolStripMenuItem();
            this.tsm_editor_menu_file_export = new System.Windows.Forms.ToolStripMenuItem();
            this.tss_editor_menu_file_export_sendTo = new System.Windows.Forms.ToolStripSeparator();
            this.sendProjectMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.DeviceList = new System.Windows.Forms.ToolStripComboBox();
            this.refreshDeviceList = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.sendProject = new System.Windows.Forms.ToolStripMenuItem();
            this.DeviceDebug = new System.Windows.Forms.ToolStripMenuItem();
            this.tss_editor_menu_file_connection_exit = new System.Windows.Forms.ToolStripSeparator();
            this.tsm_editor_menu_file_exit = new System.Windows.Forms.ToolStripMenuItem();
            this.tsm_editor_menu_edit = new System.Windows.Forms.ToolStripMenuItem();
            this.tsm_editor_menu_edit_copie = new System.Windows.Forms.ToolStripMenuItem();
            this.tsm_editor_menu_edit_paste = new System.Windows.Forms.ToolStripMenuItem();
            this.tsm_editor_menu_edit_delete = new System.Windows.Forms.ToolStripMenuItem();
            this.tsm_editor_menu_test = new System.Windows.Forms.ToolStripMenuItem();
            this.tsm_editor_menu_test_startImage = new System.Windows.Forms.ToolStripMenuItem();
            this.tsm_editor_menu_test_startVideo = new System.Windows.Forms.ToolStripMenuItem();
            this.tsm_editor_menu_test_startWithVirtualCamera = new System.Windows.Forms.ToolStripMenuItem();
            this.tss_editor_meu_test_loadVideo_togleDebug = new System.Windows.Forms.ToolStripSeparator();
            this.tsm_editor_menu_test_togleDebug = new System.Windows.Forms.ToolStripMenuItem();
            this.trackableDruckenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tsm_editor_menu_help = new System.Windows.Forms.ToolStripMenuItem();
            this.tsm_editor_menu_help_help = new System.Windows.Forms.ToolStripMenuItem();
            this.tsm_editor_menu_help_info = new System.Windows.Forms.ToolStripMenuItem();
            this.pnl_editor_preview = new System.Windows.Forms.Panel();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.miniToolStrip = new System.Windows.Forms.MenuStrip();
            this.panel1 = new System.Windows.Forms.Panel();
            this.pnl_editor_selection.SuspendLayout();
            this.pnl_editor_status.SuspendLayout();
            this.pnl_editor_scenes.SuspendLayout();
            this.pnl_editor_properties.SuspendLayout();
            this.mst_editor_menu.SuspendLayout();
            this.pnl_editor_preview.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnl_editor_selection
            // 
            this.pnl_editor_selection.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.pnl_editor_selection.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnl_editor_selection.Controls.Add(this.cmb_editor_selection_toolSelection);
            this.pnl_editor_selection.Location = new System.Drawing.Point(0, 27);
            this.pnl_editor_selection.Name = "pnl_editor_selection";
            this.pnl_editor_selection.Size = new System.Drawing.Size(135, 673);
            this.pnl_editor_selection.TabIndex = 1;
            // 
            // cmb_editor_selection_toolSelection
            // 
            this.cmb_editor_selection_toolSelection.DisplayMember = "CategoryName";
            this.cmb_editor_selection_toolSelection.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmb_editor_selection_toolSelection.DropDownWidth = 128;
            this.cmb_editor_selection_toolSelection.FormattingEnabled = true;
            this.cmb_editor_selection_toolSelection.ItemHeight = 13;
            this.cmb_editor_selection_toolSelection.Location = new System.Drawing.Point(3, 3);
            this.cmb_editor_selection_toolSelection.MaxDropDownItems = 4;
            this.cmb_editor_selection_toolSelection.Name = "cmb_editor_selection_toolSelection";
            this.cmb_editor_selection_toolSelection.Size = new System.Drawing.Size(128, 21);
            this.cmb_editor_selection_toolSelection.TabIndex = 0;
            this.cmb_editor_selection_toolSelection.SelectedIndexChanged += new System.EventHandler(this.cmb_editor_selection_toolSelection_SelectedIndexChanged);
            // 
            // pnl_editor_status
            // 
            this.pnl_editor_status.Controls.Add(this.lbl_version);
            this.pnl_editor_status.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnl_editor_status.Location = new System.Drawing.Point(0, 707);
            this.pnl_editor_status.Name = "pnl_editor_status";
            this.pnl_editor_status.Size = new System.Drawing.Size(1008, 23);
            this.pnl_editor_status.TabIndex = 3;
            // 
            // lbl_version
            // 
            this.lbl_version.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lbl_version.AutoSize = true;
            this.lbl_version.Location = new System.Drawing.Point(931, 10);
            this.lbl_version.Name = "lbl_version";
            this.lbl_version.Size = new System.Drawing.Size(76, 13);
            this.lbl_version.TabIndex = 0;
            this.lbl_version.Text = "ARdevKit v0.2";
            // 
            // btn_editor_scene_new
            // 
            this.btn_editor_scene_new.BackColor = System.Drawing.Color.DarkGray;
            this.btn_editor_scene_new.Location = new System.Drawing.Point(3, 3);
            this.btn_editor_scene_new.Name = "btn_editor_scene_new";
            this.btn_editor_scene_new.Size = new System.Drawing.Size(45, 46);
            this.btn_editor_scene_new.TabIndex = 0;
            this.btn_editor_scene_new.Text = "+";
            this.btn_editor_scene_new.UseVisualStyleBackColor = true;
            this.btn_editor_scene_new.Click += new System.EventHandler(this.btn_editor_scene_scene_new);
            // 
            // btn_editor_scene_delete
            // 
            this.btn_editor_scene_delete.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_editor_scene_delete.Location = new System.Drawing.Point(611, 3);
            this.btn_editor_scene_delete.Name = "btn_editor_scene_delete";
            this.btn_editor_scene_delete.Size = new System.Drawing.Size(45, 46);
            this.btn_editor_scene_delete.TabIndex = 2;
            this.btn_editor_scene_delete.Text = "-";
            this.btn_editor_scene_delete.UseVisualStyleBackColor = true;
            this.btn_editor_scene_delete.Click += new System.EventHandler(this.btn_editor_scene_scene_remove);
            // 
            // pnl_editor_scenes
            // 
            this.pnl_editor_scenes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnl_editor_scenes.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnl_editor_scenes.Controls.Add(this.btn_editor_scene_delete);
            this.pnl_editor_scenes.Controls.Add(this.btn_editor_scene_new);
            this.pnl_editor_scenes.Location = new System.Drawing.Point(141, 645);
            this.pnl_editor_scenes.Name = "pnl_editor_scenes";
            this.pnl_editor_scenes.Size = new System.Drawing.Size(661, 55);
            this.pnl_editor_scenes.TabIndex = 2;
            // 
            // pnl_editor_properties
            // 
            this.pnl_editor_properties.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnl_editor_properties.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnl_editor_properties.Controls.Add(this.cmb_editor_properties_objectSelection);
            this.pnl_editor_properties.Controls.Add(this.propertyGrid1);
            this.pnl_editor_properties.Location = new System.Drawing.Point(808, 27);
            this.pnl_editor_properties.Name = "pnl_editor_properties";
            this.pnl_editor_properties.Size = new System.Drawing.Size(200, 673);
            this.pnl_editor_properties.TabIndex = 2;
            // 
            // cmb_editor_properties_objectSelection
            // 
            this.cmb_editor_properties_objectSelection.Dock = System.Windows.Forms.DockStyle.Right;
            this.cmb_editor_properties_objectSelection.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmb_editor_properties_objectSelection.FormattingEnabled = true;
            this.cmb_editor_properties_objectSelection.Location = new System.Drawing.Point(0, 0);
            this.cmb_editor_properties_objectSelection.MaxDropDownItems = 1;
            this.cmb_editor_properties_objectSelection.Name = "cmb_editor_properties_objectSelection";
            this.cmb_editor_properties_objectSelection.Size = new System.Drawing.Size(198, 21);
            this.cmb_editor_properties_objectSelection.Sorted = true;
            this.cmb_editor_properties_objectSelection.TabIndex = 1;
            this.cmb_editor_properties_objectSelection.SelectedIndexChanged += new System.EventHandler(this.cmb_editor_properties_objectSelection_SelectedIndexChanged);
            // 
            // propertyGrid1
            // 
            this.propertyGrid1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.propertyGrid1.Location = new System.Drawing.Point(-1, 20);
            this.propertyGrid1.Name = "propertyGrid1";
            this.propertyGrid1.Size = new System.Drawing.Size(199, 647);
            this.propertyGrid1.TabIndex = 2;
            // 
            // mst_editor_menu
            // 
            this.mst_editor_menu.AccessibleRole = System.Windows.Forms.AccessibleRole.MenuBar;
            this.mst_editor_menu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsm_editor_menu_file,
            this.tsm_editor_menu_edit,
            this.tsm_editor_menu_test,
            this.tsm_editor_menu_help});
            this.mst_editor_menu.Location = new System.Drawing.Point(0, 0);
            this.mst_editor_menu.Name = "mst_editor_menu";
            this.mst_editor_menu.Size = new System.Drawing.Size(1008, 24);
            this.mst_editor_menu.TabIndex = 0;
            this.mst_editor_menu.Text = "menuStrip1";
            // 
            // tsm_editor_menu_file
            // 
            this.tsm_editor_menu_file.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsm_editor_menu_file_new,
            this.tsm_editor_menu_file_open,
            this.tss_editor_menu_file_opnen_save,
            this.tsm_editor_menu_file_save,
            this.tsm_editor_menu_file_saveAs,
            this.tsm_editor_menu_file_export,
            this.tss_editor_menu_file_export_sendTo,
            this.sendProjectMenu,
            this.tss_editor_menu_file_connection_exit,
            this.tsm_editor_menu_file_exit});
            this.tsm_editor_menu_file.Name = "tsm_editor_menu_file";
            this.tsm_editor_menu_file.Size = new System.Drawing.Size(46, 20);
            this.tsm_editor_menu_file.Text = "Datei";
            this.tsm_editor_menu_file.Click += new System.EventHandler(this.tsm_editor_menu_file_Click);
            // 
            // tsm_editor_menu_file_new
            // 
            this.tsm_editor_menu_file_new.Name = "tsm_editor_menu_file_new";
            this.tsm_editor_menu_file_new.ShortcutKeyDisplayString = "STRG+N";
            this.tsm_editor_menu_file_new.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this.tsm_editor_menu_file_new.Size = new System.Drawing.Size(175, 22);
            this.tsm_editor_menu_file_new.Text = "Neu";
            this.tsm_editor_menu_file_new.Click += new System.EventHandler(this.tsm_editor_menu_file_new_Click);
            // 
            // tsm_editor_menu_file_open
            // 
            this.tsm_editor_menu_file_open.Name = "tsm_editor_menu_file_open";
            this.tsm_editor_menu_file_open.ShortcutKeyDisplayString = "STRG+O";
            this.tsm_editor_menu_file_open.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.tsm_editor_menu_file_open.Size = new System.Drawing.Size(175, 22);
            this.tsm_editor_menu_file_open.Text = "Öffnen";
            this.tsm_editor_menu_file_open.Click += new System.EventHandler(this.tsm_editor_menu_file_open_Click_1);
            // 
            // tss_editor_menu_file_opnen_save
            // 
            this.tss_editor_menu_file_opnen_save.Name = "tss_editor_menu_file_opnen_save";
            this.tss_editor_menu_file_opnen_save.Size = new System.Drawing.Size(172, 6);
            // 
            // tsm_editor_menu_file_save
            // 
            this.tsm_editor_menu_file_save.Name = "tsm_editor_menu_file_save";
            this.tsm_editor_menu_file_save.ShortcutKeyDisplayString = "STRG+S";
            this.tsm_editor_menu_file_save.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.tsm_editor_menu_file_save.Size = new System.Drawing.Size(175, 22);
            this.tsm_editor_menu_file_save.Text = "Speichern";
            this.tsm_editor_menu_file_save.Click += new System.EventHandler(this.tsm_editor_menu_file_save_Click);
            // 
            // tsm_editor_menu_file_saveAs
            // 
            this.tsm_editor_menu_file_saveAs.Name = "tsm_editor_menu_file_saveAs";
            this.tsm_editor_menu_file_saveAs.Size = new System.Drawing.Size(175, 22);
            this.tsm_editor_menu_file_saveAs.Text = "Speichern unter...";
            this.tsm_editor_menu_file_saveAs.Click += new System.EventHandler(this.tsm_editor_menu_file_saveAs_Click);
            // 
            // tsm_editor_menu_file_export
            // 
            this.tsm_editor_menu_file_export.Name = "tsm_editor_menu_file_export";
            this.tsm_editor_menu_file_export.Size = new System.Drawing.Size(175, 22);
            this.tsm_editor_menu_file_export.Text = "Exportieren";
            this.tsm_editor_menu_file_export.Click += new System.EventHandler(this.tsm_editor_menu_file_export_Click);
            // 
            // tss_editor_menu_file_export_sendTo
            // 
            this.tss_editor_menu_file_export_sendTo.Name = "tss_editor_menu_file_export_sendTo";
            this.tss_editor_menu_file_export_sendTo.Size = new System.Drawing.Size(172, 6);
            // 
            // sendProjectMenu
            // 
            this.sendProjectMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.DeviceList,
            this.refreshDeviceList,
            this.toolStripSeparator2,
            this.sendProject,
            this.DeviceDebug});
            this.sendProjectMenu.Name = "sendProjectMenu";
            this.sendProjectMenu.Size = new System.Drawing.Size(175, 22);
            this.sendProjectMenu.Text = "Projekt versenden";
            // 
            // DeviceList
            // 
            this.DeviceList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.DeviceList.Name = "DeviceList";
            this.DeviceList.Size = new System.Drawing.Size(150, 23);
            // 
            // refreshDeviceList
            // 
            this.refreshDeviceList.Name = "refreshDeviceList";
            this.refreshDeviceList.Size = new System.Drawing.Size(218, 22);
            this.refreshDeviceList.Text = "Liste akualisieren";
            this.refreshDeviceList.Click += new System.EventHandler(this.refreshDeviceList_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(215, 6);
            // 
            // sendProject
            // 
            this.sendProject.Name = "sendProject";
            this.sendProject.Size = new System.Drawing.Size(218, 22);
            this.sendProject.Text = "Projekt an Gerät senden";
            this.sendProject.Click += new System.EventHandler(this.sendProject_Click);
            // 
            // DeviceDebug
            // 
            this.DeviceDebug.Name = "DeviceDebug";
            this.DeviceDebug.Size = new System.Drawing.Size(218, 22);
            this.DeviceDebug.Text = "Gerätedebugmodus starten";
            this.DeviceDebug.Click += new System.EventHandler(this.DeviceDebug_Click);
            // 
            // tss_editor_menu_file_connection_exit
            // 
            this.tss_editor_menu_file_connection_exit.Name = "tss_editor_menu_file_connection_exit";
            this.tss_editor_menu_file_connection_exit.Size = new System.Drawing.Size(172, 6);
            // 
            // tsm_editor_menu_file_exit
            // 
            this.tsm_editor_menu_file_exit.Name = "tsm_editor_menu_file_exit";
            this.tsm_editor_menu_file_exit.ShortcutKeyDisplayString = "STRG+Q";
            this.tsm_editor_menu_file_exit.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Q)));
            this.tsm_editor_menu_file_exit.Size = new System.Drawing.Size(175, 22);
            this.tsm_editor_menu_file_exit.Text = "Beenden";
            this.tsm_editor_menu_file_exit.Click += new System.EventHandler(this.tsm_editor_menu_file_exit_Click);
            // 
            // tsm_editor_menu_edit
            // 
            this.tsm_editor_menu_edit.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsm_editor_menu_edit_copie,
            this.tsm_editor_menu_edit_paste,
            this.tsm_editor_menu_edit_delete});
            this.tsm_editor_menu_edit.Name = "tsm_editor_menu_edit";
            this.tsm_editor_menu_edit.Size = new System.Drawing.Size(75, 20);
            this.tsm_editor_menu_edit.Text = "Bearbeiten";
            // 
            // tsm_editor_menu_edit_copie
            // 
            this.tsm_editor_menu_edit_copie.Enabled = false;
            this.tsm_editor_menu_edit_copie.Name = "tsm_editor_menu_edit_copie";
            this.tsm_editor_menu_edit_copie.ShortcutKeyDisplayString = "STRG+C";
            this.tsm_editor_menu_edit_copie.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.tsm_editor_menu_edit_copie.Size = new System.Drawing.Size(181, 22);
            this.tsm_editor_menu_edit_copie.Text = "Kopieren";
            // 
            // tsm_editor_menu_edit_paste
            // 
            this.tsm_editor_menu_edit_paste.Enabled = false;
            this.tsm_editor_menu_edit_paste.Name = "tsm_editor_menu_edit_paste";
            this.tsm_editor_menu_edit_paste.ShortcutKeyDisplayString = "STRG+V";
            this.tsm_editor_menu_edit_paste.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V)));
            this.tsm_editor_menu_edit_paste.Size = new System.Drawing.Size(181, 22);
            this.tsm_editor_menu_edit_paste.Text = "Einfügen";
            // 
            // tsm_editor_menu_edit_delete
            // 
            this.tsm_editor_menu_edit_delete.Enabled = false;
            this.tsm_editor_menu_edit_delete.Name = "tsm_editor_menu_edit_delete";
            this.tsm_editor_menu_edit_delete.ShortcutKeyDisplayString = "STRG+DEL";
            this.tsm_editor_menu_edit_delete.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Delete)));
            this.tsm_editor_menu_edit_delete.Size = new System.Drawing.Size(181, 22);
            this.tsm_editor_menu_edit_delete.Text = "Löschen";
            // 
            // tsm_editor_menu_test
            // 
            this.tsm_editor_menu_test.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsm_editor_menu_test_startImage,
            this.tsm_editor_menu_test_startVideo,
            this.tsm_editor_menu_test_startWithVirtualCamera,
            this.tss_editor_meu_test_loadVideo_togleDebug,
            this.tsm_editor_menu_test_togleDebug,
            this.trackableDruckenToolStripMenuItem});
            this.tsm_editor_menu_test.Name = "tsm_editor_menu_test";
            this.tsm_editor_menu_test.Size = new System.Drawing.Size(41, 20);
            this.tsm_editor_menu_test.Text = "Test";
            // 
            // tsm_editor_menu_test_startImage
            // 
            this.tsm_editor_menu_test_startImage.Name = "tsm_editor_menu_test_startImage";
            this.tsm_editor_menu_test_startImage.Size = new System.Drawing.Size(176, 22);
            this.tsm_editor_menu_test_startImage.Text = "Bild laden";
            this.tsm_editor_menu_test_startImage.Click += new System.EventHandler(this.tsm_editor_menu_test_startImage_Click);
            // 
            // tsm_editor_menu_test_startVideo
            // 
            this.tsm_editor_menu_test_startVideo.Name = "tsm_editor_menu_test_startVideo";
            this.tsm_editor_menu_test_startVideo.Size = new System.Drawing.Size(176, 22);
            this.tsm_editor_menu_test_startVideo.Text = "Video laden";
            this.tsm_editor_menu_test_startVideo.Click += new System.EventHandler(this.tsm_editor_menu_test_startVideo_Click);
            // 
            // tsm_editor_menu_test_startWithVirtualCamera
            // 
            this.tsm_editor_menu_test_startWithVirtualCamera.Name = "tsm_editor_menu_test_startWithVirtualCamera";
            this.tsm_editor_menu_test_startWithVirtualCamera.Size = new System.Drawing.Size(176, 22);
            this.tsm_editor_menu_test_startWithVirtualCamera.Text = "vCam nutzen";
            this.tsm_editor_menu_test_startWithVirtualCamera.Click += new System.EventHandler(this.tsm_editor_menu_test_startWithVirtualCamera_Click);
            // 
            // tss_editor_meu_test_loadVideo_togleDebug
            // 
            this.tss_editor_meu_test_loadVideo_togleDebug.Name = "tss_editor_meu_test_loadVideo_togleDebug";
            this.tss_editor_meu_test_loadVideo_togleDebug.Size = new System.Drawing.Size(173, 6);
            // 
            // tsm_editor_menu_test_togleDebug
            // 
            this.tsm_editor_menu_test_togleDebug.CheckOnClick = true;
            this.tsm_editor_menu_test_togleDebug.Name = "tsm_editor_menu_test_togleDebug";
            this.tsm_editor_menu_test_togleDebug.Size = new System.Drawing.Size(176, 22);
            this.tsm_editor_menu_test_togleDebug.Text = "Debug";
            // 
            // trackableDruckenToolStripMenuItem
            // 
            this.trackableDruckenToolStripMenuItem.Name = "trackableDruckenToolStripMenuItem";
            this.trackableDruckenToolStripMenuItem.Size = new System.Drawing.Size(176, 22);
            this.trackableDruckenToolStripMenuItem.Text = "Trackables drucken";
            this.trackableDruckenToolStripMenuItem.Click += new System.EventHandler(this.trackableDruckenToolStripMenuItem_Click);
            // 
            // tsm_editor_menu_help
            // 
            this.tsm_editor_menu_help.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsm_editor_menu_help_help,
            this.tsm_editor_menu_help_info});
            this.tsm_editor_menu_help.Name = "tsm_editor_menu_help";
            this.tsm_editor_menu_help.Size = new System.Drawing.Size(44, 20);
            this.tsm_editor_menu_help.Text = "Hilfe";
            // 
            // tsm_editor_menu_help_help
            // 
            this.tsm_editor_menu_help_help.Name = "tsm_editor_menu_help_help";
            this.tsm_editor_menu_help_help.Size = new System.Drawing.Size(99, 22);
            this.tsm_editor_menu_help_help.Text = "Hilfe";
            this.tsm_editor_menu_help_help.Click += new System.EventHandler(this.tsm_editor_menu_help_help_Click);
            // 
            // tsm_editor_menu_help_info
            // 
            this.tsm_editor_menu_help_info.Name = "tsm_editor_menu_help_info";
            this.tsm_editor_menu_help_info.Size = new System.Drawing.Size(99, 22);
            this.tsm_editor_menu_help_info.Text = "Info";
            this.tsm_editor_menu_help_info.Click += new System.EventHandler(this.tsm_editor_menu_help_info_Click);
            // 
            // pnl_editor_preview
            // 
            this.pnl_editor_preview.AllowDrop = true;
            this.pnl_editor_preview.BackColor = System.Drawing.SystemColors.Control;
            this.pnl_editor_preview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnl_editor_preview.Controls.Add(this.menuStrip1);
            this.pnl_editor_preview.Location = new System.Drawing.Point(3, 3);
            this.pnl_editor_preview.MinimumSize = new System.Drawing.Size(320, 240);
            this.pnl_editor_preview.Name = "pnl_editor_preview";
            this.pnl_editor_preview.Size = new System.Drawing.Size(653, 604);
            this.pnl_editor_preview.TabIndex = 3;
            this.pnl_editor_preview.SizeChanged += new System.EventHandler(this.pnl_editor_preview_SizeChanged);
            this.pnl_editor_preview.Click += new System.EventHandler(this.pnl_editor_preview_Click);
            this.pnl_editor_preview.DragDrop += new System.Windows.Forms.DragEventHandler(this.pnl_editor_preview_DragDrop);
            this.pnl_editor_preview.DragEnter += new System.Windows.Forms.DragEventHandler(this.pnl_editor_preview_DragEnter);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(651, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // miniToolStrip
            // 
            this.miniToolStrip.AutoSize = false;
            this.miniToolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this.miniToolStrip.Location = new System.Drawing.Point(6, 2);
            this.miniToolStrip.Name = "miniToolStrip";
            this.miniToolStrip.Size = new System.Drawing.Size(651, 24);
            this.miniToolStrip.TabIndex = 0;
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.AutoScroll = true;
            this.panel1.AutoScrollMargin = new System.Drawing.Size(3, 3);
            this.panel1.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.pnl_editor_preview);
            this.panel1.Location = new System.Drawing.Point(141, 27);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(661, 612);
            this.panel1.TabIndex = 4;
            this.panel1.DragDrop += new System.Windows.Forms.DragEventHandler(this.pnl_editor_preview_DragDrop);
            this.panel1.DragEnter += new System.Windows.Forms.DragEventHandler(this.pnl_editor_preview_DragEnter);
            // 
            // EditorWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1008, 730);
            this.Controls.Add(this.pnl_editor_status);
            this.Controls.Add(this.pnl_editor_scenes);
            this.Controls.Add(this.pnl_editor_properties);
            this.Controls.Add(this.pnl_editor_selection);
            this.Controls.Add(this.mst_editor_menu);
            this.Controls.Add(this.panel1);
            this.MainMenuStrip = this.miniToolStrip;
            this.MaximumSize = new System.Drawing.Size(1920, 1080);
            this.MinimumSize = new System.Drawing.Size(1024, 768);
            this.Name = "EditorWindow";
            this.Text = "ARdevKit";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.EditorWindow_FormClosing);
            this.pnl_editor_selection.ResumeLayout(false);
            this.pnl_editor_status.ResumeLayout(false);
            this.pnl_editor_status.PerformLayout();
            this.pnl_editor_scenes.ResumeLayout(false);
            this.pnl_editor_properties.ResumeLayout(false);
            this.mst_editor_menu.ResumeLayout(false);
            this.mst_editor_menu.PerformLayout();
            this.pnl_editor_preview.ResumeLayout(false);
            this.pnl_editor_preview.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Panel pnl_editor_selection;
        private ComboBox cmb_editor_selection_toolSelection;
        private Panel pnl_editor_status;
        private Label lbl_version;
        private Button btn_editor_scene_new;
        private Button btn_editor_scene_delete;
        private Panel pnl_editor_scenes;
        private Panel pnl_editor_properties;
        private PropertyGrid propertyGrid1;
        private ComboBox cmb_editor_properties_objectSelection;
        private ToolStripMenuItem tsm_editor_menu_file;
        private ToolStripMenuItem tsm_editor_menu_file_new;
        private ToolStripMenuItem tsm_editor_menu_file_open;
        private ToolStripSeparator tss_editor_menu_file_opnen_save;
        private ToolStripMenuItem tsm_editor_menu_file_save;
        private ToolStripMenuItem tsm_editor_menu_file_saveAs;
        private ToolStripMenuItem tsm_editor_menu_file_export;
        private ToolStripSeparator tss_editor_menu_file_export_sendTo;
        private ToolStripMenuItem tsm_editor_menu_file_sendTo;
        private ToolStripMenuItem tsm_editor_menu_sendTo_win8Device;
        private ToolStripSeparator tss_editor_menu_file_sendTo_win8Device_togleDebug;
        private ToolStripMenuItem tsm_editor_menu_file_sendTo_togleDebug;
        private ToolStripMenuItem tsm_editor_menu_file_connection;
        private ToolStripSeparator tss_editor_menu_file_connection_exit;
        private ToolStripMenuItem tsm_editor_menu_file_exit;
        private ToolStripMenuItem tsm_editor_menu_edit;
        private ToolStripMenuItem tsm_editor_menu_edit_copie;
        private ToolStripMenuItem tsm_editor_menu_edit_paste;
        private ToolStripMenuItem tsm_editor_menu_test;
        private ToolStripMenuItem tsm_editor_menu_test_startImage;
        private ToolStripMenuItem tsm_editor_menu_test_startVideo;
        private ToolStripMenuItem tsm_editor_menu_test_startWithVirtualCamera;
        private ToolStripSeparator tss_editor_meu_test_loadVideo_togleDebug;
        private ToolStripMenuItem tsm_editor_menu_test_togleDebug;
        private ToolStripMenuItem tsm_editor_menu_help;
        private ToolStripMenuItem tsm_editor_menu_help_help;
        private ToolStripMenuItem tsm_editor_menu_help_info;
        private MenuStrip mst_editor_menu;
        private ToolStripMenuItem trackableDruckenToolStripMenuItem;
        private ToolStripMenuItem tsm_editor_menu_edit_delete;
        private System.Windows.Forms.Button btn_editor_scene_scene_1;
        private ToolStripMenuItem sendProjectMenu;
        private ToolStripComboBox DeviceList;
        private ToolStripMenuItem refreshDeviceList;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripMenuItem sendProject;
        private ToolStripMenuItem DeviceDebug;
        private Panel pnl_editor_preview;
        private MenuStrip menuStrip1;
        private MenuStrip miniToolStrip;
        private Panel panel1;

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Gets or sets the PropertyGrid1.
        /// </summary>
        ///
        /// <value>
        /// PropertyGrid.
        /// </value>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public System.Windows.Forms.PropertyGrid PropertyGrid1
        {
            get { return propertyGrid1; }
            set { propertyGrid1 = value; }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Gets or sets the pnl editor preview.
        /// </summary>
        ///
        /// <value>
        /// The pnl editor preview.
        /// </value>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public System.Windows.Forms.Panel Pnl_editor_preview
        {
            get { return pnl_editor_preview; }
            set { pnl_editor_preview = value; }
        }

        /**
         * <summary>    Gets or sets the pnl editor selection. </summary>
         *
         * <value>  The pnl editor selection. </value>
         */

        public System.Windows.Forms.Panel Pnl_editor_selection
        {
            get { return pnl_editor_selection; }
            set { pnl_editor_selection = value; }
        }

        /**
         * <summary>    Gets or sets the cmb editor selection tool selection. </summary>
         *
         * <value>  The cmb editor selection tool selection. </value>
         */

        public System.Windows.Forms.ComboBox Cmb_editor_selection_toolSelection
        {
            get { return cmb_editor_selection_toolSelection; }
            set { cmb_editor_selection_toolSelection = value; }
        }

        /**
         * <summary>    Gets or sets the cmb editor properties object selection. </summary>
         *
         * <value>  The cmb editor properties object selection. </value>
         */

        public System.Windows.Forms.ComboBox Cmb_editor_properties_objectSelection
        {
            get { return cmb_editor_properties_objectSelection; }
            set { cmb_editor_properties_objectSelection = value; }
        }

        /// <summary>
        /// Gets or sets the tsm_editor_menu_edit_copie.
        /// </summary>
        /// <value>
        /// The tsm_editor_menu_edit_copie.
        /// </value>
        public System.Windows.Forms.ToolStripMenuItem Tsm_editor_menu_edit_copie
        {
            get { return tsm_editor_menu_edit_copie; }
            set {tsm_editor_menu_edit_copie = value; }
        }

        /// <summary>
        /// Gets or sets the tsm_editor_menu_edit_paste.
        /// </summary>
        /// <value>
        /// The tsm_editor_menu_edit_paste.
        /// </value>
        public System.Windows.Forms.ToolStripMenuItem Tsm_editor_menu_edit_paste
        {
            get { return tsm_editor_menu_edit_paste; }
            set { tsm_editor_menu_edit_paste = value; }
        }

        /// <summary>
        /// Gets or sets the tsm_editor_menu_edit_delete.
        /// </summary>
        /// <value>
        /// The tsm_editor_menu_edit_delete.
        /// </value>
        public System.Windows.Forms.ToolStripMenuItem Tsm_editor_menu_edit_delete
        {
            get { return tsm_editor_menu_edit_delete; }
            set { tsm_editor_menu_edit_delete = value; }
        }
    }
}

