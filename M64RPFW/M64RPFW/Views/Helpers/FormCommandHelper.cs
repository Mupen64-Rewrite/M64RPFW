using System;
using Eto.Forms;

namespace M64RPFW.Misc;

public static class FormCommandHelper
{
    public static T DoPostInit<T>(T obj, Action<T> postInit) where T : class
    {
        postInit(obj);
        return obj;
    }
}