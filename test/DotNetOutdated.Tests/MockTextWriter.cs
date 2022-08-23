// Copyright (c) Down Syndrome Education Enterprises CIC. All Rights Reserved.
// Information contained herein is PROPRIETARY AND CONFIDENTIAL.

using System.Text;

namespace DotNetOutdated.Tests;

internal class MockTextWriter : TextWriter
{
    private readonly StringBuilder _sb;

    public MockTextWriter()
    {
        _sb = new StringBuilder();
    }

    public override void Write(char value)
    {
        _sb.Append(value);
    }

    public string Contents => _sb.ToString();

    public override Encoding Encoding => Encoding.Unicode;
}
