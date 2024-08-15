public class OriginalScript
{
    // 読み込んだ変数や関数を格納しておく場所
    public OsEnviroment env;

    // プログラムソース
    public string input;

    public OriginalScript(string input)
    {
        this.input = input;
        env = new OsEnviroment();
    }
    public OriginalScript(string input, OsEnviroment env)
    {
        this.input = input;
        this.env = env;
    }

    // メインクラス
    public OsObject run()
    {
        OsLexer lexer = new OsLexer(input);
        OsParser parser = new OsParser(lexer);
        AstRoot root = parser.parseProgram();
        OsEvaluator evaluator = new OsEvaluator();
        return evaluator.eval(root, env);
    }

    // 新規環境を作成する（読み込んだ変数や関数を破棄）
    public void createNewEnv()
    {
        env = new OsEnviroment();
    }

    // プログラムソースを再読み込みする
    public void changeScript(string input)
    {
        this.input = input;
    }

    // 環境に該当の変数や関数があるのか確認する
    public bool contains(string name)
    {
        return env.contains(name);
    }
}
