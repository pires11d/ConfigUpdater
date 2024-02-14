using Newtonsoft.Json.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace System
{
    public static class Extensions
    {
        public static bool HasValue(this string value)
        {
            return !string.IsNullOrEmpty(value);
        }

        public static string Fix(this string value)
        {
            if (value == null) return null;

            if (value.StartsWith("\""))
            {
                value = value.Substring(1);
            }

            if (value.EndsWith("\""))
            {
                value = value.Remove(value.Length - 1, 1);
            }

            return value;
        }

        public static string PrettifyXml(this string value)
        {
            StringBuilder stringBuilder = new StringBuilder();
            XElement xElement = XElement.Parse(value);
            XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
            xmlWriterSettings.OmitXmlDeclaration = true;
            xmlWriterSettings.Indent = true;
            using (XmlWriter writer = XmlWriter.Create(stringBuilder, xmlWriterSettings))
            {
                xElement.Save(writer);
            }

            return stringBuilder.ToString();
        }

        public static List<XmlNode> ToList(this XmlNodeList list)
        {
            var nodes = new List<XmlNode>();
            foreach (XmlNode node in list)
            {
                nodes.Add(node);
            }
            return nodes;
        }

        public static string GetValue(this XmlNode node, string path = null)
        {
            if (node is null) return null;

            if (!path.HasValue())
            {
                return node.InnerText;
            }
            else
            {
                return node.SelectSingleNode(path)?.InnerText;
            }
        }

        public static void SetValue(this XmlNode node, string value, string path = null)
        {
            if (node is null) return;

            if (!path.HasValue())
            {
                node.InnerText = value;
            }
            else
            {
                var childNode = node.SelectSingleNode(path);
                if (childNode is not null)
                {
                    childNode.InnerText = value;
                }
            }
        }

        public static XmlNode GetNode(this XmlNodeList nodes, string path, string xid)
        {
            foreach (XmlNode node in nodes)
            {
                if (node.GetValue(path) == xid)
                {
                    return node;
                }
            }
            return null;
        }

        public static List<XmlNode> GetNodes(this XmlNodeList nodes, string path, string xid)
        {
            var nodeList = new List<XmlNode>();
            foreach (XmlNode node in nodes)
            {
                if (node.GetValue(path) == xid)
                {
                    nodeList.Add(node);
                }
            }
            return nodeList;
        }

        public static JObject ReplaceByPath<T>(this JToken root, string path, T newValue)
        {
            if (root == null || path == null)
            {
                throw new ArgumentNullException();
            }

            foreach (var value in root.SelectTokens(path).ToList())
            {
                if (value == root)
                {
                    root = JToken.FromObject(newValue);
                }
                else
                {
                    value.Replace(JToken.FromObject(newValue));
                }
            }

            return (JObject)root;
        }
    }
}
