using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class InfixParser : MonoBehaviour
{


    //what type can a token be?

    public enum TokenType
    {
        OPERATOR, //infix operator
        FUNCTION, //function f()
        //VALUE,    //any value, fixed or otherwise
        VALUE_FLOAT,
        VALUE_ATTRIBUTE,
        OTHER     //parantheses and commas 
    }
    public enum Associativity //is an operato left or right associative
    {
        LEFT,
        RIGHT,
        NONE
    }

    public class Token
    {
        public string name; //name
        public string identifier;   //what string identifies it in the expre4sison to be parsed
        public TokenType type;      //what type is the operator
        public Associativity associativity; // left or right associative
        public int shaderToken = 0; // integer that represents what action shall be taken in the shader
        public int numParams = 0;  //how many parameters deos this function expect
        public int precedence = 0; //precedence for order of operations
        public float value = 0; //value of this token is a value
        public readonly int tokenID; //unique identifier for this token, not sure if necessary

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
            this.type = TokenType.VALUE_FLOAT;
            this.tokenID = -1;
            associativity = Associativity.NONE;
            shaderToken = -1;
        }

        public static readonly Dictionary<string, Token> tokensByName = new();//dict for all tokens by their name
        public static readonly Dictionary<string, Token> tokensByIdentifier = new();//dict for all tokens by their identifier
        public static readonly List<Token> tokenList = new();   //a singleton list of every token used for parsing




        //is thi right, or should a list be created every time a new parser is made?


        //add a token to the list of all tokens and the dicts
        public static void AddToken(string name, string identifier, TokenType type, Associativity associativity, int shaderToken, int numParams, int precedence,float value =0)
        {

            if (!tokensByIdentifier.ContainsKey(identifier))
            {
                Token t = new Token(name, identifier, type, associativity, shaderToken, numParams, precedence);
                t.value = value;
                tokensByName.Add(t.name, t);
                tokensByIdentifier.Add(t.identifier, t);
                tokenList.Add(t);
            }

        }

        //public static void AddToken(string name, string identifier, TokenType type, Associativity associativity, int shaderToken, int numParams, int precedence, float value) {

        //    AddToken(name, identifier, type, associativity, shaderToken, numParams, precedence);
        
        
        //}

        //use all tokens in the list to make a regex string for splitting an expression into operator parts
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

        //all important basic tokens
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

            AddToken("uv x", "uvx", TokenType.VALUE_ATTRIBUTE, Associativity.NONE, -2, 0, 0);
            AddToken("uv y", "uvy", TokenType.VALUE_ATTRIBUTE, Associativity.NONE, -3, 0, 0);


            AddToken("Square Root", "sqrt", TokenType.FUNCTION, Associativity.NONE, 7, 1, 5);
            AddToken("Map Value", "map", TokenType.FUNCTION, Associativity.NONE, 20, 5, 5);
            AddToken("Clip to Range", "clip", TokenType.FUNCTION, Associativity.NONE, 21, 3, 5);


            AddToken("Output single Value", "col", TokenType.FUNCTION, Associativity.LEFT, 100, 1, 5);//check what exact precedence is good for functions
            AddToken("Output RGB", "rgb", TokenType.FUNCTION, Associativity.LEFT, 101, 3, 5);

        }


        public static void AddAttributeTokens()
        {

            foreach (attributesEnum attribute in Enum.GetValues(typeof(attributesEnum)))
            {
                var s = AttributeUtils.attributeNames[attribute];
                var identifier = AttributeUtils.attributeIdentifiers[attribute];
                Debug.Log(s);
                AddToken(s, identifier, TokenType.VALUE_ATTRIBUTE, Associativity.NONE, -4, 0, 0,(int)attribute);
            }

        }


    }



    //take a string expression and turn into a list of tokens
    List<Token> tokenize(string input)
    {
        //init the list of valid tokens, this has to only be done once, but i am not sure where it would be best to call this from
        Token.initTokenList();
        Token.AddAttributeTokens();

        var output = new List<Token>();


        //regex for splitting along floats in the expression
        //taken from https://stackoverflow.com/questions/4703390/how-to-extract-a-floating-number-from-a-string
        //added paranthesis around the entire expression to make it emitt a single group, made all other groups non emitting
        //TODO discriminate negative sign operator or sign
        string float_regex = "([-]?(?:\\d+(?:[.]\\d*)?|[.]\\d+)(?:[eE][-+]?\\d+)?)";

        //split along the floats
        var floatParts = Regex.Split(input, float_regex);

        Debug.Log(input);
        string tokenRegexString = Token.makeTokenRegexString();
        Debug.Log(tokenRegexString);


        //cheks for each part, wether it is a float
        foreach (var floatPart in floatParts)
        {
            if (floatPart == "") { continue; }

            if (float.TryParse(floatPart, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float result))
            {
                //if it is a  float, make a new value token and add it to the output
                Debug.Log("Float " + result.ToString());
                output.Add(new Token(result));
            }
            else
            {
                //if it not a float, split the part with the operator regex and add the token for each operator part
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
                            //could not find that part in the token dict
                            //unknown identifier
                            Debug.LogError(string.Format("Could not find Token for {0}", tokenMatch));
                        }
                    }
                }
            }
        }
        return output;
    }


    //take a string expression and return a list representing the expression in postfix/reverse polish notation
    //this is done with the shunting yard algorithm and is in large part taken from https://en.wikipedia.org/wiki/Shunting_yard_algorithm
    public List<Token> parseInfix(string input)
    {
        //create tokens from expression
        var inputTokenized = tokenize(input);
        List<Token> output = new();
        Stack<Token> operatorStack = new();
        var leftParenthesis = Token.tokensByIdentifier["("].tokenID;
        var rightParenthesis = Token.tokensByIdentifier[")"].tokenID;
        var comma = Token.tokensByIdentifier[","].tokenID;

        foreach (var t in inputTokenized)
        {
            if (t.type == TokenType.VALUE_FLOAT||t.type==TokenType.VALUE_ATTRIBUTE)
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
            if (t.type==TokenType.VALUE_FLOAT)
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


    //prepare the values for transfer to the shader
    //takes the list of tokens in rpn form and creates the array of constants and array ot shader tokens


    public (List<float>, List<float>,List<attributesEnum>) parseToShaderArrays(string input)
    {
        var tokens = parseInfix(input);

        var attibutesNeeded = extractNeededAttributes(tokens);


        List<float> values = new();
        List<float> shaderTokens = new();
        foreach (var t in tokens)
        {
            if (t.type==TokenType.VALUE_FLOAT ||t.type==TokenType.VALUE_ATTRIBUTE)
            {

                if (t.shaderToken != -4)
                {
                    values.Add(t.value);
                }
                else {
                    values.Add(attibutesNeeded.FindIndex(a=>a==(attributesEnum)t.value));
                }
            }
            shaderTokens.Add(t.shaderToken);
        }

        string values_string = "";
        foreach (float f in values)
        {
            values_string += " " + f.ToString();
        }

        string tokens_string = "";
        foreach (float f in shaderTokens)
        {
            tokens_string += " " + f.ToString();
        }
        Debug.Log(values_string);
        Debug.Log(tokens_string);

        return (values, shaderTokens,attibutesNeeded);
    }


    public List<attributesEnum> extractNeededAttributes(List<Token> parsedExpression) {
        var outp = new List<attributesEnum>();
        
        foreach (var t in parsedExpression) {
            if (t.shaderToken == -4) { //token for an attribute
                outp.Add((attributesEnum)(int)t.value);
            }
        }
        return outp;
    }


    private void Start()
    {
    }


}
