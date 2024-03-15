using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DBDPakInstallerGUI
{
    public partial class dbdPakInstaller : Form
    {
        private TextBox paksTextBox;
        private TextBox pakFileTextBox;
        private TextBox paksTextBoxDisplay;
        private TextBox pakFileTextBoxDisplay;
        private Logger logger;
        private string paksFolderPath;

        public dbdPakInstaller()
        {
            InitializeComponent();

            //Textbox field instances
            paksTextBox = new TextBox();
            pakFileTextBox = new TextBox();

            //display TextBoxes
            paksTextBoxDisplay = new TextBox();
            paksTextBoxDisplay.ReadOnly = true;
            pakFileTextBoxDisplay = new TextBox();
            pakFileTextBoxDisplay.ReadOnly = true;

            // Get the application directory path
            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;

            // Initialize logger with the log file path in the application directory
            string logFilePath = Path.Combine(appDirectory, "logs.txt");
            logger = new Logger(logFilePath);
            logger.Log("Application started.");

            // Set the paksFolderPath to the PaksFolderPath.txt file in the application directory
            paksFolderPath = Path.Combine(appDirectory, "PaksFolderPath.txt");
        }

        private void dbdPakInstaller_Load(object sender, EventArgs e)
        {
            logger.Log("Form loaded.");
        }

        private void button1_Click(object sender, EventArgs e) // selecting paks folder
        {
            logger.Log("Selecting Paks folder.");
            using (var folderBrowserDialog = new FolderBrowserDialog())
            {
                string paksFolder = string.Empty;

                // Check if the PaksFolderPath.txt file exists and contains a valid path
                if (File.Exists(paksFolderPath))
                {
                    paksFolder = File.ReadAllText(paksFolderPath).Trim();
                    if (Directory.Exists(paksFolder))
                    {
                        logger.Log($"Paks folder found at: {paksFolder}");
                        DialogResult result = MessageBox.Show($"Paks folder is already found at: {paksFolder}\nDo you want to select a different folder?", "Paks Folder Found", MessageBoxButtons.YesNo);
                        if (result == DialogResult.No)
                        {
                            paksTextBox.Text = paksFolder;
                            paksTextBoxDisplay.Text = paksFolder;
                            return;
                        }
                    }
                }

                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    paksFolder = folderBrowserDialog.SelectedPath;
                    logger.Log($"Paks folder selected: {paksFolder}");
                    paksTextBox.Text = paksFolder;
                    paksTextBoxDisplay.Text = paksFolder;

                    // Save the selected Paks folder path to PaksFolderPath.txt
                    File.WriteAllText(paksFolderPath, paksFolder);
                }
            }
        }

        private void button3_Click(object sender, EventArgs e) // For choosing .pak or .bak file
        {
            logger.Log("Selecting PAK or BAK file.");
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "PAK files (*.pak)|*.pak|BAK files (*.bak)|*.bak";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string chosenPakFileName = openFileDialog.FileName;
                    logger.Log($"PAK or BAK file selected: {chosenPakFileName}");
                    pakFileTextBox.Text = chosenPakFileName;
                    pakFileTextBoxDisplay.Text = chosenPakFileName; // Update display TextBox

                    // Get the Paks folder path from the paksTextBox
                    string paksFolder = paksTextBox.Text;

                    // Check if the Paks folder is valid
                    if (!string.IsNullOrEmpty(paksFolder) && Directory.Exists(paksFolder))
                    {
                        // Create copies of pakchunk348-EGS.sig
                        CreatePakSigCopies(paksFolder);
                    }
                    else
                    {
                        logger.Log("Invalid Paks folder selected.");
                        MessageBox.Show("Please select a valid Paks folder before selecting the PAK/BAK file.");
                    }
                }
            }
        }

        private void button4_Click(object sender, EventArgs e) // For executing
        {
            logger.Log("Executing operation.");
            string paksFolder = paksTextBox.Text;
            string chosenPakFileName = pakFileTextBox.Text;

            if (string.IsNullOrEmpty(paksFolder) || string.IsNullOrEmpty(chosenPakFileName))
            {
                logger.Log("Paks folder or PAK/BAK file not selected.");
                MessageBox.Show("Please select the Paks folder and PAK/BAK file.");
                return;
            }

            string chosenPakFile = Path.Combine(paksFolder, chosenPakFileName);
            if (!File.Exists(chosenPakFile) || (!chosenPakFileName.EndsWith(".pak") && !chosenPakFileName.EndsWith(".bak")))
            {
                logger.Log("Invalid file name or file not found.");
                MessageBox.Show("Error: invalid file name or file not found.");
                return;
            }

            logger.Log("Operation started.");
            CopyPakOrBakFile(chosenPakFile, paksFolder);
            RenameCopies(paksFolder, chosenPakFileName);
            logger.Log("Operation completed successfully!");
            MessageBox.Show("Operation completed successfully! HAPPY MODDING!");
        }

        private void CopyPakOrBakFile(string sourceFile, string destinationFolder)
        {
            logger.Log($"Copying {Path.GetFileName(sourceFile)} to {destinationFolder}");
            string destinationFile = Path.Combine(destinationFolder, Path.GetFileName(sourceFile));
            try
            {
                File.Copy(sourceFile, destinationFile, true); // Overwrite if the file already exists
                logger.Log($"{Path.GetFileName(sourceFile)} copied successfully.");
            }
            catch (IOException ex)
            {
                logger.Log($"Error occurred while copying {Path.GetFileName(sourceFile)} to Paks folder: {ex.Message}");
                MessageBox.Show($"Error occurred while copying {Path.GetFileName(sourceFile)} to Paks folder: {ex.Message}");
            }
        }

        private void UpdateCopies(string paksFolder, string fileName) // Updating the copies 
        {
            logger.Log("Updating copies of pakchunk348-EGS.sig.");
            try
            {
                File.WriteAllText(Path.Combine(paksFolder, "pakchunk348-EGS - Copy.sig"), Path.GetFileNameWithoutExtension(fileName));
                File.WriteAllText(Path.Combine(paksFolder, "pakchunk348-EGS - Copy (2).sig"), Path.GetFileNameWithoutExtension(fileName));
                logger.Log("Copies updated successfully.");
            }
            catch (IOException ex)
            {
                logger.Log($"Error occurred while updating copies of pakchunk348-EGS.sig: {ex.Message}");
                MessageBox.Show($"Error occurred while updating copies of pakchunk348-EGS.sig: {ex.Message}");
            }
        }

        private void RenameCopies(string paksFolder, string fileName)
        {
            logger.Log("Renaming copies.");
            string renamedFile1 = Path.Combine(paksFolder, "pakchunk348-EGS - Copy.sig");
            string renamedFile2 = Path.Combine(paksFolder, "pakchunk348-EGS - Copy (2).sig");

            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            string newName1 = Path.Combine(paksFolder, $"{fileNameWithoutExtension}.sig");
            string newName2 = Path.Combine(paksFolder, $"{fileNameWithoutExtension}.kek");

            try
            {
                File.Move(renamedFile1, newName1);
                File.Move(renamedFile2, newName2);
                logger.Log("Files renamed successfully.");
            }
            catch (IOException ex)
            {
                logger.Log($"Error occurred while renaming files: {ex.Message}");
                MessageBox.Show($"Error occurred while renaming files: {ex.Message}");
            }
        }

        private void CreatePakSigCopies(string paksFolder)
        {
            string pakSigFile = Path.Combine(paksFolder, "pakchunk348-EGS.sig");

            if (File.Exists(pakSigFile))
            {
                // Make copies of the "pakchunk348-EGS.sig" file
                try
                {
                    logger.Log("Creating copies of pakchunk348-EGS.sig.");
                    File.Copy(pakSigFile, Path.Combine(paksFolder, "pakchunk348-EGS - Copy.sig"), true);
                    File.Copy(pakSigFile, Path.Combine(paksFolder, "pakchunk348-EGS - Copy (2).sig"), true);
                    logger.Log("Copies created successfully.");
                }
                catch (IOException ex)
                {
                    logger.Log($"Error occurred while copying files: {ex.Message}");
                    MessageBox.Show($"Error occurred while copying files: {ex.Message}");
                }
            }
            else
            {
                logger.Log("pakchunk348-EGS.sig not found.");
                MessageBox.Show("pakchunk348-EGS.sig is not found. Please select the Paks folder again.");
            }
        }

        public class Logger
        {
            private string logFilePath;

            public Logger(string logFilePath)
            {
                this.logFilePath = logFilePath;
            }

            public void Log(string message)
            {
                try
                {
                    using (StreamWriter writer = new StreamWriter(logFilePath, true))
                    {
                        writer.WriteLine($"{DateTime.Now.ToString()} - {message}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error occurred while logging: {ex.Message}");
                }
            }
        }
    }
}