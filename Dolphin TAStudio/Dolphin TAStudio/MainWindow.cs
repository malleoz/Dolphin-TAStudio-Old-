/*TODO:
 * If the user focuses on frame column and presses backspace or delete, remove the row and adjust framecount accordingly.
 * If the user focuses on frame column and presses enter, add a new row below.
 */

/*RECENTLY ADDED FEATURES:
 * Upon exiting out of the form, prompt the user to save if unsaved modifications.
 * Users can now copy-paste row data to overwrite another row.
 * Users can now copy-paste individual cell data to overwrite the selected cell,
 *      as well as any cells below and to the right so long as there is still leftover data stored in the buffer.
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
using System.Text.RegularExpressions;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        private DataTable table = new DataTable();  //Used to store framedata.
        private string fileName;  //Keeps track of input file name for saving purposes and telling if we have a file currently open.
        private int frameCount;  //Keeps track of the last frame of input in our table.
        private bool changed = false;  //Keeps track of whether or not the user has made changes to the input table since saving/opening a file.
        private List<string> buffer = new List<string>();  //Acts as the clipboard.
        /* BUFFER FORMAT:
         * Each index represents a frame.
         * Within an index, each space represents a different attribute (button or analog value).
         * A # symbol represents that we do not wish to overwrite the given attribute on a specific frame.
         * */

        public Form1()
        {
            InitializeComponent();
            pasteCtrlVToolStripMenuItem.Enabled = false;  //Initialize the Paste button to disabled to prevent pasting empty data.
        }

        public void aboutToolStripMenuItem_Click(object sender, System.EventArgs e)  //Help->About button
        {
            About about = new About();
            about.Show();
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

        private void copyRows()  //Runs when the user is trying to copy an entire row of framedata.
        {
            buffer.Clear();  //Remove any data previously stored in the buffer.
            string data;
            foreach (DataGridViewRow row in dataGridView1.SelectedRows)
            {
                data = "";
                for (int i = 1; i < dataGridView1.ColumnCount; i++)
                {
                    data += row.Cells[i].Value.ToString() + " ";
                }
                data = data.Substring(0, data.Length - 1);  //Chop off the extra space at the end.
                buffer.Add(data);
            }
        }

        private List<int> populateRowList()
        {
            List<int> rowList = new List<int>();
            foreach (DataGridViewCell cell in dataGridView1.SelectedCells)
            {
                if (!rowList.Contains(cell.RowIndex))
                {
                    rowList.Add(cell.RowIndex);
                }
            }
            return rowList;
        }

        private List<int> populateColumnList()
        {
            List<int> colList = new List<int>();
            foreach (DataGridViewCell cell in dataGridView1.SelectedCells)
            {
                if (!colList.Contains(cell.ColumnIndex))
                {
                    colList.Add(cell.ColumnIndex);
                }
            }
            return colList;
        }

        private void copyCells()
        {
            buffer.Clear();  //Remove any data previously stored in the buffer.
            List<int> rowList = new List<int>();

            //First populate rowList with the list of rows that have selected cells somewhere.
            rowList = populateRowList();
            rowList.Sort();

            //Next generate a data string that either has the cell data of copied cells or a # if a cell was not selected.
            string data;
            bool start;
            foreach (int row in rowList)
            {
                data = "";
                start = false;
                for (int i = 1; i < dataGridView1.ColumnCount; i++)
                {
                    if (dataGridView1.Rows[row].Cells[i].Selected)
                    {
                        start = true;
                        data += dataGridView1.Rows[row].Cells[i].Value.ToString() + " ";
                    }
                    else if (start)
                    {
                        data += "# ";
                    }
                }
                data = data.Substring(0, data.Length - 1);
                buffer.Add(data);
            }
        }

        private void copyData()
        {
            //First make sure the user isn't dumb and tries to copy data without opening a file.
            if (fileName == null)
            {
                return;
            }

            pasteCtrlVToolStripMenuItem.Enabled = true;
            if (dataGridView1.SelectedRows.Count > 0)
            {
                copyRows();  //Runs if the user is trying to copy an entire row.
            }

            else if (dataGridView1.SelectedCells.Count > 0)
            {
                copyCells();  //Runs if the user is only copying a select group of cells.
            }
        }

        private void pasteData()
        {
            //First, check to make sure the user isn't dumb and doesn't try to paste when a file isn't loader or when they haven't copied data.
            if (fileName == null || buffer.Count == 0)
            {
                return;
            }
            List<int> rowList = new List<int>();
            List<int> columnList = new List<int>();

            //First populate rowList with the list of rows that have selected cells somewhere. Then keep track of the first column selected.
            rowList = populateRowList();
            rowList.Sort();
            int firstRow = rowList[0];

            //Next, populate the columnList with the list of columns that have selected cells somewhere. Then keep track of the first column selected.
            columnList = populateColumnList();
            columnList.Sort();
            int userColumnOffset = columnList[0];
            if (userColumnOffset == 0)
            {
                for (int row = 0; row < buffer.Count; row++)
                {
                    string framedata = buffer[row];
                    string[] attributes = framedata.Split(' ');

                    for (int i = 1; i < dataGridView1.ColumnCount; i++)
                    {
                        dataGridView1.Rows[firstRow + row].Cells[i].Value = attributes[i - 1];
                    }
                }
            }
            else
            {
                for (int row = 0; row < buffer.Count; row++)
                {
                    string framedata = buffer[row];
                    string[] attributes = framedata.Split(' ');

                    for (int i = 1; i < dataGridView1.ColumnCount; i++)
                    {
                        if (!attributes[i - 1].Equals("#"))
                        {
                            dataGridView1.Rows[firstRow + row].Cells[i - 1 + userColumnOffset].Value = attributes[i - 1];
                        }
                        if (i == attributes.Length - 1)
                        {
                            break;
                        }
                    }
                }
            }

            dataGridView1.Update();
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.C)  //User is attempting to copy data.
            {
                copyData();
            }

            if (e.Control && e.KeyCode == Keys.V)  //User wants to paste row data.
            {
                pasteData();
            }

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
            if (changed)
            {
                saveBeforeClosing(sender, e);
            }
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
            //Do nothing if there isn't a file open.
            if (fileName == null)
            {
                return;
            }

            if (changed)  //Does the user have unsaved changes?
            {
                saveBeforeClosing(sender, e);
            }
            else  //If the user didn't make changes, close the table.
            {
                clearDataTable();
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
                clearDataTable();  //This makes sure we don't clear the table if the user pressed Cancel.
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

        private void copyCtrlCToolStripMenuItem_Click(object sender, EventArgs e)
        {
            copyData();
        }

        private void pasteCtrlVToolStripMenuItem_Click(object sender, EventArgs e)
        {
            pasteData();
        }
    }
}