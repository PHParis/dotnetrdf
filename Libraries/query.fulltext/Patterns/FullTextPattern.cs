﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Algebra;

namespace VDS.RDF.Query.Patterns
{
    public class FullTextPattern
        : BaseTriplePattern
    {
        private List<TriplePattern> _origPatterns = new List<TriplePattern>();
        private PatternItem _matchVar, _scoreVar, _searchTerm, _thresholdTerm, _limitTerm;

        public FullTextPattern(IEnumerable<TriplePattern> origPatterns)
        {
            this._origPatterns.AddRange(origPatterns.OrderBy(tp => tp.Predicate.ToString()));
            PatternItem matchVar = null;
            PatternItem searchVar = null;
            Dictionary<String, PatternItem> firsts = new Dictionary<string, PatternItem>();
            Dictionary<String, PatternItem> rests = new Dictionary<string, PatternItem>();

            foreach (TriplePattern tp in this._origPatterns)
            {
                NodeMatchPattern predItem = tp.Predicate as NodeMatchPattern;
                if (predItem == null) continue;
                IUriNode predUri = predItem.Node as IUriNode;
                if (predUri == null) continue;

                switch (predUri.Uri.ToString())
                {
                    case FullTextHelper.FullTextMatchPredicateUri:
                        //Extract the Search Term
                        if (searchVar != null) throw new RdfQueryException("More than one pf:textMatch property specified");
                        if (tp.Object.VariableName == null)
                        {
                            this._searchTerm = tp.Object;
                        }
                        else
                        {
                            searchVar = tp.Object;
                        }

                        //Extract the Match Variable
                        if (matchVar != null) throw new RdfQueryException("More than one pf:textMatch property specified");
                        if (tp.Subject.VariableName != null && !tp.Subject.VariableName.StartsWith("_:"))
                        {
                            this._matchVar = tp.Subject;
                            if (this._origPatterns.Count > 1 && searchVar == null) throw new RdfQueryException("Too many patterns provided");
                        }
                        else
                        {
                            matchVar = tp.Subject;
                        }
                        break;

                    case RdfSpecsHelper.RdfListFirst:
                        firsts.Add(tp.Subject.VariableName.ToString(), tp.Object);
                        break;

                    case RdfSpecsHelper.RdfListRest:
                        rests.Add(tp.Subject.VariableName.ToString(), tp.Object);
                        break;
                    default:
                        throw new RdfQueryException("Unexpected pattern");
                }
            }

            //Use the first and rest lists to determine Match and Score Variables if necessary
            if (this._matchVar == null)
            {
                firsts.TryGetValue(matchVar.VariableName, out this._matchVar);
                String restKey = rests[matchVar.VariableName].VariableName;
                firsts.TryGetValue(restKey, out this._scoreVar);
            }
            //Use the first and rest lists to determine search term, threshold and limit if necessary
            if (this._searchTerm == null)
            {
                firsts.TryGetValue(searchVar.VariableName, out this._searchTerm);
                String restKey = rests[searchVar.VariableName].VariableName;
                firsts.TryGetValue(restKey, out this._thresholdTerm);
                PatternItem last = rests[restKey];
                if (!last.ToString().Equals("<" + RdfSpecsHelper.RdfListNil + ">"))
                {
                    restKey = rests[restKey].VariableName;
                    firsts.TryGetValue(restKey, out this._limitTerm);
                }
                else
                {
                    //TODO: Add some logic here re: whether with only two arguments the 2nd argument is a threshold or a limit
                }
            }

            if (this._matchVar == null) throw new RdfQueryException("Failed to specify match variable");
            if (this._searchTerm == null) this._searchTerm = searchVar;
            if (this._searchTerm == null) throw new RdfQueryException("Failed to specify search terms");
        }

        public FullTextPattern(PatternItem matchVar, PatternItem scoreVar, PatternItem searchTerm)
        {
            this._matchVar = matchVar;
            this._scoreVar = scoreVar;
            this._searchTerm = searchTerm;

            NodeFactory factory = new NodeFactory();
            if (this._scoreVar != null)
            {
                BlankNodePattern a = new BlankNodePattern(factory.GetNextBlankNodeID());
                BlankNodePattern b = new BlankNodePattern(factory.GetNextBlankNodeID());
                this._origPatterns.Add(new TriplePattern(a, new NodeMatchPattern(factory.CreateUriNode(new Uri(FullTextHelper.FullTextMatchPredicateUri))), this._searchTerm));
                this._origPatterns.Add(new TriplePattern(a, new NodeMatchPattern(factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfListFirst))), this._matchVar));
                this._origPatterns.Add(new TriplePattern(a, new NodeMatchPattern(factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfListRest))), b));
                this._origPatterns.Add(new TriplePattern(b, new NodeMatchPattern(factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfListFirst))), this._scoreVar));
                this._origPatterns.Add(new TriplePattern(b, new NodeMatchPattern(factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfListRest))), new NodeMatchPattern(factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfListNil)))));
            }
            else
            {
                this._origPatterns.Add(new TriplePattern(this._matchVar, new NodeMatchPattern(factory.CreateUriNode(new Uri(FullTextHelper.FullTextMatchPredicateUri))), this._searchTerm));
            }
        }

        public IEnumerable<TriplePattern> OriginalPatterns
        {
            get
            {
                return this._origPatterns;
            }
        }

        public PatternItem MatchVariable
        {
            get
            {
                return this._matchVar;
            }
        }

        public PatternItem ScoreVariable
        {
            get
            {
                return this._scoreVar;
            }
        }

        public PatternItem SearchTerm
        {
            get
            {
                return this._searchTerm;
            }
        }

        public double ScoreThreshold
        {
            get
            {
                try
                {
                    return SparqlSpecsHelper.ToDouble((this._thresholdTerm as NodeMatchPattern).Node as ILiteralNode);
                }
                catch
                {
                    return Double.NaN;
                }
            }
        }

        public int Limit
        {
            get
            {
                try
                {
                    return Convert.ToInt32(SparqlSpecsHelper.ToInteger((this._limitTerm as NodeMatchPattern).Node as ILiteralNode));
                }
                catch
                {
                    return -1;
                }
            }
        }
        
        public override void Evaluate(SparqlEvaluationContext context)
        {
            Bgp bgp = new Bgp(this._origPatterns);
            context.Evaluate(bgp);
        }

        public override bool IsAcceptAll
        {
            get 
            {
                return false;
            }
        }

        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            foreach (TriplePattern tp in this._origPatterns)
            {
                output.AppendLine(tp.ToString());
            }
            return output.ToString();
        }
    }
}
