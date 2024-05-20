// 
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace SmartFormat.ZString;

// --SONAR-OFF-- // This file is excluded from SonarCloud analysis.

/// <summary>
/// A 1:1 wrapper around <see cref="Cysharp.Text.ZStringWriter"/>,
/// so that we don't have to reference <see cref="Cysharp.Text"/> classes directly.
/// </summary>
/// <remarks>
/// It's important to make sure the writer is always properly disposed.
/// </remarks>
[ExcludeFromCodeCoverage]
public sealed class ZStringWriter : TextWriter
{
    private readonly Cysharp.Text.ZStringWriter _zw;
    private Encoding? _encoding;

    /// <summary>
    /// Creates a new instance using <see cref="CultureInfo.CurrentCulture"/> as format provider.
    /// </summary>
    public ZStringWriter() : this(CultureInfo.CurrentCulture)
    {
        _zw = new Cysharp.Text.ZStringWriter(CultureInfo.CurrentCulture);
    }

    /// <summary>
    /// Creates a new instance with given format provider.
    /// </summary>
    public ZStringWriter(IFormatProvider formatProvider) : base(formatProvider)
    {
        _zw = new Cysharp.Text.ZStringWriter(formatProvider);
    }

    /// <summary>
    /// Disposes this instance, operations are no longer allowed.
    /// </summary>
    public override void Close()
    {
        Dispose(true);
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        _zw.Dispose();
        base.Dispose(disposing);
    }

    /// <inheritdoc/>
    public override Encoding Encoding => _encoding ??= new UnicodeEncoding(false, false);

    /// <inheritdoc/>
    public override void Write(char value)
    {
        _zw.Write(value);
    }

    /// <inheritdoc/>
    public override void Write(char[] buffer, int index, int count)
    {
        _zw.Write(buffer, index, count);
    }

    /// <inheritdoc/>
    public override void Write(string? value)
    {
        _zw.Write(value ?? string.Empty);
    }

    /// <inheritdoc/>
    public override void Write(bool value)
    {
        _zw.Write(value);
    }

    /// <inheritdoc/>
    public override void Write(decimal value)
    {
        _zw.Write(value);
    }

    /// <inheritdoc/>
    public override Task WriteAsync(char value)
    {
        return _zw.WriteAsync(value);
    }

    /// <inheritdoc/>
    public override Task WriteAsync(string? value)
    {
        return _zw.WriteAsync(value ?? string.Empty);
    }

    /// <inheritdoc/>
    public override Task WriteAsync(char[] buffer, int index, int count)
    {
        return _zw.WriteAsync(buffer, index, count);
    }

    /// <inheritdoc/>
    public override Task WriteLineAsync(char value)
    {
        return _zw.WriteLineAsync(value);
    }

    /// <inheritdoc/>
    public override Task WriteLineAsync(string? value)
    {
        return _zw.WriteLineAsync(value ?? string.Empty);
    }

    /// <inheritdoc/>
    public override Task WriteLineAsync(char[] buffer, int index, int count)
    {
        return _zw.WriteLineAsync(buffer, index, count);
    }

    /// <summary>
    /// No-op.
    /// </summary>
    public override Task FlushAsync()
    {
        return _zw.FlushAsync();
    }

    /// <summary>
    /// Materializes the current state from underlying string builder.
    /// </summary>
    public override string ToString()
    {
        return _zw.ToString();
    }

#if NETSTANDARD2_1 || NET6_0_OR_GREATER

    /// <summary>
    /// Writes a span of characters.
    /// </summary>
    /// <param name="buffer"></param>
    public override void Write(ReadOnlySpan<char> buffer)
    {
        _zw.Write(buffer);
    }

    /// <summary>
    /// Writes a span of characters followed by a line terminator.
    /// /// </summary>
    /// <param name="buffer"></param>
    public override void WriteLine(ReadOnlySpan<char> buffer)
    {
        _zw.WriteLine(buffer);
    }

    /// <summary>
    /// Writes a span of characters.
    /// </summary>
    public override Task WriteAsync(ReadOnlyMemory<char> buffer, CancellationToken cancellationToken = default)
    {
        return _zw.WriteAsync(buffer, cancellationToken);
    }

    /// <summary>
    /// Writes a span of characters followed by a line terminator.
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override Task WriteLineAsync(ReadOnlyMemory<char> buffer, CancellationToken cancellationToken = default)
    {
        return _zw.WriteAsync(buffer, cancellationToken);
    }
#endif
}
