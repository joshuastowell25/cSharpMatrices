/// <author>Josh Stowell</author>
/// <since>2015</since>
/// <version>1.0</version>
/// <summary>A highly efficient implementation of moving average matrices for finding 
/// good moving average stock market strategies</summary>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Threading;


//My dad, Ralph, encouraged me to write this program
namespace RalphsProgram
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1()); 
        }
    }

    //specifies a condition that must hold true to be long
    public struct positionCondition{
        public int setA;
        public int setB;
        public int aCount;
        public int bCount;
        public int aFrac;
        public int bFrac;
    }

    public struct threadArg
    {
        public int[] sys;
        public int number;
    }

    public struct gtCalc
    {
        public double grandtotal;
        public double wintotal;
        public double losstotal;
        public int tradecount;
        public int wincount;
        public int losscount;
        public int tiecount;
        public double avgwin;
        public double avgloss;
        public double biggestloss;
        public double biggestwin;

        public int lastposition;
        public int currentposition;
        public double lastpositionchangeprice;
    }

    class workerClass
    {
        bool writeToFiles = false;
        public bool useDataChangeNotData = false;
        public int divisorCount = 0;    //the largest divisor in any system plus one (for the zero divisor)
        public int systemCount = 0;     //the number of systems
        public int dataCount = 0;       //the number of pieces of data
        public int setCount = 0;        //the number of sets
        public int conditionCount = 1;
        public int[,] systems;          //holds the divisors/multipliers that make up each system
        public int[,] sets;             //holds the systems that make up each set
        public int[,] conditionMatrix;  //holds -1,0,1 for each condition for each piece of data
        public int[,] conditionsTogetherMatrix;  //when you use all the conditions to make a play
        public int[] setIndex;          //holds the number of systems -1 for each set (the index of the last system)
        public double[] data;           //the user's data
        public int[] dataChange;        //each entry in this array contains either a -1,0, or 1 depending on whether the data that tick was less same or more than the last piece
        public positionCondition[] setConditions;  //array of conditions between sets
        public gtCalc[] totals;         //the array of gtCalcs for every system, set, condition, and the conditions together
        double[,] baseMatrix;           //the base matrix calculated for everydivisor up to divisorCount for all the data

        double[,] systemsMatrix;
        double[,] setsMatrix;           //the calculated out sets matrix
        int lastbaseMatrixwidth = 0;
        int lastbaseMatrixlength = 0;

        public workerClass()
        {
            this.setConditions = new positionCondition[10];
        }

        public void calculateTotals()
        {
            totals = new gtCalc[systemCount + setCount * 2 + conditionCount * 2 + 2];
            if (data != null)
            {
                calculatetotalsaux_MostRecentPosition_double(0, 0, systemCount, ref systemsMatrix);
                //calculatetotalsaux_Flat_double(0, 0, systemCount, ref systemsMatrix);

                calculatetotalsaux_MostRecentPosition_double(systemCount, 0, setCount, ref setsMatrix);
                calculatetotalsaux_Flat_double(systemCount + setCount, setCount, setCount, ref setsMatrix);

                calculatetotalsaux_MostRecentPosition_int(systemCount + setCount * 2, 0, conditionCount, ref conditionMatrix);
                calculatetotalsaux_Flat_int(systemCount + setCount * 2 + conditionCount, conditionCount, conditionCount, ref conditionMatrix);

                calculatetotalsaux_MostRecentPosition_int(systemCount + setCount * 2 + conditionCount * 2, 0, 1, ref conditionsTogetherMatrix);
                calculatetotalsaux_Flat_int(systemCount + setCount * 2 + conditionCount * 2 + 1, 1, 1, ref conditionsTogetherMatrix);
            }
        }

        public void doThreads()
        {
            int threadCount = 1;
            threadArg[] argArray = new threadArg[threadCount];
            /*argArray[0].sys = new int[] { 38, 66, 94, 122, 150, 178, 206, 234, 262, 290 };
            argArray[0].number = 0;
            argArray[1].sys = new int[] { 20, 40, 60, 120, 160, 200, 240, 220, 260, 290 };
            argArray[1].number = 1;*/
            argArray[0].sys = new int[] {2,4,6,8,10,12,14,16,18,20,22,24,26,28,30};
            argArray[0].number = 0;
            /*argArray[3].sys = new int[] { 40, 40, 100, 130, 150, 180, 200, 230, 260, 290 };
            argArray[3].number = 3;
            argArray[4].sys = new int[] { 34, 54, 94, 122, 144, 178, 200, 200, 262, 290 };
            argArray[4].number = 4;
            */

            Thread[] threadArray = new Thread[threadCount];
            for(int i = 0; i< threadCount; i++)
            {
                threadArray[i] = new Thread(new ParameterizedThreadStart(this.createSystem));
                threadArray[i].Start(argArray[i]);
            }
        }

        public void gothroughAll()
        {
            String outfilename = "D:\\systemsALLOUT.csv";
            String infilename = "D:\\systemsALL.csv";
            string line;
            int max = 0;
            int min = 0;
            int num = 0;
            String[] items;

            DateTime start = DateTime.UtcNow;

            if (File.Exists(infilename))
            {

                try
                {
                    System.IO.StreamWriter outfile = new System.IO.StreamWriter(outfilename);
                    System.IO.StreamReader infile = new System.IO.StreamReader(infilename);
                    
                    while ((line = infile.ReadLine()) != null)
                    {
                        items = line.Split(',');
                        num = int.Parse(items[1]);
                        if (num > max || num < min)
                        {
                            if (num > max)
                            {
                                max = num;
                            }else if(num < min)
                            {
                                min = num;
                            }
                            
                            outfile.WriteLine(line);
                            outfile.Flush();
                        }
                        

                    }
                    outfile.Write("0");
                    infile.Close();
                    outfile.Close();
                }
                catch (Exception e)
                {
                    MessageBox.Show("Error Message: " + e.ToString(), "Error Title", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }

            DateTime end = DateTime.UtcNow;

            MessageBox.Show("Took " + (end - start).TotalSeconds + " seconds", "Done", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        public void createSystem(object thing1)
        {
            //read 1 system convert to a real system entry
            //calculate it, outputting the system, gt, and trades to another file
            //do the next one
            threadArg t1 = (threadArg)thing1;

            int systemsChecked = 0;
            int[] systemNums = t1.sys;
            int maxdiv = 501;
            systemCount = 1;
            divisorCount = maxdiv;
            calculateBaseMatrix();
            systems = new int[1, maxdiv];//initializes to zeros by default for int arrays

            String outfilename = "D:\\systemsALL.csv";
            String infilename = "D:\\systems.txt";
            string system;
            int mult;
            int i = 0;

            DateTime start = DateTime.UtcNow;

            if (File.Exists(infilename))
            {

                try
                {
                    System.IO.StreamWriter outfile = new System.IO.StreamWriter(outfilename);
                    System.IO.StreamReader infile = new System.IO.StreamReader(infilename);
                    while ((system=infile.ReadLine()) != null)
                    {
                        foreach(char c in system)
                        {
                            mult = int.Parse(c.ToString());
                            mult--;
                            systems[0, systemNums[i++]] = mult;
                        }

                        i = 0;
                        calculateSystemsMatrix();
                        totals = new gtCalc[1];
                        calculatetotalsaux_MostRecentPosition_double(0, 0, systemCount, ref systemsMatrix);

                        outfile.WriteLine(system+","+totals[0].grandtotal+","+totals[0].tradecount+",");
                        systemsChecked++;
                        if (systemsChecked%1000 ==0) { Console.WriteLine("systems checked: "+systemsChecked); }
                    }
                    outfile.Write("0");
                    infile.Close();
                    outfile.Close();
                }
                catch (Exception e)
                {
                    MessageBox.Show("Error Message: "+e.ToString(), "Error Title", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }

            DateTime end = DateTime.UtcNow;

            MessageBox.Show("Took "+ (end- start).TotalSeconds +" seconds", "Done", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        public string sysString(List<int> sys)
        {
            string sysstring = "";

            sys.ForEach(delegate (int s)
            {
                sysstring += s + " ";
            });
            return sysstring;
        }


        public void setSystems(List<int> divisors, List<int> multipliers, int maxdivisor)
        {
            int m = 0;
            try {
                systems = new int[1, maxdivisor];//initializes to zeros by default for int arrays
                for (m = 0; m < divisors.Count; m++)
                {
                    systems[0, divisors[m]] += multipliers[m];
                }
            } catch (Exception e) {
                MessageBox.Show("m is "+m+", divisor length is "+divisors.Count +", multipliers length is "+multipliers.Count+
                    "\nmultipliers: "+listToString(multipliers)+
                    "\ndivisors: "+listToString(divisors) +
                    "\n Error Message: " + e.ToString(), "Error Title", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        public String listToString(List<int> list)
        {
            String result = "";
            for(int i  = 0; i<list.Count(); i++)
            {
                result += list[i].ToString()+" ";
            }

            return result;
        }

        /*this modifies systems using genetic algorithms and keeps improvements
        thus finding better systems until getting stuck at a local maxima*/
        public void BestFirstSearch(String companyName, long timeLimit)
        {
            List<int> divisors = new List<int>() { 70, 72, 74, 76, 78, 80};
            List<int> multipliers = new List<int>() { 1, 1, 1, -1, -1, -1 };
            int maxdivisor = 401;
            setSystems(divisors, multipliers, maxdivisor);
            
            systemCount = 1;
            divisorCount = maxdivisor;
            calculateBaseMatrix();

            String outfilename = "C:\\datafiles\\"+companyName+"_BestFirstSearch_Normal.csv";
            DateTime start = DateTime.UtcNow;
            int oldmult;
            Boolean innerImproved = false;
            Boolean outterImproved = false;
            int olddiv;
            int loopsNotImproved = 0;
            try
            {
                System.IO.StreamWriter outfile = new System.IO.StreamWriter(outfilename);


                totals = new gtCalc[1];
                calculateSystemsMatrix();
                calculatetotalsaux_MostRecentPosition_double(0, 0, systemCount, ref systemsMatrix);
                double bestGT = totals[0].grandtotal;
                double oldbestGT = bestGT;
                int bestTC = totals[0].tradecount;
                outfile.WriteLine(sysString(divisors) + "," + sysString(multipliers) + "," + totals[0].grandtotal + "," + totals[0].tradecount + ",");
                outfile.Flush();
                Console.WriteLine(sysString(divisors) + "," + sysString(multipliers) + "," + totals[0].grandtotal + "," + totals[0].tradecount + ",");

                while (loopsNotImproved < divisors.Count)
                {
                    for (int i = 0; i < divisors.Count; i++)
                    {
                        olddiv = divisors[i];
                        oldmult = multipliers[i];
                        for (int j = 2; j < maxdivisor; j += 2)
                        {
                            divisors[i] = j;
                            
                            if (j == olddiv) { multipliers[i] = oldmult - 1; }//lower it
                            else { multipliers[i] = -1; }//try neg
                            setSystems(divisors, multipliers, maxdivisor);
                            totals = new gtCalc[1];
                            calculateSystemsMatrix();
                            calculatetotalsaux_MostRecentPosition_double(0, 0, systemCount, ref systemsMatrix);
                            if (Math.Abs(totals[0].grandtotal) < Math.Abs(bestGT))
                            {
                                divisors[i] = olddiv;
                                multipliers[i] = oldmult;
                            }
                            else//it improved
                            {
                                innerImproved = true;
                                bestGT = totals[0].grandtotal;
                                olddiv = divisors[i];
                                oldmult = multipliers[i];
                            }
                            if (j == olddiv) { multipliers[i] = oldmult + 1; }
                            else { multipliers[i] = 1; }//try pos

                            setSystems(divisors, multipliers, maxdivisor);
                            totals = new gtCalc[1];
                            calculateSystemsMatrix();
                            calculatetotalsaux_MostRecentPosition_double(0, 0, systemCount, ref systemsMatrix);
                            if (Math.Abs(totals[0].grandtotal) < Math.Abs(bestGT))
                            {
                                divisors[i] = olddiv;
                                multipliers[i] = oldmult;
                            }
                            else//it improved
                            {
                                innerImproved = true;
                                bestGT = totals[0].grandtotal;
                                olddiv = divisors[i];
                                oldmult = multipliers[i];
                            }


                            if (!innerImproved)
                            {
                                //set your old divisors again
                                divisors[i] = olddiv;
                                multipliers[i] = oldmult;
                            }
                            else
                            {
                                outterImproved = true;
                            }
                            innerImproved = false;
                        }

                        if(bestGT == oldbestGT)
                        {//then you havent improved
                            loopsNotImproved++;

                            //call to mutate should go here
                            if (loopsNotImproved == divisors.Count)
                            {
                                loopsNotImproved = 0;

                                int rand = GetRandomNumber(0, 100);
                                if (divisors.Count< 20 && rand % 10 == 0)
                                {
                                    divisors.Add(GetRandomNumber(0,400));
                                    multipliers.Add(1);
                                }
                                else if (divisors.Count > 1)
                                {
                                    divisors.Remove(divisors.Count);
                                    multipliers.Remove(multipliers.Count);
                                }

                                divisors = mutate(divisors, maxdivisor);

                                bestGT = 0;

                            }
                        }
                        else
                        {
                            oldbestGT = bestGT;
                            setSystems(divisors, multipliers, maxdivisor);
                            totals = new gtCalc[1];
                            calculateSystemsMatrix();
                            calculatetotalsaux_MostRecentPosition_double(0, 0, systemCount, ref systemsMatrix);
                            outfile.WriteLine(sysString(divisors) + "," + sysString(multipliers) + "," + totals[0].grandtotal + "," + totals[0].tradecount + ",");
                            outfile.Flush();
                            Console.WriteLine(sysString(divisors) + "," + sysString(multipliers) + "," + totals[0].grandtotal + "," + totals[0].tradecount + ",");
                        }

                    }

                    if ((DateTime.UtcNow-start).Milliseconds > timeLimit)
                    {
                        outfile.Write("0");
                        outfile.Close();
                        MessageBox.Show("all dome with "+companyName);
                        return;
                    }
                }

                outfile.Write("0");
                outfile.Close();
            }catch(System.ArgumentOutOfRangeException a)
            {
                MessageBox.Show("Error Message: " + a.ToString(), "Error Title", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

            }
            catch (Exception e)
            {
                MessageBox.Show("Error Message: " + e.ToString(), "Error Title", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }


            DateTime end = DateTime.UtcNow;

            MessageBox.Show("All done. Took " + (end - start).TotalSeconds + " seconds", "Done", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

        }

        private static List<int> mutate(List<int> startlist, int maxdiv)
        {

            startlist[GetRandomNumber(0, startlist.Count - 1)] = randby2(startlist, maxdiv);
            startlist[GetRandomNumber(0, startlist.Count - 1)] = randby2(startlist, maxdiv);
            startlist[GetRandomNumber(0, startlist.Count - 1)] = randby2(startlist, maxdiv);
            startlist[GetRandomNumber(0, startlist.Count - 1)] = randby2(startlist, maxdiv);
            startlist[GetRandomNumber(0, startlist.Count - 1)] = randby2(startlist, maxdiv);
            startlist[GetRandomNumber(0, startlist.Count - 1)] = randby2(startlist, maxdiv);
            startlist[GetRandomNumber(0, startlist.Count - 1)] = randby2(startlist, maxdiv);
            startlist[GetRandomNumber(0, startlist.Count - 1)] = randby2(startlist, maxdiv);
            startlist[GetRandomNumber(0, startlist.Count - 1)] = randby2(startlist, maxdiv);
            startlist[GetRandomNumber(0, startlist.Count - 1)] = randby2(startlist, maxdiv);



            return startlist;
        }


        //Function to get random number
        private static readonly Random getrandom = new Random();
        private static readonly object syncLock = new object();
        public static int GetRandomNumber(int min, int max)
        {
            lock (syncLock)
            { // synchronize
                return getrandom.Next(min, max);
            }
        }

        public static int randby2(List<int> startlist, int maxdiv)
        {
            int rand = 1;
            while (rand %2 != 0)// make sure you choose an even number for a divisor
            {
                rand = GetRandomNumber(0,startlist.Max());
            }
            return rand;
        }

        public void calculatetotalsaux_MostRecentPosition_double(int totalsOffset, int matrixOffset, int depth, ref double[,] matrix)
        {
            int i, j, currentposition, lastposition;
            double trade;
            for (i = 0; i < depth; i++)
            {
                totals[i + totalsOffset].lastposition = 0;
                totals[i + totalsOffset].lastpositionchangeprice = data[0];

                for (j = 1; j < dataCount; j++)
                {
                    lastposition = totals[i + totalsOffset].lastposition;

                    //gives your position at the ith system
                    if (matrix[j, i + matrixOffset] > 0.0)
                    {
                        currentposition = 1;
                        totals[i + totalsOffset].currentposition = 1;
                    }
                    else if (matrix[j, i + matrixOffset] < 0.0)
                    {
                        currentposition = -1;
                        totals[i + totalsOffset].currentposition = -1;
                    }
                    else//matrix[j,i]==0
                    {
                        //the following if else else is what makes the systems computation different from the other computations,
                        //you MUST reverse position when you run into a 0 unless the last position was also 0
                        if (lastposition == -1)
                        {
                            totals[i + totalsOffset].currentposition = 1;
                            currentposition = 1;
                        }else if(lastposition == 1)
                        {
                            totals[i + totalsOffset].currentposition = -1;
                            currentposition = -1;
                        }
                        else
                        {
                            totals[i + totalsOffset].currentposition = 0;
                            currentposition = 0;
                        }
                    }

                    if (currentposition != lastposition)
                    {//then you need to make a trade possibly and set last position to current position
                        if (lastposition == 0)
                        {
                            trade = 0.0;
                        }
                        else if (lastposition == -1)
                        {
                            trade = totals[i + totalsOffset].lastpositionchangeprice - data[j];
                        }
                        else//last position was 1 (long)
                        {
                            trade = data[j] - totals[i + totalsOffset].lastpositionchangeprice;
                        }

                        if (trade < 0)
                        {
                            totals[i + totalsOffset].losscount++;
                            totals[i + totalsOffset].losstotal += trade;
                        }
                        else if (trade > 0)
                        {
                            totals[i + totalsOffset].wincount++;
                            totals[i + totalsOffset].wintotal += trade;
                        }

                        if (totals[i + totalsOffset].lastpositionchangeprice == data[j])
                        {
                            totals[i + totalsOffset].tiecount++;
                        }

                        if (trade < totals[i + totalsOffset].biggestloss)
                        {
                            totals[i + totalsOffset].biggestloss = trade;
                        }
                        else if (trade > totals[i + totalsOffset].biggestwin)
                        {
                            totals[i + totalsOffset].biggestwin = trade;
                        }

                        totals[i + totalsOffset].tradecount++;
                        totals[i + totalsOffset].grandtotal += trade;
                        totals[i + totalsOffset].lastpositionchangeprice = data[j];
                        totals[i + totalsOffset].lastposition = totals[i + totalsOffset].currentposition;
                    }
                }

                totals[i + totalsOffset].avgwin = totals[i + totalsOffset].wintotal / totals[i + totalsOffset].wincount;
                totals[i + totalsOffset].avgloss = totals[i + totalsOffset].losstotal / totals[i + totalsOffset].losscount;
            }
        }

        public void calculatetotalsaux_MostRecentPosition_int(int totalsOffset, int matrixOffset, int depth, ref int[,] matrix)
        {
            int i, j, currentposition, lastposition;
            double trade;
            for (i = 0; i < depth; i++)
            {
                totals[i + totalsOffset].lastposition = 0;
                totals[i + totalsOffset].lastpositionchangeprice = data[0];

                for (j = 1; j < dataCount; j++)
                {
                    lastposition = totals[i + totalsOffset].lastposition;

                    //gives your position at the ith system
                    if (matrix[j, i + matrixOffset] > 0)
                    {
                        currentposition = 1;
                        totals[i + totalsOffset].currentposition = 1;
                    }
                    else if (matrix[j, i + matrixOffset] < 0)
                    {
                        currentposition = -1;
                        totals[i + totalsOffset].currentposition = -1;
                    }
                    else//matrix[j,i]==0
                    {
                        //the following if else else is what makes the systems computation different from the other computations,
                        //you MUST reverse position when you run into a 0 unless the last position was also 0
                        if (lastposition == -1)
                        {
                            totals[i + totalsOffset].currentposition = 1;
                            currentposition = 1;
                        }
                        else if (lastposition == 1)
                        {
                            totals[i + totalsOffset].currentposition = -1;
                            currentposition = -1;
                        }
                        else
                        {
                            totals[i + totalsOffset].currentposition = 0;
                            currentposition = 0;
                        }
                    }

                    if (currentposition != lastposition)
                    {//then you need to make a trade possibly and set last position to current position
                        if (lastposition == 0)
                        {
                            trade = 0.0;
                        }
                        else if (lastposition == -1)
                        {
                            trade = totals[i + totalsOffset].lastpositionchangeprice - data[j];
                        }
                        else//last position was 1 (long)
                        {
                            trade = data[j] - totals[i + totalsOffset].lastpositionchangeprice;
                        }

                        if (trade < 0)
                        {
                            totals[i + totalsOffset].losscount++;
                            totals[i + totalsOffset].losstotal += trade;
                        }
                        else if (trade > 0)
                        {
                            totals[i + totalsOffset].wincount++;
                            totals[i + totalsOffset].wintotal += trade;
                        }

                        if (totals[i + totalsOffset].lastpositionchangeprice == data[j])
                        {
                            totals[i + totalsOffset].tiecount++;
                        }

                        if (trade < totals[i + totalsOffset].biggestloss)
                        {
                            totals[i + totalsOffset].biggestloss = trade;
                        }
                        else if (trade > totals[i + totalsOffset].biggestwin)
                        {
                            totals[i + totalsOffset].biggestwin = trade;
                        }

                        totals[i + totalsOffset].tradecount++;
                        totals[i + totalsOffset].grandtotal += trade;
                        totals[i + totalsOffset].lastpositionchangeprice = data[j];
                        totals[i + totalsOffset].lastposition = totals[i + totalsOffset].currentposition;
                    }
                }

                totals[i + totalsOffset].avgwin = totals[i + totalsOffset].wintotal / totals[i + totalsOffset].wincount;
                totals[i + totalsOffset].avgloss = totals[i + totalsOffset].losstotal / totals[i + totalsOffset].losscount;
            }
        }

        public void calculatetotalsaux_Flat_int(int totalsOffset, int matrixOffset, int depth, ref int[,] matrix)
        {
            int i, j, currentposition, lastposition;
            double trade;
            for (i = 0; i < depth; i++)
            {
                totals[i + totalsOffset].lastposition = 0;
                totals[i + totalsOffset].lastpositionchangeprice = data[0];

                for (j = 1; j < dataCount; j++)
                {
                    lastposition = totals[i + totalsOffset].lastposition;

                    //gives your position at the ith system
                    if (matrix[j, i + matrixOffset] > 0)
                    {
                        currentposition = 1;
                        totals[i + totalsOffset].currentposition = 1;
                    }
                    else if (matrix[j, i + matrixOffset] < 0)
                    {
                        currentposition = -1;
                        totals[i + totalsOffset].currentposition = -1;
                    }
                    else//matrix[j,i] == 0
                    {
                        currentposition = 0;
                        totals[i + totalsOffset].currentposition = 0;
                    }

                    if (currentposition != lastposition)
                    {//then you need to make a trade possibly and set last position to current position
                        if (lastposition == 0)
                        {
                            trade = 0.0;
                        }
                        else if (lastposition == -1)
                        {
                            trade = totals[i + totalsOffset].lastpositionchangeprice - data[j];
                        }
                        else//last position was 1 (long)
                        {
                            trade = data[j] - totals[i + totalsOffset].lastpositionchangeprice;
                        }

                        if (trade < 0)
                        {
                            totals[i + totalsOffset].losscount++;
                            totals[i + totalsOffset].losstotal += trade;
                        }
                        else if (trade > 0)
                        {
                            totals[i + totalsOffset].wincount++;
                            totals[i + totalsOffset].wintotal += trade;
                        }

                        if (totals[i + totalsOffset].lastpositionchangeprice == data[j])
                        {
                            totals[i + totalsOffset].tiecount++;
                        }

                        if (trade < totals[i + totalsOffset].biggestloss)
                        {
                            totals[i + totalsOffset].biggestloss = trade;
                        }
                        else if (trade > totals[i + totalsOffset].biggestwin)
                        {
                            totals[i + totalsOffset].biggestwin = trade;
                        }

                        totals[i + totalsOffset].tradecount++;
                        totals[i + totalsOffset].grandtotal += trade;
                        totals[i + totalsOffset].lastpositionchangeprice = data[j];
                        totals[i + totalsOffset].lastposition = totals[i + totalsOffset].currentposition;
                    }
                }

                totals[i + totalsOffset].avgwin = totals[i + totalsOffset].wintotal / totals[i + totalsOffset].wincount;
                totals[i + totalsOffset].avgloss = totals[i + totalsOffset].losstotal / totals[i + totalsOffset].losscount;
            }
        }

        public void calculatetotalsaux_Flat_double(int totalsOffset, int matrixOffset, int depth, ref double[,] matrix)
        {
            int i,j, currentposition, lastposition;
            double trade;
            for (i = 0; i < depth; i++)
            {
                totals[i + totalsOffset].lastposition = 0;
                totals[i + totalsOffset].lastpositionchangeprice = data[0];

                for (j = 1; j < dataCount; j++)
                {
                    lastposition = totals[i + totalsOffset].lastposition;

                    //gives your position at the ith system
                    if (matrix[j, i + matrixOffset] > 0.0)
                    {
                        currentposition = 1;
                        totals[i + totalsOffset].currentposition = 1;
                    }
                    else if (matrix[j, i + matrixOffset] < 0.0)
                    {
                        currentposition = -1;
                        totals[i + totalsOffset].currentposition = -1;
                    }
                    else//it is zero
                    {
                        currentposition = 0;
                        totals[i + totalsOffset].currentposition = 0;
                    }

                    if (currentposition != lastposition)
                    {//then you need to make a trade possibly and set last position to current position
                        if (lastposition == 0)
                        {
                            trade = 0.0;
                        }
                        else if (lastposition == -1)
                        {
                            trade = totals[i + totalsOffset].lastpositionchangeprice - data[j];
                        }
                        else//last position was 1 (long)
                        {
                            trade = data[j] - totals[i + totalsOffset].lastpositionchangeprice;
                        }

                        if (trade < 0)
                        {
                            totals[i + totalsOffset].losscount++;
                            totals[i + totalsOffset].losstotal += trade;
                        }
                        else if (trade > 0)
                        {
                            totals[i + totalsOffset].wincount++;
                            totals[i + totalsOffset].wintotal += trade;
                        }

                        if (totals[i + totalsOffset].lastpositionchangeprice == data[j])
                        {
                            totals[i + totalsOffset].tiecount++;
                        }

                        if (trade < totals[i + totalsOffset].biggestloss)
                        {
                            totals[i + totalsOffset].biggestloss = trade;
                        }
                        else if (trade > totals[i + totalsOffset].biggestwin)
                        {
                            totals[i + totalsOffset].biggestwin = trade;
                        }

                        totals[i + totalsOffset].tradecount++;
                        totals[i + totalsOffset].grandtotal += trade;
                        totals[i + totalsOffset].lastpositionchangeprice = data[j];
                        totals[i + totalsOffset].lastposition = totals[i + totalsOffset].currentposition;
                    }
                }

                totals[i + totalsOffset].avgwin = totals[i + totalsOffset].wintotal / totals[i + totalsOffset].wincount;
                totals[i + totalsOffset].avgloss = totals[i + totalsOffset].losstotal / totals[i + totalsOffset].losscount;
            }
        }

        public void calculateBaseMatrix()
        {
            calculateBaseMatrix_splitMovingAvg();
            //calculateBaseMatrix_normalMovingAvg();
        }


        public void calculateBaseMatrix_splitMovingAvg()
        {
            int i, j, k;
            int length = dataCount;    //so you don't have to recalculate the base matrix if you've already done so
            int width = divisorCount;

            if (!(length == lastbaseMatrixlength) && !(width == lastbaseMatrixwidth))
            {
                baseMatrix = new double[dataCount, divisorCount];

                double[] fore = new double[divisorCount];
                double[] aft = new double[divisorCount];

                if (useDataChangeNotData == false)
                {
                    for (i = 0; i < dataCount; i++)             //row
                    {
                        for (j = 0; j < divisorCount; j++)      //col
                        {
                            if (i > (j - 1) && (j % 2 == 0))
                            {
                                fore[j] = fore[j] - data[i - j] + data[i - ((int)Math.Ceiling(((double)j) / 2))];  //remove old forA add new forB
                                aft[j] = aft[j] + data[i] - data[i - ((int)Math.Floor(((double)j + 1) / 2))];         //add new aftB remove old aftA
                                baseMatrix[i, j] = aft[j] - fore[j];
                            }
                            else if (i == (j - 1) && (j % 2 == 0))
                            {
                                //then you can calculate fore and aft for this column
                                fore[j] = 0;
                                aft[j] = 0;
                                //loops
                                for (k = (i - ((int)Math.Floor((double)j / 2)) + 1); k < (i + 1); k++)
                                {
                                    if (j % 2 == 0)
                                    {
                                        fore[j] += data[k - ((int)Math.Ceiling(((double)j) / 2))];//even
                                    }
                                    else {
                                        fore[j] += data[k - ((int)Math.Ceiling(((double)j) / 2))];//odd
                                    }
                                    aft[j] += data[k];
                                }
                                baseMatrix[i, j] = aft[j] - fore[j];
                            }
                            else
                            {
                                baseMatrix[i, j] = 0;
                            }
                        }
                    }
                }
                else {
                    for (i = 0; i < dataCount; i++)             //row
                    {
                        for (j = 0; j < divisorCount; j++)      //col
                        {
                            if (i > (j - 1) && (j % 2 == 0))
                            {
                                fore[j] = fore[j] - dataChange[i - j] + dataChange[i - ((int)Math.Ceiling(((double)j) / 2))];  //remove old forA add new forB
                                aft[j] = aft[j] + dataChange[i] - dataChange[i - ((int)Math.Floor(((double)j + 1) / 2))];         //add new aftB remove old aftA
                                baseMatrix[i, j] = aft[j] - fore[j];
                            }
                            else if (i == (j - 1) && (j % 2 == 0))
                            {
                                //then you can calculate fore and aft for this column
                                fore[j] = 0;
                                aft[j] = 0;
                                //loops
                                for (k = (i - ((int)Math.Floor((double)j / 2)) + 1); k < (i + 1); k++)
                                {
                                    if (j % 2 == 0)
                                    {
                                        fore[j] += dataChange[k - ((int)Math.Ceiling(((double)j) / 2))];//even
                                    }
                                    else {
                                        fore[j] += dataChange[k - ((int)Math.Ceiling(((double)j) / 2))];//odd
                                    }
                                    aft[j] += dataChange[k];
                                }
                                baseMatrix[i, j] = aft[j] - fore[j];
                            }
                            else
                            {
                                baseMatrix[i, j] = 0;
                            }
                        }
                    }
                }
            }
            
            matrixToFile(@"C:\Users\Josh\Desktop\splitAvgBaseMatrix.csv", ref baseMatrix, dataCount, divisorCount);
        }

        public void calculateBaseMatrix_normalMovingAvg()
        {
            int i, j, k;
            int length = dataCount;    //so you don't have to recalculate the base matrix if you've already done so
            int width = divisorCount;

            if (!(length == lastbaseMatrixlength) && !(width == lastbaseMatrixwidth))
            {
                baseMatrix = new double[dataCount, divisorCount];
                double sum = 0;

                //first put the sum in it
                for (i = 0; i < dataCount; i++)             //row
                {
                    for (j = 1; j < divisorCount; j++)      //col
                    {
                        if(i > (j - 1))
                        {
                            baseMatrix[i, j] = baseMatrix[i - 1, j] + data[i] - data[i-j];  //memoization
                        }
                        else if (i == (j - 1))
                        {
                            for(k=0;k< j; k++)
                            {
                                sum = sum + data[i-k];
                            }

                            baseMatrix[i, j] = sum;      // / j;
                            sum = 0;
                        }
                        else
                        {
                            baseMatrix[i, j] = 0;
                        }
                    }
                }

                //then divide it by j
                for (i = 0; i < dataCount; i++)             //row
                {
                    for (j = 1; j < divisorCount; j++)      //col
                    {
                        baseMatrix[i, j] = baseMatrix[i, j] / j;
                    }
                }
            }

            matrixToFile(@"C:\Users\Josh\Desktop\normalAvgBaseMatrix.csv", ref baseMatrix, dataCount, divisorCount);
        }

        public void calculateSystemsMatrix()
        {
            int i, j, k;
            //calculate systemsMatrix here
            systemsMatrix = new double[dataCount, systemCount];
            int multiplier;
            for (i = 0; i < systemCount; i++)
            {            //each row in systems holds a system
                for (j = 0; j < divisorCount; j++)
                {          //each column is a divisor and each entry here is a multiplier to be applied to an entry from the baseMatrix
                    multiplier = systems[i, j];
                    //divisor is j
                    if (multiplier != 0)
                    {
                        for (k = 0; k < dataCount; k++)
                        {
                            try {
                                systemsMatrix[k, i] += multiplier * baseMatrix[k, j];
                            }
                            catch (System.IndexOutOfRangeException e)
                            {
                                MessageBox.Show("Systems Matrix is "+dataCount +" rows and "+systemCount+" columns \n"
                                    +"You are trying to access index "+ k + ","+i+" "
                                    +e.Message);
                            }
                        }
                    }
                }
            }

            matrixToFile(@"C:\Users\Josh\Desktop\systemsmatrix.csv", ref systemsMatrix, dataCount, systemCount);
        }

        public void calculateConditionMatrix()
        {
            int i,j,k;
            conditionMatrix = new int[dataCount, conditionCount*2];
            int[] bCountGreaterThan;
            int[] bCountLessThan;
            int h;
            int bFracSatisfyGreaterCount;
            int bFracSatisfyLessCount;
            try
            {
                for (i = 0; i < conditionCount; i++)//for each column in the conditionMatrix
                {
                    bCountGreaterThan = new int[setConditions[i].bCount];
                    bCountLessThan = new int[setConditions[i].bCount];
                    //MessageBox.Show("setConditions[0].aCount: "+ setConditions[0].aCount + " setConditions[0].bCount:"+ setConditions[0].bCount);
                    for (j = 0; j < dataCount; j++)
                    { //go through all the rows of data (and all the rows of the systemsMatrix) and see if the ith condition evals to 1,0,or -1
                      //if setConditions[i].aCount of the systems from setConditions[i].setA set are > than setConditions[i].bCount of the systems from setConditions[i].setB
                      //at row j then conditionmatrix[j][i] is 1, 
                      //if that many of them are < then it's -1, else 0
                      //so get the greaterthan count, and the lessthan count for each of the bCount systems 
                        bFracSatisfyGreaterCount = 0;
                        bFracSatisfyLessCount = 0;

                        for (k = 0; k < setConditions[i].bCount; k++)
                        {

                            //for each system in setConditions[i].setB see how many from setA are > and how many are < 
                            //setB specifies system numbers from the sets[][] matrix, each row is a set, entries in the row specifies a system from the systemsMatrix
                            //we're in the jth row
                            for (h = 0; h < setConditions[i].aCount; h++)
                            {
                                if (systemsMatrix[j, sets[setConditions[i].setA - 1, h] - 1] > systemsMatrix[j, sets[setConditions[i].setB - 1, k] - 1])
                                {
                                    bCountGreaterThan[k]++;
                                }
                                else if (systemsMatrix[j, sets[setConditions[i].setA - 1, h] - 1] < systemsMatrix[j, sets[setConditions[i].setB - 1, k] - 1])
                                {
                                    bCountLessThan[k]++;
                                }
                                //else it's the same exact value
                            }

                            if (bCountGreaterThan[k] >= setConditions[i].aFrac)
                            {
                                bFracSatisfyGreaterCount++;
                            }
                            if (bCountLessThan[k] >= setConditions[i].aFrac)
                            {
                                bFracSatisfyLessCount++;
                            }

                            bCountLessThan[k] = 0;
                            bCountGreaterThan[k] = 0;
                        }

                        //now you know for each system in setB, how many systems in setA are greater than it and less than it  
                        //you want at least aFrac systems from setA greater than at least bFrac systems from setB
                        if (bFracSatisfyGreaterCount >= setConditions[i].bFrac)
                        {
                            conditionMatrix[j, i] = 1;
                            conditionMatrix[j, i + conditionCount] = 1;
                        }
                        else if (bFracSatisfyLessCount >= setConditions[i].bFrac)
                        {
                            conditionMatrix[j, i] = -1;
                            conditionMatrix[j, i + conditionCount] = -1;
                        }
                        else {

                            //flat on tie is the right side of the conditionMatrix
                            conditionMatrix[j, i + conditionCount] = 0;

                            if (j > 0)
                            {
                                //most recent position on tie is the left side of the conditionMatrix
                                conditionMatrix[j, i] = conditionMatrix[j - 1, i];
                            }
                            else
                            {
                                conditionMatrix[j, i] = 0;
                            }

                        }
                    }
                }
            }
            catch (Exception)
            {

            }
            matrixToFile(@"C:\Users\Josh\Desktop\conditionMatrix.csv", ref conditionMatrix, dataCount, conditionCount*2);
        }

        public void calculateSetsMatrix()
        {
            setsMatrix = new double[dataCount, setCount*2];
            int i, j, k;
            int negcount, poscount, zerocount;

            for (i = 0; i< setCount; i++)//for each set
            {
                //look at each system in the systems matrix of that set
                for(k=0; k < dataCount; k++)//for every piece of data
                {
                    poscount = 0;
                    negcount = 0;
                    zerocount = 0;

                    for (j=0; j<setIndex[i];j++)//for every system in that set
                    {
                        if (systemsMatrix[k, sets[i, j]-1] > 0)
                        {
                            poscount++;
                        }else if(systemsMatrix[k, sets[i, j]-1] < 0)
                        {
                            negcount++;
                        }
                        else
                        {
                            if (k > 0)//we're at a zero so look back to decide
                            {
                                if(systemsMatrix[k-1, sets[i, j]-1] > 0) { negcount++; }else if(systemsMatrix[k-1, sets[i, j]-1] < 0) { poscount++; } else { zerocount++; }
                            }
                            else
                            {
                                zerocount++;
                            }
                        }

                        if(poscount == setIndex[i])
                        {
                            setsMatrix[k, i] = 1;
                            setsMatrix[k, i + setCount] = 1;
                        }else if(negcount == setIndex[i])
                        {
                            setsMatrix[k, i] = -1;
                            setsMatrix[k, i + setCount] = -1; 
                        }
                        else
                        {
                            //flat on "not all agree" on right side
                            setsMatrix[k, i + setCount] = 0;

                            //mrp on "not all agree" on left side
                            if (k > 0) {
                                setsMatrix[k, i] = setsMatrix[k - 1, i];
                            }
                            else
                            {
                                setsMatrix[k, i] = 0;
                            }

                        }
                    }
                }
            }


            //file it
            matrixToFile(@"C:\Users\Josh\Desktop\setsmatrix.csv", ref setsMatrix, dataCount, setCount*2);
        }

        public void calculateConditionsTogetherMatrix()
        {
            conditionsTogetherMatrix = new int[dataCount, 2];

            int i, j;
            int posCount, negCount, zeroCount;
            for (i = 0; i < dataCount; i++)
            {
                posCount = 0;
                negCount = 0;
                zeroCount = 0;
                for(j = 0; j<conditionCount; j++)
                {
                    if (conditionMatrix[i, j] > 0)
                    {
                        posCount++;
                    }
                    else if(conditionMatrix[i, j] < 0)//we're using the most recent position half (the left half) of the condition matrix so we know its either >0 or <0
                    {
                        negCount++;
                    }
                    else
                    {
                        zeroCount++;
                    }
                }

                if(negCount == conditionCount)
                {//then we're short
                    conditionsTogetherMatrix[i, 0] = -1;
                    conditionsTogetherMatrix[i, 1] = -1;
                }
                else if(posCount == conditionCount)
                {//then we're long
                    conditionsTogetherMatrix[i, 0] = 1;
                    conditionsTogetherMatrix[i, 1] = 1;
                }
                else if(zeroCount == conditionCount)
                {//then all the conditions are flat so we are too
                    conditionsTogetherMatrix[i, 0] = 0;
                    conditionsTogetherMatrix[i, 1] = 0;
                }
                else
                {
                    //mrp or flat
                    //flat
                    conditionsTogetherMatrix[i, 1] = 0;

                    if (i > 0)
                    {   //mrp
                        conditionsTogetherMatrix[i, 0] = conditionsTogetherMatrix[i-1,0];
                    }
                    else
                    {
                        conditionsTogetherMatrix[i, 0] = 0;
                    }
                }
            }

            matrixToFile(@"C:\Users\Josh\Desktop\conditionTogetherMatrix.csv", ref conditionsTogetherMatrix, dataCount, 2);
        }

        //this is a 'generic' method
        void matrixToFile<T>(string filename, ref T[,] matrix, int rows, int cols)
        {
            try {
                if (writeToFiles)
                {
                    if (rows > 0)
                    {
                        using (System.IO.StreamWriter file = new System.IO.StreamWriter(filename))
                        {
                            for (int i = 0; i < rows - 1; i++)
                            {
                                for (int j = 0; j < cols; j++)
                                {
                                    file.Write(matrix[i, j] + ",");
                                }
                                file.WriteLine();
                            }
                            for (int j = 0; j < cols - 1; j++)
                            {
                                file.Write(matrix[rows - 1, j] + ",");
                            }
                            if ((rows - 1) > -1 && (cols - 1) > -1)
                            {
                                file.Write(matrix[rows - 1, cols - 1]);
                                file.WriteLine();
                            }
                        }
                    }
                }
            }
            catch (IOException e)
            {
                //the file is probably open in excel you dummy!...
                MessageBox.Show(e.Message);
            }
        }


    }
}
