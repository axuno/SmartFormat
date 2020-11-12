


 

namespace RTF
{
    using System;
    using System.Collections.Generic;

    // ----------------------------------------------------------------------------------------
    //    _                ___        _..-._   Date: 12/11/08    23:33
    //    \`.|\..----...-'`   `-._.-'' _.-..'     
    //    /  ' `         ,       __.-'' 
    //    )/` _/     \   `-_,   /     Solution: RTFLib
    //    `-'" `"\_  ,_.-;_.-\_ ',    Project : RTFLib                                 
    //        _.-'_./   {_.'   ; /    Author  : Anton
    //       {_.-``-'         {_/     Assembly: 1.0.0.0
    //                                Copyright © 2005-2008, Rogue Trader/MWM
    //        Project Item Name:      IRTFRow.cs - Code
    //        Purpose:                Row Interface
    // ----------------------------------------------------------------------------------------
    /// <summary>
    /// Row Interface
    /// </summary>
    public interface IRTFRow : IDisposable, IEnumerable<IBuilderContent>
    {
    }
}

