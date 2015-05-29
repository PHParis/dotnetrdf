/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2015 dotNetRDF Project (dotnetrdf-develop@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Graphs;
using VDS.RDF.Nodes;

namespace VDS.RDF.Writing.Formatting
{
    /// <summary>
    /// Formatter for generating CSV
    /// </summary>
    public class CsvFormatter 
        : BaseFormatter, IQuadFormatter
    {
        /// <summary>
        /// Creates a new CSV Formatter
        /// </summary>
        public CsvFormatter()
            : base("CSV") { }

        /// <summary>
        /// Formats URIs for CSV output
        /// </summary>
        /// <param name="u">URI</param>
        /// <param name="segment">Triple Segment</param>
        /// <returns></returns>
        protected override string FormatUriNode(INode u, QuadSegment? segment)
        {
            return this.FormatUri(u.Uri);
        }

        /// <summary>
        /// Formats Literals for CSV output
        /// </summary>
        /// <param name="l">Literal</param>
        /// <param name="segment">Triple Segment</param>
        /// <returns></returns>
        protected override string FormatLiteralNode(INode l, QuadSegment? segment)
        {
            String value = l.Value;
            if (value.Contains('"') || value.Contains(',') || value.Contains('\n') || value.Contains('\r'))
            {
                return '"' + value.Replace("\"", "\"\"") + '"';
            }
            else if (value.Equals(String.Empty))
            {
                return "\"\"";
            }
            else
            {
                return value;
            }
        }

        public string Format(Quad q)
        {
            return this.Format(q.AsTriple()) + "," + this.Format(q.Graph);
        }
    }
}
