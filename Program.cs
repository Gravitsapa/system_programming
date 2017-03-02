using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Practika2
{
    enum words { BEGINL = 257, ENDL, IFL, DOL, WHILEL, RETURNL, CONSTL, INTL, THENL, READL, PRINTL, IDEN, NUMB };

    //"begin", "end", "if", "do", "while", "return", "const", "int", "then", "read", "print"

    class Program
    {
        StreamReader sr;
        StreamWriter sw;
        char nch = '\n';
        int lex;
        string[] TNM = new string[400]; //таблиця ідентифікаторів
        int ptn = 0;//лічильник кількості занесених в таблицю TNM ідентифікаторів
        int lval; //адреса ідентифікатора (його порядковий номер в таблиці TNM).


        public Program(string file1, string file2)
        {
            sr = new StreamReader(file1);
            sw = new StreamWriter(file2);
        }

        public char getc()
        {
            return (char)sr.Read();
        }

        public void get()
        {
            nch = getc();
            while (nch != '@')
            {
                if (nch.Equals(' ') || char.IsControl(nch))
                {
                }
                else if (char.IsLetter(nch))
                {
                    word();
                    continue;
                }
                else if (char.IsDigit(nch))
                {
                    number();
                }
                else if (nch == '(' || nch == ')' || nch == ';' || nch == '+' || nch == '-' || nch == '*' || nch == '/' || nch == '%' || nch == '=')
                {
                    sw.WriteLine("{0} is a special symbol", nch);
                }
                
                nch = getc();
            }
            return;
        }

        int add(string nm)
        {
            try
            {
                for(int i = 0; i < ptn; i++)
                {
                    if (TNM[i].Equals(nm))
                    {
                        return i;
                    }
                }
                TNM[ptn] = nm;
                ptn++;
                
            }
             catch (IndexOutOfRangeException)
            {
                Console.WriteLine("Переповнення таблиці TNM");
            }
            return ptn - 1;
        }

        void number()
        {
            lval = 0;
             while (char.IsDigit(nch)) {
                lval = lval * 10 + nch - '0';
                //lval =  
                nch = getc();
            }
            

            lex = (int)words.NUMB;
            Console.WriteLine("number lval={0} lex={1}", lval, lex);
            sw.WriteLine("number lval={0} lex={1}", lval, lex);
            lval = 0;

        }

        public void word()
        {
            string tx = " ";
            string[] serv = { "begin", "end", "if", "do", "while", "return", "const", "int", "then", "read", "print" };
            int[] cdl = { (int)words.BEGINL, (int)words.ENDL, (int)words.IFL, (int)words.DOL, (int)words.WHILEL, (int)words.RETURNL, (int)words.CONSTL, (int)words.INTL, (int)words.THENL, (int)words.READL, (int)words.PRINTL };
            while (char.IsLetterOrDigit(nch))
            {
                tx += nch;
                nch = getc();
            }
            tx += " ";
            string txx = tx.Trim();

            
            for (int j = 0; j < serv.Length; j++)
            {
                if (serv[j] == txx)
                {
                    lex = cdl[j];
                    Console.WriteLine("{0} lex={1}", serv[j], lex);
                    sw.WriteLine("{0} lex={1}", serv[j], lex);
                    return;
                }
            }
            lex = (int)words.IDEN;
            lval = add(tx);
            sw.WriteLine("{0} is an identifier lex={1} lval={2}", tx, lex, lval);
            return;
        }

        static void Main(string[] args)
        {
            string name1 = "C:\\Users\\Andrey\\Documents\\sp\\f1.txt";
            string name2 = "C:\\Users\\Andrey\\Documents\\sp\\f2.txt";

            Program ob = new Program(name1, name2);
            ob.get();

            ob.sw.Close();
        }
    }
}
