﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;

using AForge;
using AForge.Video;
using AForge.Video.FFMPEG;

using ARdevKit.Controller.ProjectController;
using ARdevKit.Model.Project;
using ARdevKit.Model.Project.File;
using ARdevKit.View;

namespace ARdevKit.Controller.TestController
{
    /// <summary>
    /// The <see cref="TestController" /> manages the start of the <see cref="Player" />.
    /// </summary>
    static class TestController
    {
        /// <summary>
        /// The location where the temporary frames where extracted to.
        /// </summary>
        private const string TMP_VIDEO_PATH = "videoTemp\\video";

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Used in <see cref="StartPlayer(string, int)"/> to load an image
        /// </summary>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public const int IMAGE = 0;

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Used in <see cref="StartPlayer(string, int)"/> load a video
        /// </summary>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public const int VIDEO = 1;

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Used in <see cref="StartPlayer(string, int)"/> start a virtual camera
        /// </summary>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public const int CAMERA = 2;

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// New process for the player.
        /// </summary>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static Process player;

        /// <summary>
        /// True if user chose to show debug information.
        /// </summary>
        private static bool showDebug;

        /// <summary>
        /// The editor window.
        /// </summary>
        private static EditorWindow editorWindow;
        
        /// <summary>
        /// A window showing the progress of processing the video.
        /// </summary>
        private static ProcessVideoWindow progressVideoWindow;

        /// <summary>
        /// The frame extractor.
        /// </summary>
        private static FrameExtractor frameExtractor;

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// The path to the player.
        /// </summary>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        private static string playerPath = "Player.exe";

        /// <summary>
        /// Starts the player.
        /// </summary>
        /// <param name="ew">The ew.</param>
        /// <param name="project">The project.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="_showDebug">if set to <c>true</c> [show debug].</param>
        public static void StartPlayer(EditorWindow ew, Project project, int mode, int width, int height, bool _showDebug)
        {
            editorWindow = ew;
            showDebug = _showDebug;

            if (ew == null)
            {
                MessageBox.Show("Der EditorWindow wurde nicht richtig instanziiert!");
                return;
            }

            if (project == null)
            {
                MessageBox.Show("Es ist kein Projekt vorhanden!");
                return;
            }

            if (mode < 0 || mode > 2)
            {
                MessageBox.Show("The mode must be between 0 to 2!");
                return;
            }

            if (width < 20 || height < 20)
            {
                throw new ArgumentException("Die Höhe und/oder Breite ist zu klein! Sie müssen mindestens einen Wert von 20 haben.");
            }

            IDFactory.Reset();
            FolderBrowserDialog exportDialog = new FolderBrowserDialog();
            DialogResult exportDialogResult = DialogResult.OK;
            if (project.ProjectPath == null)
            {
                MessageBox.Show("Das Projekt muss zuerst exportiert werden");
                exportDialog.RootFolder = Environment.SpecialFolder.MyDocuments;
                if ((exportDialogResult = exportDialog.ShowDialog()) == DialogResult.OK)
                {
                    project.ProjectPath = exportDialog.SelectedPath;
                }
            }
            if (exportDialogResult == DialogResult.OK)
            {
                if (ew.ExportProject(false))
                {
                    player = new Process();
                    player.EnableRaisingEvents = true;
                    player.Exited += player_Exited;
                    player.StartInfo.Arguments = "-" + width + " -" + height + " -" + "\"" + project.ProjectPath + "\"" + " -" + mode;

                    bool open = false;
                    switch (mode)
                    {
                        case (IMAGE):
                            OpenFileDialog openTestImageDialog = new OpenFileDialog();
                            openTestImageDialog.InitialDirectory = Environment.CurrentDirectory + "\\res\\testFiles\\imagesToLoadForTesting";
                            openTestImageDialog.Title = "Bitte ein Bild auswählen, an dem getestet werden soll";
                            openTestImageDialog.Filter = "Supported image files (*.jpg, *.png, *.bmp, *.ppm, *.pgm)|*.jpg; *.png; *.bmp; *.ppm; *.pgm";
                            if (openTestImageDialog.ShowDialog() == DialogResult.OK)
                            {
                                string testFilePath = openTestImageDialog.FileName;
                                player.StartInfo.Arguments += " -" + testFilePath;
                                OpenPlayer();
                            }
                            break;
                        case (VIDEO):
                            OpenFileDialog openTestVideoDialog = new OpenFileDialog();
                            openTestVideoDialog.InitialDirectory = Environment.CurrentDirectory + "\\res\\testFiles\\videosToLoadForTesting";
                            openTestVideoDialog.Title = "Bitte ein Video auswählen, an dem getestet werden soll";
                            if (openTestVideoDialog.ShowDialog() == DialogResult.OK)
                            {
                                string testFilePath = openTestVideoDialog.FileName;

                                if (Directory.Exists(TMP_VIDEO_PATH))
                                {
                                    foreach (string path in Directory.GetFiles(TMP_VIDEO_PATH))
                                       File.Delete(path);
                                }
                                else
                                    Directory.CreateDirectory(TMP_VIDEO_PATH);

                                player.StartInfo.Arguments += " -" + TMP_VIDEO_PATH;

                                progressVideoWindow = new ProcessVideoWindow();
                                progressVideoWindow.FormClosed += progressVideoWindow_FormClosed;

                                try
                                {
                                    frameExtractor = new FrameExtractor(progressVideoWindow, testFilePath, TMP_VIDEO_PATH);
                                }
                                catch (Exception e)
                                {
                                    MessageBox.Show("Das Video konnte nicht verarbeitet werden:\nEventuell fehlt einer der folgenden Dateien: AForge.Video.FFMPEG.dll, AForge.Video.dll, AForge.Video.FFMPEG.dll.\n" + e.Message);
                                    break;
                                }
                                if (frameExtractor.Ready)
                                {
                                    progressVideoWindow.Show();
                                    frameExtractor.RunWorkerAsync();
                                }
                            }
                            break;
                        case (CAMERA):
                            OpenFileDialog openVirualCameraPathDialog = new OpenFileDialog();
                            if (Environment.Is64BitOperatingSystem && Environment.Is64BitProcess)
                                openVirualCameraPathDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                            else
                                openVirualCameraPathDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
                            openVirualCameraPathDialog.Title = "Bitte virtuelle Kamera auswählen";
                            openVirualCameraPathDialog.Filter = "Executable (*.exe)|*.exe";
                            if (openVirualCameraPathDialog.ShowDialog() == DialogResult.OK)
                            {
                                string virtualCameraPath = openVirualCameraPathDialog.FileName;

                                Process vCam = new Process();
                                vCam.StartInfo.FileName = virtualCameraPath;
                                vCam.Start();
                                OpenPlayer();
                            }
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Handles the Exited event of the player control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private static void player_Exited(object sender, EventArgs e)
        {
            if (Directory.Exists(TMP_VIDEO_PATH))
                try
                {
                    Directory.Delete(TMP_VIDEO_PATH, true);
                } catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            editorWindow.PlayerClosed();
        }

        /// <summary>
        /// Opens the player.
        /// </summary>
        private static void OpenPlayer()
        {
            if (File.Exists(playerPath))
            {
                if (showDebug)
                {
                    player.StartInfo.FileName = playerPath;
                    //player.StartInfo.CreateNoWindow = false;
                    //player.StartInfo.UseShellExecute = true;
                    //player.StartInfo.RedirectStandardOutput = true;
                    //player.OutputDataReceived += new DataReceivedEventHandler(player_OutputDataReceived);
                    player.Start();
                    
                    /* not working atm
                    debugWindow = new DebugWindow();
                    debugWindow.Show();
                    //new MethodInvoker(player.BeginOutputReadLine).BeginInvoke(null, null);
                    */
                }
                else
                {
                    player.StartInfo.FileName = playerPath;
                    player.StartInfo.UseShellExecute = false;
                    player.StartInfo.CreateNoWindow = true;
                    player.Start();
                }
                editorWindow.PlayerStarted();
            }
            else
            {
                OpenFileDialog openPlayerDialog = new OpenFileDialog();
                openPlayerDialog.Title = "Bitte Player auswählen";
                openPlayerDialog.Filter = "Player (Player.exe)|Player.exe";
                if (openPlayerDialog.ShowDialog() == DialogResult.OK)
                {
                    playerPath = openPlayerDialog.FileName;
                    OpenPlayer();
                }
            }
        }

        /* not working atm
        static void player_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            debugWindow.AppendText(e.Data + Environment.NewLine);
        }
        */

        /// <summary>
        /// Handles the FormClosed event of the progressVideoWindow control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="FormClosedEventArgs"/> instance containing the event data.</param>
        static void progressVideoWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (frameExtractor.Finished)
            {
                player.StartInfo.Arguments += " -" + frameExtractor.FPS;
                OpenPlayer();
            }
        }
    }
}
