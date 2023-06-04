import re
import shunting_yard_functions as syf
#expression = "3*(2+1)^2+4*exp(3.141592e0)"
expression = "3*(2+1)^2+4*3.141592e0"


operators =         ["+", "-", "*", "/", "^", "exp", "(", ")"]
op_type =           ["o", "o", "o", "o", "o", "f", "", ""]
asscociativity =    ["l", "l", "l", "l", "r", "l", "", ""]
evaluations=        [syf.add,syf.sub,syf.mul,syf.div,syf.pow,syf.exp,None,None]
num_params=         [ 2 , 2 , 2 , 2 , 2 , 1 , 0 , 0]
precedences = [2, 2, 3, 3, 4, 3, -1, -1]

operator_index_dict = {operators[i]:i for i in range(len(operators))}

print(operator_index_dict)


def operator_regex_string(operators):
    str = "("
    first = True
    for op in operators:
        if not first:
            str += "|"

        str += re.escape(op)
        first = False
    str += ")"
    return str


def tokenize(expression):
    values = []
    tokens = []

    # https://stackoverflow.com/questions/4703390/how-to-extract-a-floating-number-from-a-string
    # float_regex=r"[-+]?(\d+([.,]\d*)?|[.,]\d+)([eE][-+]?\d+)?"
    float_regex = r"([-]?(?:\d+(?:[.,]\d*)?|[.,]\d+)(?:[eE][-+]?\d+)?)"

    operator_regex = operator_regex_string(operators)

    parts = re.split(float_regex, expression)
    parts = filter(None, parts)
    print(parts)
    for p in parts:
        try:
            print("float: ", float(p))
            tokens.append((-1, float(p)))
        except:

            ops = list(filter(None, re.split(operator_regex, p)))
            for op in ops:
                try:
                    index = operators.index(op)
                    print(op)
                    tokens.append((index, 0))
                except ValueError as e:
                    print("token " + op + " not recognized")

    return tokens


#https://en.wikipedia.org/wiki/Shunting_yard_algorithm
def shunting_yard(tokens):
    output = []
    operator_stack = []
    t_value = -1
    t_left_parenthesis = operator_index_dict['(']
    t_right_parenthesis = operator_index_dict[')']

    for t, v in tokens:

        if t == t_value:  # token is a value
            output.append((t, v))
            continue

        if op_type[t] == "f":
            operator_stack.append(t)
        if op_type[t] == "o":
            while (len(operator_stack) > 0) \
                    and (operator_stack[-1] != t_left_parenthesis) \
                    and (precedences[operator_stack[-1]] > precedences[t] or (
                    precedences[operator_stack[-1]] == precedences[t] and asscociativity[t] == "l")):
                output.append(operator_stack.pop())
            operator_stack.append(t)

        if t == t_left_parenthesis:
            operator_stack.append(t)
        if t == t_right_parenthesis:
            while operator_stack[-1] != t_left_parenthesis:
                output.append(operator_stack.pop())
            operator_stack.pop()
            if len(operator_stack) > 0 and op_type[operator_stack[-1]] == "f":
                output.append(operator_stack.pop())
    while len(operator_stack)>0:
        output.append(operator_stack.pop())
    print(output)
    print(operator_stack)
    return output


def print_rpn(rpn):
    for t in rpn:
        if type(t) is tuple:
            print(t[1], end=" ")
        else:
            print(operators[t],end=" ")
    print()

def evaluate_rpn(rpn):
    value_stack=[]
    for t in rpn:
        if type(t) is tuple:
            value_stack.append(t[1])
        else:
            params=[]
            for i in range(num_params[t]):
                params.append(value_stack.pop())
                params.reverse()
            value_stack.append(evaluations[t](*params))
    return value_stack[-1]
print(operator_regex_string(operators))

tokens = tokenize(expression)
print(tokens)
print(expression)
rpn=shunting_yard(tokens)
print_rpn(rpn)
print()
print("\"%s\"="%expression)
print(evaluate_rpn(rpn))