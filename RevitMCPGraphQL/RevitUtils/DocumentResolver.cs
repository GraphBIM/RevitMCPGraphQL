using System;
using System.IO;
using System.Linq;
using Autodesk.Revit.DB;

namespace RevitMCPGraphQL.RevitUtils
{
    public static class DocumentResolver
    {
        /// <summary>
        /// Resolve the target document: if <paramref name="documentName"/> is null or empty, returns <paramref name="hostDoc"/>.
        /// Otherwise, searches loaded Revit links by title/instance name/path for a match.
        /// Returns host document if no match is found.
        /// </summary>
        public static Document? ResolveDocument(Document? hostDoc, string? documentName)
        {
            if (hostDoc == null) return null;
            if (string.IsNullOrWhiteSpace(documentName)) return hostDoc;

            var target = FindLinkByName(hostDoc, documentName!);
            return target ?? hostDoc;
        }

        /// <summary>
        /// Resolve the target document by RevitLinkInstance element id in the host document.
        /// If <paramref name="documentId"/> is null or not positive, returns <paramref name="hostDoc"/>.
        /// Returns host document if no matching link is found or it is not loaded.
        /// </summary>
        public static Document? ResolveDocument(Document? hostDoc, long? documentId)
        {
            if (hostDoc == null) return null;
            if (!documentId.HasValue || documentId.Value <= 0) return hostDoc;

            try
            {
                var el = hostDoc.GetElement(new ElementId(documentId.Value)) as RevitLinkInstance;
                var ld = el?.GetLinkDocument();
                return ld ?? hostDoc;
            }
            catch
            {
                return hostDoc;
            }
        }

        private static Document? FindLinkByName(Document hostDoc, string name)
        {
            var nameLower = name.Trim().ToLowerInvariant();

            var links = new FilteredElementCollector(hostDoc)
                .OfClass(typeof(RevitLinkInstance))
                .Cast<RevitLinkInstance>()
                .ToList();

            foreach (var li in links)
            {
                var ld = li.GetLinkDocument();
                if (ld == null) continue; // not loaded

                var title = ld.Title ?? string.Empty;
                var instName = li.Name ?? string.Empty;
                var path = ld.PathName ?? string.Empty;
                var fileName = Path.GetFileName(path) ?? string.Empty;
                var fileStem = Path.GetFileNameWithoutExtension(path) ?? string.Empty;

                if (string.Equals(title, name, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(instName, name, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(fileName, name, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(fileStem, name, StringComparison.OrdinalIgnoreCase))
                {
                    return ld;
                }
            }

            // Fallback: contains match
            foreach (var li in links)
            {
                var ld = li.GetLinkDocument();
                if (ld == null) continue;
                var title = (ld.Title ?? string.Empty).ToLowerInvariant();
                var instName = (li.Name ?? string.Empty).ToLowerInvariant();
                var fileName = (Path.GetFileName(ld.PathName) ?? string.Empty).ToLowerInvariant();
                var fileStem = (Path.GetFileNameWithoutExtension(ld.PathName) ?? string.Empty).ToLowerInvariant();
                if (title.Contains(nameLower) || instName.Contains(nameLower) || fileName.Contains(nameLower) || fileStem.Contains(nameLower))
                {
                    return ld;
                }
            }

            return null;
        }
    }
}
