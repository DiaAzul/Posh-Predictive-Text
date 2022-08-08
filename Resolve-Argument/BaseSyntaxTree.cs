using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResolveArgument
{
    internal class Element
    {
        internal string Name { get; set; }
        internal string Type { get; set; }
        internal string[] Next { get; set; }

        internal Element(string name, string type, string[] next)
        {
            this.Name = name;
            this.Type = type;
            this.Next = next;
        }
    }

    internal class BaseSyntaxTree
    {
        private List<Element> Elements = new();

        internal BaseSyntaxTree()
        {
        //    Elements.Add(new("conda", "thing", ["search", "b"]));
        //    Elements.Add(new("search", "thing", ["-f", "b"]));
        }
    }
}
