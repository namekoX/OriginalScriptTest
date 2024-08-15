
// NULLの評価結果を格納するクラス
public class OsNull : OsObject
{
    override public string ToString() => "";
    public OsObjectType type() => OsObjectType.NULL;
}