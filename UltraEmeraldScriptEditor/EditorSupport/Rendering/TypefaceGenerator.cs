using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace EditorSupport.Rendering
{
    public sealed class TypefaceGenerator
    {
        private class TypefaceLibrary
        {
            public Typeface Typeface { get; set; }
            public GlyphTypeface GlyphTypeface { get; set; }
        }

        #region Singleton
        public static TypefaceGenerator GetInstance()
        {
            if (_instance == null)
            {
                _instance = new TypefaceGenerator();
            }
            return _instance;
        }

        public static void PurgeInstance()
        {
            _instance = null;
            GC.Collect();
        }

        private static TypefaceGenerator _instance;

        private TypefaceGenerator()
        {
            _cache = new Dictionary<int, TypefaceLibrary>();
        }
        #endregion

        public Typeface GenerateTypeface(FontFamily ff, FontStyle style, FontWeight weight, FontStretch stretch)
        {
            Int32 hash = Hash(ff, style, weight, stretch);
            if (!_cache.ContainsKey(hash))
            {
                var typeface = new Typeface(ff, style, weight, stretch);
                GlyphTypeface glyphTypeface;
                typeface.TryGetGlyphTypeface(out glyphTypeface);
                _cache.Add(hash, new TypefaceLibrary
                {
                    Typeface = typeface,
                    GlyphTypeface = glyphTypeface,
                });
            }
            var lib = _cache[hash];
            return lib.Typeface;
        }

        public GlyphTypeface GenerateGlyphTypeface(FontFamily ff, FontStyle style, FontWeight weight, FontStretch stretch)
        {
            Int32 hash = Hash(ff, style, weight, stretch);
            if (!_cache.ContainsKey(hash))
            {
                var typeface = new Typeface(ff, style, weight, stretch);
                GlyphTypeface glyphTypeface;
                typeface.TryGetGlyphTypeface(out glyphTypeface);
                _cache.Add(hash, new TypefaceLibrary
                {
                    Typeface = typeface,
                    GlyphTypeface = glyphTypeface,
                });
            }
            var lib = _cache[hash];
            return lib.GlyphTypeface;
        }

        private Int32 Hash(FontFamily ff, FontStyle style, FontWeight weight, FontStretch stretch)
        {
            unchecked
            {
                return ff.GetHashCode() + style.GetHashCode() * 1229 + weight.GetHashCode() * 4373 + stretch.GetHashCode() * 9803;
            }
        }

        private Dictionary<Int32, TypefaceLibrary> _cache;
    }
}
