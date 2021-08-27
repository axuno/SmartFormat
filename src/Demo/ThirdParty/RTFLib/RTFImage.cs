


namespace RTF
{
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;

    public partial class RTFBuilder
    {
        #region Nested type: RTFImage

        public class RTFImage
        {
            // Not used in this application.  Descriptions can be found with documentation
            // of Windows GDI function SetMapMode

            #region Fields

            private const string FF_UNKNOWN = "UNKNOWN";

            // The number of hundredths of millimeters (0.01 mm) in an inch
            // For more information, see GetImagePrefix() method.
            private const int HMM_PER_INCH = 2540;
            private const int MM_ANISOTROPIC = 8;
            private const int MM_HIENGLISH = 5;
            private const int MM_HIMETRIC = 3;
            private const int MM_ISOTROPIC = 7;
            private const int MM_LOENGLISH = 4;
            private const int MM_LOMETRIC = 2;
            private const int MM_TEXT = 1;
            private const int MM_TWIPS = 6;

            // Ensures that the metafile maintains a 1:1 aspect ratio

            // The number of twips in an inch
            // For more information, see GetImagePrefix() method.
            private const int TWIPS_PER_INCH = 1440;


            private readonly RTFBuilder _builder;
            private readonly StringBuilder sb;
            private string RTF_IMAGE_POST = "}\r\n";

            #endregion

            #region Constructor

            public RTFImage(RTFBuilder builder)
            {
                this._builder = builder;
                this.sb = new StringBuilder();
            }

            #endregion

            #region Static Methods

            /// <summary>
            /// Use the EmfToWmfBits function in the GDI+ specification to convert a 
            /// Enhanced Metafile to a Windows Metafile
            /// </summary>
            /// <param name="_hEmf">
            /// A handle to the Enhanced Metafile to be converted
            /// </param>
            /// <param name="_bufferSize">
            /// The size of the buffer used to store the Windows Metafile bits returned
            /// </param>
            /// <param name="_buffer">
            /// An array of bytes used to hold the Windows Metafile bits returned
            /// </param>
            /// <param name="_mappingMode">
            /// The mapping mode of the image.  This control uses MM_ANISOTROPIC.
            /// </param>
            /// <param name="_flags">
            /// Flags used to specify the format of the Windows Metafile returned
            /// </param>
            [DllImport("gdiplus.dll")]
            private static extern uint GdipEmfToWmfBits(IntPtr _hEmf, uint _bufferSize, byte[] _buffer, int _mappingMode, EmfToWmfBitsFlags _flags);

            #endregion

            #region Public Methods

            public void InsertImage(Image image)
            {
                // The horizontal resolution at which the control is being displayed
                float xDpi;

                // The vertical resolution at which the control is being displayed
                float yDpi;

                using (Graphics graphics = Graphics.FromImage(image))
                {
                    xDpi = graphics.DpiX;
                    yDpi = graphics.DpiY;
                }


                // Create the image control string and append it to the RTF string
                this.WriteImagePrefix(image, xDpi, yDpi);


                // Create the Windows Metafile and append its bytes in HEX format
                this.WriteRtfImage(image);

                // Close the RTF image control string
                this.sb.Append(this.RTF_IMAGE_POST);

                this._builder._sb.Append(this.sb.ToString());
            }

            #endregion

            #region Methods

            /// <summary>
            /// Creates the RTF control string that describes the image being inserted.
            /// This description (in this case) specifies that the image is an
            /// MM_ANISOTROPIC metafile, meaning that both X and Y axes can be scaled
            /// independently.  The control string also gives the images current dimensions,
            /// and its target dimensions, so if you want to control the size of the
            /// image being inserted, this would be the place to do it. The prefix should
            /// have the form ...
            /// 
            /// {\pict\wmetafile8\picw[A]\pich[B]\picwgoal[C]\pichgoal[D]
            /// 
            /// where ...
            /// 
            /// A    = current width of the metafile in hundredths of millimeters (0.01mm)
            ///    = Image Width in Inches * Number of (0.01mm) per inch
            ///    = (Image Width in Pixels / Graphics Context's Horizontal Resolution) * 2540
            ///    = (Image Width in Pixels / Graphics.DpiX) * 2540
            /// 
            /// B    = current height of the metafile in hundredths of millimeters (0.01mm)
            ///    = Image Height in Inches * Number of (0.01mm) per inch
            ///    = (Image Height in Pixels / Graphics Context's Vertical Resolution) * 2540
            ///    = (Image Height in Pixels / Graphics.DpiX) * 2540
            /// 
            /// C    = target width of the metafile in twips
            ///    = Image Width in Inches * Number of twips per inch
            ///    = (Image Width in Pixels / Graphics Context's Horizontal Resolution) * 1440
            ///    = (Image Width in Pixels / Graphics.DpiX) * 1440
            /// 
            /// D    = target height of the metafile in twips
            ///    = Image Height in Inches * Number of twips per inch
            ///    = (Image Height in Pixels / Graphics Context's Horizontal Resolution) * 1440
            ///    = (Image Height in Pixels / Graphics.DpiX) * 1440
            ///    
            /// </summary>
            /// <remarks>
            /// The Graphics Context's resolution is simply the current resolution at which
            /// windows is being displayed.  Normally it's 96 dpi, but instead of assuming
            /// I just added the code.
            /// 
            /// According to Ken Howe at pbdr.com, "Twips are screen-independent units
            /// used to ensure that the placement and proportion of screen elements in
            /// your screen application are the same on all display systems."
            /// 
            /// Units Used
            /// ----------
            /// 1 Twip = 1/20 Point
            /// 1 Point = 1/72 Inch
            /// 1 Twip = 1/1440 Inch
            /// 
            /// 1 Inch = 2.54 cm
            /// 1 Inch = 25.4 mm
            /// 1 Inch = 2540 (0.01)mm
            /// </remarks>
            /// <param name="_image"></param>
            /// <returns></returns>
            private void WriteImagePrefix(Image _image, float xDpi, float yDpi)
            {
                // Get the horizontal and vertical resolutions at which the object is
                // being displayed

                // Calculate the current width of the image in (0.01)mm
                int picw = (int) Math.Round((_image.Width / xDpi) * HMM_PER_INCH);

                // Calculate the current height of the image in (0.01)mm
                int pich = (int) Math.Round((_image.Height / yDpi) * HMM_PER_INCH);

                // Calculate the target width of the image in twips
                int picwgoal = (int) Math.Round((_image.Width / xDpi) * TWIPS_PER_INCH);

                // Calculate the target height of the image in twips
                int pichgoal = (int) Math.Round((_image.Height / yDpi) * TWIPS_PER_INCH);

                // Append values to RTF string
                this.sb.Append(@"{\pict\wmetafile8");
                this.sb.Append(@"\picw");
                this.sb.Append(picw);
                this.sb.Append(@"\pich");
                this.sb.Append(pich);
                this.sb.Append(@"\picwgoal");
                this.sb.Append(picwgoal);
                this.sb.Append(@"\pichgoal");
                this.sb.Append(pichgoal);
                this.sb.Append(" ");
            }

            /// <summary>
            /// Wraps the image in an Enhanced Metafile by drawing the image onto the
            /// graphics context, then converts the Enhanced Metafile to a Windows
            /// Metafile, and finally appends the bits of the Windows Metafile in HEX
            /// to a string and returns the string.
            /// </summary>
            /// <param name="_image"></param>
            /// <returns>
            /// A string containing the bits of a Windows Metafile in HEX
            /// </returns>
            private void WriteRtfImage(Image _image)
            {
                // Used to store the enhanced metafile
                MemoryStream _stream = null;

                // Used to create the metafile and draw the image
                Graphics _graphics = null;

                // The enhanced metafile
                Metafile _metaFile = null;

                // Handle to the device context used to create the metafile
                IntPtr _hdc;

                try
                {
                    using (_stream = new MemoryStream())
                    {
                        // Get a graphics context from the RichTextBox
                        using (_graphics = Graphics.FromImage(_image))
                        {
                            // Get the device context from the graphics context
                            _hdc = _graphics.GetHdc();

                            // Create a new Enhanced Metafile from the device context
                            _metaFile = new Metafile(_stream, _hdc);

                            // Release the device context
                            _graphics.ReleaseHdc(_hdc);
                        }

                        // Get a graphics context from the Enhanced Metafile
                        using (_graphics = Graphics.FromImage(_metaFile))
                        {
                            // Draw the image on the Enhanced Metafile
                            _graphics.DrawImage(_image, new Rectangle(0, 0, _image.Width, _image.Height));
                        }
                        byte[] _buffer = null;
                        using (_metaFile)
                        {
                            // Get the handle of the Enhanced Metafile
                            IntPtr _hEmf = _metaFile.GetHenhmetafile();

                            // A call to EmfToWmfBits with a null buffer return the size of the
                            // buffer need to store the WMF bits.  Use this to get the buffer
                            // size.
                            uint _bufferSize = GdipEmfToWmfBits(_hEmf, 0, null, MM_ANISOTROPIC, EmfToWmfBitsFlags.EmfToWmfBitsFlagsDefault);

                            // Create an array to hold the bits
                            _buffer = new byte[_bufferSize];

                            // A call to EmfToWmfBits with a valid buffer copies the bits into the
                            // buffer an returns the number of bits in the WMF.  
                            uint _convertedSize = GdipEmfToWmfBits(_hEmf, _bufferSize, _buffer, MM_ANISOTROPIC, EmfToWmfBitsFlags.EmfToWmfBitsFlagsDefault);
                        }
                        // Append the bits to the RTF string
                        for (int i = 0; i < _buffer.Length; ++i)
                        {
                            this.sb.Append(String.Format("{0:X2}", _buffer[i]));
                        }
                        if (_stream != null)
                        {
                            _stream.Flush();
                            _stream.Close();
                        }
                    }
                }
                finally
                {
                    if (_graphics != null)
                    {
                        _graphics.Dispose();
                    }
                    if (_metaFile != null)
                    {
                        _metaFile.Dispose();
                    }
                    if (_stream != null)
                    {
                        _stream.Flush();
                        _stream.Close();
                    }
                }
            }

            #endregion

            #region Nested type: EmfToWmfBitsFlags

            private enum EmfToWmfBitsFlags
            {
                // Use the default conversion
                EmfToWmfBitsFlagsDefault = 0x00000000,

                // Embedded the source of the EMF metafiel within the resulting WMF
                // metafile
                EmfToWmfBitsFlagsEmbedEmf = 0x00000001,

                // Place a 22-byte header in the resulting WMF file.  The header is
                // required for the metafile to be considered placeable.
                EmfToWmfBitsFlagsIncludePlaceable = 0x00000002,

                // Don't simulate clipping by using the XOR operator.
                EmfToWmfBitsFlagsNoXORClip = 0x00000004
            } ;

            #endregion
        }

        #endregion
    }
}