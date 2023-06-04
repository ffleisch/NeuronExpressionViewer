using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class InfixParser : MonoBehaviour
{

    public enum TokenType
    {
        OPERATOR,
        FUNCTION,
        VALUE,
        OTHER
    }
    public enum Associativity
    {
        LEFT,
        RIGHT,
        NONE
    }

    public class Token
    {
        public string name;
        public string identifier;
        public TokenType type;
        public Associativity associativity;
        public int shaderToken = 0;
        public int numParams = 0;
        public int precedence = 0;
        public float value = 0;
        public readonly int tokenID;

        private static int runningID;
        private Token(string name, string identifier, TokenType type, Associativity associativity, int shaderToken, int numParams, int precedence)
        {
            this.name = name;
            this.identifier = identifier;
            this.type = type;
            this.associativity = associativity;
            this.shaderToken = shaderToken;
            this.numParams = numParams;
            this.precedence = precedence;

            tokenID = runningID;
            runningID += 1;
        }

        public Token(float value)
        {
            this.name = "Float Value";
            this.value = value;
            this.type = TokenType.VALUE;
            this.tokenID = -1;
            associativity = Associativity.NONE;
            shaderToken = -1;
        }

        public static readonly Dictionary<string, Token> tokensByName = new();
        public static readonly Dictionary<string, Token> tokensByIdentifier = new();
        public static readonly List<Token> tokenList = new();
        public static void AddToken(string name, string identifier, TokenType type, Associativity associativity, int shaderToken, int numParams, int precedence)
        {

            if (!tokensByIdentifier.ContainsKey(identifier))
            {
                Token t = new Token(name, identifier, type, associativity, shaderToken, numParams, precedence);
                tokensByName.Add(t.name, t);
                tokensByIdentifier.Add(t.identifier, t);
                tokenList.Add(t);
            }

        }

        public static string makeTokenRegexString()
        {
            string s = "(";
            bool first = true;
            foreach (var t in tokenList)
            {
                if (!first)
                {
                    s += "|";
                }
                else
                {
                    first = false;
                }
                s += Regex.Escape(t.identifier);
            }
            s += ")";
            return s;
        }

        public static void initTokenList()
        {
            AddToken("Left Paranthesis", "(", TokenType.OTHER, Associativity.NONE, 0, 0, 0);
            AddToken("Right Paranthesis", ")", TokenType.OTHER, Associativity.NONE, 0, 0, 0);
            AddToken("Comma", ",", TokenType.OTHER, Associativity.NONE, 0, 0, 0);

            AddToken("Addition", "+", TokenType.OPERATOR, Associativity.LEFT, 1, 2, 2);
            AddToken("Multiplication", "*", TokenType.OPERATOR, Associativity.LEFT, 2, 2, 3);
            AddToken("Substraction", "-", TokenType.OPERATOR, Associativity.LEFT, 3, 2, 2);
            AddToken("Division", "/", TokenType.OPERATOR, Associativity.LEFT, 4, 2, 3);
            AddToken("Power", "^", TokenType.OPERATOR, Associativity.RIGHT, 6, 2, 4);
            //AddToken("Modulus", "%", TokenType.OPERATOR, Associativity.LEFT, , 2, 3);

            AddToken("uv x", "uvx", TokenType.VALUE, Associativity.NONE, -2, 0, 0);
            AddToken("uv y", "uvy", TokenType.VALUE, Associativity.NONE, -3, 0, 0);

        }

    }




    List<Token> tokenize(string input)
    {
        Token.initTokenList();
        var output = new List<Token>();

        string float_regex = "([-]?(?:\\d+(?:[.,]\\d*)?|[.,]\\d+)(?:[eE][-+]?\\d+)?)";
        var floatParts = Regex.Split(input, float_regex);

        Debug.Log(input);
        string tokenRegexString = Token.makeTokenRegexString();
        Debug.Log(tokenRegexString);
        foreach (var floatPart in floatParts)
        {
            if (floatPart == "") { continue; }

            if (float.TryParse(floatPart, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float result))
            {
                Debug.Log("Float " + result.ToString());
                output.Add(new Token(result));
            }
            else
            {
                foreach (var tokenMatch in Regex.Split(floatPart, tokenRegexString))
                {
                    if (tokenMatch != "")
                    {
                        if (Token.tokensByIdentifier.ContainsKey(tokenMatch))
                        {
                            var t = Token.tokensByIdentifier[tokenMatch];
                            Debug.Log(string.Format("Token {0}", t.name));
                            output.Add(t);
                        }
                        else
                        {
                            Debug.LogError(string.Format("Could not find Token for {0}", tokenMatch));
                        }
                    }
                }
            }
        }
        return output;
    }



    public List<Token> parseInfix(string input)
    {

        var inputTokenized = tokenize(input);
        List<Token> output = new();
        Stack<Token> operatorStack = new();
        var leftParenthesis = Token.tokensByIdentifier["("].tokenID;
        var rightParenthesis = Token.tokensByIdentifier[")"].tokenID;
        var comma = Token.tokensByIdentifier[","].tokenID;

        foreach (var t in inputTokenized)
        {
            if (t.type == TokenType.VALUE)
            {
                output.Add(t);
                continue;
            }

            if (t.type == TokenType.FUNCTION)
            {
                operatorStack.Push(t);
            }

            if (t.type == TokenType.OPERATOR)
            {
                while (operatorStack.Count > 0 &&
                    operatorStack.Peek().tokenID != leftParenthesis &&
                    (operatorStack.Peek().precedence > t.precedence ||
                    (operatorStack.Peek().precedence == t.precedence && t.associativity == Associativity.LEFT)
                    ))
                {
                    output.Add(operatorStack.Pop());
                }
                operatorStack.Push(t);
            }


            if (t.type == TokenType.OTHER)
            {
                if (t.tokenID == comma)
                {
                    while (operatorStack.Peek().tokenID != leftParenthesis)
                    {
                        output.Add(operatorStack.Pop());
                    }
                }
                else
                if (t.tokenID == leftParenthesis)
                {
                    operatorStack.Push(t);
                }
                else
                if (t.tokenID == rightParenthesis)
                {
                    while (operatorStack.Peek().tokenID != leftParenthesis)
                    {
                        output.Add(operatorStack.Pop());
                    }
                    operatorStack.Pop();
                    if (operatorStack.Count > 0 && operatorStack.Peek().type == TokenType.FUNCTION)
                    {
                        output.Add(operatorStack.Pop());
                    }
                }
            }

        }
        while (operatorStack.Count > 0)
        {
            output.Add(operatorStack.Pop());
        }
        Debug.Log("rpn:");
        string sRpn = "";
        foreach (var t in output)
        {
            if (t.name=="Float Value")
            {
                sRpn += t.value;
            }
            else
            {
                sRpn += t.identifier;
            }
        }
        Debug.Log(sRpn);
        return output;
    }

    public (List<float>, List<float>) parseToShaderArrays(string input) {
        var tokens = parseInfix(input);
        List<float> values=new();
        List<float> shaderTokens=new();
        foreach (var t in tokens) {
            if (t.name == "Float Value") {
                values.Add(t.value);            
            }
            shaderTokens.Add(t.shaderToken);
        }

        string values_string="";
        foreach (float f in values) {
            values_string += " " + f.ToString();        
        }

        string tokens_string="";
        foreach (float f in shaderTokens) {
            tokens_string += " " + f.ToString();        
        }
        Debug.Log(values_string);
        Debug.Log(tokens_string);

        return (values,shaderTokens);
    }



    private void Start()
    {
    }


}
