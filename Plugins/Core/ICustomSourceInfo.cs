using System;
using System.ComponentModel;



namespace StringFormatEx.Plugins.Core
{
    /// <summary>
    /// Contains all the data necessary to evaluate the Item Selector.
    /// Provides the Current item, as well as the Selector.
    /// 
    /// In the example "{0.Date.Year:N4}", there will be 3 iterations: Nothing + "0";  arg(0) + "Date";  arg(0).Date + "Year".
    /// </summary>
    public interface ICustomSourceInfo
    {
	    /// <summary>
	    /// The string that determines what object to use for formatting.
	    /// 
	    /// In the example "{0.Date.Year:N4}", the selectors are "0", "Date", and "Year", and the Format is "N4".
	    /// </summary>
	    string Selector { get; }

    
    
        /// <summary>
	    /// The index of the selector in the placeholder.
	    /// For example, {0.Address.State.ToUpper}, if Selector = State, then SelectorIndex = 2
	    /// </summary>
        int SelectorIndex { get; }
	
    
    
        /// <summary>
	    /// Returns the Current object.
	    /// 
	    /// Use Selector to evaluate the current object, and set the result here.
	    /// 
	    /// In the example "{0.Date.Year:N4}", there will be 3 iterations:
	    /// Current is Nothing, Selector = "0";
	    /// Current is Arguments(0), Selector = "Date";
	    /// Current is Arguments(0).Date, Selector = "Year".
	    /// The result is Current = Arguments(0).Date.Year.
	    /// </summary>
	    object Current { get; set; }
	
    
    
        /// <summary>
	    /// An array of all the original arguments passed to the CustomFormat function.
	    /// 
	    /// This is not used often, but provides "global" access to these objects.
	    /// </summary>
	    [EditorBrowsable(EditorBrowsableState.Advanced)]
	    object[] Arguments { get; }

    
    
        /// <summary>
	    /// Determines if the ExtendCustomSource event has been handled.
	    /// Automatically set to True when you set the Current item.
	    /// </summary>
	    [EditorBrowsable(EditorBrowsableState.Advanced)]
	    bool Handled { get; set; }
    }
}