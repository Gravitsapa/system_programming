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
        private int nst = 0;

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
            if (nch == '@')
            {
                lex = '@';
                return;
            }
            // while (nch != '@')
            //{
            while (nch.Equals(' ') || char.IsControl(nch))
            {
                if (nch == '\n')
                    nst++;
                nch = getc();

            }

            if (char.IsLetter(nch))
            {
                word();
            }
            else if (char.IsDigit(nch))
            {
                number();
            }
            else if (nch == '(' || nch == ')' || nch == ',' || nch == ';' || nch == '+' || nch == '-' || nch == '*' || nch == '/' || nch == '%' || nch == '=')
            {
                Console.WriteLine("{0} is a special symbol", nch);
                sw.WriteLine("{0} is a special symbol", nch);
                lex = nch;
                nch = getc();
                
            }
            else
            {
                nch = '@';
                lex = '@';
            }

            ////nch = getc();
            //  }
            return;
        }

        int add(string nm)
        {
            try
            {
                for (int i = 0; i < ptn; i++)
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
            while (char.IsDigit(nch))
            {
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
            Console.WriteLine("{0} is an identifier lex={1} lval={2}", tx, lex, lval);
            sw.WriteLine("{0} is an identifier lex={1} lval={2}", tx, lex, lval);
            return;
        }

        public void exam(int lx)
        {
            if (lex != lx)
            {
                Console.WriteLine("Не співпадають лексеми lex={0}  та lx={1} в рядку nst={2}", lex, lx, nst);
            }
            get();
            return;
        }

        void prog()
        {
            while (lex != '@')
            {
                switch (lex)
                {
                    case (int)words.IDEN: dfunc(); break;
                    case (int)words.INTL: dvarb(); break;
                    case (int)words.CONSTL: dconst(); break;
                    default:
                        Console.WriteLine("Ошибка синтаксиса в строке {0}. Лексема lex={1}", nst, lex);
                        lex = '@';
                        break;
                }
            }
            return;
        }

        void dconst()
        {
            do
            {
                get();
                cons();
            } while (lex == ',');
            exam(';');
            return;
        }

        void cons()
        {
            exam((int)words.IDEN);
            exam('=');
            if (lex == '+' || lex == '-')
                get();
            exam((int)words.NUMB);
            return;
        }

        void dvarb()
        {
            do
            {
                get();
                exam((int)words.IDEN);
            } while (lex == ',');
            exam(';');
            return;
        }

        void dfunc()
        {
            get();
            param();
            body();
            return;
        }

        void param()
        {
            exam('(');
            if (lex != ')')
            {
                exam((int)words.IDEN);
                while (lex == ',')
                {
                    get();
                    exam((int)words.IDEN);
                }
            }
            exam(')');
            return;
        }

        void body()
        {
            exam((int)words.BEGINL);
            while (lex == (int)words.INTL || lex == (int)words.CONSTL)
                if (lex == (int)words.INTL) dvarb();
                else
                    dconst();
            stml();
            exam((int)words.ENDL);
            return;
        }

        void stml()
        {
            stat();
            while (lex == ';')
            {
                get();
                stat();
            }
            return;
        }

        void stat()
        {
            switch (lex)
            {
                case (int)words.IDEN: get(); exam('='); expr(); break;
                case (int)words.READL: get(); exam((int)words.IDEN); break;
                case (int)words.PRINTL: get(); expr(); break;
                case (int)words.RETURNL: get(); expr(); break;
                case (int)words.IFL: get(); expr(); exam((int)words.THENL); stml(); exam((int)words.ENDL); break;
                case (int)words.WHILEL: get(); expr(); exam((int)words.DOL); stml(); exam((int)words.ENDL); break;
                default:
                    Console.WriteLine("“stat {0} \n", nst);
                    break;
            }
            return;
        }

        void expr()
        {
            if (lex == '+' || lex == '-')
                get();
            term();
            while (lex == '+' || lex == '-')
            {
                get();
                term();
            }
            return;
        }

        public void term()

        {
            fact();
            while (lex == '*' || lex == '/' || lex == '%')
            {
                get();
                fact();
            }
            return;
        }

        public void fact()
        {
            switch (lex)
            {
                case '(':
                    get();
                    expr();
                    exam(')');
                    break;
                case (int)words.IDEN:
                    get();
                    if (lex == '(')
                    {
                        get();
                        if (lex != ')')
                            fctl();
                        exam(')');
                    }
                    break;
                case (int)words.NUMB: get(); break;
                default:
                    Console.WriteLine("Error in fact nst={0}", nst);
                    break;
            }
            return;
        }

        public void fctl()
        {
            expr();
            while (lex == ',')
            {
                get();
                expr();
            }
            return;
        }


        static void Main(string[] args)
        {
            string name1 = "C:\\Users\\Andrey\\Documents\\sp\\f1.txt";
            string name2 = "C:\\Users\\Andrey\\Documents\\sp\\f2.txt";

            Program ob = new Program(name1, name2);
            ob.get();
            ob.prog();

            ob.sw.Close();
        }
    }
}
