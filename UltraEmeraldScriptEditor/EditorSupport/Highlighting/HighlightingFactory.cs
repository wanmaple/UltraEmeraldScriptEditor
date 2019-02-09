using EditorSupport.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml;

namespace EditorSupport.Highlighting
{
    public sealed class HighlightingFactory
    {
        private class HighlightLibrary
        {
            public IHighlightRuler HighlightRuler { get; set; }
            public IHighlighter Highlighter { get; set; }
        }

        #region Singleton
        public static HighlightingFactory GetInstance()
        {
            if (_instance == null)
            {
                _instance = new HighlightingFactory();
            }
            return _instance;
        }

        public static void PurgeInstance()
        {
            _instance = null;
            GC.Collect();
        }

        private static HighlightingFactory _instance;

        private HighlightingFactory()
        {
            _libraries = new Dictionary<string, HighlightLibrary>();

            _rulerCreators = new Dictionary<string, Func<XmlNode, IHighlightRuler>>();
            RegisterInnerRulerCreators();
        }
        #endregion

        public IHighlightRuler GetHighlightRuler(String key)
        {
            var library = GetLibrary(key);
            return library.HighlightRuler;
        }

        public IHighlighter GetHighlighter(String key)
        {
            var library = GetLibrary(key);
            return library.Highlighter;
        }

        public void RegisterRulerCreator(String key, Func<XmlNode, IHighlightRuler> creator)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            if (creator == null)
            {
                throw new ArgumentNullException("creator");
            }
            if (_rulerCreators.ContainsKey(key))
            {
                throw new ArgumentException(String.Format("\"{0}\" ruler creator already existed.", key));
            }

            _rulerCreators.Add(key, creator);
        }

        private HighlightLibrary GetLibrary(String key)
        {
            if (!_libraries.ContainsKey(key))
            {
                String xmlPath = Path.Combine("Configs", key + ".xml");
                if (!File.Exists(xmlPath))
                {
                    throw new IOException(String.Format("\"{0}\" doesn't exist in config directory.", key));
                }
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlPath);
                var library = ParseLibraryFromXml(xmlDoc);
                _libraries.Add(key, library);
            }
            return _libraries[key];
        }

        private HighlightLibrary ParseLibraryFromXml(XmlDocument xmlDoc)
        {
            XmlNode root = xmlDoc.SelectSingleNode("/root");
            if (root == null)
            {
                throw new XmlException("Invalid highlighting config.");
            }
            // ruler
            IHighlightRuler ruler = null;
            XmlNode rulerNode = root.SelectSingleNode("ruler");
            if (rulerNode == null)
            {
                throw new XmlException("Invalid highlighting config.");
            }
            XmlNode rulerTypeAttrNode = rulerNode.Attributes["type"];
            if (rulerTypeAttrNode == null)
            {
                throw new XmlException("Invalid highlighting config.");
            }
            String rulerType = rulerTypeAttrNode.Value;
            if (!_rulerCreators.ContainsKey(rulerType))
            {
                throw new InvalidDataException(String.Format("\"{0}\" ruler creator is not registered."));
            }
            var creator = _rulerCreators[rulerType];
            ruler = creator(rulerNode);
            // highlighter
            IHighlighter highlighter = null;
            XmlNode highlighterNode = root.SelectSingleNode("styles");
            if (highlighterNode == null)
            {
                throw new XmlException("Invalid highlighting config.");
            }
            highlighter = CreateHighlighterFromNode(highlighterNode);
            return new HighlightLibrary
            {
                Highlighter = highlighter,
                HighlightRuler = ruler,
            };
        }

        private IHighlighter CreateHighlighterFromNode(XmlNode highlighterNode)
        {
            var ret = new CommonHighlighter();
            if (highlighterNode.HasChildNodes)
            {
                Int32 idx = 0;
                foreach (XmlNode child in highlighterNode.SelectNodes("style"))
                {
                    var style = new HighlightStyle();
                    if (child.HasChildNodes)
                    {
                        foreach (XmlNode grandchild in child.ChildNodes)
                        {
                            if (grandchild.NodeType == XmlNodeType.Comment)
                            {
                                continue;
                            }
                            switch (grandchild.Name)
                            {
                                case "foreground":
                                    style.Foreground = CommonUtilities.ColorFromHexString(grandchild.InnerText);
                                    break;
                                case "background":
                                    style.Background = CommonUtilities.ColorFromHexString(grandchild.InnerText);
                                    break;
                                case "fontstyle":
                                    if (_fontStyleMap.ContainsKey(grandchild.InnerText))
                                    {
                                        style.FontStyle = _fontStyleMap[grandchild.InnerText];
                                    }
                                    break;
                                case "fontweight":
                                    if (_fontWeightMap.ContainsKey(grandchild.InnerText))
                                    {
                                        style.FontWeight = _fontWeightMap[grandchild.InnerText];
                                    }
                                    break;
                                case "fontstretch":
                                    if (_fontStretchMap.ContainsKey(grandchild.InnerText))
                                    {
                                        style.FontStretch = _fontStretchMap[grandchild.InnerText];
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    ret.StyleMap.Add(idx, style);
                    ++idx;
                }
            }
            return ret;
        }

        private void RegisterInnerRulerCreators()
        {
            RegisterRulerCreator("regex", RegexRulerCreator);
        }

        #region Ruler creators
        private IHighlightRuler RegexRulerCreator(XmlNode rulerNode)
        {
            var ret = new RegexHighlightRuler();
            XmlNode splitterAttrNode = rulerNode.Attributes["splitter"];
            if (splitterAttrNode != null)
            {
                ret.Splitters.AddRange(splitterAttrNode.Value);
            }
            if (rulerNode.HasChildNodes)
            {
                Int32 idx = 0;
                foreach (XmlNode child in rulerNode.SelectNodes("rule"))
                {
                    XmlNode typeAttrNode = child.Attributes["type"];
                    Dictionary<String, Int32> map = null;
                    if (typeAttrNode != null)
                    {
                        switch (typeAttrNode.Value)
                        {
                            case "regex":
                                map = ret.RegexMap;
                                break;
                            case "prefix":
                                map = ret.PrefixMap;
                                break;
                            default:
                                break;
                        }
                    }
                    else
                    {
                        map = ret.RegexMap;
                    }
                    foreach (XmlNode grandchild in child.ChildNodes)
                    {
                        if (grandchild.NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        map.Add(grandchild.InnerText, idx);
                    }
                    ++idx;
                }
            }
            return ret;
        } 
        #endregion

        private Dictionary<String, Func<XmlNode, IHighlightRuler>> _rulerCreators;
        private Dictionary<String, HighlightLibrary> _libraries;
        private readonly Dictionary<String, FontWeight> _fontWeightMap = new Dictionary<string, FontWeight>
        {
            { "normal", FontWeights.Normal },
            { "bold", FontWeights.Bold },
            { "black", FontWeights.Black },
            { "demi_bold", FontWeights.DemiBold },
            { "thin", FontWeights.Thin },
            { "extra_bold", FontWeights.ExtraBold },
            { "semi_bold", FontWeights.SemiBold },
            { "ultra_bold", FontWeights.UltraBold },
            { "medium", FontWeights.Medium },
            { "extra_black", FontWeights.ExtraBlack },
            { "ultra_black", FontWeights.UltraBlack },
            { "extra_light", FontWeights.ExtraLight },
            { "light", FontWeights.Light },
            { "ultra_light", FontWeights.UltraLight },
            { "heavy", FontWeights.Heavy },
            { "regular", FontWeights.Regular },
        };
        private readonly Dictionary<String, FontStyle> _fontStyleMap = new Dictionary<string, FontStyle>
        {
            { "normal", FontStyles.Normal },
            { "italic", FontStyles.Italic },
            { "oblique", FontStyles.Oblique },
        };
        private readonly Dictionary<String, FontStretch> _fontStretchMap = new Dictionary<string, FontStretch>
        {
            {"normal", FontStretches.Normal },
            {"condensed", FontStretches.Condensed },
            {"expanded", FontStretches.Expanded },
            {"extra_condensed", FontStretches.ExtraCondensed },
            {"extra_expanded", FontStretches.ExtraExpanded },
            {"medium", FontStretches.Medium },
            {"semi_condensed", FontStretches.SemiCondensed },
            {"semi_expanded", FontStretches.SemiExpanded },
            {"ultra_condensed", FontStretches.UltraCondensed },
            {"ultra_expanded", FontStretches.UltraExpanded },
        };
    }
}
