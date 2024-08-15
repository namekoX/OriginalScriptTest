
// float型を宣言する文を扱うためのクラスです
public class AstDecFloat : AstDecVar
{
    // この型クラスに格納されている値が適切か判断する
    public new bool check()
    {
        // nullかfloatの場合はOK
        if (value == null || value is AstFloat)
        {
            return true;
        }

        // intの場合は変換してOKとする
        if (value is AstInteger intVal)
        {
            value = intVal.toFloat();
            return true;
        }

        return false;

    }
}