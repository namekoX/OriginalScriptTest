
using System;

public class OsLexer
{
    private string input;
    private char? currentChar = null;
    private char? nextChar = null;
    private char? nextNextChar = null;
    private int position = 0;
    public OsLexer(string input)
    {
        this.input = OsReplacer.replace(input);
        readChar();
    }

    public OsToken getNextToken()
    {
        // スペース、タブ、改行は読み込み不要なので飛ばす
        skipWhiteSpace();
        OsToken token = null;
        switch (currentChar)
        {
            case '=':
                if (nextChar == '=')
                {
                    token = new OsToken(OsTokenType.EQ, "==");
                    readChar();
                }
                else if (nextChar == '>')
                {
                    token = new OsToken(OsTokenType.GTEQ, ">=");
                    readChar();
                }
                else if (nextChar == '<')
                {
                    token = new OsToken(OsTokenType.LTEQ, "<=");
                    readChar();
                }
                else
                {
                    token = new OsToken(OsTokenType.ASSIGN, currentChar.ToString());
                }
                break;
            case ';':
                token = new OsToken(OsTokenType.SEMICOLON, currentChar.ToString());
                break;
            case '.':
                token = new OsToken(OsTokenType.DOT, currentChar.ToString());
                break;
            case '(':
                token = new OsToken(OsTokenType.LPAREN, currentChar.ToString());
                break;
            case ')':
                token = new OsToken(OsTokenType.RPAREN, currentChar.ToString());
                break;
            case '{':
                token = new OsToken(OsTokenType.LBRACE, currentChar.ToString());
                break;
            case '}':
                token = new OsToken(OsTokenType.RBRACE, currentChar.ToString());
                break;
            case '[':
                token = new OsToken(OsTokenType.LBRACKET, currentChar.ToString());
                break;
            case ']':
                token = new OsToken(OsTokenType.RBRACKET, currentChar.ToString());
                break;
            case '+':
                if (nextChar == '=')
                {
                    token = new OsToken(OsTokenType.PLUSEQ, "+=");
                    readChar();
                }
                else
                {
                    token = new OsToken(OsTokenType.PLUS, currentChar.ToString());
                }
                break;
            case '-':
                if (nextChar == '=')
                {
                    token = new OsToken(OsTokenType.MINUSEQ, "-=");
                    readChar();
                }
                else
                {
                    token = new OsToken(OsTokenType.MINUS, currentChar.ToString());
                }
                break;
            case '*':
                if (nextChar == '=')
                {
                    token = new OsToken(OsTokenType.ASTERISKEQ, "*=");
                    readChar();
                }
                else
                {
                    token = new OsToken(OsTokenType.ASTERISK, currentChar.ToString());
                }
                break;
            case '/':
                if (nextChar == '=')
                {
                    token = new OsToken(OsTokenType.SLASHEQ, "/=");
                    readChar();
                }
                else if (nextChar == '/')
                {
                    // コメント(単行)の場合、改行が来るまで飛ばす
                    while (nextChar != null && nextChar != '\r' && nextChar != '\n')
                    {
                        readChar();
                    }
                    token = new OsToken(OsTokenType.COMMENT, "");
                }
                else if (nextChar == '*')
                {
                    // コメント(複数行)の場合、*/が来るまで飛ばす
                    while (nextChar != null && !(currentChar == '*' && nextChar == '/'))
                    {
                        readChar();
                    }
                    readChar();
                    token = new OsToken(OsTokenType.COMMENT, "");
                }
                else
                {
                    // 次がスラッシュでもアスタリスクでもなければ除算
                    token = new OsToken(OsTokenType.SLASH, currentChar.ToString());
                }
                break;
            case '!':
                if (nextChar == '=')
                {
                    token = new OsToken(OsTokenType.NOT_EQ, "!=");
                    readChar();
                }
                else
                {
                    token = new OsToken(OsTokenType.BANG, currentChar.ToString());
                }
                break;
            case '>':
                if (nextChar == '=')
                {
                    token = new OsToken(OsTokenType.GTEQ, ">=");
                    readChar();
                }
                else
                {
                    token = new OsToken(OsTokenType.GT, currentChar.ToString());
                }
                break;
            case '<':
                if (nextChar == '=')
                {
                    token = new OsToken(OsTokenType.LTEQ, "<=");
                    readChar();
                }
                else
                {
                    token = new OsToken(OsTokenType.LT, currentChar.ToString());
                }
                break;
            case ',':
                token = new OsToken(OsTokenType.COMMA, currentChar.ToString());
                break;
            case '"':
                string str = readString();
                token = new OsToken(OsTokenType.STRING, str);
                break;
            case null:
                token = new OsToken(OsTokenType.EOF, "");
                break;
            default:
                if (isLetter(currentChar))
                {
                    string identifier = readIdentifier();
                    OsTokenType type = OsToken.lookupIdentifier(identifier);
                    token = new OsToken(type, identifier);

                    if (type == OsTokenType.ELSE)
                    {
                        // elseの場合に、次にifが続くなら else if とする
                        if (nextNextChar == 'i')
                        {
                            token = new OsToken(OsTokenType.ELSEIF, "elseif");
                            // else ifの終わりまで進める
                            readChar();
                            skipWhiteSpace();
                            readChar();
                        }
                    }
                }
                else if (isDigit(currentChar))
                {
                    string number = readNumber();
                    if (nextChar == '.' && isDigit(nextNextChar))
                    {
                        // ドットがあるなら少数として判断する
                        readChar();

                        // 小数点以下を獲得
                        string dec = readNumber();

                        token = new OsToken(OsTokenType.FLOAT, number + dec);
                    }
                    else
                    {
                        // 整数型
                        token = new OsToken(OsTokenType.INT, number);
                    }
                }
                else
                {
                    token = new OsToken(OsTokenType.ILLEGAL, currentChar.ToString());
                }
                break;
        }

        readChar();
        return token;
    }

    // 1文字読み進める
    private void readChar()
    {
        if (position >= input.Length)
        {
            currentChar = null;
        }
        else
        {
            currentChar = input[position];
        }

        if (position + 1 >= input.Length)
        {
            nextChar = null;
        }
        else
        {
            nextChar = input[position + 1];
        }

        if (position + 2 >= input.Length)
        {
            nextNextChar = null;
        }
        else
        {
            nextNextChar = input[position + 2];
        }

        position += 1;
    }

    // 次の文字がアルファベットまたはアンダースコア以外になるまで読み進めて返却する
    // (この操作を行うことで、識別子または予約語が返却される)
    private string readIdentifier()
    {
        string identifier = currentChar.ToString();

        // 次の文字がアルファベットまたはアンダースコアであればそれを読んで加える
        while (isLetter(nextChar))
        {
            identifier += nextChar;
            readChar();
        }

        return identifier;
    }

    // 引数で与えた文字がアルファベットまたはアンダースコアであること（識別子として使用可能な文字であること）
    private bool isLetter(char? c)
    {
        if (c == null)
        {
            return false;
        }
        return ('a' <= c && c <= 'z')
            || ('A' <= c && c <= 'Z')
            || c == '_';
    }

    // 次の文字が数値以外になるまで読み進めて返却する
    // (この操作を行うことで、数値リテラルが返却される)
    private string readNumber()
    {
        string number = currentChar.ToString();

        // 次の文字が数値であればそれを読んで加える
        while (isDigit(nextChar))
        {
            number += nextChar;
            readChar();
        }

        return number;
    }


    // 引数で与えた文字が数値であること（数値リテラルとして使用可能な文字であること）
    private bool isDigit(char? c)
    {
        if (c == null)
        {
            return false;
        }
        return '0' <= c && c <= '9';
    }

    // スペース、タブ、改行は読み込み不要なので飛ばす
    private void skipWhiteSpace()
    {
        while (currentChar == ' '
            || currentChar == '\t'
            || currentChar == '\r'
            || currentChar == '\n')
        {
            readChar();
        }
    }


    // 文字列を返却する　ダブルクォーテーションが現れるまで読み進めてその中身を返却
    private string readString()
    {
        // 最初のダブルクォーテーションを捨てる
        readChar();
        string str = "";

        // 次の文字がアルファベットまたはアンダースコアであればそれを読んで加える
        while (currentChar != '"' || currentChar == null)
        {
            str += currentChar;
            readChar();
        }

        return str;
    }
}