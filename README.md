# cSharpMatrices
Author: Josh Stowell, joshuastowell25@gmail.com
I learned c# and wrote this program within a month, December 2015.

This README was created 1/9/2017.

A little background info for the unfamiliar:
This program was written for persons who use moving averages to play the stock market.
A moving average takes multiple stock market datum and averages them
For example a 20 day moving average on the S&P 500 for today is simple the average 
of the last 20 closing (end of day) prices of the S&P.

(An alternative to moving averages are split moving averages where you take the second half of 
a period and subtract the first half. A 20 day split moving average is the average of the 10
most recent days minus the average of the 10 days prior.) 

Now some people (I am not one of these people) believe that you can use moving averages
to gain some insight into where a stock or a stock index is heading in the next few minutes,
days, or years. This program was written to prove that moving averages cannot be used to do
such things.

If one wants to use a moving average "system" to play the stock market they will sum the moving
averages they think are the best and then on any specific day their position in that stock
will be dictated by the sum of the moving averages. For instance; if Bob thinks the 20 day and 
40 day moving average are two moving averages that work well with each other Bob will calculate
each of those averages for the last 20 and 40 days respectively and sum them up, their sum, if 
positive means Bob should own that stock today, if their sum is negative Bob should get rid of
that stock today by selling it. In fact, Bob would want to "short" that stock, see below. 

sidetrack: many people are not aware you can actually "sell short" a stock. This is like owning
the stock in reverse, i.e. you make money when the stock falls in price.

there are 3 positions you can take on a stock: long (you own it wanting it to go up),
flat (you do not own the stock, nor are you shorting it, you have to interests in it at all),
short (you are technically borrowing the stock from someone who owns it so you make money when
it goes down)

Now what can be done with moving average "systems" that involve not just one or two moving
averages but many? Bob may find that if he uses the 100 day, 50 day, and 20 day moving averages
and pits them against the 120 day moving average that he could have made a lot of money over
the last 10 years. So Bob adds the results of the 100, 50, and 20 day moving averages and then 
subtracts the 120 day moving average, MA for short. Shorthand for this "system" look like the 
following: +1x100 +1x50 +1x20 -1x120
The +1's and -1's are the "multipliers" for each MA.
Bob may find that the system made even more money on his past data if he changes the multiplier
on 120 to -2 instead of -1. 

There are an infinite number of combinations of MA's you can combine to create a system so
there is no way a computer can find the "best" one (the one that makes the most money). So in
order to get near a "best" we must limit the numbers (MAs) that can constitute a system as 
well as limiting the multipliers on each MA. Pretend we can only have the 10, 20, and 30 day
MAs at our disposal and we limit MA multipliers to be -1, 0 (effectively not using it), or 1
now we have 3 columns, column 1 represents the MA 10, 2 represents MA 20, and 3 represents 30
lets list the possible systems: 
-1,-1,-1
-1,-1, 0
-1,-1, 1
-1, 0,-1
-1, 0, 0
-1, 0, 1
-1, 1,-1
-1, 1, 0
-1, 1, 1

 0,-1,-1
 0,-1, 0
 0,-1, 1
 0, 0,-1
 0, 0, 0
 0, 0, 1
 0, 1,-1
 0, 1, 0
 0, 1, 1

 1,-1,-1
 1,-1, 0
 1,-1, 1
 1, 0,-1
 1, 0, 0
 1, 0, 1
 1, 1,-1
 1, 1, 0
 1, 1, 1
 
 There are 3^3=27 possible systems, so it is easy to choose a "best" system.
 what if we allowed MA multipliers of -2,-1,0,1,2? Then there would be 5^3 = 125 possible
 systems.
 My father who inspired me to write this program wanted to have 400 possible MAs
 3^400 is about 7x10^190, even if your computer could check a million systems per second it
 would take many times longer than the amount of time the universe has existed to find the 
 "best" system. This is with only the multipliers -1,0, and 1.
 
 As the program is set up currently the user specifies the name of the data file the program
 should use. The file location is hardcoded to C:\datafiles and will look for .txt or .dat 
 files. For instance if you have a file called sp500.dat simply enter sp500 and the program 
 will look for sp500.txt first and sp500.dat if no .txt is found. This is hardcoded to make
 it easier for less experienced users.
 
 The user then enters the idea they have for a system, as shown in the example box in the UI
 they enter the MA then a space then the multiplier followed by a space if they want to enter
 another. When the user clicks the solve all tab, they can see how much money the system made
 and how many trades it made and other relevant statistics. 
 
 The user can enter multiple systems, and group systems into sets of systems if they choose
 sets are used in conjunction with the "conditions & results" tab where the user can see how
 much money could have been made by playing systems only when they coincide with others as
 a confirmation. Meaning some number of systems in the set must agree in order to go long or
 short, and you take a flat position when they are in disagreement. 
 
 There is a much more complex functionality built into the program that the average user wont
 see. A separate program or a person could generate a file and place it at D:\systemsALL.csv
 where system columns are written similar to the format shown in the example earlier, when 
 the Form1.cs file's SolveAllTab_Enter method has the mainbaseMatrix.gothroughAll() call 
 enabled the program will work on analyzing all the systems in the D:\systemsAll.csv file and
 place the results in D:\systemsALLOUT.csv this enables a person to check millions of systems 
 per hour (3.4GHz processor, 64GB RAM) This will allow the user to find an "approximate best"
 
 If the "approximate best" really were viable then it should work reasonably well on the same
 data placed in reverse order, but if you run the program on your data placed in forwards 
 order then in backwards order you will find that the best system in the forwards order is 
 only mediocre with data in the backwards order thus meaning there is no reliability to the
 "system" and if it did work well with some future data it would only be a lucky coincidence.
 
 This is why the program itself works as a proof against moving average theory.
 
 
