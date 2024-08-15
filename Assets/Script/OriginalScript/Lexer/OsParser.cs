using System.Collections;
using System.Collections.Generic;
using System;
using PrefixParseFn = System.Func<AstExpression>;
using InfixParseFn = System.Func<AstExpression, AstExpression>;


// トークン列を入力として受け取り、抽象構文木を生成するクラス
public class OsParser
{
    public OsToken currentToken;
    public OsToken nextToken;
    public OsLexer lexer;

    // エラー
    public List<string> errors = new List<string>();

    // 前置構文解析関数
    // 演算子が前に来るもの　例えば - 1、!trueなど
    private Dictionary<OsTokenType, PrefixParseFn> prefixParseFns = new Dictionary<OsTokenType, PrefixParseFn>();

    // 中置構文解析関数
    // 演算子が中間に来るもの　例えば 1 + 1、1 != 2など
    private Dictionary<OsTokenType, InfixParseFn> infixParseFns = new Dictionary<OsTokenType, InfixParseFn>();

    // 式を計算するときの優先順位
    // 例えば + と * では*を先に計算しないといけない、そのため何を優先的に計算するかを順序付ける必要がある
    private enum Precedence
    {
        LOWEST = 1,
        ASSIGN,      // =
        EQUALS,      // ==
        LESSGREATER, // >, <
        SUM,         // +
        PRODUCT,     // *
        PREFIX,      // -x, !x
        DOT,        // .
        CALL,        // myFunction(x)
        INDEX,       // 配列の添え字 myArray[x]
    }

    // トークンと優先順位の紐づけを定義
    private Dictionary<OsTokenType, Precedence> precedences { get; set; } =
        new Dictionary<OsTokenType, Precedence>()
        {
                { OsTokenType.EQ, Precedence.EQUALS },
                { OsTokenType.NOT_EQ, Precedence.EQUALS },
                { OsTokenType.LT, Precedence.LESSGREATER },
                { OsTokenType.GT, Precedence.LESSGREATER },
                { OsTokenType.PLUS, Precedence.SUM },
                { OsTokenType.MINUS, Precedence.SUM },
                { OsTokenType.SLASH, Precedence.PRODUCT },
                { OsTokenType.ASTERISK, Precedence.PRODUCT },
                { OsTokenType.LPAREN, Precedence.CALL },
                { OsTokenType.ASSIGN, Precedence.ASSIGN },
                { OsTokenType.PLUSEQ, Precedence.ASSIGN },
                { OsTokenType.MINUSEQ, Precedence.ASSIGN },
                { OsTokenType.SLASHEQ, Precedence.ASSIGN },
                { OsTokenType.ASTERISKEQ, Precedence.ASSIGN },
                { OsTokenType.LBRACKET, Precedence.INDEX },
                { OsTokenType.DOT, Precedence.DOT },
        };

    // 現在のトークンの優先順位を取得
    private Precedence currentPrecedence
    {
        get
        {
            if (precedences.TryGetValue(currentToken.type, out Precedence p))
            {
                return p;
            }
            return Precedence.LOWEST;
        }
    }

    // 次のトークンの優先順位を取得
    private Precedence nextPrecedence
    {
        get
        {
            if (precedences.TryGetValue(nextToken.type, out var p))
            {
                return p;
            }
            return Precedence.LOWEST;
        }
    }


    // コンストラクタ 読み取り対象のトークン列を指定
    public OsParser(OsLexer lexer)
    {
        this.lexer = lexer;

        // 2つ分のトークンを読み込んでセットしておく
        currentToken = lexer.getNextToken();
        nextToken = lexer.getNextToken();

        // トークンの種類と構文解析関数を紐づける
        registerPrefixParseFns();
        registerInfixParseFns();
    }

    // プログラムをParseする。抽象構文木ルートノードを生成して返却する
    public AstRoot parseProgram()
    {
        // ルートノードを生成
        AstRoot root = new AstRoot();
        root.statements = new List<AstStatement>();

        // トークンを抽象構文木に設定していく
        while (currentToken.type != OsTokenType.EOF)
        {
            // 構文の場合
            AstStatement statement = parseStatement();
            if (statement != null)
            {
                root.statements.Add(statement);
            }

            // トークンを読み進める
            readToken();
        }
        return root;
    }

    // トークンの種類と前置構文解析関数を紐づける
    private void registerPrefixParseFns()
    {
        // 識別子
        prefixParseFns.Add(OsTokenType.IDENT, parseIdentifier);
        // 整数リテラル
        prefixParseFns.Add(OsTokenType.INT, parseInteger);
        // 実数リテラル
        prefixParseFns.Add(OsTokenType.FLOAT, parseFloat);
        // 文字列リテラル
        prefixParseFns.Add(OsTokenType.STRING, parseString);
        // 否定(!)
        prefixParseFns.Add(OsTokenType.BANG, parsePrefixExpression);
        // マイナス
        prefixParseFns.Add(OsTokenType.MINUS, parsePrefixExpression);
        // 真偽値
        prefixParseFns.Add(OsTokenType.TRUE, parseBoolean);
        prefixParseFns.Add(OsTokenType.FALSE, parseBoolean);
        // 括弧
        prefixParseFns.Add(OsTokenType.LPAREN, parseGrouped);
        // IF
        prefixParseFns.Add(OsTokenType.IF, parseIf);
        // 関数
        prefixParseFns.Add(OsTokenType.FUNCTION, parseFunction);
        // 配列
        prefixParseFns.Add(OsTokenType.LBRACKET, parseArray);
    }

    // トークンの種類と中間構文解析関数を紐づける
    private void registerInfixParseFns()
    {
        // プラス
        infixParseFns.Add(OsTokenType.PLUS, parseInfixExpression);
        infixParseFns.Add(OsTokenType.PLUSEQ, parseInfixExpression);
        // マイナス
        infixParseFns.Add(OsTokenType.MINUS, parseInfixExpression);
        infixParseFns.Add(OsTokenType.MINUSEQ, parseInfixExpression);
        // スラッシュ(割算)
        infixParseFns.Add(OsTokenType.SLASH, parseInfixExpression);
        infixParseFns.Add(OsTokenType.SLASHEQ, parseInfixExpression);
        // アスタリスク(掛算)
        infixParseFns.Add(OsTokenType.ASTERISK, parseInfixExpression);
        infixParseFns.Add(OsTokenType.ASTERISKEQ, parseInfixExpression);
        // イコール
        infixParseFns.Add(OsTokenType.EQ, parseInfixExpression);
        infixParseFns.Add(OsTokenType.ASSIGN, parseInfixExpression);
        // ノットイコール
        infixParseFns.Add(OsTokenType.NOT_EQ, parseInfixExpression);
        // <
        infixParseFns.Add(OsTokenType.LT, parseInfixExpression);
        infixParseFns.Add(OsTokenType.LTEQ, parseInfixExpression);
        // >
        infixParseFns.Add(OsTokenType.GT, parseInfixExpression);
        infixParseFns.Add(OsTokenType.GTEQ, parseInfixExpression);
        // 関数呼び出し式
        infixParseFns.Add(OsTokenType.LPAREN, parseCall);
        // 配列
        infixParseFns.Add(OsTokenType.LBRACKET, parseIndex);
        // ドット
        infixParseFns.Add(OsTokenType.DOT, parseMethod);
    }

    // 識別子用の構文解析関数
    private AstExpression parseIdentifier()
    {
        return new AstIdentifier(currentToken, currentToken.literal);
    }

    // 整数リテラル用の構文解析関数
    private AstExpression parseInteger()
    {
        // リテラルを整数値に変換
        if (int.TryParse(currentToken.literal, out int result))
        {
            return new AstInteger(currentToken, result);
        }

        // 型変換失敗時
        errors.Add($"{currentToken.literal} を数値に変換できません。");
        return null;
    }

    // 実数リテラル用の構文解析関数
    private AstExpression parseFloat()
    {
        // リテラルを整数値に変換
        if (float.TryParse(currentToken.literal, out float result))
        {
            return new AstFloat(currentToken, result);
        }

        // 型変換失敗時
        errors.Add($"{currentToken.literal} を数値に変換できません。");
        return null;
    }

    // 文字列用の構文解析関数
    private AstExpression parseString()
    {
        return new AstString(currentToken, currentToken.literal);
    }

    // 真偽値用の構文解析関数
    private AstExpression parseBoolean()
    {
        return new AstBoolean(currentToken, currentToken.type == OsTokenType.TRUE);
    }

    // 括弧用の構文解析関数
    private AstExpression parseGrouped()
    {
        // "(" を読み飛ばす
        readToken();

        // 括弧内の式を解析する
        AstExpression expression = parseExpression(Precedence.LOWEST);

        // 閉じ括弧 ")" がないとエラーになる
        if (!expectPeek(OsTokenType.RPAREN))
        {
            return null;
        }

        return expression;
    }

    // If文の構文解析関数
    private AstExpression parseIf()
    {
        AstIf expression = new AstIf();
        expression.token = currentToken;

        // if の後は括弧 ( でなければならない
        if (!expectPeek(OsTokenType.LPAREN))
        {
            return null;
        }

        // 括弧 ( を読み飛ばす
        readToken();

        // ifの条件式を解析する
        expression.conditions.Add(parseExpression(Precedence.LOWEST));

        // 閉じ括弧 ) 中括弧が続く 
        if (!expectPeek(OsTokenType.RPAREN))
        {
            return null;
        }
        if (!expectPeek(OsTokenType.LBRACE))
        {
            return null;
        }

        // この時点で { が現在のトークン
        // ブロック文の解析関数を呼ぶ
        expression.consequences.Add(parseBlockStatement());

        // else if句があれば解析する
        while (nextToken.type == OsTokenType.ELSEIF)
        {
            // else if 読み進める
            readToken();

            // else if の後は括弧 ( でなければならない
            if (!expectPeek(OsTokenType.LPAREN))
            {
                return null;
            }

            // 括弧 ( を読み飛ばす
            readToken();

            // else ifの条件式を解析する
            expression.conditions.Add(parseExpression(Precedence.LOWEST));

            // 閉じ括弧 ) 中括弧が続く 
            if (!expectPeek(OsTokenType.RPAREN))
            {
                return null;
            }
            if (!expectPeek(OsTokenType.LBRACE))
            {
                return null;
            }

            // この時点で { が現在のトークン
            // ブロック文の解析関数を呼ぶ
            expression.consequences.Add(parseBlockStatement());
        }


        // else句があれば解析する
        if (nextToken.type == OsTokenType.ELSE)
        {
            // else 読み進める
            readToken();

            // else の後に { が続かなければならない
            if (!expectPeek(OsTokenType.LBRACE))
            {
                return null;
            }

            // この時点で { が現在のトークン
            // ブロック文の解析関数を呼ぶ
            expression.alternative = parseBlockStatement();
        }

        return expression;
    }

    // ブロック文用({ })の構文解析関数
    private AstBlock parseBlockStatement()
    {
        AstBlock block = new AstBlock();
        block.token = currentToken;
        block.statements = new List<AstStatement>();

        // "{" を読み飛ばす
        readToken();

        // "}" が出現するか文の最後になるまで中身とする
        while (currentToken.type != OsTokenType.RBRACE && currentToken.type != OsTokenType.EOF)
        {
            // ブロックの中身を解析する
            AstStatement statement = parseStatement();
            if (statement != null)
            {
                block.statements.Add(statement);
            }
            readToken();
        }

        return block;
    }

    // For文用の構文解析関数
    private AstFor parseFor()
    {
        AstFor astfor = new AstFor();
        astfor.token = currentToken;

        // for の後は括弧 ( でなければならない
        if (!expectPeek(OsTokenType.LPAREN))
        {
            return null;
        }

        // 括弧 ( を読み飛ばす
        readToken();

        // 変数初期化処理
        astfor.init = parseStatement();

        // セミコロンは不要なので読み飛ばす
        readToken();

        // 継続条件
        astfor.condition = parseExpression(Precedence.LOWEST);

        // セミコロンは不要なので読み飛ばす
        readToken();
        readToken();

        // 1ループ終了時の処理
        astfor.next = parseExpression(Precedence.LOWEST);

        // 1ループ終了時処理 の後は括弧 ）でなければならない
        if (!expectPeek(OsTokenType.RPAREN))
        {
            return null;
        }

        // 括弧 )を読み飛ばす
        readToken();

        astfor.statements = parseBlockStatement();

        return astfor;
    }

    // while文用の構文解析関数
    private AstWhile parseWhile()
    {
        AstWhile astWhile = new AstWhile();
        astWhile.token = currentToken;

        // while の後は括弧 ( でなければならない
        if (!expectPeek(OsTokenType.LPAREN))
        {
            return null;
        }

        // 括弧 ( を読み飛ばす
        readToken();

        // 継続条件
        astWhile.condition = parseExpression(Precedence.LOWEST);

        // 1ループ終了時処理 の後は括弧 ）でなければならない
        if (!expectPeek(OsTokenType.RPAREN))
        {
            return null;
        }

        // 括弧 )を読み飛ばす
        readToken();

        astWhile.statements = parseBlockStatement();

        return astWhile;
    }

    // 関数の構文解析関数
    private AstExpression parseFunction()
    {
        AstFunction fn = new AstFunction();
        fn.token = currentToken;

        readToken();

        fn.name = new AstIdentifier(currentToken, currentToken.literal);

        // function の後は括弧 ( でなければならない
        if (!expectPeek(OsTokenType.LPAREN))
        {
            return null;
        }

        // パラメータをパース
        fn.parameters = parseParameters();

        // function の後は括弧 ) でなければならない
        if (!expectPeek(OsTokenType.LBRACE))
        {
            return null;
        }

        // ブロックの中身を解析する
        fn.body = parseBlockStatement();

        return fn;
    }

    // パラメータの構文解析関数
    private List<AstIdentifier> parseParameters()
    {
        List<AstIdentifier> parameters = new List<AstIdentifier>();

        // () となってパラメータがないときは空のリストを返す
        if (nextToken.type == OsTokenType.RPAREN)
        {
            readToken();
            return parameters;
        }

        // ( を読み飛ばす
        readToken();

        // var を読み飛ばす
        readToken();

        // 最初のパラメータ
        parameters.Add(new AstIdentifier(currentToken, currentToken.literal));

        // 2つ目以降のパラメータをカンマが続く限り処理する
        while (nextToken.type == OsTokenType.COMMA)
        {
            // すでに処理した識別子とその後ろのカンマを飛ばす
            readToken();
            readToken();

            // var を読み飛ばす
            readToken();

            // 識別子を処理
            parameters.Add(new AstIdentifier(currentToken, currentToken.literal));
        }

        if (!expectPeek(OsTokenType.RPAREN))
        {
            return null;
        }

        return parameters;
    }

    // 関数呼び出し式の構文解析関数
    private AstExpression parseCall(AstExpression fn)
    {
        AstCall expression = new AstCall();
        expression.token = currentToken;
        expression.functionName = fn;
        expression.arguments = parseCallArguments();
        return expression;
    }

    // 関数引数の構文解析関数
    private List<AstExpression> parseCallArguments()
    {
        List<AstExpression> args = new List<AstExpression>();

        // ( を読み飛ばす
        readToken();

        // 引数なしの場合
        if (currentToken.type == OsTokenType.RPAREN)
        {
            return args;
        }

        // 引数ありの場合は1つ目の引数を解析
        args.Add(parseExpression(Precedence.LOWEST));

        // 2つ目以降の引数があればそれを解析
        while (nextToken.type == OsTokenType.COMMA)
        {
            // カンマ直前のトークンとカンマトークンを読み飛ばす
            readToken();
            readToken();
            args.Add(parseExpression(Precedence.LOWEST));
        }

        // 閉じかっこがなければエラー
        if (!expectPeek(OsTokenType.RPAREN))
        {
            return null;
        }

        return args;
    }

    // 前置演算子用の構文解析関数
    private AstExpression parsePrefixExpression()
    {
        AstPrefixExpression expression = new AstPrefixExpression();
        expression.token = currentToken;
        expression.ope = currentToken.literal;

        readToken();

        expression.value = parseExpression(Precedence.PREFIX);
        return expression;
    }

    // 中間演算子用の構文解析関数
    private AstExpression parseInfixExpression(AstExpression left)
    {
        AstInfixExpression expression = new AstInfixExpression();
        expression.token = currentToken;
        expression.ope = currentToken.literal;
        expression.left = left;

        Precedence precedence = currentPrecedence;
        readToken();

        expression.right = parseExpression(precedence);
        return expression;
    }

    // トークンを1つ読み進める
    private void readToken()
    {
        currentToken = nextToken;
        nextToken = lexer.getNextToken();
    }


    // 構文の場合の振り分け処理
    private AstStatement parseStatement()
    {
        switch (currentToken.type)
        {
            case OsTokenType.DECVAR:
                return parseDeclaration(OsTokenType.DECVAR);
            case OsTokenType.DECFLOAT:
                return parseDeclaration(OsTokenType.DECFLOAT);
            case OsTokenType.DECINT:
                return parseDeclaration(OsTokenType.DECINT);
            case OsTokenType.DECSTRING:
                return parseDeclaration(OsTokenType.DECSTRING);
            case OsTokenType.FOR:
                return parseFor();
            case OsTokenType.WHILE:
                return parseWhile();
            case OsTokenType.RETURN:
                return parseReturn();
            default:
                return parseExpressionStatement();
        }
    }

    // 式文の場合
    private AstExpressionStatement parseExpressionStatement()
    {
        AstExpressionStatement statement = new AstExpressionStatement();
        statement.token = currentToken;

        statement.expression = parseExpression(Precedence.LOWEST);

        // セミコロンは不要なので読み飛ばす
        ignoreSemiColon();

        return statement;
    }

    // 式の場合のパース処理
    private AstExpression parseExpression(Precedence precedence)
    {
        prefixParseFns.TryGetValue(currentToken.type, out PrefixParseFn prefix);
        if (prefix == null)
        {
            errors.Add($"{currentToken.literal} に関連付けられた Prefix Parse Function が存在しません。");
            return null;
        }
        AstExpression left = prefix();

        // 優先順位を考慮して格納を行う
        while (nextToken.type != OsTokenType.SEMICOLON && precedence < nextPrecedence)
        {
            infixParseFns.TryGetValue(nextToken.type, out InfixParseFn infix);
            if (infix == null)
            {
                return left;
            }

            readToken();
            left = infix(left);
        }

        return left;
    }

    // 変数宣言文の場合
    private AstDecVar parseDeclaration(OsTokenType type)
    {
        AstDecVar statement;

        // 宣言される型によって作成するクラスを変更
        switch (type)
        {
            case OsTokenType.DECFLOAT:
                statement = new AstDecFloat();
                break;
            case OsTokenType.DECINT:
                statement = new AstDecInt();
                break;
            case OsTokenType.DECSTRING:
                statement = new AstDecString();
                break;
            default:
                statement = new AstDecVar();
                break;
        }

        statement.token = currentToken;

        // 識別子(var文の左辺)
        if (!expectPeek(OsTokenType.IDENT))
        {
            return null;
        }
        statement.name = new AstIdentifier(currentToken, currentToken.literal);

        // 次のトークンセミコロンなら代入は無し
        if (nextToken.type == OsTokenType.SEMICOLON)
        {
            readToken();
            return statement;
        }

        // 次のトークン[なら代入無しの配列
        if (nextToken.type == OsTokenType.LBRACKET)
        {
            // [ の次まで行く
            readToken();
            readToken();

            AstArray arr = new AstArray();

            // 配列要素数を指定
            AstExpression size = parseExpression(Precedence.LOWEST);
            if (!(size is AstInteger))
            {
                errors.Add("配列の要素数を指定してください");
                return null;
            }

            // 空の配列を作成
            List<AstExpression> list = new List<AstExpression>(new AstExpression[((AstInteger)size).value]);
            arr.values = list;
            statement.value = arr;

            // ] を飛ばす
            if (!expectPeek(OsTokenType.RBRACKET))
            {
                return null;
            }

            // セミコロンを飛ばす
            ignoreSemiColon();

            return statement;
        }

        // 等号( = )　使用しないので読み飛ばす
        if (!expectPeek(OsTokenType.ASSIGN))
        {
            return null;
        }

        // 式(var文の右辺)
        readToken();
        statement.value = parseExpression(Precedence.LOWEST);

        // 型と値が一致しているのか確認する
        if (!statement.check())
        {
            errors.Add($"型が不一致です {statement.value.toCode()}");
            return null;
        }

        // セミコロンは不要なので読み飛ばす
        ignoreSemiColon();

        return statement;
    }

    // returnの場合
    private AstReturn parseReturn()
    {
        AstReturn statement = new AstReturn();
        statement.token = currentToken;
        readToken();

        statement.returnValue = parseExpression(Precedence.LOWEST);

        // セミコロンは不要なので読み飛ばす
        ignoreSemiColon();

        return statement;
    }

    // 配列の場合
    private AstArray parseArray()
    {
        AstArray arr = new AstArray();
        arr.token = currentToken;
        arr.values = parseExpressionList();

        return arr;

    }

    // 配列の中身を解析する関数
    private List<AstExpression> parseExpressionList()
    {
        List<AstExpression> list = new List<AstExpression>();

        // [ を読み飛ばす
        readToken();

        // 引数なしの場合
        if (currentToken.type == OsTokenType.RBRACKET)
        {
            return list;
        }

        // 引数ありの場合は1つ目の引数を解析
        list.Add(parseExpression(Precedence.LOWEST));

        // 2つ目以降の引数があればそれを解析
        while (nextToken.type == OsTokenType.COMMA)
        {
            // カンマ直前のトークンとカンマトークンを読み飛ばす
            readToken();
            readToken();
            list.Add(parseExpression(Precedence.LOWEST));
        }

        // ]がなければエラー
        if (!expectPeek(OsTokenType.RBRACKET))
        {
            return null;
        }

        return list;
    }

    // 配列添え字の場合
    private AstIndex parseIndex(AstExpression left)
    {
        AstIndex idx = new AstIndex();
        idx.left = left;

        // [を飛ばす
        readToken();

        idx.index = parseExpression(Precedence.LOWEST);

        // ]がなければエラー
        if (!expectPeek(OsTokenType.RBRACKET))
        {
            return null;
        }

        return idx;

    }

    // メソッドの場合
    private AstMethod parseMethod(AstExpression left)
    {
        AstMethod method = new AstMethod();
        if (left is AstIdentifier idLeft)
        {
            method.left = idLeft;
        }
        else
        {
            errors.Add($"{left.toCode()} が識別子ではありません。");
        }

        // .を飛ばす
        readToken();

        AstExpression right = parseExpression(Precedence.LOWEST);

        if (right is AstCall clRight)
        {
            method.right = (AstIdentifier)clRight.functionName;
            method.arguments = clRight.arguments;
        }
        else
        {
            errors.Add($"{right.toCode()} が識別子ではありません。");
        }

        return method;
    }

    // 次のトークンを確認し、使用する物であれば2重に読み込むのを防ぐために読み進めてしまう
    private bool expectPeek(OsTokenType type)
    {
        // 次のトークンが期待するものであれば読み進める
        if (nextToken.type == type)
        {
            readToken();
            return true;
        }
        addNextTokenError(type, nextToken.type);
        return false;
    }

    // 次のトークンが想定外だった場合のエラー
    private void addNextTokenError(OsTokenType expected, OsTokenType actual)
    {
        errors.Add($"{actual} ではなく {expected} が来なければなりません。");
    }

    // セミコロンを読み飛ばす処理
    private void ignoreSemiColon()
    {
        if (nextToken.type == OsTokenType.SEMICOLON)
        {
            readToken();
        }
    }
}