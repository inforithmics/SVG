using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Svg.Css
{
    internal class SvgElementOpsFunc : ICssSelectorOps<SvgElement>
    {
        private readonly SvgElementFactory _elementFactory;

        public SvgElementOpsFunc(SvgElementFactory elementFactory)
        {
            _elementFactory = elementFactory;
        }

        public IEnumerable<SvgElement> DebugNodes(IEnumerable<SvgElement> nodes, [CallerMemberName] string caller = null)
        {
            if (caller != NodeDebug)
            {
                foreach (var it in nodes)
                {
                    yield return it;
                }

                yield break;
            }

            Debug.WriteLine(Environment.NewLine);
            Debug.WriteLine(nameof(DebugNodes) + ": " + caller);
            Debug.WriteLine(Environment.NewLine);
            foreach (var it in nodes)
            {
                Debug.WriteLine(it.ElementName);
                yield return it;
            }
        }

        public static string NodeDebug { get; set; }

        public Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>> Type(string name)
        {
            Debug.WriteLine(nameof(Type) + name);
            if (_elementFactory.AvailableElementsDictionary.TryGetValue(name, out var types))
            {
                return nodes => DebugNodes(nodes).Where(n => types.Contains(n.GetType()));
            }
            return nodes => Enumerable.Empty<SvgElement>();
        }

        public Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>> Universal()
        {
            Debug.WriteLine(nameof(Universal));
            return nodes => DebugNodes(nodes);
        }

        public Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>> Id(string id)
        {
            Debug.WriteLine(nameof(Id) + id);
            return nodes => DebugNodes(nodes).Where(n => n.ID == id);
        }

        public Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>> Class(string clazz)
        {
            Debug.WriteLine(nameof(Class) + clazz);
            return AttributeIncludes("class", clazz);
        }

        public Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>> AttributeExists(string name)
        {
            Debug.WriteLine(nameof(AttributeExists) + name);
            return nodes => DebugNodes(nodes).Where(n => n.ContainsAttribute(name));
        }

        public Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>> AttributeExact(string name, string value)
        {
            Debug.WriteLine(nameof(AttributeExact) + name + value);
            return nodes => DebugNodes(nodes).Where(n => (n.TryGetAttribute(name, out var val) && val == value));
        }

        public Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>> AttributeIncludes(string name, string value)
        {
            Debug.WriteLine(nameof(AttributeIncludes) + name + value);
            return nodes => DebugNodes(nodes).Where(n => (n.TryGetAttribute(name, out var val) && val.Split(' ').Contains(value)));
        }

        public Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>> AttributeDashMatch(string name, string value)
        {
            Debug.WriteLine(nameof(AttributeDashMatch) + name + value);
            return string.IsNullOrEmpty(value)
                 ? (Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>>)(nodes => Enumerable.Empty<SvgElement>())
                 : (nodes => DebugNodes(nodes).Where(n => (n.TryGetAttribute(name, out var val) && val.Split('-').Contains(value))));
        }

        public Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>> AttributePrefixMatch(string name, string value)
        {
            Debug.WriteLine(nameof(AttributePrefixMatch) + name + value);
            return string.IsNullOrEmpty(value)
                 ? (Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>>)(nodes => Enumerable.Empty<SvgElement>())
                 : (nodes => DebugNodes(nodes).Where(n => (n.TryGetAttribute(name, out var val) && val.StartsWith(value))));
        }

        public Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>> AttributeSuffixMatch(string name, string value)
        {
            Debug.WriteLine(nameof(AttributeSuffixMatch) + name + value);
            return string.IsNullOrEmpty(value)
                 ? (Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>>)(nodes => Enumerable.Empty<SvgElement>())
                 : (nodes => DebugNodes(nodes).Where(n => (n.TryGetAttribute(name, out var val) && val.EndsWith(value))));
        }

        public Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>> AttributeSubstring(string name, string value)
        {
            Debug.WriteLine(nameof(AttributeSubstring) + name + value);
            return string.IsNullOrEmpty(value)
                 ? (Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>>)(nodes => Enumerable.Empty<SvgElement>())
                 : (nodes => DebugNodes(nodes).Where(n => (n.TryGetAttribute(name, out var val) && val.Contains(value))));
        }

        public Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>> FirstChild()
        {
            Debug.WriteLine(nameof(FirstChild));
            return nodes => DebugNodes(nodes).Where(n => n.Parent == null || n.Parent.Children.First() == n);
        }

        public Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>> LastChild()
        {
            Debug.WriteLine(nameof(LastChild));
            return nodes => DebugNodes(nodes).Where(n => n.Parent == null || n.Parent.Children.Last() == n);
        }

        private IEnumerable<T> GetByIds<T>(IList<T> items, IEnumerable<int> indices)
        {
            foreach (var i in indices)
            {
                if (i >= 0 && i < items.Count) yield return items[i];
            }
        }

        public Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>> NthChild(int a, int b)
        {
            Debug.WriteLine(nameof(NthChild) + a + b);
            return nodes => DebugNodes(nodes).Where(n => n.Parent != null && GetByIds(n.Parent.Children, (from i in Enumerable.Range(0, n.Parent.Children.Count / a) select a * i + b)).Contains(n));
        }

        public Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>> OnlyChild()
        {
            Debug.WriteLine(nameof(OnlyChild));
            return nodes => DebugNodes(nodes).Where(n => n.Parent == null || n.Parent.Children.Count == 1);
        }

        public Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>> Empty()
        {
            Debug.WriteLine(nameof(Empty));
            return nodes => DebugNodes(nodes).Where(n => n.Children.Count == 0);
        }

        public Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>> Child()
        {
            Debug.WriteLine(nameof(Child));
            return nodes => DebugNodes(nodes).SelectMany(n => n.Children);
        }

        public Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>> Descendant()
        {
            Debug.WriteLine(nameof(Descendant));
            return nodes => DebugNodes(nodes).SelectMany(n => Descendants(n));
        }

        private IEnumerable<SvgElement> Descendants(SvgElement elem)
        {
            foreach (var child in elem.Children)
            {
                yield return child;
                foreach (var descendant in child.Descendants())
                {
                    yield return descendant;
                }
            }
        }

        public Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>> Adjacent()
        {
            Debug.WriteLine(nameof(Adjacent));
            return nodes => DebugNodes(nodes).SelectMany(n => ElementsAfterSelf(n).Take(1));
        }

        public Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>> GeneralSibling()
        {
            Debug.WriteLine(nameof(GeneralSibling));
            return nodes => DebugNodes(nodes).SelectMany(n => ElementsAfterSelf(n));
        }

        private IEnumerable<SvgElement> ElementsAfterSelf(SvgElement self)
        {
            return (self.Parent == null ? Enumerable.Empty<SvgElement>() : self.Parent.Children.Skip(self.Parent.Children.IndexOf(self) + 1));
        }

        public Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>> NthLastChild(int a, int b)
        {
            Debug.WriteLine(nameof(NthLastChild));
            throw new NotImplementedException();
        }
    }
}
