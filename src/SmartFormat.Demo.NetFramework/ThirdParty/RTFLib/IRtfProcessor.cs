


namespace CurrentPatient
{
    using RTF;

    // ----------------------------------------------------------------------------------------
    //    _                ___        _..-._   Date: 12/11/08    23:33
    //    \`.|\..----...-'`   `-._.-'' _.-..'     
    //    /  ' `         ,       __.-'' 
    //    )/` _/     \   `-_,   /     Solution: RTFLib
    //    `-'" `"\_  ,_.-;_.-\_ ',    Project : RTFLib                                 
    //        _.-'_./   {_.'   ; /    Author  : Anton
    //       {_.-``-'         {_/     Assembly: 1.0.0.0
    //                                Copyright © 2005-2008, Rogue Trader/MWM
    //        Project Item Name:      IRtfProcessor.cs - Code
    //        Purpose:                Processor of RTF
    // ----------------------------------------------------------------------------------------
    /// <summary>
    /// Processor of RTF
    /// </summary>
    public interface IRtfProcessor
    {
        #region Public Properties

        RTFBuilderbase Builder { get; }

        #endregion

        #region Abstract Methods

        string Process(string rtf);

        #endregion
    }
}