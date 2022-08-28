using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace IconConverter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private IDataObject data;
        private FileInfo inputFile;

        private void groupBox_Drop(object sender, DragEventArgs e)
        {
            processDropEvent(sender, e);
        }

        private void processDropEvent(object sender, DragEventArgs e)
        {
            string format = "Unrecognized";
            string filePath = "";
            if (e.Data.GetDataPresent("FileGroupDescriptor"))
            {
                format = "Attachment File";
                try
                {
                    //try to obtain FileGroupDescriptor and extract file name
                    Stream theStream = (Stream)e.Data.GetData("FileGroupDescriptor");
                    byte[] fileGroupDescriptor = new byte[512];
                    theStream.Read(fileGroupDescriptor, 0, 512);
                    StringBuilder fileName = new StringBuilder("");
                    for(int i = 76; fileGroupDescriptor[i] != 0; i++)
                    {
                        fileName.Append(Convert.ToChar(fileGroupDescriptor[i]));
                    }
                    theStream.Close();

                    string path = System.IO.Path.GetTempPath();
                    string theFile = path + fileName.ToString();

                    //next,get the actual raw content into memory
                    MemoryStream ms = (MemoryStream)e.Data.GetData("FileContents", true);
                    byte[] fileBytes = new byte[ms.Length];
                    ms.Position = 0;
                    ms.Read(fileBytes, 0, fileBytes.Length);
                    FileStream fs = new FileStream(theFile, FileMode.Create);
                    fs.Write(fileBytes, 0, fileBytes.Length);

                    fs.Close();

                    FileInfo tempFile = new FileInfo(theFile);

                    //make sure we really have created the file
                    if (tempFile.Exists)
                    {
                        filePath = tempFile.Name;
                        inputFile = tempFile;
                    }
                    else
                    {
                        Trace.WriteLine("File was not created!");
                    }
                }catch(Exception ex)
                {
                    format = "Unrecognized";
                    filePath = "N/A";
                }
            }else if (e.Data.GetDataPresent("FileName"))
            {
                format = "Local File";
                string fileName = ((string[])e.Data.GetData("FileName"))[0];
                FileInfo tempFile = new FileInfo(fileName);
                filePath = tempFile.Name;
                inputFile = tempFile;
            }
            label.Content = string.Format("Format={0}{1}Name={2}", format, Environment.NewLine, filePath);
            data = e.Data;
        }

        private void label_Drop(object sender, DragEventArgs e)
        {
            processDropEvent(sender, e);
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            if(inputFile == null)
            {
                MessageBox.Show("Please specify JPG/PNG for conversion first!", "Warning");
                return;
            }else if(!inputFile.Extension.ToUpper().EndsWith("JPG") && !inputFile.Extension.ToUpper().EndsWith("PNG"))
            {
                MessageBox.Show("Input File Type not supported!", "Warning");
                return;
            }

            string sizeStr = ((ListBoxItem)comboBox.SelectedValue).Content.ToString().Replace("px", "");
            int size = 0;
            Int32.TryParse(sizeStr, out size);

            string input = inputFile.FullName;
            string output = inputFile.FullName.Replace(inputFile.Extension, "_" + size + ".ico");
            Utils.Convert(input, output, size);

            MessageBox.Show("Successfully convert into ico file!", "Info");
        }
    }
}
