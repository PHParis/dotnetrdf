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
using VDS.RDF.Query.Engine;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query.Sorting
{
    /// <summary>
    /// Interface for sort conditions
    /// </summary>
    public interface ISortCondition
        : IEquatable<ISortCondition>
    {
        /// <summary>
        /// Gets whether this is an ascending sort condition
        /// </summary>
        /// <returns>True if an ascending sort condition, false if a descending sort condition</returns>
        bool IsAscending { get; }

        /// <summary>
        /// Gets the expression used for sorting
        /// </summary>
        IExpression Expression { get; }

        IComparer<ISolution> CreateComparer(IExpressionContext context);
            
        String ToString();

        String ToString(IAlgebraFormatter formatter);

        String ToPrefixString();

        String ToPrefixString(IAlgebraFormatter formatter);
    }
}
