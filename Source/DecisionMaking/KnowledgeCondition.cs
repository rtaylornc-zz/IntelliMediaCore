//---------------------------------------------------------------------------------------
// Copyright 2014 North Carolina State University
//
// Center for Educational Informatics
// http://www.cei.ncsu.edu/
//
// Redistribution and use in source and binary forms, with or without modification,
// are permitted provided that the following conditions are met:
//
//   * Redistributions of source code must retain the above copyright notice, this 
//     list of conditions and the following disclaimer.
//   * Redistributions in binary form must reproduce the above copyright notice, 
//     this list of conditions and the following disclaimer in the documentation 
//     and/or other materials provided with the distribution.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE 
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE 
// GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) 
// HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT 
// LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT
// OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//
//---------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;

namespace IntelliMedia.DecisionMaking
{
    public class KnowledgeCondition : Condition
    {
        public enum Scope
        {
            Local,
            Global
        }

        public enum Comparison
        {
            Exists,
            DoesNotExist,
            Equal,
            NotEqual,
            GreaterThan,
            LessThan
        }

        Scope _scope;
        Comparison _comparison;
        string _name;
        object _rhs;

        public KnowledgeCondition(Scope scope, Comparison comparison, string name, object rhs = null)
        {
            _scope = scope;
            _comparison = comparison;
            _name = name;
            _rhs = rhs;
        }

        public override bool Test() 
        {
            Blackboard knowledge = (_scope == Scope.Global ? State.Context.GlobalKnowledge :  State.Context.LocalKnowledge);
            switch (_comparison)
            {
            case Comparison.Exists:
                return knowledge.Exists(_name);

            case Comparison.DoesNotExist:
                return !knowledge.Exists(_name);

            case Comparison.Equal:
                return knowledge.Get<object>(_name) == _rhs;

            case Comparison.NotEqual:
                return knowledge.Get<object>(_name) != _rhs;

//            case Comparison.GreaterThan:
//                return knowledge.Get<object>(_name) > _rhs;
//
//            case Comparison.LessThan:
//                return knowledge.Get<object>(_name) < _rhs;

            default:
                throw new Exception("Unsupported comparison: " + _comparison);
            }
        }
    }
}