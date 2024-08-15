using System.Collections;
using System.IO;
using UnityEngine;


public class Test : MonoBehaviour
{

    void Start()
    {
        // ゲームのルートフォルダの下にある TestData\test.txt ファイルのパスを取得
        string filePath = Path.Combine(Application.dataPath, "../TestData/test.txt");

        // UTF-8エンコーディングでファイルの内容を読み込む
        string fileContents = readTextFile(filePath);

        // スクリプト実行準備
        OsEnviroment env = new OsEnviroment();
        OriginalScript os = new OriginalScript(fileContents, env);
        os.run();

        // スクリプト内にあるdebugLog関数を実行
        os.changeScript(@"debugLog(""ひろゆき"" , 1 , 1)");
        OsObject ret = os.run();

        // 戻り値を表示
        Debug.Log("戻り値: " + ret.ToString());
    }

    // ファイルを開く
    private string readTextFile(string path)
    {
        // ファイルが存在するか確認
        if (File.Exists(path))
        {
            // ファイルの内容をUTF-8で読み込む
            return File.ReadAllText(path, System.Text.Encoding.UTF8);
        }
        else
        {
            Debug.LogError("ファイルが見つかりません: " + path);
            return "";
        }
    }
}