using System;

namespace M64RPFW.Views.Helpers;

public static class FormCommandHelper
{
    /// <summary>
    /// Performs an <see cref="Action"/> on an object.
    /// 
    /// <para>
    /// Intended for use with initializer lists to set up things that cannot
    /// be done in an initializer list. You could totally use an IIFE, I just
    /// think this looks better.
    /// </para>
    /// </summary>
    /// <param name="obj">the object</param>
    /// <param name="postInit">the action to perform on it</param>
    /// <returns>the object after performing the action</returns>
    public static T DoPostInit<T>(T obj, Action<T> postInit) where T : class
    {
        postInit(obj);
        return obj;
    }
}