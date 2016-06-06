using System;
using System.Linq;

namespace DocDb
{
    public class WrappedId<T>
    {
        public static WrappedId<T> FromType()
        {
            return Build(typeof(T));
        }

        public static WrappedId<T> FromIds(params Guid[] ids)
        {
            return Build(typeof(T), ids.Select(GuidToString).ToArray());
        }

        public static WrappedId<T> FromIds(params string[] ids)
        {
            return Build(typeof(T), ids);
        }

        public static WrappedId<T> FromIds(Guid id1, string id2)
        {
            return Build(typeof(T), GuidToString(id1), id2);
        }

        public static WrappedId<T> FromIds(Guid id1, Guid id2, string id3)
        {
            return Build(typeof(T), GuidToString(id1), GuidToString(id2), id3);
        }

        public static WrappedId<T> FromIds(Guid id1, string id2, string id3)
        {
            return Build(typeof(T), GuidToString(id1), id2, id3);
        }

        public static WrappedId<T> FromDocumentId(string documentId)
        {
            return new WrappedId<T>(documentId);
        }

        // Private

        private readonly string documentId;

        /// <summary>
        /// Returns the id of the document-db document.
        /// </summary>
        /// <returns>the id of the document-db document</returns>
        public override string ToString()
        {
            return documentId;
        }

        public string Id => documentId;

        // private

        static WrappedId<T> Build(Type type, params string[] ids)
        {
            var documentType = DocumentTypeString(type);
            if (ids == null || ids.Length == 0)
                return new WrappedId<T>($"{documentType}{IdDevider}"); // (base) type only
            var documentId = DocumentIdString(ids);
            return new WrappedId<T>($"{documentType}{IdDevider}{documentId}"); // type and id
        }

        private WrappedId(string documentId)
        {
            this.documentId = documentId;
        }

        static string GuidToString(Guid id) => id.ToString("N");

        static string BaseClassTypesDevider = "::";
        static string IdDevider = "|";

        private static string DocumentTypeString(Type theTypeOfTheDocument)
        {
            var baseType = theTypeOfTheDocument.BaseType;
            if (baseType == null)
                return theTypeOfTheDocument.FullName;

            var typeFullName = theTypeOfTheDocument.Name;
            while (baseType != typeof(object))
            {
                typeFullName = baseType.Name + BaseClassTypesDevider + typeFullName;
                baseType = baseType.BaseType;
            }
            return typeFullName;
        }

        private static string DocumentIdString(params string[] selectIds)
        {
            return string.Join(IdDevider, selectIds);
        }
    }
}
