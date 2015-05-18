using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JSLexer {
    //Finite state machine class
    public class FSM {

        private TT state;
        private int line;
        private string buffer;
        private int marginLeft = 0;
        private int symb = 0;
        public bool comment = false;

        private List<Token> tokens;

        /*private enum FT {
            ALL = 0,
            SPACE = 1,
            LETTER = 2,
            NUMBER = 3,
            OPERATOR = 4,
            PUNCT = 5,
            SPECIAL = 6
        }; */

        public enum TT {
            Integer, Double,
            Uoperator, Boperator,
            Punctuation,
            Identifier, IdenStart,
            StringOpened, String,
            Comment, CommentEnd,
            Empty,
            Error
        }; 

        private Transition[] scheme = {
            new Transition("Integer", new TT[] {TT.Empty, TT.Integer}, TT.Integer),
            new Transition("Double", new TT[] {TT.Double}, TT.Double),
            new Transition("DoublePoint", new TT[] {TT.Integer}, TT.Double),
            new Transition("Uoperator", new TT[] {TT.Empty}, TT.Uoperator),
            new Transition("Boperator", new TT[] {TT.Uoperator}, TT.Boperator),
            new Transition("Punctuation", new TT[] {TT.Empty}, TT.Punctuation),
            new Transition("Identifier", new TT[] {TT.IdenStart, TT.Identifier}, TT.Identifier), 
            new Transition("IdenStart", new TT[] {TT.Empty, TT.Identifier}, TT.Identifier),
            new Transition("StringStart", new TT[] {TT.Empty}, TT.StringOpened),
            new Transition("StringCont", new TT[] {TT.StringOpened}, TT.StringOpened),
            new Transition("StringFinish", new TT[] {TT.StringOpened}, TT.String),
            new Transition("CommentStart", new TT[] {TT.Uoperator}, TT.Comment),
            new Transition("Comment", new TT[] {TT.Comment}, TT.Comment)
        };

        class Transition {
            public string name;
            public TT to;
            public TT[] from;

            public Transition(string tk, TT[] fr, TT t) {
                this.name = tk;
                this.from = fr;
                this.to = t;
            }
        }

        public class Token {
            public TT type;
            public string value;
            private string color;
            public string htmlValue {
                get {
                    if (color == String.Empty) color = this.type.ToString();
                    return "<span class='" + this.color + "'>" + value + "</span>";
                }
                set { }
            }

            public Token(TT t, string s) {
                this.type = t;
                this.value = s;
                this.color = String.Empty;
            }

            public void SetColor(string clas) {
                this.color = clas;
            }
        }

        public FSM(string inputString, int l, bool isPrevComment) {
            this.state = TT.Empty;
            this.line = l;
            char[] chars = inputString.ToCharArray();
            if (isPrevComment) this.ApplyComment(chars);
            try { this.Run(chars); }
            catch (SyntaxErrorException e) {
                var s = new String(chars, this.symb, chars.Length - this.symb);
                var t = new Token(TT.Error, s);
                this.tokens.Add(t);
            }
        }

        private void Run(char[] input) {
            this.tokens = new List<Token>();
            foreach (var i in input) {
                if (Char.IsLetter(i) || i.Equals('$')) this.MakeTransition(new string[] { "Identifier", "IdenStart", "StringCont", "Comment"});
                else if (i.Equals('/') || i.Equals('*')) this.MakeTransition(new string[] { "Uoperator", "StringCont", "CommentStart", "Comment" });
                else if ("+-=&|".Contains(i)) this.MakeTransition(new string[] { "Uoperator", "Boperator", "StringCont", "Comment" });
                else if ("<>^!.".Contains(i)) this.MakeTransition(new string[] { "Uoperator", "StringCont", "Comment" });
                else if ("\"'".Contains(i)) this.MakeTransition(new string[] { "StringStart", "StringFinish",  });
                else if ("{}[](),:;".Contains(i)) this.MakeTransition(new string[] { "Punctuation", "StringCont", "Comment" });
                else if (Char.IsNumber(i)) this.MakeTransition(new string[] { "Identifier", "Integer", "Double", "StringCont", "Comment" });
                else if (Char.IsWhiteSpace(i) || this.symb >= input.Length) this.CompleteToken();
                this.buffer += i;
                this.symb++;
            }
            if(this.state == TT.Comment) this.state = TT.CommentEnd;
            this.CompleteToken();
            this.ApplyMargin();
            this.CleanUp();
            this.FindKeywords();
        }

        private void MakeTransition(string[] to) {
            if (this.comment) return;
            if (this.state == TT.Boperator) this.CompleteToken();
            if (this.state == TT.Punctuation) this.CompleteToken();
            if (!to.Contains<string>("CommentStart") && (this.state == TT.Identifier|| !to.Contains<string>("Boperator"))) {
                if (this.state == TT.Uoperator) {
                    this.CompleteToken();
                }
                if (to.Contains<string>("Punctuation")) this.CompleteToken();
                else if (to.Contains<string>("Uoperator")) this.CompleteToken();
                else if (to.Contains<string>("Boperator")) this.CompleteToken();
            }
            
            foreach (var transition in to) {
                foreach (var t in this.scheme) {
                    //if (transition.Equals("Boperator") && t.name == "Boperator" && Debugger.IsAttached) Debugger.Break();
                    if (transition.Equals(t.name) && this.IsPossibleTransition(t)) {
                        this.state = t.to;
                        if (this.state == TT.Comment) this.comment = true;
                        return;
                    }
                }
            }
            //this.CompleteToken();
            throw new SyntaxErrorException("Syntax error at line " + this.line);
        }

        private bool IsPossibleTransition(Transition trans) {
            return trans.from.Contains<TT>(this.state);
            //return (this.state == TT.Integer) ? true : false;
        }

        private void CompleteToken() {
            if (this.buffer == null || this.buffer == String.Empty) return;
            if (this.state == TT.StringOpened) return;
            if (this.state == TT.Comment) return;
            var token = new Token(this.state, this.buffer.Trim());
            this.tokens.Add(token);
            this.buffer = String.Empty;
            this.state = TT.Empty;
        }

        private void CleanUp() {
            this.tokens.RemoveAll(item => item.type == TT.Empty);
        }

        private void ApplyMargin() {
            try {
                while (this.tokens[this.marginLeft].type == TT.Empty) {
                    this.marginLeft++;
                }
            }
            catch (ArgumentOutOfRangeException) { }
        }

        private void ApplyComment(char[] input) {
            var c = 0;
            while (input[c] == ' ') {
                c++;
            }
            if (input[c] == '*') {
                this.comment = true;
                this.state = TT.Comment;
            }
        }

        private void FindKeywords() {
            KewordsFinder.Find(this.tokens);
        }

        public new string ToString() {
            var s = "<div style='margin-left: " + this.marginLeft * 10 +"'>";
            foreach(var t in this.tokens){
                s += t.htmlValue + " ";
                //s += t.value + " ";
            }
            s += "</div>";
            return s;
        }
    }
}
