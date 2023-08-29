using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Xml.Linq;
using Fizzler;

namespace Svg.Css
{
    internal class SvgElementOps : IElementOps<SvgElement>
    {
        private readonly SvgElementFactory _elementFactory;

        public SvgElementOps(SvgElementFactory elementFactory)
        {
            _elementFactory = elementFactory;
        }

        public Selector<SvgElement> Type(NamespacePrefix prefix, string name)
        {
            Debug.WriteLine(nameof(Type) + name);
            if (_elementFactory.AvailableElementsDictionary.TryGetValue(name, out var types))
            {
                return nodes => DebugNodes(nodes, name).Where(n => types.Contains(n.GetType()));
            }
            return nodes => Enumerable.Empty<SvgElement>();
        }

        public IEnumerable<SvgElement> DebugNodes(IEnumerable<SvgElement> nodes, string param, [CallerMemberName] string caller = null)
        {
            Debug.WriteLine(nameof(DebugNodes) + ": " + caller + param);

            if (caller != NodeDebug)
            {
                foreach (var it in nodes)
                {
                    yield return it;
                }

                yield break;
            }

            foreach (var it in nodes)
            {
                Debug.WriteLine(it.ElementName);
                yield return it;
            }
        }

        public static string NodeDebug { get; set; }

        public Selector<SvgElement> Universal(NamespacePrefix prefix)
        {
            Debug.WriteLine(nameof(Universal));
            return nodes => DebugNodes(nodes, string.Empty);
        }

        public Selector<SvgElement> Id(string id)
        {
            Debug.WriteLine(nameof(Id) + id);
            return nodes => DebugNodes(nodes, id).Where(n => n.ID == id);
        }

        public Selector<SvgElement> Class(string clazz)
        {
            Debug.WriteLine(nameof(Class) + clazz);
            return AttributeIncludes(NamespacePrefix.None, "class", clazz);
        }

        public Selector<SvgElement> AttributeExists(NamespacePrefix prefix, string name)
        {
            Debug.WriteLine(nameof(AttributeExists) + name);
            return nodes => DebugNodes(nodes, name).Where(n => n.ContainsAttribute(name));
        }

        public Selector<SvgElement> AttributeExact(NamespacePrefix prefix, string name, string value)
        {
            Debug.WriteLine(nameof(AttributeExact) + name + value);
            return nodes => DebugNodes(nodes, name + value).Where(n =>
            {
                string val = null;
                return (n.TryGetAttribute(name, out val) && val == value);
            });
        }

        public Selector<SvgElement> AttributeIncludes(NamespacePrefix prefix, string name, string value)
        {
            Debug.WriteLine(nameof(AttributeIncludes) + name + value);
            return nodes => DebugNodes(nodes, name + value).Where(n =>
            {
                string val = null;
                return (n.TryGetAttribute(name, out val) && val.Split(' ').Contains(value));
            });
        }

        public Selector<SvgElement> AttributeDashMatch(NamespacePrefix prefix, string name, string value)
        {
            Debug.WriteLine(nameof(AttributeDashMatch) + name + value);
            return string.IsNullOrEmpty(value)
                 ? (Selector<SvgElement>)(nodes => Enumerable.Empty<SvgElement>())
                 : (nodes => DebugNodes(nodes, name + value).Where(n =>
                    {
                        string val = null;
                        return (n.TryGetAttribute(name, out val) && val.Split('-').Contains(value));
                    }));
        }

        public Selector<SvgElement> AttributePrefixMatch(NamespacePrefix prefix, string name, string value)
        {
            Debug.WriteLine(nameof(AttributePrefixMatch) + name + value);
            return string.IsNullOrEmpty(value)
                 ? (Selector<SvgElement>)(nodes => Enumerable.Empty<SvgElement>())
                 : (nodes => DebugNodes(nodes, name + value).Where(n =>
                     {
                         string val = null;
                         return (n.TryGetAttribute(name, out val) && val.StartsWith(value));
                     }));
        }

        public Selector<SvgElement> AttributeSuffixMatch(NamespacePrefix prefix, string name, string value)
        {
            Debug.WriteLine(nameof(AttributeSuffixMatch) + name + value);
            return string.IsNullOrEmpty(value)
                 ? (Selector<SvgElement>)(nodes => Enumerable.Empty<SvgElement>())
                 : (nodes => DebugNodes(nodes, name + value).Where(n =>
                 {
                     string val = null;
                     return (n.TryGetAttribute(name, out val) && val.EndsWith(value));
                 }));
        }

        public Selector<SvgElement> AttributeSubstring(NamespacePrefix prefix, string name, string value)
        {
            Debug.WriteLine(nameof(AttributeSubstring) + name + value);
            return string.IsNullOrEmpty(value)
                 ? (Selector<SvgElement>)(nodes => Enumerable.Empty<SvgElement>())
                 : (nodes => DebugNodes(nodes, name + value).Where(n =>
                 {
                     string val = null;
                     return (n.TryGetAttribute(name, out val) && val.Contains(value));
                 }));
        }

        public Selector<SvgElement> FirstChild()
        {
            Debug.WriteLine(nameof(FirstChild));
            return nodes => DebugNodes(nodes, string.Empty).Where(n => n.Parent == null || n.Parent.Children.First() == n);
        }

        public Selector<SvgElement> LastChild()
        {
            Debug.WriteLine(nameof(LastChild));
            return nodes => DebugNodes(nodes, string.Empty).Where(n => n.Parent == null || n.Parent.Children.Last() == n);
        }

        private IEnumerable<T> GetByIds<T>(IList<T> items, IEnumerable<int> indices)
        {
            foreach (var i in indices)
            {
                if (i >= 0 && i < items.Count) yield return items[i];
            }
        }

        public Selector<SvgElement> NthChild(int a, int b)
        {
            Debug.WriteLine(nameof(NthChild) + a + b);
            return nodes => DebugNodes(nodes, a.ToString() + b).Where(n => n.Parent != null && GetByIds(n.Parent.Children, (from i in Enumerable.Range(0, n.Parent.Children.Count / a) select a * i + b)).Contains(n));
        }

        public Selector<SvgElement> OnlyChild()
        {
            Debug.WriteLine(nameof(OnlyChild));
            return nodes => DebugNodes(nodes, string.Empty).Where(n => n.Parent == null || n.Parent.Children.Count == 1);
        }

        public Selector<SvgElement> Empty()
        {
            Debug.WriteLine(nameof(Empty));
            return nodes => DebugNodes(nodes, string.Empty).Where(n => n.Children.Count == 0);
        }

        public Selector<SvgElement> Child()
        {
            Debug.WriteLine(nameof(Child));
            return nodes => DebugNodes(nodes, string.Empty).SelectMany(n => n.Children);
        }

        public Selector<SvgElement> Descendant()
        {
            Debug.WriteLine(nameof(Descendant));
            return nodes => DebugNodes(nodes, string.Empty).SelectMany(n => Descendants(n));
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

        public Selector<SvgElement> Adjacent()
        {
            Debug.WriteLine(nameof(Adjacent));
            return nodes => DebugNodes(nodes, string.Empty).SelectMany(n => ElementsAfterSelf(n).Take(1));
        }

        public Selector<SvgElement> GeneralSibling()
        {
            Debug.WriteLine(nameof(GeneralSibling));
            return nodes => DebugNodes(nodes, string.Empty).SelectMany(n => ElementsAfterSelf(n));
        }

        private IEnumerable<SvgElement> ElementsAfterSelf(SvgElement self)
        {
            return (self.Parent == null ? Enumerable.Empty<SvgElement>() : self.Parent.Children.Skip(self.Parent.Children.IndexOf(self) + 1));
        }

        public Selector<SvgElement> NthLastChild(int a, int b)
        {
            Debug.WriteLine(nameof(NthLastChild));
            throw new NotImplementedException();
        }
    }
}
