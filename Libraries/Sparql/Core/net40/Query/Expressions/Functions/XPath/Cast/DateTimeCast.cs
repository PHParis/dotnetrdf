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
using VDS.RDF.Nodes;
using VDS.RDF.Query.Engine;
using VDS.RDF.Specifications;

namespace VDS.RDF.Query.Expressions.Functions.XPath.Cast
{
    /// <summary>
    /// Class representing an XPath Date Time Cast Function
    /// </summary>
    public class DateTimeCast
        : BaseCast
    {
        /// <summary>
        /// Creates a new XPath Date Time Cast Function Expression
        /// </summary>
        /// <param name="expr">Expression to be cast</param>
        public DateTimeCast(IExpression expr) 
            : base(expr) { }

        public override IExpression Copy(IExpression argument)
        {
            return new DateTimeCast(argument);
        }

        /// <summary>
        /// Casts the value of the inner Expression to a Date Time
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override IValuedNode Evaluate(ISolution solution, IExpressionContext context)
        {
            IValuedNode n = this.Argument.Evaluate(solution, context);

            if (n == null)
            {
                throw new RdfQueryException("Cannot cast a Null to a xsd:dateTime");
            }

            switch (n.NodeType)
            {
                case NodeType.Blank:
                case NodeType.GraphLiteral:
                case NodeType.Uri:
                    throw new RdfQueryException("Cannot cast a Blank/URI/Graph Literal Node to a xsd:dateTime");

                case NodeType.Literal:
                    if (n is DateTimeNode) return n;
                    //See if the value can be cast
                    INode lit = n;
                    if (lit.DataType != null)
                    {
                        string dt = lit.DataType.ToString();
                        if (dt.Equals(XmlSpecsHelper.XmlSchemaDataTypeDateTime))
                        {
                            //Already a xsd:dateTime
                            DateTimeOffset d;
                            if (DateTimeOffset.TryParse(lit.Value, out d))
                            {
                                //Parsed OK
                                return new DateTimeNode(d);
                            }
                            throw new RdfQueryException("Invalid lexical form for xsd:dateTime");
                        }
                        else if (dt.Equals(XmlSpecsHelper.XmlSchemaDataTypeString))
                        {
                            DateTimeOffset d;
                            if (DateTimeOffset.TryParse(lit.Value, out d))
                            {
                                //Parsed OK
                                return new DateTimeNode(d);
                            }
                            else
                            {
                                throw new RdfQueryException("Cannot cast the value '" + lit.Value + "' to a xsd:double");
                            }
                        }
                        else
                        {
                            throw new RdfQueryException("Cannot cast a Literal typed <" + dt + "> to a xsd:dateTime");
                        }
                    }
                    else
                    {
                        DateTimeOffset d;
                        if (DateTimeOffset.TryParse(lit.Value, out d))
                        {
                            //Parsed OK
                            return new DateTimeNode(d);
                        }
                        else
                        {
                            throw new RdfQueryException("Cannot cast the value '" + lit.Value + "' to a xsd:dateTime");
                        }
                    }
                default:
                    throw new RdfQueryException("Cannot cast an Unknown Node to a xsd:string");
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return XmlSpecsHelper.XmlSchemaDataTypeDateTime;
            }
        }
    }
}
