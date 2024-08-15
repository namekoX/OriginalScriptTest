
// 整数型の評価結果を格納するクラス
public class OsInteger : OsObject
{
    // 設定値
    public int value;

    public OsInteger(int value) => this.value = value;
    override public string ToString() => value.ToString();
    public OsObjectType type() => OsObjectType.INTEGER;
}