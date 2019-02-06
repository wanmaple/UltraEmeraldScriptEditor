using EditorSupport.Document;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace EditorSupport.Rendering
{
    public sealed class VisualLineImageCache
    {
        #region Singleton
        public static VisualLineImageCache GetInstance()
        {
            if (_instance == null)
            {
                _instance = new VisualLineImageCache();
            }
            return _instance;
        }

        public static void PurgeInstance()
        {
            _instance = null;
            GC.Collect();
        }

        private static VisualLineImageCache _instance;

        private VisualLineImageCache()
        {
            _cache = new Dictionary<TextDocument, Dictionary<DocumentLine, DrawingImage>>();
        }
        #endregion

        public DrawingImage GetCache(TextDocument doc, DocumentLine line)
        {
            if (!_cache.ContainsKey(doc))
            {
                return null;
            }
            var docCache = _cache[doc];
            if (!docCache.ContainsKey(line))
            {
                return null;
            }
            return docCache[line];
        }

        public void AddCache(TextDocument doc, DocumentLine line, DrawingImage img)
        {
            Dictionary<DocumentLine, DrawingImage> docCache = null;
            if (!_cache.ContainsKey(doc))
            {
                _cache.Add(doc, new Dictionary<DocumentLine, DrawingImage>());
            }
            docCache = _cache[doc];
            if (docCache.ContainsKey(line))
            {
                docCache[line] = img;
            }
            else
            {
                docCache.Add(line, img);
            }
        }

        public void RemoveCache(TextDocument doc, DocumentLine line)
        {
            if (_cache.ContainsKey(doc))
            {
                var docCache = _cache[doc];
                docCache.Remove(line);
            }
        }

        public void RemoveCache(TextDocument doc)
        {
            _cache.Remove(doc);
        }

        public void Clear()
        {
            _cache.Clear();
        }

        private Dictionary<TextDocument, Dictionary<DocumentLine, DrawingImage>> _cache;
    }
}
