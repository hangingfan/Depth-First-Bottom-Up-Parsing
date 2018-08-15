using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// temporarily just applicable for 9th rules in csv,
///  if max than 9 rules, the algorithm should be optimized
/// </summary>
namespace Depth_First_Bottom_Up_Parsing
{
    //first shift: when find the matched rule, add the rule index to prediction and go on
    //             when reach the input end, unshift
    //unshift:     when meet the index, delete then index, replace, record the replace rule and shift
    //             when meet the non-Terminal, unreduce
    //unreduce:    after unreduce, unshift
    class Program
    {
        private static int csvMax = 5;
        private static char StartSymbol = 'S';
        private static int intToChar = 48;
        private static char illegalChar = '0';

        private static List<KeyValuePair<char, string>> getFileContent(string name)
        {
            var csvReader = new StreamReader(name);
            int index = 0; //the first line is for description ,not real data, skip it
            var content = new List<KeyValuePair<char, string>>();
            while (!csvReader.EndOfStream)
            {
                var line = csvReader.ReadLine();

                index++;
                if (index == 1)
                {
                    continue;
                }

                if (line == null)
                {
                    break;
                }

                var values = line.Split(',');
                if (values[0] != "" && values[0] != " ")
                {
                    content.Add(new KeyValuePair<char, string>(values[0][0], values[1]));
                }
            }

            return content;
        }

        static int canBePlacedByNonTerminal(string StackString)
        {
            for (int i = 0; i < content.Count; ++i)
            {
                if (content[i].Value == StackString)
                {
                    return i;
                }
            }
            return -1;
        }

        private static List<KeyValuePair<char, string>> content;

        private static Stack<char> restInputStack = new Stack<char>();
        private static Stack<char> currentPredictionStack = new Stack<char>();

        private static Stack<KeyValuePair<char, string>> reduceStack = new Stack<KeyValuePair<char, string>>(); //store the reducing character for unreducing

        private static string originalInputString = "";

        static void Main(string[] args)
        {
            content = getFileContent("rules.csv");
            if (content.Count == 0)
            {
                return;
            }

            restInputStack.Clear();
            Console.WriteLine("input your input string:");
            var UserInput = Console.ReadLine();
            if (UserInput.Length == 0)
            {
                return;
            }
            for (int i = UserInput.Length - 1; i > -1; i--)
            {
                restInputStack.Push(UserInput[i]);
            }

            originalInputString = getReplaceRules(restInputStack);

            judgeAndshifting();

            Console.ReadLine();
        }

        static void judgeAndshifting()
        {
            while (restInputStack.Count > 0)
            {
                char nowChar = restInputStack.Pop();
                currentPredictionStack.Push(nowChar);

                //traverse all the string from the top, for example,  a  aa aaa aaaa aaaab
                Stack<char> tempStack = new Stack<char>(currentPredictionStack.Reverse());
                string content = "";
                while (tempStack.Count > 0)
                {
                    if (tempStack.Peek() > -1 && tempStack.Peek() < 10) //integer
                    {
                        tempStack.Pop();
                        continue;
                    }
                    content = tempStack.Pop() + content;
                    int ret = canBePlacedByNonTerminal(content);
                    if (ret != -1)  //only for the max 9 rules
                    {
                        //store the available choice and go on shifting
                        currentPredictionStack.Push(Convert.ToChar(ret));
                    }
                }
                showCurrent("shifting  <-----:");
            }

            unShifting();

            if (-1 != canBePlacedByNonTerminal(getReplaceRulesWithoutInt(currentPredictionStack)))
            {
                Console.WriteLine("correct: " + getReplaceRulesReverse(currentPredictionStack));
            }
        }

        //traverse back, then replace, delete the subscript and store the replace character
        private static void unShifting()
        {
            //when the originalInputString is the same with the restInputStack,the traverse has finished
            if (originalInputString == getReplaceRules(restInputStack))
            {
                Console.WriteLine("finish:----------");
                return;
            }

            if (currentPredictionStack.Count > 0)
            {
                showCurrent("unshifting------->");
                if (currentPredictionStack.Peek() > -1 && currentPredictionStack.Peek() < 10)
                {
                    int index = currentPredictionStack.Pop();  //delete the subscript
                    showCurrent("delete the subscript:");
                    var reduceRule = content[index];
                    int reduceCount = reduceRule.Value.Length;

                    string replace = "";
                    while (reduceCount > 0)
                    {
                        if (currentPredictionStack.Peek() > -1 && currentPredictionStack.Peek() < 10)
                        {
                            replace = currentPredictionStack.Pop() + replace;
                            continue;
                        }
                        --reduceCount;
                        replace = currentPredictionStack.Pop() + replace;
                    }
                    //store the replace character
                    reduceStack.Push(new KeyValuePair<char, string>(reduceRule.Key, replace));
                    //replace
                    currentPredictionStack.Push(reduceRule.Key);
                    showCurrent("replace:");

                    judgeAndshifting();
                }
                //meet the non-terminal, unreduce
                else if (reduceStack.Count > 0 && reduceStack.Peek().Key == currentPredictionStack.Peek())
                {
                    unReducing();
                    unShifting();
                }
                else
                {
                    //move on unshifting
                    restInputStack.Push(currentPredictionStack.Pop());
                    unShifting();
                }
            }
        }

        private static void unReducing()
        {
            currentPredictionStack.Pop();
            string unreduceString = reduceStack.Pop().Value;
            for (int i = 0; i < unreduceString.Length; i++)
            {
                currentPredictionStack.Push(unreduceString[i]);
            }
            showCurrent("after unreducing<--->");
        }

        private static void showCurrent(string title)
        {
            Console.WriteLine(title);
            Console.WriteLine("{0,15}   {1,-15}",
                getReplaceRulesReverse(currentPredictionStack), getReplaceRules(restInputStack));
            if (reduceStack.Count > 0)
            {
                Console.WriteLine("                        stored replacing character:");
                var tempStack = new Stack<KeyValuePair<char, string>>(reduceStack.Reverse());
                string content = "";
                while (tempStack.Count != 0)
                {
                    var pair = tempStack.Pop();
                    Console.WriteLine("{0,30} ----> {1,-30}", pair.Key, getOutputStringForConsole(pair.Value));
                }
            }
        }

        private static string getReplaceRulesWithoutInt(Stack<char> currentStack)
        {
            Stack<char> tempStack = new Stack<char>(currentStack.Reverse());
            string content = "";
            while (tempStack.Count != 0)
            {
                int pop = tempStack.Pop();
                if (pop >= 9) //integer
                {
                    content = pop + content;
                }
            }
            return content;
        }

        private static string getReplaceRulesReverse(Stack<char> currentStack)
        {
            Stack<char> tempStack = new Stack<char>(currentStack.Reverse());
            string content = "";
            while (tempStack.Count != 0)
            {
                int pop = tempStack.Pop();
                if (pop < 10 && pop > -1)//  0-9(int) change to 0-9(unicode)
                {
                    content = intToUnicodeCharShow(tempStack.Pop()) + intToUnicodeCharShow(pop) + content;
                }
                else
                {
                    content = intToUnicodeCharShow(pop) + content;
                }
            }
            return content;
        }

        private static string getReplaceRules(Stack<char> currentStack)
        {
            Stack<char> tempStack = new Stack<char>(currentStack);
            string content = "";
            while (tempStack.Count != 0)
            {
                int pop = tempStack.Pop();
                if (pop < 10 && pop > -1)//  0-9(int) change to 0-9(unicode)
                {
                    content = intToUnicodeCharShow(tempStack.Pop()) + intToUnicodeCharShow(pop) + content;
                }
                else
                {
                    content = intToUnicodeCharShow(pop) + content;
                }
            }
            return content;
        }

        private static string intToUnicodeCharShow(int pop)
        {
            if (pop < 10 && pop > -1)//  0-9(int) change to 0-9(unicode)
            {
                return Convert.ToChar(pop + 1 + intToChar).ToString();
            }
            else
            {
                return Convert.ToChar(pop).ToString();
            }
        }

        private static string getOutputStringForConsole(string content)
        {
            string special = "";
            for (int i = 0; i < content.Length; i++)
            {
                special += intToUnicodeCharShow(content[i]);
            }
            return special;
        }
    }
}
