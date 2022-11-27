using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


public static class ShapeCashe
{
    //holds convex polys made with 4 points.

    static (int pointNum, List<ConvexPolygon> convex)[] ConvexCashe = new (int pointNum, List<ConvexPolygon> convex)[]
    {
        (3, new List<ConvexPolygon>(50)),
        (4, new List<ConvexPolygon>(50)),
        (5, new List<ConvexPolygon>(50)),
    };

    static List<CircleShape> CircleCashe = new List<CircleShape>(50);

    public static void TryCasheConvex (ConvexPolygon poly)
    {
        int count = poly.GetOriginalModel().Length;

        for(int i = 0; i < ConvexCashe.Length; ++i)
        {
            var curr = ConvexCashe[i];

            if(count == curr.pointNum)
            {
                curr.convex.Add(poly);

                break;
            }
        }
    }

    public static bool TryGetConvex (int pointsCount, out ConvexPolygon poly)
    {
        List<ConvexPolygon> convex;

        for(int i = 0; i < ConvexCashe.Length; ++i)
        {
            var curr = ConvexCashe[i];

            if(pointsCount == curr.pointNum)
            {
                convex = curr.convex;

                goto start;
            }
        }

        poly = null;
        return false;

        start:

        int cCount = convex.Count;

        bool hasIt = cCount != 0;

        if(hasIt)
        {
            int pos = cCount - 1;

            poly = convex[pos];
            convex.RemoveAt(pos);
            
            goto end;
        }

        poly = null;

        end:
        return hasIt;
    }

    public static void CasheCircle (CircleShape circle)
    {
        CircleCashe.Add(circle);
    }

    public static bool TryGetCircle (out CircleShape circle)
    {
        int cInd = CircleCashe.Count - 1;
        if(cInd != -1)
        {
            circle = CircleCashe[cInd];
            CircleCashe.RemoveAt(cInd);

            return true;
        }

        circle = null;
        return false;
    }
}