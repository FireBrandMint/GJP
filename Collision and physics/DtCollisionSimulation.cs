using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GJham.Physics.Util;


public class DtCollisionSimulation
{
    public int IDNEXT = 0;

    public XYList<Shape> Grid = new XYList<Shape>(50, 1000, 10);

    public int GetId()
    {
        int id = IDNEXT;
        ++IDNEXT;
        return id;
    }

    //Ordered dictionary of the list of shapes testing for collision
    //Key is the ID of the object that registered it to make it deterministic
    //Value is the shape in question.
    public WTFDictionary<int, Shape> ShapesDetecting = new WTFDictionary<int,Shape>(100);
    
    public void Tick()
    {
        var colObjs = ShapesDetecting.GetValues();

        CollisionResult result = new CollisionResult();

        for(int i1 = 0; i1 < colObjs.Length; ++i1)
        {
            var currShape = colObjs[i1];

            if(!currShape.IsActive()) continue;

            var closeShapes = Shape.GetShapesInGrid(currShape, Grid);

            if(currShape.IntersectOnly)
            {
                for (int i2 = 0; i2 < closeShapes.Length; ++i2)
                {
                    var secondShape = closeShapes[i2];

                    if(currShape == secondShape || !secondShape.IsActive()) continue;

                    bool intersects = currShape.Intersect(secondShape);
                    if(!intersects) continue;
                    result.Intersects = intersects;

                    currShape.ObjectUsingIt.OnCollision(secondShape.ObjectUsingIt, ref result);
                    secondShape.ObjectUsingIt.OnCollision(currShape.ObjectUsingIt, ref result);
                }
            }
            else
            {
                for (int i2 = 0; i2 < closeShapes.Length; ++i2)
                {
                    var secondShape = closeShapes[i2];

                    if(currShape == secondShape || !secondShape.IsActive()) continue;

                    if(secondShape.IntersectOnly)
                    {
                        bool intersects = currShape.Intersect(secondShape);
                        if(!intersects) continue;
                        
                        result.Intersects = intersects;
                        currShape.ObjectUsingIt.OnCollision(secondShape.ObjectUsingIt, ref result);
                    }
                    else
                    {
                        currShape.IntersectsInfo(secondShape, ref result);

                        if(!result.Intersects) continue;

                        if(secondShape.ObjectUsingIt == null) throw new Exception("No object using the shape.");

                        var currUsingIt = currShape.ObjectUsingIt;
                        var secondUsingIt = secondShape.ObjectUsingIt;

                        currUsingIt.ResolveOverlap(secondUsingIt, ref result);

                        result.Separation *= -1;
                        result.SeparationDirection *= -1;

                        secondUsingIt.ResolveOverlap(currUsingIt, ref result);

                        result.Separation *= -1;
                        result.SeparationDirection *= -1;

                        currUsingIt.OnCollision(secondUsingIt, ref result);

                        result.Separation *= -1;
                        result.SeparationDirection *= -1;

                        secondUsingIt.OnCollision(currUsingIt, ref result);
                    }
                }

            }
        }
    }
    

    public void TickShape(int id, Shape shape)
    {
        ShapesDetecting.Add(id, shape);
    }

    public void DontTickShape(int id)
    {
        ShapesDetecting.TryRemove(id);
    }
}