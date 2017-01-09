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
using System.Threading;

namespace RalphsProgram
{

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.systemsGrid.Rows[0].HeaderCell.Value = "System 1";
            this.setsGrid.Rows[0].HeaderCell.Value = "Systems in Set 1";
            this.setsGrid.Columns[0].Width = 3000;
            this.setsGrid.RowHeadersWidth = 200;

            conditionTextBoxGrid = new TextBox[10,2];

            conditionTextBoxGrid[0, 0] = condition1afrac_textbox;
            conditionTextBoxGrid[1, 0] = condition2afrac_textbox;
            conditionTextBoxGrid[2, 0] = condition3afrac_textbox;
            conditionTextBoxGrid[3, 0] = condition4afrac_textbox;
            conditionTextBoxGrid[4, 0] = condition5afrac_textbox;
            conditionTextBoxGrid[5, 0] = condition6afrac_textbox;
            conditionTextBoxGrid[6, 0] = condition7afrac_textbox;
            conditionTextBoxGrid[7, 0] = condition8afrac_textbox;
            conditionTextBoxGrid[8, 0] = condition9afrac_textbox;
            conditionTextBoxGrid[9, 0] = condition10afrac_textbox;

            conditionTextBoxGrid[0, 1] = condition1bfrac_textbox;
            conditionTextBoxGrid[1, 1] = condition2bfrac_textbox;
            conditionTextBoxGrid[2, 1] = condition3bfrac_textbox;
            conditionTextBoxGrid[3, 1] = condition4bfrac_textbox;
            conditionTextBoxGrid[4, 1] = condition5bfrac_textbox;
            conditionTextBoxGrid[5, 1] = condition6bfrac_textbox;
            conditionTextBoxGrid[6, 1] = condition7bfrac_textbox;
            conditionTextBoxGrid[7, 1] = condition8bfrac_textbox;
            conditionTextBoxGrid[8, 1] = condition9bfrac_textbox;
            conditionTextBoxGrid[9, 1] = condition10bfrac_textbox;

            conditionDropBoxGrid = new ComboBox[10, 2];

            conditionDropBoxGrid[0, 0] = condition1setA_dropbox;
            conditionDropBoxGrid[1, 0] = condition2setA_dropbox;
            conditionDropBoxGrid[2, 0] = condition3setA_dropbox;
            conditionDropBoxGrid[3, 0] = condition4setA_dropbox;
            conditionDropBoxGrid[4, 0] = condition5setA_dropbox;
            conditionDropBoxGrid[5, 0] = condition6setA_dropbox;
            conditionDropBoxGrid[6, 0] = condition7setA_dropbox;
            conditionDropBoxGrid[7, 0] = condition8setA_dropbox;
            conditionDropBoxGrid[8, 0] = condition9setA_dropbox;
            conditionDropBoxGrid[9, 0] = condition10setA_dropbox;

            conditionDropBoxGrid[0, 1] = condition1setB_dropbox;
            conditionDropBoxGrid[1, 1] = condition2setB_dropbox;
            conditionDropBoxGrid[2, 1] = condition3setB_dropbox;
            conditionDropBoxGrid[3, 1] = condition4setB_dropbox;
            conditionDropBoxGrid[4, 1] = condition5setB_dropbox;
            conditionDropBoxGrid[5, 1] = condition6setB_dropbox;
            conditionDropBoxGrid[6, 1] = condition7setB_dropbox;
            conditionDropBoxGrid[7, 1] = condition8setB_dropbox;
            conditionDropBoxGrid[8, 1] = condition9setB_dropbox;
            conditionDropBoxGrid[9, 1] = condition10setB_dropbox;
        }

        private void dataFileTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)//enter key
            {
                systemsGrid.Focus();
                systemsGrid.CurrentCell = systemsGrid[0, 0];
            }    
        }

        private void systemsDatagrid_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            for(int i = 0; i< systemsGrid.RowCount; i++)
            {
                systemsGrid.Rows[i].HeaderCell.Value = "System " + (i + 1);
            }
        }

        private void setsDatagrid_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            for (int i = 0; i < setsGrid.RowCount; i++)
            {
                setsGrid.Rows[i].HeaderCell.Value = "Systems in Set " + (i + 1);
            }
        }

        private void uploadSystems(object sender, EventArgs e)
        {
            //determine how many systems the user entered and what the largest divisor of all those systems is
            //so that you can create the systems 2d array

            //delete empty rows
            foreach (DataGridViewRow rw in this.systemsGrid.Rows)
            {
                if (rw.Cells[0].Value == null || rw.Cells[0].Value == DBNull.Value || String.IsNullOrWhiteSpace(rw.Cells[0].Value.ToString()))
                {
                    if (rw.Index != systemsGrid.RowCount-1)
                    {
                        systemsGrid.Rows.RemoveAt(rw.Index);
                    }
                }
            }

            int[,] entries = new int[systemsGrid.RowCount, globalvars.MAXNUMBERSINSINGLESYSTEMENTRY];
            int actualSyscount = 0;
            int colIndex = 0;
            int parsedEntry;

            foreach (DataGridViewRow rw in this.systemsGrid.Rows)
            {
                if (rw.Cells[0].Value == null || rw.Cells[0].Value == DBNull.Value || String.IsNullOrWhiteSpace(rw.Cells[0].Value.ToString()))
                {
                }
                else
                {
                    actualSyscount++;
                    string[] numbers = Regex.Split(rw.Cells[0].Value.ToString(), @"(\-*\d+)");
                        
                    foreach (string value in numbers)
                    {
                        if (!string.IsNullOrEmpty(value.ToString()))
                        {
                            if (int.TryParse(value, out parsedEntry))
                            {
                                entries[rw.Index, colIndex++] = parsedEntry;
                            }
                        }
                    }

                    if(colIndex %2 == 1)
                    {
                        MessageBox.Show("You dont have 1 multiplier for each divisor in System " + (rw.Index+ 1) +".\nFix the issue and try again.");
                        return;
                    }
                }
                colIndex = 0;
            }//end datagridrow loop

            int[,] systems = new int[systemsGrid.RowCount-1, globalvars.MAXDIV];//initializes to zeros by default for int arrays
            int indexer = 0;
            int maxDivisor = 0;
            int entry;

            for(int i = 0; i< systemsGrid.RowCount-1; i++)
            {
                for(int j = 0; j< 2000; j++)
                {
                    entry = entries[i, j];

                    if(j%2 == 0)
                    {   //this is a divisor
                        if(indexer > -1) {
                            indexer = entry;
                        }
                        if(indexer > maxDivisor)
                        {
                            maxDivisor = indexer;
                        }
                    }
                    else//it's a multiplier
                    {
                        systems[i, indexer] = entry;
                        //Console.Write(indexer + "*" + entry + " ");
                    }
                }
                //Console.WriteLine();
            }
            mainbaseMatrix.divisorCount = maxDivisor + 1;
            //set the systems in the Program
            mainbaseMatrix.systemCount = systemsGrid.RowCount - 1;
            mainbaseMatrix.systems = new int[systemsGrid.RowCount - 1, mainbaseMatrix.divisorCount];

            for(int i = 0; i<mainbaseMatrix.systemCount; i++)
            {
                for(int k = 0; k < mainbaseMatrix.divisorCount; k++)
                {
                    mainbaseMatrix.systems[i, k] = systems[i, k];
                }
            }

            mainbaseMatrix.calculateBaseMatrix();
            mainbaseMatrix.calculateSystemsMatrix();
        }

        private void setsDataGrid_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
        {
            int aft = e.RowIndex + e.RowCount;
            for (int i = aft - 1; i < setsGrid.RowCount; i++)
            {
                setsGrid.Rows[i].HeaderCell.Value = "Systems in Set " + (i - e.RowCount + 2);
            }
        }

        private void systemDataGrid_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
        {
            int aft = e.RowIndex + e.RowCount;
            //renames the rows header to be the correct systems number for how many rows are left
            for(int i = aft-1; i< systemsGrid.RowCount; i++)
            {
                systemsGrid.Rows[i].HeaderCell.Value = "System " + (i - e.RowCount+2);
            }
        }

        private void dataGridView1_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            SendKeys.Send("{Right}");
        }

        private void clearSystemsButton_Click(object sender, EventArgs e)
        {
            systemsGrid.RowCount = 1;
            systemsGrid.Focus();
            systemsGrid.CurrentCell = systemsGrid[0, 0];
            clearSetsButton_Click(sender, e);
        }

        private void clearSetsButton_Click(object sender, EventArgs e)
        {
            setsGrid.RowCount = 1;
            setsGrid.Focus();
            setsGrid.CurrentCell = setsGrid[0, 0];
            for(int i = 0; i < 10; i++)
            {
                removeConditionsButton_Click(sender, e);
            }
        }

        private int uploadSets()
        {
            int maxCountOfSystemsInaSet=0;
            //delete empty rows
            foreach (DataGridViewRow rw in this.setsGrid.Rows)
            {

                if (rw.Cells[0].Value == null || rw.Cells[0].Value == DBNull.Value || String.IsNullOrWhiteSpace(rw.Cells[0].Value.ToString()))
                {
                    if (rw.Index != setsGrid.RowCount - 1)
                    {
                        setsGrid.Rows.RemoveAt(rw.Index);
                    }
                }
                else
                {
                    string[] numbers = Regex.Split(rw.Cells[0].Value.ToString(), @"(\-*\d+)");
                    if (numbers.Length > maxCountOfSystemsInaSet) { maxCountOfSystemsInaSet = numbers.Length; }
                }
            }

            mainbaseMatrix.setCount = setsGrid.RowCount - 1;
            mainbaseMatrix.setIndex = new int[mainbaseMatrix.setCount];
            int[,] sets = new int[setsGrid.RowCount-1, maxCountOfSystemsInaSet];
            int colIndex = 0;

            foreach (DataGridViewRow rw in this.setsGrid.Rows)
            {
                if (rw.Cells[0].Value == null || rw.Cells[0].Value == DBNull.Value || String.IsNullOrWhiteSpace(rw.Cells[0].Value.ToString()))
                {
                    //do nothing for the last row that is always empty
                }
                else
                {
                    string[] numbers = Regex.Split(rw.Cells[0].Value.ToString(), @"(\-*\d+)");

                    foreach (string value in numbers)
                    {
                        if (!string.IsNullOrEmpty(value.ToString()))
                        {
                            int j;
                            if (int.TryParse(value, out j))
                            {
                                sets[rw.Index, colIndex++] = j;
                                if (j > mainbaseMatrix.systemCount)
                                {
                                    return rw.Index +1;
                                }
                            }
                        }
                    }
                    mainbaseMatrix.setIndex[rw.Index] = colIndex;//set how many systems are in the set in this row
                    colIndex = 0;
                }
            }//end datagridrow loop

            mainbaseMatrix.sets = sets;
            
            object[] setoptions = new object[mainbaseMatrix.setCount];
            for(int i = 0; i< mainbaseMatrix.setCount; i++)
            {
                setoptions[i] = i + 1;
            }

            condition1setA_dropbox.Items.Clear();
            condition1setA_dropbox.Items.AddRange(setoptions);
            condition1setB_dropbox.Items.Clear();
            condition1setB_dropbox.Items.AddRange(setoptions);
            condition2setA_dropbox.Items.Clear();
            condition2setA_dropbox.Items.AddRange(setoptions);
            condition2setB_dropbox.Items.Clear();
            condition2setB_dropbox.Items.AddRange(setoptions);
            condition3setA_dropbox.Items.Clear();
            condition3setA_dropbox.Items.AddRange(setoptions);
            condition3setB_dropbox.Items.Clear();
            condition3setB_dropbox.Items.AddRange(setoptions);
            condition4setA_dropbox.Items.Clear();
            condition4setA_dropbox.Items.AddRange(setoptions);
            condition4setB_dropbox.Items.Clear();
            condition4setB_dropbox.Items.AddRange(setoptions);
            condition5setA_dropbox.Items.Clear();
            condition5setA_dropbox.Items.AddRange(setoptions);
            condition5setB_dropbox.Items.Clear();
            condition5setB_dropbox.Items.AddRange(setoptions);
            condition6setA_dropbox.Items.Clear();
            condition6setA_dropbox.Items.AddRange(setoptions);
            condition6setB_dropbox.Items.Clear();
            condition6setB_dropbox.Items.AddRange(setoptions);
            condition7setA_dropbox.Items.Clear();
            condition7setA_dropbox.Items.AddRange(setoptions);
            condition7setB_dropbox.Items.Clear();
            condition7setB_dropbox.Items.AddRange(setoptions);
            condition8setA_dropbox.Items.Clear();
            condition8setA_dropbox.Items.AddRange(setoptions);
            condition8setB_dropbox.Items.Clear();
            condition8setB_dropbox.Items.AddRange(setoptions);
            condition9setA_dropbox.Items.Clear();
            condition9setA_dropbox.Items.AddRange(setoptions);
            condition9setB_dropbox.Items.Clear();
            condition9setB_dropbox.Items.AddRange(setoptions);
            condition10setA_dropbox.Items.Clear();
            condition10setA_dropbox.Items.AddRange(setoptions);
            condition10setB_dropbox.Items.Clear();
            condition10setB_dropbox.Items.AddRange(setoptions);
            return 0;
        }

        private void displayConditions()
        {
            panel1.Visible = false;
            panel2.Visible = false;
            panel3.Visible = false;
            panel4.Visible = false;
            panel5.Visible = false;
            panel6.Visible = false;
            panel7.Visible = false;
            panel8.Visible = false;
            panel9.Visible = false;
            panel10.Visible = false;
            if (globalvars.visibleConditions >= 1) panel1.Visible = true;
            if (globalvars.visibleConditions >= 2) panel2.Visible = true;
            if (globalvars.visibleConditions >= 3) panel3.Visible = true;
            if (globalvars.visibleConditions >= 4) panel4.Visible = true;
            if (globalvars.visibleConditions >= 5) panel5.Visible = true;
            if (globalvars.visibleConditions >= 6) panel6.Visible = true;
            if (globalvars.visibleConditions >= 7) panel7.Visible = true;
            if (globalvars.visibleConditions >= 8) panel8.Visible = true;
            if (globalvars.visibleConditions >= 9) panel9.Visible = true;
            if (globalvars.visibleConditions >= 10) panel10.Visible = true;
        }

        private void addConditionsButton_Click(object sender, EventArgs e)
        {
            if (globalvars.visibleConditions < globalvars.MAXCONDITIONS)
            {
                globalvars.visibleConditions++;
                mainbaseMatrix.conditionCount = globalvars.visibleConditions;
                displayConditions();
            }
        }

        private void removeConditionsButton_Click(object sender, EventArgs e)
        {
            if (globalvars.visibleConditions > 0)
            {
                globalvars.visibleConditions--;
                mainbaseMatrix.conditionCount--;
                mainbaseMatrix.setConditions[globalvars.visibleConditions].aCount = 0;
                mainbaseMatrix.setConditions[globalvars.visibleConditions].bCount = 0;
                mainbaseMatrix.setConditions[globalvars.visibleConditions].setA = 0;
                mainbaseMatrix.setConditions[globalvars.visibleConditions].setB = 0;
                mainbaseMatrix.setConditions[globalvars.visibleConditions].aFrac = 0;
                mainbaseMatrix.setConditions[globalvars.visibleConditions].bFrac = 0;

                conditionTextBoxGrid[mainbaseMatrix.conditionCount, 0].Text = "";
                conditionTextBoxGrid[mainbaseMatrix.conditionCount, 1].Text = "";
                conditionDropBoxGrid[mainbaseMatrix.conditionCount, 0].Text = "";
                conditionDropBoxGrid[mainbaseMatrix.conditionCount, 1].Text = "";
                displayConditions(); 
            }
        }

        private void condition_textbox_TextChanged(object sender, EventArgs e)
        {
            globalvars.setConditionsGridMatchesWhatsInMemory = false;
            if (sender.Equals(conditionTextBoxGrid[0, 0])) { updateFrac(conditionTextBoxGrid[0, 0].Text.ToString(), 0, 0); }
            if (sender.Equals(conditionTextBoxGrid[1, 0])) { updateFrac(conditionTextBoxGrid[1, 0].Text.ToString(), 1, 0); }
            if (sender.Equals(conditionTextBoxGrid[2, 0])) { updateFrac(conditionTextBoxGrid[2, 0].Text.ToString(), 2, 0); }
            if (sender.Equals(conditionTextBoxGrid[3, 0])) { updateFrac(conditionTextBoxGrid[3, 0].Text.ToString(), 3, 0); }
            if (sender.Equals(conditionTextBoxGrid[4, 0])) { updateFrac(conditionTextBoxGrid[4, 0].Text.ToString(), 4, 0); }
            if (sender.Equals(conditionTextBoxGrid[5, 0])) { updateFrac(conditionTextBoxGrid[5, 0].Text.ToString(), 5, 0); }
            if (sender.Equals(conditionTextBoxGrid[6, 0])) { updateFrac(conditionTextBoxGrid[6, 0].Text.ToString(), 6, 0); }
            if (sender.Equals(conditionTextBoxGrid[7, 0])) { updateFrac(conditionTextBoxGrid[7, 0].Text.ToString(), 7, 0); }
            if (sender.Equals(conditionTextBoxGrid[8, 0])) { updateFrac(conditionTextBoxGrid[8, 0].Text.ToString(), 8, 0); }
            if (sender.Equals(conditionTextBoxGrid[9, 0])) { updateFrac(conditionTextBoxGrid[9, 0].Text.ToString(), 9, 0); }

            if (sender.Equals(conditionTextBoxGrid[0, 1])) { updateFrac(conditionTextBoxGrid[0, 1].Text.ToString(), 0, 1); }
            if (sender.Equals(conditionTextBoxGrid[1, 1])) { updateFrac(conditionTextBoxGrid[1, 1].Text.ToString(), 1, 1); }
            if (sender.Equals(conditionTextBoxGrid[2, 1])) { updateFrac(conditionTextBoxGrid[2, 1].Text.ToString(), 2, 1); }
            if (sender.Equals(conditionTextBoxGrid[3, 1])) { updateFrac(conditionTextBoxGrid[3, 1].Text.ToString(), 3, 1); }
            if (sender.Equals(conditionTextBoxGrid[4, 1])) { updateFrac(conditionTextBoxGrid[4, 1].Text.ToString(), 4, 1); }
            if (sender.Equals(conditionTextBoxGrid[5, 1])) { updateFrac(conditionTextBoxGrid[5, 1].Text.ToString(), 5, 1); }
            if (sender.Equals(conditionTextBoxGrid[6, 1])) { updateFrac(conditionTextBoxGrid[6, 1].Text.ToString(), 6, 1); }
            if (sender.Equals(conditionTextBoxGrid[7, 1])) { updateFrac(conditionTextBoxGrid[7, 1].Text.ToString(), 7, 1); }
            if (sender.Equals(conditionTextBoxGrid[8, 1])) { updateFrac(conditionTextBoxGrid[8, 1].Text.ToString(), 8, 1); }
            if (sender.Equals(conditionTextBoxGrid[9, 1])) { updateFrac(conditionTextBoxGrid[9, 1].Text.ToString(), 9, 1); }
            uploadsetConditions();
            resultsTab_Enter(sender,e);
        }

        private void condition_dropbox_TextChanged(object sender, EventArgs e)
        {
            globalvars.setConditionsGridMatchesWhatsInMemory = false;
            if (sender.Equals(conditionDropBoxGrid[0, 0])) { updateSet(conditionDropBoxGrid[0, 0].Text.ToString(), 0, 0); }
            if (sender.Equals(conditionDropBoxGrid[1, 0])) { updateSet(conditionDropBoxGrid[1, 0].Text.ToString(), 1, 0); }
            if (sender.Equals(conditionDropBoxGrid[2, 0])) { updateSet(conditionDropBoxGrid[2, 0].Text.ToString(), 2, 0); }
            if (sender.Equals(conditionDropBoxGrid[3, 0])) { updateSet(conditionDropBoxGrid[3, 0].Text.ToString(), 3, 0); }
            if (sender.Equals(conditionDropBoxGrid[4, 0])) { updateSet(conditionDropBoxGrid[4, 0].Text.ToString(), 4, 0); }
            if (sender.Equals(conditionDropBoxGrid[5, 0])) { updateSet(conditionDropBoxGrid[5, 0].Text.ToString(), 5, 0); }
            if (sender.Equals(conditionDropBoxGrid[6, 0])) { updateSet(conditionDropBoxGrid[6, 0].Text.ToString(), 6, 0); }
            if (sender.Equals(conditionDropBoxGrid[7, 0])) { updateSet(conditionDropBoxGrid[7, 0].Text.ToString(), 7, 0); }
            if (sender.Equals(conditionDropBoxGrid[8, 0])) { updateSet(conditionDropBoxGrid[8, 0].Text.ToString(), 8, 0); }
            if (sender.Equals(conditionDropBoxGrid[9, 0])) { updateSet(conditionDropBoxGrid[9, 0].Text.ToString(), 9, 0); }

            if (sender.Equals(conditionDropBoxGrid[0, 1])) { updateSet(conditionDropBoxGrid[0, 1].Text.ToString(), 0, 1); }
            if (sender.Equals(conditionDropBoxGrid[1, 1])) { updateSet(conditionDropBoxGrid[1, 1].Text.ToString(), 1, 1); }
            if (sender.Equals(conditionDropBoxGrid[2, 1])) { updateSet(conditionDropBoxGrid[2, 1].Text.ToString(), 2, 1); }
            if (sender.Equals(conditionDropBoxGrid[3, 1])) { updateSet(conditionDropBoxGrid[3, 1].Text.ToString(), 3, 1); }
            if (sender.Equals(conditionDropBoxGrid[4, 1])) { updateSet(conditionDropBoxGrid[4, 1].Text.ToString(), 4, 1); }
            if (sender.Equals(conditionDropBoxGrid[5, 1])) { updateSet(conditionDropBoxGrid[5, 1].Text.ToString(), 5, 1); }
            if (sender.Equals(conditionDropBoxGrid[6, 1])) { updateSet(conditionDropBoxGrid[6, 1].Text.ToString(), 6, 1); }
            if (sender.Equals(conditionDropBoxGrid[7, 1])) { updateSet(conditionDropBoxGrid[7, 1].Text.ToString(), 7, 1); }
            if (sender.Equals(conditionDropBoxGrid[8, 1])) { updateSet(conditionDropBoxGrid[8, 1].Text.ToString(), 8, 1); }
            if (sender.Equals(conditionDropBoxGrid[9, 1])) { updateSet(conditionDropBoxGrid[9, 1].Text.ToString(), 9, 1); }
            uploadsetConditions();
            resultsTab_Enter(sender, e);
        }

        private void updateFrac(String text, int i, int j)
        {
            int value;
            if (int.TryParse(text, out value)) {
                if (j == 0)
                {
                    if(value < (int)Math.Ceiling((((double)(mainbaseMatrix.setConditions[i].aCount+1)) / ((double)2))))
                    {
                        conditionTextBoxGrid[i, j].Text = ((int)Math.Ceiling((((double)(mainbaseMatrix.setConditions[i].aCount + 1)) / ((double)2)))).ToString();
                        MessageBox.Show(
                            "Warning:\n\nThe value you have entered in the box on the far left makes it possible \nfor "
                            + value + " systems in set " + mainbaseMatrix.setConditions[i].setA + " to be > than the other set " +
                            "\nAND for " + value + " systems in set " + mainbaseMatrix.setConditions[i].setA + " to be < than the other set at the SAME TIME.\n\n" +
                            "Enter a value greater than or equal to: " + (int)Math.Ceiling((((double)(mainbaseMatrix.setConditions[i].aCount + 1)) / ((double)2))) +
                            "\n"+(int)Math.Ceiling((((double)(mainbaseMatrix.setConditions[i].aCount + 1)) / ((double)2)))+" has been entered for you."
                        );
                        value = (int)Math.Ceiling((((double)(mainbaseMatrix.setConditions[i].aCount + 1)) / ((double)2)));
                        mainbaseMatrix.setConditions[i].aFrac = value;
                    }

                    mainbaseMatrix.setConditions[i].aFrac = value;
                }
                else//j==1
                {
                    mainbaseMatrix.setConditions[i].bFrac = value;
                }
            }
            else if(!text.Equals(""))
            {
                MessageBox.Show("Numerical input only please.");
                SendKeys.Send("\b");
            }
        }

        private void updateSet(String text, int conditionIndex, int j)
        {
            int value;
            if (int.TryParse(text, out value))
            {
                if (value>mainbaseMatrix.setCount) {
                    value = mainbaseMatrix.setCount;
                    conditionDropBoxGrid[conditionIndex, j].Text = value.ToString();
                    MessageBox.Show("You only have " + value + "sets. The value was set to " + value);
                }

                try {
                    if (j == 0)
                    {
                        

                        mainbaseMatrix.setConditions[conditionIndex].setA = value;
                        mainbaseMatrix.setConditions[conditionIndex].aCount = mainbaseMatrix.setIndex[value - 1];

                        if (mainbaseMatrix.setConditions[conditionIndex].aFrac < (int)Math.Ceiling((((double)(mainbaseMatrix.setConditions[conditionIndex].aCount + 1)) / ((double)2))))
                        {
                            conditionTextBoxGrid[conditionIndex, j].Text = ((int)Math.Ceiling((((double)(mainbaseMatrix.setConditions[conditionIndex].aCount + 1)) / ((double)2)))).ToString();
                            MessageBox.Show(
                                "Warning:\n\nThe value you have entered in the box on the far left makes it possible \nfor "
                                + value + " systems in set " + mainbaseMatrix.setConditions[conditionIndex].setA + " to be > than the other set " +
                                "\nAND for " + value + " systems in set " + mainbaseMatrix.setConditions[conditionIndex].setA + " to be < than the other set at the SAME TIME.\n\n" +
                                "Enter a value greater than or equal to: " + (int)Math.Ceiling((((double)(mainbaseMatrix.setConditions[conditionIndex].aCount + 1)) / ((double)2))) +
                                "\n" + (int)Math.Ceiling((((double)(mainbaseMatrix.setConditions[conditionIndex].aCount + 1)) / ((double)2))) + " has been entered for you."
                            );
                            value = (int)Math.Ceiling((((double)(mainbaseMatrix.setConditions[conditionIndex].aCount + 1)) / ((double)2)));
                            mainbaseMatrix.setConditions[conditionIndex].aFrac = value;
                        }
                    }
                    else//j==1
                    {
                        mainbaseMatrix.setConditions[conditionIndex].setB = value;
                        mainbaseMatrix.setConditions[conditionIndex].bCount = mainbaseMatrix.setIndex[value - 1];
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("Invalid input. Have you entered your sets correctly?\nYou might have entered a set number that doesn't exist.");
                    conditionDropBoxGrid[conditionIndex, j].Text = "";
                }
            }
            else if (!text.Equals(""))
            {
                MessageBox.Show("Numerical input only please.");
                SendKeys.Send("\b");
            }
        }

        bool flagsystems = false;
        private void systemsDatagrid_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (flagsystems == false) { 
                DataGridViewTextBoxEditingControl tb = (DataGridViewTextBoxEditingControl)e.Control;
                //tb.KeyPress += new KeyPressEventHandler(systemsDatagrid_KeyPress);

                e.Control.KeyPress += new KeyPressEventHandler(systemsDatagrid_KeyPress);
                flagsystems = true;
            }
        }

        bool flagsets = false;
        private void setsDatagrid_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (flagsets == false)
            {
                DataGridViewTextBoxEditingControl tb = (DataGridViewTextBoxEditingControl)e.Control;
                //tb.KeyPress += new KeyPressEventHandler(systemsDatagrid_KeyPress);

                e.Control.KeyPress += new KeyPressEventHandler(systemsDatagrid_KeyPress);
                flagsets = true;
            }
        }

        bool handlingspace = false;
        private void systemsDatagrid_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsLetter(e.KeyChar))
            {
                e.Handled = true;
                MessageBox.Show("Numerical Input only please.");
            }else if(e.KeyChar == ' ')
            {
                if (handlingspace == false)
                {
                    handlingspace = true;
                    SendKeys.Send("    ");
                    SendKeys.Flush();
                    handlingspace = false;
                }
            }
        }

        private void uploadsetConditions()
        {
            for(int i = 0; i< globalvars.MAXCONDITIONS; i++)
            {

                //updateFrac and updateSet is the area where you have force the grid to be something that works
                updateFrac(conditionTextBoxGrid[i, 0].Text.ToString(),i,0);
                updateSet(conditionDropBoxGrid[i, 0].Text.ToString(),i,0);

                updateFrac(conditionTextBoxGrid[i, 1].Text.ToString(), i, 1);
                updateSet(conditionDropBoxGrid[i, 1].Text.ToString(), i, 1);
            }
        }

        private void dataFileTextBox_Leave(object sender, EventArgs e)
        {
            String filename = "D:\\Josh\\Documents\\" + dataFileTextBox.Text.ToString() + ".txt";
            if (!File.Exists(filename))
            {
                filename = "C:\\datafiles\\" + dataFileTextBox.Text.ToString() + ".txt";
                if (!File.Exists(filename))
                {
                    filename = "C:\\datafiles\\" + dataFileTextBox.Text.ToString() + ".dat";
                }
            }

            if (File.Exists(filename))
            {
                int lineIndex = 0;
                string line;
                double value = 0.0;
                double lastValue = 0.0;

                // Read the file and display it line by line.
                try
                {

                    System.IO.StreamReader file = new System.IO.StreamReader(filename);
                    while (file.ReadLine() != null)
                    {
                        lineIndex++;
                    }
                    file.Close();

                    mainbaseMatrix.data = new double[lineIndex];
                    mainbaseMatrix.dataChange = new int[lineIndex];
                    file = new System.IO.StreamReader(filename);
                    lineIndex = 0;
                    while ((line = file.ReadLine()) != null)
                    {
                        if (Double.TryParse(line, out value) == true)
                        {
                            mainbaseMatrix.data[lineIndex] = value;
                            if (lineIndex > 0) {
                                if (value > lastValue) {
                                    mainbaseMatrix.dataChange[lineIndex] = 1;
                                }else if (value < lastValue)
                                {
                                    mainbaseMatrix.dataChange[lineIndex] = -1;
                                }
                                else//no change
                                {
                                    mainbaseMatrix.dataChange[lineIndex] = 0;
                                }
                            }
                            else
                            {
                                mainbaseMatrix.dataChange[lineIndex] = 0;
                            }
                            lineIndex++;
                            lastValue = value;
                        }
                    }
                    mainbaseMatrix.dataCount = lineIndex;

                    //MessageBox.Show("Read " + mainbaseMatrix.dataCount + " pieces of data.");
                }
                catch (System.IO.IOException ex)
                {
                    MessageBox.Show(ex.Message + " Please close the file and press OK.");
                }
                //count how many lines there are and then fill in the data array
            }
            else
            {
                MessageBox.Show("The data file you entered is misspelled or does not exist");
                //possibly enter code to create the data file and enter the data here
            }
        }

        private void fillresultsgrid()
        {
            //systemCount, setCount, setCount, conditionCount, conditionCount, 2
            int startForRegAvg = mainbaseMatrix.systemCount + mainbaseMatrix.setCount * 2;
            int totalcols;
            if (mainbaseMatrix.conditionCount == 0)
            {
                totalcols = mainbaseMatrix.systemCount + mainbaseMatrix.setCount + mainbaseMatrix.setCount + mainbaseMatrix.conditionCount + mainbaseMatrix.conditionCount;
            }
            else
            {
                totalcols = mainbaseMatrix.systemCount + mainbaseMatrix.setCount + mainbaseMatrix.setCount + mainbaseMatrix.conditionCount + mainbaseMatrix.conditionCount + 2;
            }
            resultsGrid.ColumnCount = 0;

            resultsGrid.ColumnCount = totalcols;
            resultsGrid.RowCount = 11;

            int i = 0;
            for(; i<mainbaseMatrix.systemCount; i++)
            {
                resultsGrid.Columns[i].HeaderText = "System "+ (i+1);
                fillresultsaux(i);
            }
            for(; i< mainbaseMatrix.systemCount+mainbaseMatrix.setCount; i++)
            {
                resultsGrid.Columns[i].HeaderText = "Set" + (i + 1 - mainbaseMatrix.systemCount) + " MRP";
                fillresultsaux(i);
            }
            for(; i< mainbaseMatrix.systemCount+mainbaseMatrix.setCount*2; i++)
            {
                resultsGrid.Columns[i].HeaderText = "Set" + (i + 1 - mainbaseMatrix.systemCount - mainbaseMatrix.setCount) + " FLAT";
                fillresultsaux(i);
            }
            for(;i< mainbaseMatrix.systemCount + mainbaseMatrix.setCount * 2 + mainbaseMatrix.conditionCount; i++)
            {
                resultsGrid.Columns[i].HeaderText = "Condition" + (i + 1 - mainbaseMatrix.systemCount - mainbaseMatrix.setCount*2) + " MRP";
                fillresultsaux(i);
            }
            for (; i < mainbaseMatrix.systemCount + mainbaseMatrix.setCount * 2 + mainbaseMatrix.conditionCount*2; i++)
            {
                resultsGrid.Columns[i].HeaderText = "Condition" + (i + 1 - mainbaseMatrix.systemCount - mainbaseMatrix.setCount * 2 - mainbaseMatrix.conditionCount) + " FLAT";
                fillresultsaux(i);
            }

            if (mainbaseMatrix.conditionCount > 0)
            {
                for (; i < mainbaseMatrix.systemCount + mainbaseMatrix.setCount * 2 + mainbaseMatrix.conditionCount * 2 + 1; i++)
                {
                    resultsGrid.Columns[i].Width = 140;
                    resultsGrid.Columns[i].HeaderText = "AllConditions MRP";
                    fillresultsaux(i);
                }
                for (; i < mainbaseMatrix.systemCount + mainbaseMatrix.setCount * 2 + mainbaseMatrix.conditionCount * 2 + 2; i++)
                {
                    resultsGrid.Columns[i].Width = 140;
                    resultsGrid.Columns[i].HeaderText = "AllConditions FLAT";
                    fillresultsaux(i);
                }
            }

            resultsGrid.Rows[0].HeaderCell.Value = "Grand Total";
            resultsGrid.Rows[1].HeaderCell.Value = "Trade Count";
            resultsGrid.Rows[2].HeaderCell.Value = "Win Count";
            resultsGrid.Rows[3].HeaderCell.Value = "Loss Count";
            resultsGrid.Rows[4].HeaderCell.Value = "Tie Count";
            resultsGrid.Rows[5].HeaderCell.Value = "Avg Win";
            resultsGrid.Rows[6].HeaderCell.Value = "Avg Loss";
            resultsGrid.Rows[7].HeaderCell.Value = "Win Total";
            resultsGrid.Rows[8].HeaderCell.Value = "Loss Total";
            resultsGrid.Rows[9].HeaderCell.Value = "Biggest Win";
            resultsGrid.Rows[10].HeaderCell.Value = "Biggest Loss";

            resultsGrid.RowHeadersWidth = 170;

            foreach(DataGridViewColumn column in resultsGrid.Columns)
            {
                column.SortMode = DataGridViewColumnSortMode.NotSortable;
            }
        }

        private void fillresultsaux(int i)
        {
            resultsGrid.Rows[0].Cells[i].Value = String.Format("{0:N2}", mainbaseMatrix.totals[i].grandtotal);
            resultsGrid.Rows[1].Cells[i].Value = mainbaseMatrix.totals[i].tradecount.ToString();
            resultsGrid.Rows[2].Cells[i].Value = mainbaseMatrix.totals[i].wincount.ToString();
            resultsGrid.Rows[3].Cells[i].Value = mainbaseMatrix.totals[i].losscount.ToString();
            resultsGrid.Rows[4].Cells[i].Value = mainbaseMatrix.totals[i].tiecount.ToString();
            resultsGrid.Rows[5].Cells[i].Value = String.Format("{0:N2}", mainbaseMatrix.totals[i].avgwin);
            resultsGrid.Rows[6].Cells[i].Value = String.Format("{0:N2}", mainbaseMatrix.totals[i].avgloss);
            resultsGrid.Rows[7].Cells[i].Value = String.Format("{0:N2}", mainbaseMatrix.totals[i].wintotal);
            resultsGrid.Rows[8].Cells[i].Value = String.Format("{0:N2}", mainbaseMatrix.totals[i].losstotal);
            resultsGrid.Rows[9].Cells[i].Value = String.Format("{0:N2}", mainbaseMatrix.totals[i].biggestwin);
            resultsGrid.Rows[10].Cells[i].Value = String.Format("{0:N2}", mainbaseMatrix.totals[i].biggestloss);
        }

        private void resultsTab_Enter(object sender, EventArgs e)
        {
            mainbaseMatrix.calculateConditionMatrix();
            mainbaseMatrix.calculateConditionsTogetherMatrix();
            mainbaseMatrix.calculateTotals();
            fillresultsgrid();
        }

        private void systemsTab_Leave(object sender, EventArgs e)
        {
            uploadSystems(sender, e);
            int status = uploadSets();

            if (status > 0)
            {
                MessageBox.Show("You've entered a system number that doesn't exist in set " + status + " please remove it.");
            }
            else {
                mainbaseMatrix.calculateSetsMatrix();
            }
        }

        private void systemsGrid_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            var senderGrid = (DataGridView)sender;
            if (e.ColumnIndex == -1)
            {
                senderGrid.Rows[e.RowIndex].HeaderCell.Value = "clicked";
            }
        }

        private void dataChange_CheckedChanged(object sender, EventArgs e)
        {
            if(dataChangeCheckBox.Checked == true)
            {
                mainbaseMatrix.useDataChangeNotData = true;
            }else
            {
                mainbaseMatrix.useDataChangeNotData = false;
            }
        }

        /*When you click the "Solve All" tab on the program it immediately starts to calculate*/
        private void SolveAllTab_Enter(object sender, EventArgs e)
        {
            /*since this is the main point where calculation begins I have other ways of finding systems 
            commented out, gothroughAll accesses a file that has lists of systems, I wrote a separate Java
            program to generate such a file. BestFirstSearch uses AI genetic algos to mutate systems and find
            improvements*/
            mainbaseMatrix.doThreads();
            //mainbaseMatrix.gothroughAll(); //you have a file that has like a million+ systems to check
            //mainbaseMatrix.BestFirstSearch(dataFileTextBox.Text.ToString(),3600); //you alter the system through genetic mutations
        }
    }

    public static class globalvars
    {
        public static int MAXNUMBERSINSINGLESYSTEMENTRY = 2000;
        public static bool systems;
        public static bool setConditionsGridMatchesWhatsInMemory = true;  
        public static int MAXCONDITIONS = 10;
        public static int visibleConditions = 0;
        public static int MAXDIV = 10001;
    }
}
