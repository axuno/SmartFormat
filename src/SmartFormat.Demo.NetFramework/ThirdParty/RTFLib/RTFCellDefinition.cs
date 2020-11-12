


 

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
            this._x = 0;
            this._padding = padding;
            this._alignment = alignment;
            this._rTFBorderSide = rTFBorderSide;
            this._borderWidth = borderWidth;
            this._borderColor = borderColor;
            this._cellWidth = (float) cellwidth / 100;
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
            get { return this._alignment; }
            [DebuggerStepThrough]
            set { this._alignment = value; }
        }

        /// <summary>
        /// Gets the RTFborderside.
        /// </summary>
        /// <value>The RTF border side.</value>
        public RTFBorderSide RTFBorderSide
        {
            [DebuggerStepThrough]
            get { return this._rTFBorderSide; }
            [DebuggerStepThrough]
            set { this._rTFBorderSide = value; }
        }

        /// <summary>
        /// Gets the width of the border.
        /// </summary>
        /// <value>The width of the border.</value>
        public int BorderWidth
        {
            [DebuggerStepThrough]
            get { return this._borderWidth; }
            [DebuggerStepThrough]
            set { this._borderWidth = value; }
        }

        /// <summary>
        /// Gets the color of the border.
        /// </summary>
        /// <value>The color of the border.</value>
        public Color BorderColor
        {
            [DebuggerStepThrough]
            get { return this._borderColor; }
            [DebuggerStepThrough]
            set { this._borderColor = value; }
        }

        /// <summary>
        /// Gets or sets the width of the cell.
        /// </summary>
        /// <value>The width of the cell.</value>
        public float CellWidthRaw
        {
            [DebuggerStepThrough]
            get { return this._cellWidth; }
            [DebuggerStepThrough]
            set { this._cellWidth = value; }
        }

        /// <summary>
        /// Gets the X.
        /// </summary>
        /// <value>The X.</value>
        public int X
        {
            [DebuggerStepThrough]
            get { return this._x; }
        }

        public Padding Padding
        {
            [DebuggerStepThrough]
            get { return this._padding; }
            [DebuggerStepThrough]
            set { this._padding = value; }
        }

        #endregion

        #region Public Methods

        [DebuggerStepThrough]
        public void SetX(int value)
        {
            this._x = value;
        }

        #endregion
    }
}


