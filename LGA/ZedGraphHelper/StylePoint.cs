using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZedGraph;
using System.Drawing;

namespace LGA.ZedGraphManager
{
    public class StylePoint
    {
        public SymbolType type;
        public System.Drawing.Color color;
        public int size;

        //SymbolType.Triangle, System.Drawing.Color.Red
        public static StylePoint getStyleStartPoint()
        {
            StylePoint res = new StylePoint();
            res.type = SymbolType.Triangle;
            res.color = System.Drawing.Color.Red;
            res.size = 20;
            return res;
        }

        //SymbolType.Diamond, System.Drawing.Color.Green
        public static StylePoint getStyleDiffPoint()
        {
            StylePoint res = new StylePoint();
            res.type = SymbolType.Diamond;
            res.color = System.Drawing.Color.Green;
            res.size = 20;
            return res;
        }
    }
}
