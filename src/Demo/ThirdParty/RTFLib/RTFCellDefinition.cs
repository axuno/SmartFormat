


 

namespace RTF
{
    using System.Diagnostics;
    using System.Drawing;
    using System.Windows.Forms;

    // ----------------------------------------------------------------------------------------
    //    _                ___        _..-._   Date: 12/11/08    23:47
    //    \`.|\..----...-'`   `-._.-'' _.-..'     
    //    /  ' `         ,       __.-'' 
    //    )/` _/     \   `-_,   /     Solution: RTFLib
    //    `-'" `"\_  ,_.-;_.-\_ ',    Project : RTFLib                                 
    //        _.-'_./   {_.'   ; /    Author  : Anton
    //       {_.-``-'         {_/     Assembly: 1.0.0.0
    //                                Copyright © 2005-2008, Rogue Trader/MWM
    //        Project Item Name:      RTFCellDefinition.cs - Code
    //        Purpose:                Definition Of Cell In Table Row
    // ----------------------------------------------------------------------------------------
    /// <summary>
    /// Definition Of Cell In Table Row
    /// </summary>
    public struct RTFCellDefinition
    {
        #region Fields

        private RTFAlignment _alignment;
        private Color _borderColor;
        private int _borderWidth;
        private float _cellWidth;
        private Padding _padding;
        private RTFBorderSide _rTFBorderSide;

        private int _x;

        #endregion

        #region Constructor

        public RTFCellDefinition(int cellwidth, RTFAlignment alignment, RTFBorderSide rTFBorderSide, int borderWidth, Color borderColor, Padding padding)
        {
            _x = 0;
            _padding = padding;
            _alignment = alignment;
            _rTFBorderSide = rTFBorderSide;
            _borderWidth = borderWidth;
            _borderColor = borderColor;
            _cellWidth = (float) cellwidth / 100;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the alignment.
        /// </summary>
        /// <value>The alignment.</value>
        public RTFAlignment Alignment
        {
            [DebuggerStepThrough]
            get { return _alignment; }
            [DebuggerStepThrough]
            set { _alignment = value; }
        }

        /// <summary>
        /// Gets the RTFborderside.
        /// </summary>
        /// <value>The RTF border side.</value>
        public RTFBorderSide RTFBorderSide
        {
            [DebuggerStepThrough]
            get { return _rTFBorderSide; }
            [DebuggerStepThrough]
            set { _rTFBorderSide = value; }
        }

        /// <summary>
        /// Gets the width of the border.
        /// </summary>
        /// <value>The width of the border.</value>
        public int BorderWidth
        {
            [DebuggerStepThrough]
            get { return _borderWidth; }
            [DebuggerStepThrough]
            set { _borderWidth = value; }
        }

        /// <summary>
        /// Gets the color of the border.
        /// </summary>
        /// <value>The color of the border.</value>
        public Color BorderColor
        {
            [DebuggerStepThrough]
            get { return _borderColor; }
            [DebuggerStepThrough]
            set { _borderColor = value; }
        }

        /// <summary>
        /// Gets or sets the width of the cell.
        /// </summary>
        /// <value>The width of the cell.</value>
        public float CellWidthRaw
        {
            [DebuggerStepThrough]
            get { return _cellWidth; }
            [DebuggerStepThrough]
            set { _cellWidth = value; }
        }

        /// <summary>
        /// Gets the X.
        /// </summary>
        /// <value>The X.</value>
        public int X
        {
            [DebuggerStepThrough]
            get { return _x; }
        }

        public Padding Padding
        {
            [DebuggerStepThrough]
            get { return _padding; }
            [DebuggerStepThrough]
            set { _padding = value; }
        }

        #endregion

        #region Public Methods

        [DebuggerStepThrough]
        public void SetX(int value)
        {
            _x = value;
        }

        #endregion
    }
}


