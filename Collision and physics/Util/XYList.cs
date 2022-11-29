using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GJham.Physics.Util;

public class XYList<T> where T:XYBoolHolder
{
    long Multiple;

    int CapacityY;

    EqualityComparer<T> EqComparer = EqualityComparer<T>.Default;

    WTFDictionary<long, WTFDictionary<long, OneWayNode<T>>> DictX;

    ///<summary>
    ///'capacityY' should always be a very small number like 10.
    ///</summary>
    public XYList(int gridSeparation, int capacityX, int capacityY)
    {
        Multiple = gridSeparation;

        DictX = new WTFDictionary<long, WTFDictionary<long, OneWayNode<T>>>(capacityX);

        CapacityY = capacityY;
    }

    public long[] AddNode(T value, Vector2 topLeft, Vector2 bottomRight)
    {
        var ranges = GetRanges(topLeft, bottomRight);

        long x1 = ranges[0], x2 = ranges[1], y1 = ranges[2], y2 = ranges[3];

        while (x1 <= x2)
        {
            bool dictExisted;

            var rDict1 = GetDictionary();

            var dictY = DictX.AddIfNonexist(x1, rDict1, out dictExisted);

            if(dictExisted) CasheDictionary(rDict1);

            long y1f = y1;

            while(y1f <= y2)
            {
                bool nodeExisted;

                var valueNode = new OneWayNode<T>(value, value.ID);

                var node = dictY.AddIfNonexist(y1f, valueNode, out nodeExisted);

                if(nodeExisted)
                {
                    if(valueNode.Priority < node.Priority)
                    {
                        valueNode.Append(node);
                        dictY[y1f] = valueNode;
                    }
                    else node.Add(valueNode);
                }

                y1f += 1;
            }

            x1 += 1;
        }

        return ranges;
    }

    public long[] AddNode(T value, long[] ranges)
    {
        long x1 = ranges[0], x2 = ranges[1], y1 = ranges[2], y2 = ranges[3];

        //while (x1 <= x2)
        for(; x1 <= x2; ++x1)
        {
            bool dictExisted;

            var rDict1 = GetDictionary();

            var dictY = DictX.AddIfNonexist(x1, rDict1, out dictExisted);

            if(dictExisted) CasheDictionary(rDict1);


            for(long y1f = y1; y1f <= y2; ++y1f)
            {
                bool nodeExisted;

                var valueNode = new OneWayNode<T>(value, value.ID);

                var node = dictY.AddIfNonexist(y1f, valueNode, out nodeExisted);

                if(nodeExisted)
                {
                    if(valueNode.Priority < node.Priority)
                    {
                        valueNode.Append(node);
                        dictY[y1f] = valueNode;
                    }
                    else node.Add(valueNode);
                }
            }
        }

        if(x1 != x2 + 1) throw new Exception("Lol???");

        //Console.WriteLine($"Added x: {x1}-{x2}, y: {y1}-{y2}");

        return ranges;
    }

    public T[] GetValues (long[] ranges)
    {
        T[] container = new T[10];

        int cInd = 0, cSize = 10;

        long x1 = ranges[0], x2 = ranges[1], y1 = ranges[2], y2 = ranges[3];

        for(; x1 <= x2; ++x1)
        {

            var dictY = DictX[x1];

            long y1f = y1;

            while(y1f <= y2)
            {
                var node = dictY[y1f];

                while(node!= null)
                {
                    T valu = node.Value;

                    if(!valu.SelectedC)
                    {
                        valu.SelectedC = true;

                        if(cInd == cSize)
                        {
                            cSize += 10;

                            Array.Resize(ref container, cSize);
                        }

                        container[cInd] = valu;

                        ++cInd;
                    }

                    node = node.down;
                }

                y1f += 1;
            }
        }

        Array.Resize(ref container, cInd);

        for(int i = 0; i< cInd; ++i)
        {
            container[i].SelectedC = false;
        }

        return container;
    }

    public void RemoveValue (long[] ranges, T value)
    {
        (int keyX, int keyY)[] container = new (int keyX, int keyY)[10];

        int cInd = 0, cSize = 10;

        long x1 = ranges[0], x2 = ranges[1], y1 = ranges[2], y2 = ranges[3];

        //int ikX = DictX.GetInternalKey(x1);

        for(; x1 <= x2; x1 += 1)
        {
            //var v1 = DictX.GetValueOfIK(ikX);

            var keyX = (int)x1;

            var dictY = DictX[x1];

            //int ikY = dictY.GetInternalKey(y1);

            for(long y1f = y1; y1f <= y2; y1f += 1)
            {
                var node = dictY[y1f];

                var keyY = (int)y1f;

                bool first = node.down == null;

                OneWayNode<T> lastNode = null;

                while(node.down != null)
                {
                    if(EqComparer.Equals(value, node.Value)) break;

                    lastNode = node;
                    node = node.down;
                }

                if(first)
                {
                    if(cInd == cSize)
                    {
                        cSize += 10;

                        Array.Resize(ref container, cSize);
                    }

                    container[cInd] = (keyX, keyY);

                    ++cInd;
                }
                else
                {
                    if(lastNode == null)
                    {
                        //dictY.SetValueOfIK(ikY, node.down);
                        dictY[y1f] = node.down;
                        node.down = null;
                    }
                    else
                    {
                        lastNode.down = node.down;

                        node.down = null;
                    }
                }

                //ikY += 1;
            }

            //ikX+=1;
        }

        if(cInd > 0) for (int i = 0; i < cInd; ++i)
        {
            var toRemove = container[i];

            var dictY = DictX[toRemove.keyX];

            dictY.Remove(toRemove.keyY);

            if(dictY.Count == 0)
            {
                //SOLVED: something with the empty dictionaries
                //that doesn't cause the computer pain,
                //because this does.

                CasheDictionary(dictY);

                DictX.Remove(toRemove.keyX);
            }
        }
    }

    ///<summary>
    ///Gets x1,x2,y1,y2;
    ///</summary>
    public long[] GetRanges(Vector2 topLeft, Vector2 bottomRight)
    {
        long[] XYR = new long[4]
        {
            (long)topLeft.x,
            (long)bottomRight.x,
            (long)topLeft.y,
            (long)bottomRight.y
        };

        for(int i = 0; i < 4; ++i)
        {
            var r = XYR[i];

            if(r == 0) continue;

            XYR[i] = r / Multiple;
        }

        return XYR;
    }

    public long[] GetRanges(Vector2 topLeft, Vector2 bottomRight, long[] ranges)
    {
        ranges[0] = (long)topLeft.x;
        ranges[1] = (long)bottomRight.x;
        ranges[2] = (long)topLeft.y;
        ranges[3] = (long)bottomRight.y;

        for(int i = 0; i < 4; ++i)
        {
            var r = ranges[i];

            if(r == 0) continue;

            ranges[i] = r / Multiple;
        }

        return ranges;
    }

    public bool RangesDiffer (long[] currRanges, Vector2 nextTopLeft, Vector2 nextBottomRight)
    {
        long r0 = (long)nextTopLeft.x;
        long r1 = (long)nextBottomRight.x;
        long r2 = (long)nextTopLeft.y;
        long r3 = (long)nextBottomRight.y;

        return
        !(
            currRanges[0] == r0 &&
            currRanges[1] == r1 &&
            currRanges[2] == r2 &&
            currRanges[3] == r3
        );
    }

    Stack<WTFDictionary<long, OneWayNode<T>>> DictCashe = new Stack<WTFDictionary<long, OneWayNode<T>>>(50);

    private WTFDictionary<long, OneWayNode<T>> GetDictionary()
    {
        WTFDictionary<long, OneWayNode<T>> value;

        if(DictCashe.TryPop(out value)) return value;
        else return new WTFDictionary<long, OneWayNode<T>>(CapacityY);
    }

    private void CasheDictionary(WTFDictionary<long, OneWayNode<T>> dict)
    {
        dict.Clear();
        DictCashe.Push(dict);
    }
}

class OneWayNode<T>
{
    public T Value;

    public int Priority;

    public OneWayNode<T> down;

    public OneWayNode(T value, int priority)
    {
        Value = value;

        Priority = priority;

        down = null;
    }

    public void Add(OneWayNode<T> value)
    {
        OneWayNode<T> lastNode = null;

        OneWayNode<T> oneNode = this;

        while(oneNode.down != null)
        {
            if(value.Priority < oneNode.Priority)
            {
                value.down = oneNode;
                lastNode.down = value;
            }
            lastNode = oneNode;
            oneNode = oneNode.down;
        }

        oneNode.down = value;
    }

    public void Append(OneWayNode<T> value)
    {
        down = value;
    }
}