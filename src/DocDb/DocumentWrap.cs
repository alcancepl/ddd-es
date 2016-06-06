using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DocDb
{
    public class DocumentWrap<T> //: Document
    {

        static string typeInfo = GetTypeInfo(typeof(T));

        static string GetTypeInfo(Type type)
        {
            var parentTypesList = new List<string>();
            for (Type t = type; t != null && t != typeof(object); t = t.BaseType)
            {
                parentTypesList.Add(TypeToString(t));
            }
            parentTypesList.Reverse(); // from base to sub-types
            return string.Join("|", parentTypesList);
        }

        static string TypeToString(Type t)
        {
            var assemblyName = t.AssemblyQualifiedName.Split(new string[] { "," }, StringSplitOptions.None)[1].Trim() + ".";
            var name = t.ToString();
            if (name.StartsWith(assemblyName))
                return name.Substring(assemblyName.Length);
            return name;
        }

        static string GuidToString(Guid idPart) => idPart.ToString("N");

        /// <summary>
        /// String representation of the complete inheritance hierarchy of {T}.
        /// </summary>
        public static string TypeInfo => typeInfo;

        public static string BuildId(params string[] idParts)
        {
            return $"{typeInfo}|{string.Join("|", idParts)}";
        }

        public static string BuildId(params Guid[] idParts)
        {
            return BuildId(idParts.Select(GuidToString).ToArray());
        }

        /// <summary>
        /// Constructor required by the JSON deserialization.
        /// </summary>
        public DocumentWrap() : base()
        { }

        /// <summary>
        /// Creates a new DocumentWrap{T} with all properties set.
        /// </summary>
        /// <param name="document">the POCO class that will be stored</param>
        /// <param name="idParts">parts of the the unique id of the document</param>
        public DocumentWrap(T document, params string[] idParts)
        {
            Id = BuildId(idParts);
            Document = document;
        }

        /// <summary>
        /// Creates a new DocumentWrap{T} with all properties set.
        /// </summary>
        /// <param name="document">the POCO class that will be stored</param>
        /// <param name="idParts">parts of the unique id of the document</param>
        public DocumentWrap(T document, params Guid[] idParts)
        {
            Id = BuildId(idParts);
            Document = document;
        }

        [JsonProperty("document", TypeNameHandling = TypeNameHandling.All)]
        public T Document { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

    }

}

