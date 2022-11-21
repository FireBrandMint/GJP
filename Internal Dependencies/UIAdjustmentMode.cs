using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public enum UIAdjustmentMode
{
    ///<summary>
    ///Scales the UI against the smallest axis of the screen border.
    ///</summary>
    Compact = 0,
    ///<summary>
    ///Scales the UI against both axis of the screen border.
    ///</summary>
    Extended = 1
}

public static class UIGetter
{
    static Func<Vector2> UISizeGetter = null;

    public static Vector2 GetUISize()
    {
        if(UISizeGetter == null) return new Vector2(-1, 0);

        return UISizeGetter.Invoke();
    }

    public static void SetUISizeGetter(Func<Vector2> func)
    {
        UISizeGetter = func;
    }
}