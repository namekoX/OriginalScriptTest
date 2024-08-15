

using System.Collections.Generic;

public class OsToken
{
    public OsTokenType type;
    public string literal;

    public OsToken(OsTokenType type, string literal)
    {
        this.type = type;
        this.literal = literal;
    }

    // 引数で与えられた文字列が予約語または識別子であるかを判断する
    public static OsTokenType lookupIdentifier(string identifier)
    {
        if (OsKeywords.ContainsKey(identifier))
        {
            return OsKeywords[identifier];
        }
        return OsTokenType.IDENT;
    }

    // キーワード（システム予約語）
    public static Dictionary<string, OsTokenType> OsKeywords
        = new Dictionary<string, OsTokenType>() {
        { "var", OsTokenType.DECVAR },
        { "float", OsTokenType.DECFLOAT },
        { "int", OsTokenType.DECINT },
        { "string", OsTokenType.DECSTRING},
        { "function", OsTokenType.FUNCTION },
        { "if", OsTokenType.IF },
        { "elseif", OsTokenType.ELSEIF },
        { "else", OsTokenType.ELSE },
        { "return", OsTokenType.RETURN },
        { "true", OsTokenType.TRUE },
        { "false", OsTokenType.FALSE },
        { "for", OsTokenType.FOR },
        { "while", OsTokenType.WHILE },
    };

}

public enum OsTokenType
{
    ILLEGAL, // 不正なトークン
    EOF, // 終端

    // リテラル
    IDENT, // 識別子
    INT, // 整数リテラル
    FLOAT, // 整数リテラル
    STRING, // 文字列リテラル

    // 演算子
    ASSIGN, // =
    PLUS, // +
    PLUSEQ, // +=
    MINUS, // -
    MINUSEQ, // -=
    ASTERISK, // *
    ASTERISKEQ, // *=
    SLASH, // /
    SLASHEQ, // /=
    BANG, // !
    LT,　// <
    LTEQ,　// <=
    GT, // >
    GTEQ, // >=
    EQ, // ==
    NOT_EQ, // !=
    LBRACKET, //  [
    RBRACKET, //  ]
    DOT, //  .

    // デリミタ
    COMMA, // ,
    SEMICOLON, // ;

    // 括弧(){}
    LPAREN, // (
    RPAREN, // )
    LBRACE, // {
    RBRACE, // }

    // キーワード（システム予約語）
    FUNCTION,
    DECVAR, // variant変数の宣言
    DECFLOAT, // flat変数の宣言
    DECINT, // int変数の宣言
    DECSTRING, // string変数の宣言
    IF,
    ELSE,
    ELSEIF,
    RETURN,
    TRUE,
    FALSE,
    FOR,
    WHILE,

    // コメント
    COMMENT,

}