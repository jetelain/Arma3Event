using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Arma3Event.Arma3GameInfos
{
    public static class GameMarkerColorHelper
    {
        private static readonly Dictionary<GameMarkerColor,double[]> Colors = 
            new Dictionary<GameMarkerColor,double[]>() {
                {GameMarkerColor.ColorBlack 	, new double[]{0,0,0,1}},
                {GameMarkerColor.ColorGrey 	, new double[]{0.5,0.5,0.5,1}},
                {GameMarkerColor.ColorRed 	, new double[]{0.9,0,0,1}},
                {GameMarkerColor.ColorBrown 	, new double[]{0.5,0.25,0,1}},
                {GameMarkerColor.ColorOrange 	, new double[]{0.85,0.4,0,1}},
                {GameMarkerColor.ColorYellow 	, new double[]{0.85,0.85,0,1}},
                {GameMarkerColor.ColorKhaki 	, new double[]{0.5,0.6,0.4,1}},
                {GameMarkerColor.ColorGreen 	, new double[]{0,0.8,0,1}},
                {GameMarkerColor.ColorBlue 	, new double[]{0,0,1,1}},
                {GameMarkerColor.ColorPink 	, new double[]{1,0.3,0.4,1}},
                {GameMarkerColor.ColorWhite 	, new double[]{1,1,1,1}},
                {GameMarkerColor.ColorUNKNOWN 	, new double[]{0.7,0.6,0,1}},
                {GameMarkerColor.colorBLUFOR 	, new double[]{0,0.3,0.6,1}},
                {GameMarkerColor.colorOPFOR 	, new double[]{0.5,0,0,1}},
                {GameMarkerColor.colorIndependent 	, new double[]{0,0.5,0,1}},
                {GameMarkerColor.colorCivilian 	, new double[]{0.4,0,0.5,1}}
        };

        public static string ToHexa(this GameMarkerColor color)
        {
            var data = Colors[color];
            return $"{(int)(data[0]*255):X2}{(int)(data[1] * 255):X2}{(int)(data[2] * 255):X2}";
        }
        public static double[] ToColor(this GameMarkerColor color)
        {
            return Colors[color];
        }
        public static IEnumerable<GameMarkerColor> GetAll()
        {
            return Enum.GetValues(typeof(GameMarkerColor)).Cast<GameMarkerColor>();
        }
    }
}
