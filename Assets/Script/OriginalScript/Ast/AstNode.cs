
using System;
using UnityEngine;

// 抽象構文木Nodeのインターフェース
public interface AstNode
{
    // リテラルを返却
    string tokenLiteral();
    // コードを復元する
    string toCode();
}