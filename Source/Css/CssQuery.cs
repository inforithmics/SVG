using System;
using System.Collections.Generic;
using System.Linq;
using ExCSS;

namespace Svg.Css
{
    internal static class CssQuery
    {
        public static IEnumerable<SvgElement> QuerySelectorAll(this SvgElement elem, ISelector selector, SvgElementFactory elementFactory)
        {
            var input = Enumerable.Repeat(elem, 1);
            var ops = new SvgElementOpsFunc(elementFactory);

            var func = GetFunc(selector, ops, ops.Universal());
            var descendants = ops.Descendant();
            var func1 = func;
            func = f => func1(descendants(f));
            return func(input).Distinct();
        }

        private static Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>> GetFunc(
            CompoundSelector selector,
            SvgElementOpsFunc ops,
            Func<IEnumerable<SvgElement>,
                IEnumerable<SvgElement>> inFunc)
        {
            foreach (var it in selector)
            {
                inFunc = GetFunc(it, ops, inFunc);
            }

            return inFunc;
        }

        private static Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>> GetFunc(
            PseudoClassSelector selector,
            SvgElementOpsFunc ops,
            Func<IEnumerable<SvgElement>,
                IEnumerable<SvgElement>> inFunc)
        {
            Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>> pseudoFunc;
            if (selector.Class == PseudoClassNames.FirstChild)
            {
                pseudoFunc = ops.FirstChild();
            }
            else if (selector.Class == PseudoClassNames.LastChild)
            {
                pseudoFunc = ops.LastChild();
            }
            else
            {
                throw new NotImplementedException();
            }

            return f => pseudoFunc(inFunc(f));
        }

        private static Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>> GetFunc(
            ComplexSelector selector,
            SvgElementOpsFunc ops,
            Func<IEnumerable<SvgElement>,
                IEnumerable<SvgElement>> inFunc)
        {
            Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>> result = inFunc;

            foreach (var it in selector)
            {
                result = GetFunc(it.Selector, ops, result);

                Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>> combinatorFunc;
                if (it.Delimiter == Combinator.Child.Delimiter)
                {
                    combinatorFunc = ops.Child();
                }
                else if (it.Delimiter == Combinators.Descendent)
                {
                    combinatorFunc = ops.Descendant();
                }
                else if (it.Delimiter == Combinator.Deep.Delimiter)
                {
                    throw new NotImplementedException();
                }
                else if (it.Delimiter == Combinators.Adjacent)
                {
                    combinatorFunc = ops.Adjacent();
                }
                else if (it.Delimiter == Combinators.Sibling)
                {
                    combinatorFunc = ops.GeneralSibling();
                }
                else if (it.Delimiter == Combinators.Pipe)
                {
                    // Namespace
                    throw new NotImplementedException();
                }
                else if (it.Delimiter == Combinators.Column)
                {
                    throw new NotImplementedException();
                }
                else if (it.Delimiter == null)
                {
                    combinatorFunc = null;
                }
                else
                {
                    throw new NotImplementedException();
                }

                if (combinatorFunc != null)
                {
                    var result1 = result;
                    result = f => combinatorFunc(result1(f));
                }
            }

            return result;
        }

        private static Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>> GetFunc(ISelector selector, SvgElementOpsFunc ops, Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>> inFunc)
        {
            var func = selector switch
            {
                AllSelector allSelector => ops.Universal(),
                AttrAvailableSelector attrAvailableSelector => ops.AttributeExists(attrAvailableSelector.Attribute),
                AttrBeginsSelector attrBeginsSelector => ops.AttributePrefixMatch(attrBeginsSelector.Attribute, attrBeginsSelector.Value),
                AttrContainsSelector attrContainsSelector => ops.AttributeSubstring(attrContainsSelector.Attribute, attrContainsSelector.Value),
                AttrEndsSelector attrEndsSelector => ops.AttributeSuffixMatch(attrEndsSelector.Attribute, attrEndsSelector.Value),
                AttrHyphenSelector attrHyphenSelector => ops.AttributeDashMatch(attrHyphenSelector.Attribute, attrHyphenSelector.Value), // TODO:,
                AttrListSelector attrListSelector => ops.AttributeExists(attrListSelector.Attribute), // TODO:,
                AttrMatchSelector attrMatchSelector => ops.AttributeExact(attrMatchSelector.Attribute, attrMatchSelector.Value),
                AttrNotMatchSelector attrNotMatchSelector => throw new NotImplementedException(), // TODO:,
                ClassSelector classSelector => ops.Class(classSelector.Class),
                ComplexSelector complexSelector =>  GetFunc(complexSelector, ops, inFunc),
                CompoundSelector compoundSelector => GetFunc(compoundSelector, ops, inFunc),
                FirstChildSelector firstChildSelector => ops.FirstChild(), // TODO:,
                FirstColumnSelector firstColumnSelector => throw new NotImplementedException(), // TODO:,
                FirstTypeSelector firstTypeSelector => throw new NotImplementedException(), // TODO:,
                ChildSelector childSelector => throw new NotImplementedException(), // TODO:,
                ListSelector listSelector => throw new NotImplementedException(), // TODO:,
                NamespaceSelector namespaceSelector => throw new NotImplementedException(), // TODO:,
                PageSelector pageSelector => throw new NotImplementedException(), // TODO:,
                PseudoClassSelector pseudoClassSelector => GetFunc(pseudoClassSelector, ops, inFunc), // TODO:,
                PseudoElementSelector pseudoElementSelector => throw new NotImplementedException(), // TODO:,
                TypeSelector typeSelector => ops.Type(typeSelector.Name),
                UnknownSelector unknownSelector => throw new NotImplementedException(), // TODO:,
                IdSelector idSelector => ops.Id(idSelector.Id),
                _ => throw new NotImplementedException(), // TODO:,
            };

            return f => func(inFunc(f));
        }

        public static int GetSpecificity(this ISelector selector)
        {
            var specificity = 0x0;
            // ID selector
            specificity |= (1 << 12) * selector.Specificity.Ids;
            // class selector
            specificity |= (1 << 8) * selector.Specificity.Classes;
            // element selector
            specificity |= (1 << 4) * selector.Specificity.Tags;
            return specificity;
        }
    }
}
