using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

// プログラムの実行結果を評価して返却する
public class OsEvaluator
{
    // 結果が真偽値の場合の返却値
    public OsBoolean resultTrue = new OsBoolean(true);
    public OsBoolean resultFalse = new OsBoolean(false);
    private OsBoolean toBooleanObject(bool value) => value ? resultTrue : resultFalse;

    // 結果がNULLの場合の返却値
    public OsNull resultNull = new OsNull();

    public OsObject eval(AstNode node, OsEnviroment env)
    {
        switch (node)
        {
            // Root(全体)
            case AstRoot root:
                return evalRoot(root.statements, env);

            // ブロック(中括弧で囲われたエリア)
            case AstBlock block:
                return eval(block.statements, env);

            // FOR文
            case AstFor astFor:
                return evalFor(astFor, env);

            // WHILE文
            case AstWhile astWhile:
                return evalWhile(astWhile, env);

            // IF文
            case AstIf ifExpression:
                return evalIf(ifExpression, env);

            // VAR宣言文
            case AstDecVar var:
                OsObject varValue = eval(var.value, env);
                if (isError(varValue)) return varValue;
                if (env.contains(var.name.value))
                {
                    new OsError($"既に宣言されています。: var {var.name.value}");
                }
                env.set(var.name.value, varValue);
                break;

            // 識別子（変数名など）
            case AstIdentifier identifier:
                return evalIdentifier(identifier, env);

            // 関数
            case AstFunction func:
                OsFunction objFunc = new OsFunction();
                objFunc.parameters = func.parameters;
                objFunc.body = func.body;
                objFunc.env = env;
                objFunc.name = func.name;
                if (env.contains(func.name.value))
                {
                    new OsError($"既に宣言されています。: var {func.name.value}");
                }
                env.set(func.name.value, objFunc);
                return objFunc;

            // 関数呼び出し
            case AstCall callFunc:
                return applyFunction(callFunc, env);

            // 関数呼び出し
            case AstMethod method:
                return applytMethod(method, env);

            // 式文
            case AstExpressionStatement statement:
                return eval(statement.expression, env);

            // 前置演算子
            case AstPrefixExpression prefix:
                OsObject value = eval(prefix.value, env);
                if (isError(value)) return value;
                return evalPrefix(prefix.ope, value);

            // 中置演算式
            case AstInfixExpression infix:
                OsObject left = eval(infix.left, env);
                if (isError(left)) return left;
                OsObject right = eval(infix.right, env);
                if (isError(right)) return right;
                if (infix.left is AstIndex leftidx && leftidx.left is AstIdentifier idxIdent && env.contains(idxIdent.value) && isReassignment(infix.ope))
                {
                    //  宣言済みの配列に対する操作の場合
                    OsObject declared = evalInfix(leftidx, infix.ope, right, idxIdent.value, left, env);
                    if (declared != null)
                    {
                        return declared;
                    }
                    break;

                }
                else if (infix.left is AstIdentifier identleft && env.contains(identleft.value) && isReassignment(infix.ope))
                {
                    // 宣言済みの変数に対する操作の場合
                    OsObject declared = evalInfix(infix.ope, right, identleft.value, left, env);
                    if (declared != null)
                    {
                        return declared;
                    }
                    break;
                }
                return evalInfix(infix.ope, left, right);

            // 整数
            case AstInteger integer:
                return new OsInteger(integer.value);

            // 実数
            case AstFloat flo:
                return new OsFloat(flo.value);

            // 文字列
            case AstString str:
                return new OsString(str.value);

            // 真偽値
            case AstBoolean boolean:
                return toBooleanObject(boolean.value);

            // Return文の返却値
            case AstReturn returnStatement:
                OsObject ret = eval(returnStatement.returnValue, env);
                if (isError(ret))
                {
                    return ret;
                }
                return new OsReturnValue(ret);

            // 配列の返却値
            case AstArray arr:
                OsArray osarry = new OsArray();

                foreach (AstExpression val in arr.values)
                {
                    OsObject arrele = eval(val, env);
                    osarry.values.Add(arrele);
                    if (isError(arrele))
                    {
                        return arrele;
                    }
                }

                return osarry;

            // 配列添え字の返却値
            case AstIndex index:
                OsObject idxLeft = eval(index.left, env);
                if (isError(idxLeft))
                {
                    return idxLeft;
                }
                OsObject idx = eval(index.index, env);
                if (isError(idx))
                {
                    return idx;
                }


                return evalIndex(idxLeft, idx);
        }
        return resultNull;
    }

    private OsObject evalIndex(OsObject left, OsObject idx)
    {
        if (left is OsArray arrLeft && idx is OsInteger intIdx)
        {
            if (intIdx.value < 0 || intIdx.value > arrLeft.values.Count)
            {
                return new OsError($"配列の添え字が不正: {intIdx.value}　最大値{arrLeft.values.Count}");
            }
            return arrLeft.values[intIdx.value];
        }
        else
        {
            return new OsError($"未知の演算子: {left.type()}[{idx.type()}]");
        }
    }

    // 引数で渡された文をすべて実行し、その実行結果を返却する
    private OsObject eval(List<AstStatement> statements, OsEnviroment env)
    {
        OsObject result = null;
        foreach (AstStatement statement in statements)
        {
            // 式を順番に評価していく
            result = eval(statement, env);
            if (result.type() == OsObjectType.RETURN_VALUE || result.type() == OsObjectType.ERROR)
            {
                // return文の場合は中断して返却
                return result;
            }
        }
        return result;
    }

    private OsObject evalRoot(List<AstStatement> statements, OsEnviroment env)
    {
        OsObject result = null;
        foreach (AstStatement statement in statements)
        {
            // 式を順番に評価していく
            result = eval(statement, env);
            if (result is OsReturnValue returnValue)
            {
                // return文の場合は中断して返却
                return returnValue.value;
            }
            if (result is OsError errorValue)
            {
                // エラーの場合は中断して返却
                return errorValue;
            }
        }
        return result;
    }

    // 前置演算子を評価して返却する
    private OsObject evalPrefix(string op, OsObject value)
    {
        switch (op)
        {
            case "!":
                return evalBang(value);
            case "-":
                return evalMinus(value);
        }
        return new OsError($"未知の演算子: {op}{value.type()}");
    }

    // 操作が変数代入用の中間演算子か？
    private bool isReassignment(string ope)
    {
        return ope == "=" || ope == "+=" || ope == "-=" || ope == "*=" || ope == "/=";
    }

    // 中置演算式　宣言済み変数への操作の場合
    private OsObject evalInfix(string ope, OsObject right, string left, OsObject leftValue, OsEnviroment env)
    {
        // 設定値を計算する
        OsObject val = evalInfixValue(ope, right, left, leftValue);

        if (val is OsError)
        {

            // エラーの場合そのまま返却
            return val;
        }
        else
        {
            // エラーがない場合環境変数に設定する
            env.set(left, val);
            return null;
        }
    }

    // 中置演算式　宣言済み変数への操作の返却値設定
    private OsObject evalInfixValue(string ope, OsObject right, string left, OsObject leftValue)
    {
        if (right is OsInteger intRight)
        {
            // 整数の場合
            if (leftValue is OsInteger intLeft)
            {
                // 既存設定値が数値
                switch (ope)
                {
                    case "=":
                        return intRight;
                    case "+=":
                        return new OsInteger(intLeft.value + intRight.value);
                    case "-=":
                        return new OsInteger(intLeft.value - intRight.value);
                    case "*=":
                        return new OsInteger(intLeft.value * intRight.value);
                    case "/=":
                        return new OsInteger(intLeft.value / intRight.value);
                }
            }
            else if (leftValue is OsFloat floLeft)
            {
                // 既存設定値が実数
                switch (ope)
                {
                    case "=":
                        return intRight;
                    case "+=":
                        return new OsFloat(floLeft.value + intRight.value);
                    case "-=":
                        return new OsFloat(floLeft.value - intRight.value);
                    case "*=":
                        return new OsFloat(floLeft.value * intRight.value);
                    case "/=":
                        return new OsFloat(floLeft.value / intRight.value);
                }
            }
            else
            {
                // 既存設定値が数値以外
                switch (ope)
                {
                    case "=":
                        return intRight;
                }
            }
        }
        else if (right is OsFloat floRight)
        {
            // 実数の場合
            if (leftValue is OsFloat floLeft)
            {
                // 既存設定値が数値
                switch (ope)
                {
                    case "=":
                        return floRight;
                    case "+=":
                        return new OsFloat(floLeft.value + floRight.value);
                    case "-=":
                        return new OsFloat(floLeft.value - floRight.value);
                    case "*=":
                        return new OsFloat(floLeft.value * floRight.value);
                    case "/=":
                        return new OsFloat(floLeft.value / floRight.value);
                }
            }
            else if (leftValue is OsInteger intLeft)
            {
                // 既存設定値が数値
                switch (ope)
                {
                    case "=":
                        return floRight;
                    case "+=":
                        return new OsFloat(intLeft.value + floRight.value);
                    case "-=":
                        return new OsFloat(intLeft.value - floRight.value);
                    case "*=":
                        return new OsFloat(intLeft.value * floRight.value);
                    case "/=":
                        return new OsFloat(intLeft.value / floRight.value);
                }
            }
            else
            {
                // 既存設定値が数値以外
                switch (ope)
                {
                    case "=":
                        return floRight;
                }
            }
        }
        else if (right is OsString strRight)
        {
            string strLeft = leftValue == null ? "" : leftValue.ToString();

            // 文字列の場合
            switch (ope)
            {
                case "=":
                    return strRight;
                case "+=":
                    return new OsString(strLeft + strRight.value);
            }
        }
        else if (right is OsBoolean boolRight && leftValue is OsBoolean)
        {
            // 真偽値の場合
            switch (ope)
            {
                case "=":
                    return boolRight;
            }
        }

        return new OsError($"未知の代入式: {right.type()}{ope}{right}");

    }

    // 中置演算式　宣言済み配列への操作の場合
    private OsObject evalInfix(AstIndex index, string ope, OsObject right, string left, OsObject leftValue, OsEnviroment env)
    {
        (OsObject obj, bool ok) result = env.get(left);
        if (result.ok && result.obj is OsArray arr)
        {
            if (index.index is AstInteger intIdx)
            {
                // 設定値を計算する
                OsObject val = evalInfixValue(ope, right, left, leftValue);

                if (val is OsError)
                {
                    // エラーの場合そのまま返却
                    return val;
                }
                else
                {
                    // エラーがない場合環境変数に設定する
                    arr.values[intIdx.value] = val;
                    env.set(left, arr);
                    return null;
                }
            }
            else
            {
                return new OsError($"数値ではありません: {index.toCode()}");
            }
        }
        else
        {
            return new OsError($"配列ではありません: {left}");
        }
    }

    // 中置演算式を評価して返却する　受付
    private OsObject evalInfix(string ope, OsObject left, OsObject right)
    {
        if (left is OsInteger intLeft && right is OsInteger intRight)
        {
            // 整数の場合
            return evalIntegerInfix(ope, intLeft, intRight);
        }

        else if (left is OsFloat)
        {   // 実数の場合
            OsFloat floRight;
            if (right is OsFloat)
            {
                floRight = (OsFloat)right;
            }
            else if (right is OsInteger)
            {
                floRight = new OsFloat(((OsInteger)right).value);
            }
            else
            {
                return new OsError($"未知の演算子: {left.type()}{ope}{right.type()}");
            }
            return evalFloatInfix(ope, (OsFloat)left, floRight);
        }

        else if (right is OsFloat)
        {   // 実数の場合
            OsFloat floLeft;
            if (left is OsFloat)
            {
                floLeft = (OsFloat)left;
            }
            else if (left is OsInteger)
            {
                floLeft = new OsFloat(((OsInteger)left).value);
            }
            else
            {
                return new OsError($"未知の演算子: {left.type()}{ope}{right.type()}");
            }
            return evalFloatInfix(ope, floLeft, (OsFloat)right);
        }

        else if (left is OsString strLeft && right is OsString strRight)
        {
            // 文字列の場合、後述の==、!=に加えて、+を許可
            switch (ope)
            {
                case "+":
                    return new OsString(strLeft.value + strRight.value);
            }
        }

        // 整数以外は==、!=のみ（比較はサポートしない）
        switch (ope)
        {
            case "==":
                return toBooleanObject(left == null ? right == null : left.ToString().Equals(right.ToString()));
            case "!=":
                return toBooleanObject(!(left == null ? right == null : left.ToString().Equals(right.ToString())));
        }

        return new OsError($"未知の演算子: {left.type()}{ope}{right.type()}");
    }

    // 中置演算式(整数型)の評価して返却する
    private OsObject evalIntegerInfix(string ope, OsInteger left, OsInteger right)
    {
        int leftValue = left.value;
        int rightValue = right.value;

        switch (ope)
        {
            case "+":
                return new OsInteger(leftValue + rightValue);
            case "-":
                return new OsInteger(leftValue - rightValue);
            case "*":
                return new OsInteger(leftValue * rightValue);
            case "/":
                return new OsInteger(leftValue / rightValue);
            case "<":
                return toBooleanObject(leftValue < rightValue);
            case ">":
                return toBooleanObject(leftValue > rightValue);
            case "<=":
                return toBooleanObject(leftValue <= rightValue);
            case ">=":
                return toBooleanObject(leftValue >= rightValue);
            case "==":
                return toBooleanObject(leftValue == rightValue);
            case "!=":
                return toBooleanObject(leftValue != rightValue);
        }

        if (left.type() != right.type())
        {
            return new OsError($"型のミスマッチ: {left.type()} {ope} {right.type()}");
        }

        return new OsError($"未知の演算子: {left.type()}{ope}{right.type()}");
    }

    // 中置演算式(実数型)の評価して返却する
    private OsObject evalFloatInfix(string ope, OsFloat left, OsFloat right)
    {
        float leftValue = left.value;
        float rightValue = right.value;

        switch (ope)
        {
            case "+":
                return new OsFloat(leftValue + rightValue);
            case "-":
                return new OsFloat(leftValue - rightValue);
            case "*":
                return new OsFloat(leftValue * rightValue);
            case "/":
                return new OsFloat(leftValue / rightValue);
            case "<":
                return toBooleanObject(leftValue < rightValue);
            case ">":
                return toBooleanObject(leftValue > rightValue);
            case "<=":
                return toBooleanObject(leftValue <= rightValue);
            case ">=":
                return toBooleanObject(leftValue >= rightValue);
            case "==":
                return toBooleanObject(leftValue == rightValue);
            case "!=":
                return toBooleanObject(leftValue != rightValue);
        }

        if (left.type() != right.type())
        {
            return new OsError($"型のミスマッチ: {left.type()} {ope} {right.type()}");
        }

        return new OsError($"未知の演算子: {left.type()}{ope}{right.type()}");
    }

    // !を評価して返却する　値を反転させる
    private OsObject evalBang(OsObject value)
    {
        if (value == resultTrue)
        {
            return resultFalse;
        }

        if (value == resultFalse)
        {
            return resultTrue;
        }

        if (value == resultNull)
        {
            // 値がないなら反転させて、値がある(True)
            return resultTrue;
        }

        // 値があるなら反転させて、値がない(False)
        return resultFalse;
    }

    // マイナスを評価して返却する
    private OsObject evalMinus(OsObject value)
    {
        if (value is OsInteger intValue)
        {
            // 整数の場合
            return new OsInteger(-intValue.value);
        }
        else if (value is OsFloat floValue)
        {
            // 実数の場合
            return new OsFloat(-floValue.value);
        }

        return new OsError($"未知の演算子: -{value}");
    }

    // If文を評価して返却する
    private OsObject evalIf(AstIf ifExpression, OsEnviroment env)
    {
        for (int i = 0; i < ifExpression.conditions.Count; i++)
        {
            OsObject condition = eval(ifExpression.conditions[i], env);
            if (isError(condition))
            {
                return condition;
            }
            if (isTruthly(condition))
            {
                return eval(ifExpression.consequences[i].statements, env);
            }
        }

        if (ifExpression.alternative != null)
        {
            return eval(ifExpression.alternative.statements, env);
        }

        return resultNull;
    }

    // For文を評価して返却する
    private OsObject evalFor(AstFor astFor, OsEnviroment env)
    {
        // 初期処理を実行
        OsObject init = eval(astFor.init, env);
        if (isError(init))
        {
            return init;
        }

        while (isTruthly(eval(astFor.condition, env)))
        {
            eval(astFor.statements, env);
            // 1ループ終了時処理
            OsObject next = eval(astFor.next, env);
            if (isError(next))
            {
                return next;
            }
        }

        return resultNull;
    }

    // While文を評価して返却する
    private OsObject evalWhile(AstWhile astWhile, OsEnviroment env)
    {
        while (isTruthly(eval(astWhile.condition, env)))
        {
            eval(astWhile.statements, env);
        }

        return resultNull;
    }

    // 引数は真偽値が来る前提として、その真偽値を評価して返却する
    private bool isTruthly(OsObject obj)
    {
        if (obj == resultTrue) return true;
        if (obj == resultFalse) return false;
        if (obj == resultNull) return false;
        return true;
    }

    private bool isFalsey(OsObject obj)
    {
        return !isTruthly(obj);
    }

    // エラーが発生しているかどうかを判定する
    private bool isError(OsObject obj)
    {
        if (obj != null) return obj.type() == OsObjectType.ERROR;
        return false;
    }

    // 宣言済みの変数から値を取り出して返却する
    private OsObject evalIdentifier(AstIdentifier identifier, OsEnviroment env)
    {
        var (value, ok) = env.get(identifier.value);
        if (ok) return value;
        return new OsError($"識別子が見つかりません。: {identifier.value}");
    }

    // 引数(arguments)の値をevalして返却する
    private List<OsObject> evalExpressions(List<AstExpression> arguments, OsEnviroment env)
    {
        List<OsObject> result = new List<OsObject>();

        foreach (AstExpression arg in arguments)
        {
            OsObject evaluated = eval(arg, env);
            if (isError(evaluated))
                return new List<OsObject>() { evaluated };
            result.Add(evaluated);
        }

        return result;
    }

    // 関数の呼び出しを行う
    private OsObject applyFunction(AstCall callFunc, OsEnviroment env)
    {
        OsObject func = eval(callFunc.functionName, env);
        if (isError(func))
        {
            return func;
        }
        List<OsObject> args = evalExpressions(callFunc.arguments, env);
        if (args != null && args.Count > 0 && isError(args[0]))
        {
            return args[0];
        }

        // 実行結果
        OsObject evaluated = resultNull;

        if (func is OsFunction)
        {
            // ユーザー定義の関数の場合
            OsFunction fn = (OsFunction)func;

            // 関数用の環境を作成
            OsEnviroment extendedEnviroment = extendEnviroment(fn, args);
            // 実行
            evaluated = eval(fn.body, extendedEnviroment);
        }
        else if (func is OstBuiltinFunctions)
        {
            // 組み込み関数の場合
            OstBuiltinFunctions fn = (OstBuiltinFunctions)func;
            if (!fn.checkArgs(args))
            {
                return new OsError($"{callFunc.functionName}の引数が不正です。");
            }
            // 実行
            evaluated = fn.eval(args);
        }
        else
        {
            return new OsError($"function ではありません。: {callFunc.functionName}");
        }

        // return文でかえってきた場合、中身を返却する
        if (evaluated is OsReturnValue returnValue)
        {
            return returnValue.value;
        }
        return evaluated;
    }

    // メソッドの呼び出しを行う
    private OsObject applytMethod(AstMethod method, OsEnviroment env)
    {
        (OsObject func, bool ok) result = env.get(method.left.value);
        if (!result.ok)
        {
            return new OsError($"解決できません。: {method.left.value}");
        }

        if (result.func is OstBuiltinFunctions bfunc)
        {
            // 型情報を取得
            Type type = result.func.GetType();
            // メソッド情報を取得
            MethodInfo methodInfo = type.GetMethod(method.right.value);
            List<OsObject> args = evalExpressions(method.arguments, env);
            if (methodInfo != null)
            {
                return (OsObject)methodInfo.Invoke(result.func, new object[] { args });
            }
            else
            {
                return new OsError($"メソッドが見つかりません。: {method.right.value}");
            }
        }
        else
        {
            return new OsError($"解決できません。: {method.left.value}");
        }
    }

    // 関数用の環境を作成する
    private OsEnviroment extendEnviroment(OsFunction fn, List<OsObject> args)
    {
        // 外部環境を引き継いだ環境を作成
        OsEnviroment enviroment = OsEnviroment.createNewEnclosedEnviroment(fn.env);

        for (int i = 0; i < fn.parameters.Count; i++)
        {
            // パラメータを環境に設定する
            enviroment.set(fn.parameters[i].value, args[i]);
        }

        return enviroment;
    }
}