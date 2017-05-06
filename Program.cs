using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Program
{
    enum words { BEGINL = 257, ENDL, READL, PRINTL, RETRL, IFL, THENL, WHILEL, DOL, INTL, CONSTL, NUMB, IDEN };
    enum commands { OPR, LIT, LDE, LDI, STE, STI, CAL, INI, JMP, JMC };

    struct cmd
    {
        public int cod;    //код (що робити)
        public int opd;   //операнд
    }
    struct odc
    {
        public string name;
        public int what;// 1 = const, 2 = global var, 3 = local var 
        public int val;
    }

    struct fnd
    {
        public string name; //імя функції
        public int isd;     //описана (isd=1) чи ні (isd=0)
        public int cpt;     //кількість параметрів
        public int start;   // точка входу в таблиі команд
    }

    class Program
    {
        int nst = 0;
        char nch = '\n';
        int lex;
        int lval;
        StreamReader sr;
        StreamWriter sw;

        string[] TNM = new string[400];
        int ptn = 0;

        cmd[] TCD = new cmd[100];
        int tc = 0;
        int[] st = new int[500];
        int cgv = 0;
        int clv = 0;

        odc[] TOB = new odc[50];
        int pto = 0;
        int ptol = 0;
        int ut = 1;

        fnd[] TFN = new fnd[10];
        int ptf = 0;
        int adrnm;  //- точка входу в таблиці команд для функції main();
        int cpnm; //- кількість параметрів функції main();
        int t = 0; //-  номер вільного елемента у стеку або ще лічильник введених чисел у стек;
        int sp; // - адреса активації;
        int p; //- номер поточної команди в таблиці команд TCD(лічильник команд);

        public Program(string fileNameIn, string fileNameOut)
        {
            sr = new StreamReader(fileNameIn);
            sw = new StreamWriter(fileNameOut);
        }

        public char getc()
        {
            char sybmol;
            sybmol = (char)sr.Read();
            return sybmol;
        }


        public void get()
        {
            if (sr.Peek() >= 0)
            {
                while (nch.Equals(' ') || char.IsControl(nch))
                {
                    if (nch == '\n')
                    {
                        nst++;
                    }

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
                else if (nch == '(' || nch == ')' || nch == ',' || nch == '=' || nch == ';' || nch == '+' || nch == '-' || nch == '*' || nch == '/' || nch == '%')
                {
                    lex = (int)nch;
                    Console.WriteLine("lexeme={0}", lex);
                    sw.WriteLine("lexeme={0}", lex);
                    nch = getc();

                    return;
                }
            }
        }

        public int add(string nm)
        { 
            for (int i = 0; i < ptn; i++)
            {
                if (TNM[i] == nm)
                {
                    return i;
                }
            }
           
            try
            {
                TNM[ptn] = nm;
                int nomer = ptn;
                ptn++;
                return nomer;
            }

            catch (IndexOutOfRangeException)
            {
                Console.WriteLine("Переповнення таблиці TNM");
            }
            return 1;
        }

        void word()
        {

            string tx = " ";

            int[] cdl = { (int)words.BEGINL, (int)words.ENDL, (int)words.READL, (int)words.PRINTL, (int)words.RETRL, (int)words.IFL, (int)words.THENL, (int)words.WHILEL, (int)words.DOL, (int)words.INTL, (int)words.CONSTL };

            string[] serv = new string[] { "begin", "end", "read", "print", "return", "if", "then", "while", "do", "int", "const" };

            while (char.IsLetterOrDigit(nch))
            {
                tx += nch;
                nch = getc();
            }

            sw.WriteLine("tx={0}", tx);
            tx += " ";

            for (int j = 0; j < serv.Length; j++)
            {
                string txx = tx.Trim();

                if (serv[j] == txx)
                {
                    lex = cdl[j];
                    Console.WriteLine("lexeme={0}", lex);
                    Console.WriteLine("Keyword={0}", lex);
                    sw.WriteLine("Keyword={0}", lex);

                    return;
                }
            }

            lex = (int)words.IDEN;
            lval = add(tx);
            sw.WriteLine(" for ident tx={0} lval={1}", tx, lval);
            Console.WriteLine("lexeme={0}", lex);
            sw.WriteLine("lexeme={0}", lex);

            return;
        }

        void number()
        {
            lval = 0;

            while (char.IsDigit(nch))
            {
                lval = lval * 10 + nch - '0';
                nch = getc();
                Console.WriteLine("nch={0}", nch);
            }

            lex = (int)words.NUMB;
            Console.WriteLine("lexeme={0}", lex);
            sw.WriteLine("lexeme={0}", lex);

            return;
        }

        public void exam(int lx)
        {
            if (lex != lx)
            {
                Console.WriteLine("Не співпадають лексеми lex={0}  та lx={1} nst={2}", lex, lx, nst);
            }
            get();
            return;
        }

        public void prog()
        {
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

        public void dfunc()
        {
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

        public void dconst()
        {
            do
            {
                get();
                cons();
            } while (lex == ',');
            exam(';');
            return;
        }

        public void cons()
        {
            string nm = TNM[lval];
            int s;
            exam((int)words.IDEN);
            exam('=');
            s = (lex == '-') ? -1 : 1;

            if (lex == '+' || lex == '-')
            {
                get();
            }

            newob(nm, 1, s * lval);
            exam((int)words.NUMB);

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
            {
                TOB[p].val -= cp + 3;
            }

            return cp;
        }

        public int body()
        {
            int st;
            exam((int)words.BEGINL); clv = 0;

            while (lex == (int)words.INTL || lex == (int)words.CONSTL)
            {
                if (lex == (int)words.INTL)
                {
                    dvarb();
                }
                else
                {
                    dconst();
                }
            }

            st = gen((int)commands.INI, clv);
            stml();
            exam((int)words.ENDL);
            gen((int)commands.OPR, 10);
            return st;
        }

        public int findob(string nm)
        {
            int p;
            Console.WriteLine("nm={0}", nm);

            for (p = pto - 1; p >= 0; p--)
            {
                Console.WriteLine("TOB[{0}].name={1}", p, TOB[p].name);

                if (TOB[p].name == nm)
                {
                    return p;
                }
            }

            Console.WriteLine("{0} не описана");

            return 0;
        }

        public void stml()
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

                case (int)words.RETRL: get(); expr(); gen((int)commands.OPR, 9); break;

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
                {
                    Console.WriteLine("{0} описана двічі", nm);
                }
            }

            if (pto > 99)
            {
                Console.WriteLine("Переповнення таблиці об'єктів  TOB");
            }

            TOB[pto].name = nm;
            TOB[pto].what = wt;
            TOB[pto].val = vl;
            pto++;
        }

        public int gen(int co, int op)
        {
            TCD[tc].cod = co;
            TCD[tc].opd = op;

            return tc++;
        }

        public int newfn(string nm, int df, int cp, int ps)
        { 
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
        {
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

        public void defin(string nm, int cp, int ad)
        {
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

                for (c1 = TFN[p].start; c1 != -1; c1 = c2)
                {
                    c2 = TCD[c1].opd;
                    TCD[c1].opd = ad;
                }

                TFN[p].start = ad;
            }
            else
            {
                newfn(nm, 1, cp, ad);
            }
            return;
        }

        public int fmain()
        {
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
        {
            if (t > 499)
                Console.WriteLine(" Переповнення стека st ");
            st[++t] = a;
            return;
        }

        public int read()
        {
            int v;
            string s;
            Console.WriteLine("Enter a number");
            s = Console.ReadLine();
            v = Convert.ToInt32(s);
            Console.WriteLine("v={0}", v);
            return v;
        }

        public void interp()
        {
            t = -1;

            Console.WriteLine("SPL interpretation");

            for (int i = 0; i < cgv; i++)
            {
                push(0);
            }

            if (cpnm != 0)
            {
                Console.WriteLine("Введіть {0} фактичних параметрів для main ", cpnm);
                for (int i = 0; i < cpnm; i++)
                    push(read());
            }

            push(cpnm);
            push(-2);
            push(-1);
            sp = t;
            p = adrnm;
            
            do
            {
                comman();
                p++;
            } while (p >= 0);

            if (p == -1)
            {
                Console.WriteLine("st[{0}]={1}", t, st[t]);
            }
        }

        public void comman()
        {
            int a = TCD[p].opd;

            switch (TCD[p].cod)
            {
                case (int)commands.OPR: operat(a); break;
                case (int)commands.LIT: push(a); break;
                case (int)commands.LDE: push(st[a]); break;
                case (int)commands.LDI: push(st[sp + a]); break;
                case (int)commands.STE: st[a] = st[t--]; break;
                case (int)commands.STI: st[sp + a] = st[t--]; break;
                case (int)commands.CAL: push(p); push(sp); sp = t; p = a - 1; break;
                case (int)commands.INI:
                    int i;

                    for (i = 0; i < a; i++)
                    {
                        push(0);
                    }

                    break;
                case (int)commands.JMP: p = a - 1; break;
                case (int)commands.JMC: if (st[t--] <= 0) p = a - 1; break;
                default: Console.WriteLine("TCD[].cod", p, TCD[p].cod); break;
            }
        }

        public void operat(int a)
        {
            int j = t - 1;

            switch (a)
            {
                case 1:
                    Console.WriteLine("1>");
                    push(read());
                    break;
                case 2: Console.WriteLine("{0}", st[t--]); break;
                case 3: st[j] += st[t--]; break;
                case 4: st[j] -= st[t--]; break;
                case 5: st[j] *= st[t--]; break;
                case 6:
                    if (st[t] <= 0)
                    {
                        Console.WriteLine("Ділення на ноль");
                    }

                    st[j] /= st[t--]; break;
                case 7:
                    if (st[t] <= 0)
                    {
                        Console.WriteLine("Ділення на ноль");
                    }

                    st[j] %= st[t--]; break;
                case 8: st[t] = -st[t]; break;
                case 9:
                    j = st[sp - 2]; st[sp - j - 2] = st[t]; t = sp - j - 2;
                    p = st[sp - 1]; sp = st[sp]; break;
                case 10: p = -3; break;
            }
            return;
        }

        public void tables_to_file()
        {
            for (int j = 0; j < tc; j++)
            {
                sw.WriteLine("TCD[{0}].cod {1} opd={2}", j, TCD[j].cod, TCD[j].opd);
                Console.WriteLine("TCD[{0}].cod {1} opd={2}", j, TCD[j].cod, TCD[j].opd);
            }

            for (int j = 0; j < pto; j++)
            {
                sw.WriteLine("TOB[{0}].name {1} what={2} val={3}", j, TOB[j].name, TOB[j].what, TOB[j].val);
                Console.WriteLine("TOB[{0}].name {1} what={2} val={3}", j, TOB[j].name, TOB[j].what, TOB[j].val);
            }

            for (int j = 0; j < ptn; j++)
            {
                sw.WriteLine("TFN[{0}].name {1} isd={2} cpt={3} start={4}", j, TFN[j].name, TFN[j].isd, TFN[j].cpt, TFN[j].start);
                Console.WriteLine("TFN[{0}].name {1} isd={2} cpt={3} start={4}", j, TFN[j].name, TFN[j].isd, TFN[j].cpt, TFN[j].start);
            }
        }

        static void Main(string[] args)
        {
            string nameIn = "C:\\Users\\Andrey\\Documents\\sp\\f1.txt";
            string nameOut = "C:\\Users\\Andrey\\Documents\\sp\\f2.txt";

            Program ob = new Program(nameIn, nameOut);
            ob.get();
            ob.prog(); 
            ob.tables_to_file();
            ob.interp();
            ob.sw.Close();
        }
    }
}