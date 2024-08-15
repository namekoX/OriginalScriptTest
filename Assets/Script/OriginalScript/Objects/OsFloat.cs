
// 小数点を含む数値型の評価結果を格納するクラス
public class OsFloat : OsObject
{
    // 設定値
    public float value;

    public OsFloat(float value) => this.value = value;
    override public string ToString() => value.ToString();
    public OsObjectType type() => OsObjectType.FLOAT;
}