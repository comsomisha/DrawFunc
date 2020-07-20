using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Интепретатор_15
{
    public struct Name_Value // структура для хранения значений листьев дерева
    {
        public string name;
        public double value;
    }
    public enum Operation
    {
        PLUS = 1,
        MINUS = 2,
        MULTIPLY = 3,
        DIVIDE = 4,
        AND = 5,   // & 
        OR = 6,    // | 
        POWER = 7,   // ^ 
        NOT = 8,   // ! 
        LESS = 9,        // < 
        MORE = 10,        // > 
        EQUALLY = 11,     // == 
        NOTEQUALLY = 12,  // != 
        LESSEQUALLY = 13, // <= 
        MOREEQUALLY = 14, // >=
        NONE = 0
    }
    public enum TypeNode { VAL, CONST, FUNC, OPERATION } // типы узла

    public enum TypeVal { tByte, tFloat, tBool, tInt } // типы данных

    public enum OperatorType { expression, ifthen } // типы оператора

    public abstract class Node // абстрактный класс узла
    {
        protected TypeVal typeVal;
        public abstract object DoOperation();
        protected object value;
        public object Value
        {
            get { return DoOperation(); }
        }
    }

    class NodeConst : Node // отвечает за константы
    {
        public NodeConst(TypeVal typeVal, object value)
        {
            this.typeVal = typeVal;
            this.value = value;
        }
        public override object DoOperation()
        {
            return value;
        }
    }

    class NodeOperation : Node // отвечает за операции
    {
        public Operation typeOperation;
        public Node left;
        public Node right;

        public NodeOperation(Operation t, Node left, Node right)
        {
            this.left = left;
            this.right = right;
            this.typeOperation = t;
        }

        public override object DoOperation()
        {
            double a = Convert.ToDouble(left.DoOperation());
            double b = Convert.ToDouble(right.DoOperation());
            bool a1 = Convert.ToBoolean(left.DoOperation());
            bool b1 = Convert.ToBoolean(right.DoOperation());
            switch (typeOperation)
            {
                case Operation.PLUS:
                    value = a + b;
                    return value;
                case Operation.MINUS:
                    typeVal = TypeVal.tFloat;
                    value = a - b;
                    return value;
                case Operation.MULTIPLY:
                    typeVal = TypeVal.tFloat;
                    value = a * b;
                    return value;
                case Operation.DIVIDE:
                    typeVal = TypeVal.tFloat;
                    value = a / b;
                    return value;
                case Operation.AND:
                    typeVal = TypeVal.tBool;
                    value = a1 & b1;
                    return value;
                case Operation.OR:
                    typeVal = TypeVal.tBool;
                    value = a1 | b1;
                    return value;
                case Operation.POWER:
                    typeVal = TypeVal.tFloat;
                    value = Math.Pow(a, b);
                    return value;
                case Operation.NOT:
                    typeVal = TypeVal.tBool;
                    value = !a1;
                    return value;
                case Operation.LESS:
                    typeVal = TypeVal.tBool;
                    value = a < b;
                    return value;
                case Operation.MORE:
                    typeVal = TypeVal.tBool;
                    value = a > b;
                    return value;
                case Operation.EQUALLY:
                    return a == b;
                case Operation.NOTEQUALLY:
                    typeVal = TypeVal.tBool;
                    value = a != b;
                    return value;
                case Operation.LESSEQUALLY:
                    typeVal = TypeVal.tBool;
                    value = a <= b;
                    return value;
                case Operation.MOREEQUALLY:
                    typeVal = TypeVal.tBool;
                    value = a >= b;
                    return value;
                default: throw new NotImplementedException();
            }
        }
    }
    class NodeVal : Node // отвечает за переменные
    {
        protected int numVal;
        public NodeVal(int numVal)
        {
            this.numVal = numVal;
        }
        public override object DoOperation()
        {
            return TParser.aVar[numVal].value;
        }
    }
    class NodeFunc : Node // отвечает за функции
    {
        protected int numFunc;
        protected Node arg;
        protected Node arg2;
        public NodeFunc(int numFunc, Node arg, Node arg2)
        {
            this.numFunc = numFunc;
            this.arg = arg;
            this.arg2 = arg2;
        }
        public override object DoOperation()
        {
            double a = Convert.ToDouble(arg.DoOperation());
            double b = Convert.ToDouble(arg2.DoOperation());
            switch (numFunc)
            {
                case 0: value = Math.Sin(a); return value;
                case 1: value = Math.Cos(a); return value;
                case 2: value = Math.Tan(a); return value;
                case 3: value = 1 / Math.Tan(a); return value;
                case 4: value = Math.Abs(a); return value;
                case 5: value = Math.Pow(a, 1/b); return value;
                case 6: value = Math.Log(a, b); return value;
                case 7: value = -a; return value;
                default: return null;
            }
        }
    }
    // Классы для операторов
    public abstract class Operator
    {
        public Node top;
        public OperatorType operatorType;
        public abstract void Run_Formula();
    }

    class OperatorExpression : Operator
    {
        public int numVal;
        public OperatorExpression(int numVal, Node t)
        {
            this.numVal = numVal;
            this.top = t;
            this.operatorType = OperatorType.expression;
        }
        public override void Run_Formula()
        {
            TParser.aVar[numVal].value = Convert.ToDouble(top.Value);
        }
    }

    class OperatorIf : Operator
    {
        public Operator OperThen;
        public Operator OperElse;
        public OperatorIf(Node t, Operator op1, Operator op2)
        {
            this.top = t;
            this.OperElse = op2;
            this.OperThen = op1;
            this.operatorType = OperatorType.ifthen;
        }
        public override void Run_Formula()
        {
            if (Convert.ToBoolean(top.Value))
                OperThen.Run_Formula();
            else
                if (OperElse != null)
                    OperElse.Run_Formula();
        }
    }

    public class TParser
    {
        public Operator topOp;
        char[] chars = new char[26 + 26 + 10 + 1];
        public static Name_Value[] aVar = new Name_Value[2]; // для хранения значений листьев дерева
        protected static string[] nameFunc = { "Sin", "Cos", "Tan", "Ctan", "Abs", "Sqrt", "Log", "-" };
        public TParser()
        {
            for (int i = 0; i <= 25; i++) chars[i] = (char)(i + 65);
            for (int i = 0; i <= 25; i++) chars[i + 26] = (char)(i + 65 + 32);
            for (int i = 0; i <= 9; i++) chars[i + 2 * 26] = (char)(i + 48);
            chars[62] = '_';
            aVar[0].name = "x";
            aVar[1].name = "y";
        }

        public class NodeException : Exception // мой класс исключений
        {
            private byte error;
            public byte Error
            {
                get { return this.error; }
            }
            public NodeException(byte error, string Message)
                : base(Message)
            {
                this.error = error;
            }
        }


        string Pop(ref string s, byte n)
        {
            string result = s.Substring(0, n);
            s = s.Substring(n); s = s.Trim();
            return result;
        }

        bool TestCh(char ch)
        {
            int k = Array.IndexOf(chars, ch);
            return k != -1;
        }

        char Peek(ref string s)
        {
            char result = s[0];
            return result;
        }

        bool Test(char ch, params char[] nums)
        {
            return Array.IndexOf(nums, ch) >= 0;
        }

        void SetConst(ref string s, out double x, out int k, out  TypeVal t) // константы
        {
            s = s.Trim(); x = 0; k = 0;
            t = TypeVal.tInt; string st = ""; x = 0;
        begin:
            if ((s != "") && Test(s[0], '0', '1', '2', '3', '4',
                '5', '6', '7', '8', '9', ','))
            {
                if (Peek(ref s) == ',') t = TypeVal.tFloat;
                st += Pop(ref s, 1);
                goto begin;
            }
            else
            {
                if ((s != "") && Test(Peek(ref s), 'e', 'E'))
                {
                    st += Pop(ref s, 1);
                    if (t != TypeVal.tFloat) throw new NodeException(11, "ошибка с типом");
                    if (Test(s[0], '0', '1', '2', '3', '4', '5', '6', '7', '8', '9')) goto begin;
                    else { throw new NodeException(11, "ошибка с e, E"); }
                }
            }
            if (st == "") throw new NodeException(9, "ошибка в константах");
            if (t == TypeVal.tInt)
                k = Convert.ToInt32(st);
            else
                x = Convert.ToDouble(st);
            s = s.Trim();
        }

        void Factor(ref string s, out Node D) // константы, скобки, функции, переменные
        {
            D = null;
            s = s.Trim();
            if (s.Length != 0)
            {
                char[] aCh = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '+', '-' };
                int k = Array.IndexOf(aCh, s[0]);
                if (k != -1)
                {
                    double x; TypeVal t;
                    SetConst(ref s, out x, out k, out t);
                    if (t == TypeVal.tFloat)
                        D = new NodeConst(t, x);
                    else
                        D = new NodeConst(t, k);
                }
                else
                    if (s[0] == '(')
                    {
                        Pop(ref s, 1);
                        PFormula(ref s, out D);
                        s = s.Trim();
                        if ((s.Length > 0) & (s[0] != ')')) throw new NodeException(9, "ошибка со скобками");
                        Pop(ref s, 1);
                    }
                    else
                    {
                        string st = Pop(ref s, 1);
                        while ((s.Length > 0) && TestCh(s[0])) st += Pop(ref s, 1);
                        bool ok = false; int nf = -1;
                        while ((nf < nameFunc.Length - 1) && !ok)
                            ok = st == nameFunc[++nf];
                        if (ok)
                        {
                            if ((s.Length == 1) | (s[0] != '('))
                            {
                                throw new NodeException(9, "ошибка с функцией");
                            }
                            s = s.Trim();

                            st = ""; k = 1; int i = 0; int i2=-1;
                            while ((i < s.Length) & (k != 0))
                            {
                                st += s[++i];
                                switch (s[i])
                                {
                                    case '(': ++k; break;
                                    case ')': --k; break;
                                    case ';': if ((nf != 5) && (nf != 6)) throw new NodeException(9, "ошибка с функцией");
                                        else i2 = i; break;                                        
                                }
                            }
                            if (i2 == -1)
                            {
                                st = st.Substring(0, st.Length - 1);
                                s = s.Substring(i + 1, s.Length - i - 1);
                                Node Da;
                                PFormula(ref st, out Da);
                                Node Da2 = Da;
                                D = new NodeFunc(nf, Da, Da2);
                                s = s.Trim();
                            }
                            else
                            {
                                string st1 = "";
                                string st2 = "";
                                st1 = st.Substring(0, i2 - 1);
                                st2 = st.Substring(i2, st.Length - i2 - 1);
                                s = s.Substring(i + 1, s.Length - i - 1);
                                Node Da;
                                PFormula(ref st1, out Da);
                                Node Da2;
                                PFormula(ref st2, out Da2);
                                D = new NodeFunc(nf, Da, Da2);
                                s = s.Trim();
                            }
                            return;
                        }
                        // поиск переменной
                        ok = false; int nv = -1;
                        while ((nv < aVar.Length - 1) && !ok)
                            ok = st == aVar[++nv].name;
                        if (ok)
                        {
                            D = new NodeVal(nv);
                            s = s.Trim(); return;
                        }
                        throw new NodeException(10, "ошибка с переменной / лишний знак");
                    }
            }
            else
            {
                D = null; throw new NodeException(10, "нехватка данных");
            }
        }

        void Term(ref string s, out Node D) // * / & ^ 
        {
            s = s.Trim();
            if (s.Length != 0)
            {
                Factor(ref s, out D);
                while ((s.Length != 0) && ((Peek(ref s) == '*') | (Peek(ref s) == '/') | (Peek(ref s) == '&') | (Peek(ref s) == '^')))
                {
                    string znak = Pop(ref s, 1);
                    if (s.Length == 0) throw new NodeException(8, "после знака ничего не оказалось");
                    Node D2;
                    Factor(ref s, out D2);
                    Node D1 = D;

                    Operation t = Operation.MULTIPLY;
                    switch (znak)
                    {
                        case "*": t = Operation.MULTIPLY; break;
                        case "/": t = Operation.DIVIDE; break;
                        case "&": t = Operation.AND; break;
                        case "^": t = Operation.POWER; break;
                    }
                    D = new NodeOperation(t, D1, D2);
                }
            }
            else
            {
                D = null; throw new NodeException(8, "нехватка данных");
            }
        }

        public void Expression(ref string s, out Node D) // + - |
        {
            s = s.Trim();
            if (s.Length != 0)
            {
                string sign = "+";
                if ((Peek(ref s) == '+') | (Peek(ref s) == '-'))
                    sign = Pop(ref s, 1);
                Term(ref s, out D);
                if (sign == "-")
                {
                    Node D1 = D;
                    Node D2 = D1;
                    D = new NodeFunc(7, D1, D2);
                }
                s = s.Trim();
                while ((s.Length > 0) && ((Peek(ref s) == '+') | (Peek(ref s) == '-') | (Peek(ref s) == '|')))
                {
                    sign = Pop(ref s, 1);
                    if (s.Length != 0)
                    {
                        if ((Peek(ref s) == '+') | (Peek(ref s) == '-') | (Peek(ref s) == '|')) throw new NodeException(7, "лишний знак");
                    }
                    else throw new NodeException(7, "после знака ничего не оказалось");
                    Node D2;
                    Term(ref s, out D2);
                    Node D1 = D;

                    Operation t = Operation.PLUS;
                    switch (sign[0])
                    {
                        case '+': t = Operation.PLUS; break;
                        case '-': t = Operation.MINUS; break;
                        case '|': t = Operation.OR; break;
                    }
                    D = new NodeOperation(t, D1, D2);
                }
            }
            else
            {
                D = null; throw new NodeException(6, "нехватка данных");
            }
        }

        public Operation SetChRelation(string s) // для операций отношения
        {
            Operation result = Operation.NONE;
            if (s.Length >= 2)
                if ((s[0] == '=') & (s[1] == '=')) result = Operation.EQUALLY;
                else
                    if ((s[0] == '!') & (s[1] == '=')) result = Operation.NOTEQUALLY;
                    else
                        if ((s[0] == '<') & (s[1] == '=')) result = Operation.LESSEQUALLY;
                        else
                            if ((s[0] == '>') & (s[1] == '=')) result = Operation.MOREEQUALLY;
                            else
                                if (s[0] == '<') result = Operation.LESS;
                                else
                                    if (s[0] == '>') result = Operation.MORE;
            return result;
        }

        public void PFormula(ref string s, out Node D) // операции отношения
        {
            s = s.Trim();
            if (s.Length != 0)
            {
                Expression(ref s, out D);
                s = s.Trim();
                Operation chRelation = SetChRelation(s);
                if (chRelation != Operation.NONE)
                {
                    switch (chRelation)
                    {
                        case Operation.LESS:
                        case Operation.MORE:
                            Pop(ref s, 1); break;
                        case Operation.EQUALLY:
                        case Operation.NOTEQUALLY:
                        case Operation.LESSEQUALLY:
                        case Operation.MOREEQUALLY:
                            Pop(ref s, 2);
                            break;
                    }
                    Node D2;
                    Expression(ref s, out D2);
                    Node D1 = D;
                    D = new NodeOperation(chRelation, D1, D2);
                }
            }
            else
            {
                D = null; throw new NodeException(5, "ошибка в операциях отношения");
            }
        }

        public void SetOperator(ref string s, out Operator op) // оператор присваивания или if
        {
            s = s.Trim(); op = null;
            string st = "";
            while ((s.Length > 0) && TestCh(s[0])) st += Pop(ref s, 1);
            s = s.Trim();
            if (st == "if")
            {
                string sExpres = "";
                int k = 1; int i = 0;
                while ((i < s.Length) & (k != 0))
                {
                    sExpres += s[++i];
                    switch (s[i])
                    {
                        case '(': ++k; break;
                        case ')': --k; break;
                    }
                }
                sExpres = sExpres.Substring(0, sExpres.Length - 1);
                s = s.Substring(i + 1, s.Length - i - 1); s = s.Trim();
                int kThen = s.IndexOf(';');
                int kElse = s.IndexOf("else");
                if (kThen == 0) throw new NodeException(4, "ошибка в if");
                Node D;
                PFormula(ref sExpres, out D);
                Operator op1;
                Operator op2;
                if (kElse == 0)
                {
                    string sThen = s.Substring(kThen + 4, s.Length - kThen - 2);
                    SetOperator(ref sThen, out op1);
                    op2 = null;
                }
                else
                {
                    string sThen = s.Substring(0, kThen);
                    string sElse = s.Substring(kElse + 4, s.Length - kElse - 4);
                    SetOperator(ref sThen, out op1);
                    SetOperator(ref sElse, out op2);
                }
                op = new OperatorIf(D, op1, op2);
            }
            else
            {
                bool ok = false; int nv = -1;
                while ((nv < aVar.Length - 1) && !ok)
                    ok = st == aVar[++nv].name;
                if (ok)
                {
                    if ((s.Length < 1) | (s[0] != '='))
                        throw new NodeException(4, "ошибка с оператором присваивания");
                    Pop(ref s, 1);
                    Node D;
                    PFormula(ref s, out D);
                    op = new OperatorExpression(nv, D);
                }
            }
        }
    }
}
