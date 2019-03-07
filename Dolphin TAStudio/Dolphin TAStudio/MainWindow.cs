/*TODO:
 * If the user focuses on frame column and presses backspace or delete, remove the row and adjust framecount accordingly.
 *
 * 
 * 
 *
 */

/*RECENTLY ADDED FEATURES:
 * Upon exiting out of the form, prompt the user to save if unsaved modifications.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        private DataTable table = new DataTable();  //Used to store framedata.
        private string fileName;  //Keeps track of input file name for saving purposes and telling if we have a file currently open.
        private int frameCount;  //Keeps track of the last frame of input in our table.
        private bool changed = false;  //Keeps track of whether or not the user has made changes to the input table since saving/opening a file.

        public Form1()
        {
            InitializeComponent();
        }

        public void aboutToolStripMenuItem_Click(object sender, System.EventArgs e)  //Help->About button
        {
            Form2 f2 = new Form2();
            f2.Show();
        }

        private void openData(string fileLocation)  //Run when we want to load input data from a file.
        {
            changed = false;

            //Establish columns
            table.Columns.Add("Frame", typeof(int));
            table.Columns.Add("Horiz (0-14)", typeof(int));
            table.Columns.Add("Vert (0-14)", typeof(int));
            table.Columns.Add("A", typeof(int));
            table.Columns.Add("B", typeof(int));
            table.Columns.Add("L", typeof(int));
            table.Columns.Add("DU", typeof(int));
            table.Columns.Add("DD", typeof(int));
            table.Columns.Add("DL", typeof(int));
            table.Columns.Add("DR", typeof(int));

            using (StreamReader reader = new StreamReader(fileLocation))  //Begin reading from file.
            {
                string data = reader.ReadLine();
                data = reader.ReadLine(); // Skip over the first line.
                frameCount = 0;

                while (data != "" && data != "return mkw_input_reader_output") //Loop will end after the last frame of input data.
                {
                    string trimmedLine = data.Remove(0, 1);  //Trim off left curly brace.
                    trimmedLine = trimmedLine.Remove(trimmedLine.Length - 2, 2);  //Trim off right curly brace + comma/last row second curly brace.

                    string[] attributes = trimmedLine.Split(',');  //Separate frame data into its constituents.
                    string[] row = new string[9];

                    for (int i = 0; i < table.Columns.Count - 1; i++)
                    {
                        row[i] = attributes[i].Substring(attributes[i].IndexOf("=") + 1);  //Populate row with only the number within each constituent.
                    }

                    table.Rows.Add(frameCount, row[0], row[1], row[2], row[3], row[4], row[5], row[6], row[7], row[8]);  //Populate the DataTable with this data.

                    data = reader.ReadLine();  //Move to the following line.
                    frameCount++;
                }
            }
            
            dataGridView1.DataSource = table;  //Set the grid to populate with data from table.
            

            //Set column widths
            dataGridView1.Columns[0].Width = 60;
            dataGridView1.Columns[0].ReadOnly = true;
            dataGridView1.Columns[1].Width = 70;
            dataGridView1.Columns[2].Width = 70;

            for (int i = 3; i < 6; i++)
            {
                dataGridView1.Columns[i].Width = 20;
            }
            for (int i = 6; i < 10; i++)
            {
                dataGridView1.Columns[i].Width = 25;
            }

            //Disable sorting
            foreach (DataGridViewColumn column in dataGridView1.Columns)
            {
                column.SortMode = DataGridViewColumnSortMode.NotSortable;
            }

            dataGridView1.Refresh();
        }

        private void saveData(Stream s)  //Runs when we want to save data to a file.
        {
            changed = false;  //Since we are saving the file, flag changed as false so the user will not be prompted to save upon closing.
            using (s)
            using (StreamWriter sw = new StreamWriter(s))
            {
                Dictionary<int, string> col_dict = new Dictionary<int, string>
                    {
                        {0, "Frame"},
                        {1, "Horiz"},
                        {2, "Vert"},
                        {3, "A"},
                        {4, "B"},
                        {5, "L"},
                        {6, "DU"},
                        {7, "DD"},
                        {8, "DL"},
                        {9, "DR"}
                    };

                sw.WriteLine("local mkw_input_reader_output = {");

                //Print the data from each row of DataTable table to a single row in a text file.
                for (int row = 0; row < dataGridView1.RowCount - 1; row++)
                {
                    string lines = "{";

                    for (int col = 1; col < dataGridView1.ColumnCount; col++)
                    {
                        if (col < dataGridView1.ColumnCount - 1)
                        {
                            lines += String.Format("{0} = {1}, ", col_dict[col], table.Rows[row][col]);
                        }
                        else  //This occurs when we've reached the last column of a row.
                        {
                            if (row < dataGridView1.RowCount - 2)
                            {
                                lines += String.Format("{0} = {1}}},", col_dict[col], table.Rows[row][col]);
                            }
                            else  //This occurs when we've reached the last row, so end with two curly brackets.
                            {
                                lines += String.Format("{0} = {1}}}", col_dict[col], table.Rows[row][col]);
                                lines += "}";
                            }
                        }
                    }

                    sw.WriteLine(lines);
                }

                sw.WriteLine("");
                sw.WriteLine("return mkw_input_reader_output");
            }
        }

        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == frameCount)  //Occurs when we enter data into a new row.
            {
                dataGridView1.Rows[frameCount].Cells[0].Value = frameCount;
                for (int i = 1; i < dataGridView1.ColumnCount; i++)
                {
                    if (e.RowIndex == frameCount && e.ColumnIndex == i)  //Don't set a default value for the cell we just modified.
                    {
                        continue;
                    }
                    if (i == 1 || i == 2)
                    {
                        dataGridView1.Rows[frameCount].Cells[i].Value = 7;  //Default for horizontal/vertical input.
                    }
                    else if (i == 3)
                    {
                        dataGridView1.Rows[frameCount].Cells[i].Value = 1;  //Default A being pressed.
                    }
                    else
                    {
                        dataGridView1.Rows[frameCount].Cells[i].Value = 0;  //Default all other buttons are not being pressed.
                    }
                }
                frameCount++;
            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.S)
            {
                saveCtrlSToolStripMenuItem_Click(sender, e);
            }

            if (e.Control && e.Shift && e.KeyCode == Keys.S)
            {
                saveAsToolStripMenuItem_Click(sender, e);
            }

            if (e.Control && e.KeyCode == Keys.O)
            {
                openCtrlOToolStripMenuItem_Click(sender, e);
            }
        }

        private void saveCtrlSToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FileInfo fileInfo = new FileInfo(fileName);
            Stream s = fileInfo.OpenWrite();
            saveData(s);
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "Lua Script File (*.lua)|*.lua";
            saveFileDialog1.DefaultExt = "lua";
            saveFileDialog1.AddExtension = true;

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Stream s = File.Open(saveFileDialog1.FileName, FileMode.Create);
                saveData(s);
            }
        }

        private void openCtrlOToolStripMenuItem_Click(object sender, EventArgs e)
        {


            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                table.Columns.Clear();
                table.Clear();
                fileName = openFileDialog1.FileName;
                openData(fileName);
            }
        }

        private void clearDataTable()
        {
            table.Columns.Clear();
            table.Clear();

            fileName = null;
            changed = false;
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (fileName != null)
            {
                if (changed == true)
                {
                    saveBeforeClosing(sender, e);
                }
                else
                {
                    clearDataTable();
                }
            }
        }

        private void saveBeforeClosing(object sender, EventArgs e)
        {
            DialogResult dialog = MessageBox.Show("Save before closing?", "Exit", MessageBoxButtons.YesNoCancel);
            if (dialog != DialogResult.Cancel)
            {
                if (dialog == DialogResult.Yes)
                {
                    saveCtrlSToolStripMenuItem_Click(sender, e);
                }
                clearDataTable();
            }
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            changed = true;
        }

        private void Form1_FormClosed(object sender, EventArgs e)
        {
            saveBeforeClosing(sender, e);
        }
    }
}