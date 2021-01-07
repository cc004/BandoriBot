using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace BandoriBot.Services
{
    public class Trie<TValue>
    {
        private class SearchNode
        {
            public Dictionary<char, SearchNode> next = new Dictionary<char, SearchNode>();

            public TValue value;
            public bool hasValue;
        }

        private readonly SearchNode rootNode = new SearchNode();

        public int TrySearch(string text, out TValue value)
        {
            var now = rootNode;
            var length = 0;
            foreach (var c in text)
            {
                if (now.next.TryGetValue(c, out var newnode))
                {
                    now = newnode;
                    ++length;
                }
                else
                    break;
            }

            if (now.hasValue)
                value = now.value;
            else
                value = default;

            return now.hasValue ? length : -1;
        }

        public Tuple<TValue[], string[]> WordSplit(string text)
        {
            var hit = new List<TValue>();
            var unknown = new List<string>();
            int index = 0, lastindex = 0;

            while (index < text.Length)
            {
                var length = TrySearch(text.Substring(index), out TValue result);

                if (length == -1)
                {
                    ++index;
                }
                else
                {
                    if (lastindex < index)
                    {
                        unknown.Add(text.Substring(lastindex, index - lastindex));
                    }
                    hit.Add(result);
                    index += length;
                    lastindex = index;
                }
            }

            return new Tuple<TValue[], string[]>(new HashSet<TValue>(hit).ToArray(), unknown.ToArray());
        }

        public void AddWord(string text, TValue result)
        {
            var node = rootNode;
            foreach (var c in text)
            {
                if (!node.next.TryGetValue(c, out var newnode))
                {
                    var nodenew = new SearchNode();
                    node.next[c] = nodenew;
                    node = nodenew;
                }
                else
                    node = newnode;
            }

            if (node.hasValue)
                throw new InvalidOperationException("duplicated word added");

            node.hasValue = true;
            node.value = result;
        }
    }
}
