using System;
using System.Collections.Generic;
using System.Linq;

namespace JSLexer {
    public abstract class KewordsFinder {

        private static Keyword[] keywords = {
            new Keyword("ActionWord", new string[] {"break", "case", "catch", "continue", "debugger", "default", "delete", "do", "else", "finally", "goto", "if", "for", "import", "in", "instanceof", "new", "package", "return", "throw", "try", "typeof", "while", "with", "yield"}),
            new Keyword("TypeWord", new string[] {"abstract", "arguments", "boolean", "byte", "char", "class", "const", "double", "enum", "export", "extends", "final", "float", "function", "implements", "int", "interface", "long", "native", "private", "protected", "public", "short", "static", "synchronized", "throws", "transient", "var", "void", "volatile"}),
            new Keyword("Logical", new string[] {"false", "true", "super", "this"}),
            new Keyword("Value", new string[] {"Array", "Date", "Math", "Number", "Object", "String", "prototype", "length", "name", "document", "event", "navigator"}),
            new Keyword("Function", new string[] {"isFinite", "isNaN", "toString", "hasOwnProperty", "isPrototypeOf"}),
            new Keyword("Constant", new string[] { "Infinity", "NaN", "undefined"})
        };

        private class Keyword {
            public string name;
            public List<string> words;

            public Keyword(string n, string[] w) {
                this.name = n;
                this.words = w.ToList();
            }
        }

        public static void Find(List<FSM.Token> tokens) {
            foreach(var t in tokens) {
                if (t.type == FSM.TT.Identifier) {
                    KewordsFinder.CheckToken(t);
                }
            }
        }

        private static void CheckToken(FSM.Token tok) {
            foreach (var k in keywords) {
                if (k.words.Contains<string>((tok.value).Trim())) {
                    tok.SetColor(k.name);
                    return;
                }
            }
        }
    }
}
