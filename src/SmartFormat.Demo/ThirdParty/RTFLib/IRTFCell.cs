namespace RTF
{
    using System;

    // ----------------------------------------------------------------------------------------
    //    _                ___        _..-._   Date: 12/11/08    23:32
    //    \`.|\..----...-'`   `-._.-'' _.-..'     
    //    /  ' `         ,       __.-'' 
    //    )/` _/     \   `-_,   /     Solution: RTFLib
    //    `-'" `"\_  ,_.-;_.-\_ ',    Project : RTFLib                                 
    //        _.-'_./   {_.'   ; /    Author  : Anton
    //       {_.-``-'         {_/     Assembly: 1.0.0.0
    //                                Copyright © 2005-2008, Rogue Trader/MWM
    //        Project Item Name:      IRTFCell.cs - Code
    //        Purpose:                Exposes an underlying RTFBuilderbase
    // ----------------------------------------------------------------------------------------
    /// <summary>
    /// Exposes an underlying RTFBuilderbase
    /// </summary>
    public interface IBuilderContent : IDisposable
    {
        RTFBuilderbase Content { get; }
    }
}

