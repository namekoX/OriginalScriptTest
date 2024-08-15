
// int型を宣言する文を扱うためのクラスです
public class AstDecInt : AstDecVar
{
    // この型クラスに格納されている値が適切か判断する
    public new bool check()
    {
        // nullかintの場合はOK
        if (value == null || value is AstInteger)
        {
            return true;
        }

        // intの場合は変換してOKとする
        if (value is AstFloat folVal)
        {
            value = folVal.toInteger();
            return true;
        }

        return false;

    }
}