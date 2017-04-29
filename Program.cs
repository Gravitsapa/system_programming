using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Practika2
{
    enum words { BEGINL = 257, ENDL, IFL, DOL, WHILEL, RETURNL, CONSTL, INTL, THENL, READL, PRINTL, IDEN, NUMB };
    enum commands { OPR, LIT, LDE, LDI, STE, STI, CAL, INI, JMP, JMC };
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

        struct odc
        {
            public string name;
            public int what;
            public int val;
        }

        struct fnd
        {
            public string name; //імя функції
            public int isd;     //описана (isd=1) чи ні (isd=0)
            public int cpt;     //кількість параметрів
            public int start;   // точка входу в таблиі команд
        }

        fnd[] TFN = new fnd[10];
        int ptf = 0;

        struct cmd
        {
            public int cod;    //код (що робити)
            public int opd;   //операнд
        }

        cmd[] TCD = new cmd[100];
        int tc = 0;



        int[] st = new int[500];
        int cgv = 0;
        int clv = 0;
        odc[] TOB = new odc[30];
        int pto = 0;
        int ptol = 0;
        int ut = 1;

        int adrnm;  //- точка входу в таблиці команд для функції main();
        int cpnm; //- кількість параметрів функції main();
        int t = 0; //-  номер вільного елемента у стеку або ще лічильник введених чисел у стек;


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

        public void prog()
        { //check if lex is prog PROG -> (DCONST|DVARB|DFUNK) * eof
            try
            {
                while (sr.Peek() >= 0)
                {
                    switch (lex)
                    {
                        case (int)words.IDEN: dfunc(); break;
                        case (int)words.INTL: dvarb(); break;
                        case (int)words.CONSTL: dconst(); break;
                        default:
                            Console.WriteLine("Помилкова лексема lex={0} в prog() nst={1}", lex, nst);
                            break;
                    }
                }

                int p = fmain();
                adrnm = TFN[p].start;
                cpnm = TFN[p].cpt;

                return;

            }
            catch (IndexOutOfRangeException)
            {
                Console.WriteLine("Вихід за межі текста програми на SPL");
            }
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

        void cons() { 
            string nm = TNM[lval];
            int s;
        
            exam((int)words.IDEN);
            exam('=');
            s = (lex == '-') ? -1 : 1;
            if (lex == '+' || lex == '-')
                get();
            newob(nm, 1, s * lval);//Занесено об'єкт
            exam((int)words.NUMB);
            return;
        }

        public void dvarb()
        {
            do
            {
                get();
                newob(TNM[lval], (ut == 1 ? 2 : 3), (ut == 1 ? cgv++ : ++clv));
                exam((int)words.IDEN);
            } while (lex == ',');
            exam(';');
            return;
        }

        public void dfunc()
        { //parsing and creating new SLP-funct, check DFUNC -> iden PARAM BODY
            int cp, st;
            string nm = TNM[lval];
            get();
            ut = 0;
            cp = param();
            st = body();
            ut = 1;
            pto = ptol;
            defin(nm, cp, st);
            return;
        }

        public int param()
        {
            int p, cp = 0;
            exam('(');
            if (lex != ')')
            {
                newob(TNM[lval], 3, ++cp);
                exam((int)words.IDEN);
                while (lex == ',')
                {
                    get();
                    newob(TNM[lval], 3, ++cp);

                    exam((int)words.IDEN);
                }
            }
            exam(')');
            for (p = ptol; p < pto; p++)
                TOB[p].val -= cp + 3;
            return cp;
        }

        int body()
        {
            int st;
            exam((int)words.BEGINL);
            clv = 0;
            while (lex == (int)words.INTL || lex == (int)words.CONSTL)
                if (lex == (int)words.INTL) dvarb();
                else
                    dconst();
            st = gen((int)commands.INI, clv);
            stml();
            exam((int)words.ENDL);
            gen((int)commands.OPR, 10);
            return st;
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

        public void stat()
        {
            int p;
            int t1, t2;

            switch (lex)
            {
                case (int)words.IDEN:
                    p = findob(TNM[lval]); get(); exam('='); expr();
                    if (TOB[p].what == 1)
                        Console.WriteLine("{0} константа. Присвоєння неможливе", TNM[lval]);
                    gen(TOB[p].what == 2 ? (int)commands.STE : (int)commands.STI, TOB[p].val);
                    break;

                case (int)words.READL:
                    get();
                    p = findob(TNM[lval]); gen((int)commands.OPR, 1); if (TOB[p].what == 1)
                        Console.WriteLine("{0} константа. Присвоєння неможливе", TNM[lval]);
                    gen(TOB[p].what == 2 ? (int)commands.STE : (int)commands.STI, TOB[p].val); exam((int)words.IDEN);
                    break;

                case (int)words.PRINTL: get(); expr(); gen((int)commands.OPR, 2); break;

                case (int)words.RETURNL: get(); expr(); gen((int)commands.OPR, 9); break;

                case (int)words.IFL:
                    get(); expr(); exam((int)words.THENL);
                    t1 = gen((int)commands.JMC, 0); stml(); exam((int)words.ENDL); TCD[t1].opd = tc; break;

                case (int)words.WHILEL:
                    get(); t1 = tc; expr(); exam((int)words.DOL);
                    t2 = gen((int)commands.JMC, 0); stml(); gen((int)commands.JMP, t1); TCD[t2].opd = tc;
                    exam((int)words.ENDL); break;

                default:
                    Console.WriteLine("Error in stat nst={0} lex={1}", nst, lex);
                    break;
            }
        }

        public void expr()
        {

            int neg = 0;
            if (lex == '-')
            {
                neg = 1;
            }
            if (lex == '+' || lex == '-')
            {
                get();
            }
            term();

            if (neg == 1)
            {
                gen((int)commands.OPR, 8);
            }

            while (lex == '+' || lex == '-')
            {
                neg = lex == '-' ? 4 : 3;
                get();
                term();
                gen((int)commands.OPR, neg);
            }

            return;
        }

        public void term()
        {
            int op;
            fact();

            while (lex == '*' || lex == '/' || lex == '%')
            {
                op = lex == '*' ? 5 : lex == '/' ? 6 : 7;
                get();
                fact();
                gen((int)commands.OPR, op);
            }
            return;
        }

        public void fact()
        {
            string nm;
            int cp, p, p1;
            switch (lex)
            {
                case '(': get(); expr(); exam(')'); break;
                case (int)words.IDEN:
                    nm = TNM[lval]; get();

                    if (lex == '(')
                    {
                        get();
                        cp = (lex == ')') ? 0 : fctl();
                        exam(')');
                        p1 = eval(nm, cp);
                        gen((int)commands.LIT, cp);
                        cp = gen((int)commands.CAL, TFN[p1].start);

                        if (TFN[p1].isd != 1)
                        {
                            TFN[p1].start = cp;
                        }
                    }
                    else
                    {
                        p = findob(nm);
                        gen(TOB[p].what == 1 ? (int)commands.LIT : TOB[p].what == 2 ? (int)commands.LDE : (int)commands.LDI, TOB[p].val);
                    }
                    break;

                case (int)words.NUMB: gen((int)commands.LIT, lval); get(); break;

                default:
                    Console.WriteLine("Error in fact nst={0} lex={1}", nst, lex);
                    break;
            }
            return;
        }

        public int fctl()
        {
            int cf = 1;
            expr();
            while (lex == ',')
            {
                get();
                expr();
                cf++;
            }
            return cf;
        }

        public void newob(string nm, int wt, int vl)
        {
            int pe, p;
            pe = ut == 1 ? pto : ptol;
            for (p = pto - 1; p >= pe; p--)
            {
                if (nm == TOB[p].name)
                    Console.WriteLine("{0} описана двічі", nm);
            }
            if (pto > 30)
                Console.WriteLine("Переповнення таблиці об'єктів  TOB");
            TOB[pto].name = nm;
            TOB[pto].what = wt;
            TOB[pto].val = vl;
            pto++;
        }

        public int findob(string nm)
        {
            int p;
            Console.WriteLine("nm={0}", nm);
            for (p = pto - 1; p >= 0; p--)
            {
                Console.WriteLine("TOB[{0}].name={1}", p, TOB[p].name);
                if (TOB[p].name == nm)
                    return p;
            }
            Console.WriteLine("{0} не описана");
            return 0;
        }

        public void defin(string nm, int cp, int ad)
        { //descr SPL-function
            int p, c1, c2;
            p = findfn(nm);

            if (p != 0)
            {
                if (TFN[p].isd == 1)
                {
                    Console.WriteLine("{0} описана двічі", nm);
                }

                if (TFN[p].cpt != cp)
                {
                    Console.WriteLine("Не сходиться кількість параметрів для {0}", nm);
                }

                TFN[p].isd = 1;
                
                TFN[p].start = ad;
            }
            else
            {
                newfn(nm, 1, cp, ad);
            }
            return;
        }

        public int newfn(string nm, int df, int cp, int ps)
        { //add SPL-funct to table
            if (ptf > 29)
            {
                Console.WriteLine(" Переповнення таблиці функцій TFN");
            }

            TFN[ptf].name = nm;
            TFN[ptf].isd = df;
            TFN[ptf].start = ps;
            TFN[ptf].cpt = cp;

            return ptf++;
        }

        public int findfn(string nm)
        { //search SPL-funct in the table
            for (int p = ptf - 1; p >= 0; p--)
            {
                if (TFN[p].name == nm)
                {
                    return p;
                }
            }
            return 0;
        }

        int eval(string nm, int cp)
        {
            int p = findfn(nm);
            if (p == 1)
            {
                return newfn(nm, 0, cp, -1);
            }

            if (TFN[p].cpt == cp)
            {
                return p;
            }

            Console.WriteLine("Кількість параметрів для {0}не співпадає", nm);
            return 0;
        }

        public int gen(int co, int op)
        { //add comnd to table
            TCD[tc].cod = co;
            TCD[tc].opd = op;

            return tc++;
        }

        public int fmain()
        { //find main() and not described funct
            string nm = " main ";
            int pm = -1;

            for (int p = ptf - 1; p >= 0; p--)
            {
                if (TFN[p].isd != 1)
                {
                    Console.WriteLine("Функція {0} не описана", nm);
                }

                if (nm == TFN[p].name)
                {
                    pm = p;
                }
            }

            if (pm != -1)
            {
                return pm;
            }
            else
            {
                Console.WriteLine(" Відсутня функція main");
            }

            return 0;
        }

        public void push(int a)
        { //integer to stack
            if (t > 499)
                Console.WriteLine(" Переповнення стека st ");
            st[++t] = a;
            return;
        }

        public int read()
        { //integer from keyboard
            int v;
            string s;
            Console.WriteLine("Enter a number");
            s = Console.ReadLine();
            v = Convert.ToInt32(s);
            Console.WriteLine("v={0}", v);
            return v;
        }

        public void printObjInFile()
        {

            for (int j = 0; j < pto; j++)
            {
                sw.WriteLine("TOB[{0}] name ={1} what={2} val={3}", j, TOB[j].name, TOB[j].what, TOB[j].val);
                Console.WriteLine("TOB[{0}] name ={1} what={2} val={3}", j, TOB[j].name, TOB[j].what, TOB[j].val);
            }

        }


        static void Main(string[] args)
        {
            string name1 = "C:\\Users\\Andrey\\Documents\\sp\\f1.txt";
            string name2 = "C:\\Users\\Andrey\\Documents\\sp\\f2.txt";

            Program ob = new Program(name1, name2);
            ob.get();
            ob.prog();
            ob.printObjInFile();

            ob.sw.Close();
        }
    }
}
