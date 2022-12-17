using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GJham.Physics.Util;

/// <summary>
/// Deterministic simulation.
/// WARNING! If a Shape with segmented movement (AKA a Shape with StepAmount higher than 1)
/// is added while the Tick method is running and said Shape moves, it will bug out that Tick,
/// not break, crash or kill the simulation, just bug out a little.
/// </summary>
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

    /// <summary>
    /// Shapes to add to the simulation at the start of tick's code.
    /// </summary>
    /// <typeparam name="int"></typeparam>
    /// <typeparam name="Shape"></typeparam>
    /// <returns></returns>
    WTFDictionary<int, (int id, Shape value)> TickAdd = new WTFDictionary<int, (int id, Shape value)>(50);

    //Ordered dictionary of the list of shapes testing for collision
    //Key is the ID of the object that registered it to make it deterministic
    //Value is the shape in question.
    public WTFDictionary<int, Shape> ShapesDetecting = new WTFDictionary<int,Shape>(100);

    private List<Shape> SegmentedMovementHeap = new List<Shape>(100);
    
    public void Tick()
    {
        var shapesToTick = TickAdd.GetValues();

        for(int i = 0; i < shapesToTick.Length; ++i)
        {
            var curr = shapesToTick[i];
            ShapesDetecting.Add(curr.id, curr.value);
        }

        TickAdd.Clear();



        var colObjs = ShapesDetecting.GetValues();



        Span<Vector2> lastPositionCache = stackalloc Vector2[colObjs.Length];

        for(int i = 0; i< colObjs.Length; ++i)
        {
            lastPositionCache[i] = colObjs[i].Position;
        }

        var movementHeap = SegmentedMovementHeap;

        CollisionResult result = new CollisionResult();

        for(int i1 = 0; i1 < colObjs.Length; ++i1)
        {
            var currShape = colObjs[i1];

            if(!currShape.IsActive) continue;

            Shape[] closeShapes;

            if(currShape.StepAmount > 1)
            {
                if(currShape.IntersectOnly)
                {
                    int stepAmount = currShape.StepAmount;

                    Vector2 unsegmentedMov = currShape.Position - currShape.LastPosition;

                    Vector2 movSegment = unsegmentedMov / stepAmount;

                    Vector2 movSegModulus = unsegmentedMov - unsegmentedMov * stepAmount;

                    currShape.Position = currShape.LastPosition + movSegment;

                    closeShapes = Shape.GetShapesInGrid(currShape, Grid);
                    
                    for(int i2 = 0; i2 < closeShapes.Length; ++i2)
                    {
                        var secondShape = closeShapes[i2];

                        IntersectOnlyOperation_Segmented(currShape, secondShape, ref result);
                    }

                    for(int i = 0; i < stepAmount - 2;  ++i)
                    {
                        currShape.Position += movSegment;

                        closeShapes = Shape.GetShapesInGrid(currShape, Grid);

                        for(int i2 = 0; i2 < closeShapes.Length; ++i2)
                        {
                            var secondShape = closeShapes[i2];

                            IntersectOnlyOperation_Segmented(currShape, secondShape, ref result);
                        }
                    }

                    currShape.Position += movSegment + movSegModulus;

                    closeShapes = Shape.GetShapesInGrid(currShape, Grid);
                    
                    for(int i2 = 0; i2 < closeShapes.Length; ++i2)
                    {
                        var secondShape = closeShapes[i2];

                        IntersectOnlyOperation_Segmented(currShape, secondShape, ref result);
                    }

                    SegmentedMovementHeap.Clear();
                }
                else
                {
                    int stepAmount = currShape.StepAmount;

                    Vector2 unsegmentedMov = currShape.Position - currShape.LastPosition;

                    Vector2 movSegment = unsegmentedMov / stepAmount;

                    Vector2 movSegModulus = unsegmentedMov - unsegmentedMov * stepAmount;

                    currShape.Position = currShape.LastPosition + movSegment;

                    closeShapes = Shape.GetShapesInGrid(currShape, Grid);
                    
                    for(int i2 = 0; i2 < closeShapes.Length; ++i2)
                    {
                        var secondShape = closeShapes[i2];

                        if (secondShape.IntersectOnly) IntersectOnlyOperation_Segmented(currShape, secondShape, ref result);
                        else CollisionOperation_Segmented(currShape, secondShape, ref result);
                    }

                    for(int i = 0; i < stepAmount - 2;  ++i)
                    {
                        currShape.Position += movSegment;

                        closeShapes = Shape.GetShapesInGrid(currShape, Grid);

                        for(int i2 = 0; i2 < closeShapes.Length; ++i2)
                        {
                            var secondShape = closeShapes[i2];

                            if (secondShape.IntersectOnly) IntersectOnlyOperation_Segmented(currShape, secondShape, ref result);
                            else CollisionOperation_Segmented(currShape, secondShape, ref result);
                        }
                    }

                    currShape.Position += movSegment + movSegModulus;

                    closeShapes = Shape.GetShapesInGrid(currShape, Grid);
                    
                    for(int i2 = 0; i2 < closeShapes.Length; ++i2)
                    {
                        var secondShape = closeShapes[i2];

                        if (secondShape.IntersectOnly) IntersectOnlyOperation_Segmented(currShape, secondShape, ref result);
                        else CollisionOperation_Segmented(currShape, secondShape, ref result);
                    }

                    SegmentedMovementHeap.Clear();
                }

                continue;
            }

            closeShapes = Shape.GetShapesInGrid(currShape, Grid);
            
            if(currShape.IntersectOnly)
            {
                for (int i2 = 0; i2 < closeShapes.Length; ++i2)
                {
                    var secondShape = closeShapes[i2];

                    IntersectOnlyOperation(currShape, secondShape, ref result);
                }
                
            }
            else
            {
                for (int i2 = 0; i2 < closeShapes.Length; ++i2)
                {
                    var secondShape = closeShapes[i2];

                    if(secondShape.IntersectOnly)
                    {
                        IntersectOnlyOperation(currShape, secondShape, ref result);
                    }
                    else
                    {
                        CollisionOperation(currShape, secondShape, ref result);
                    }
                }

            }
        }

        for(int i = 0; i< colObjs.Length; ++i)
        {
            colObjs[i].LastPosition = lastPositionCache[i];
        }
    }

    public void TickShape(int id, Shape shape)
    {
        TickAdd.Add(id, (id, shape));
        //ShapesDetecting.Add(id, shape);
    }

    public void DontTickShape(int id)
    {
        ShapesDetecting.TryRemove(id);
        TickAdd.TryRemove(id);
    }

    private void IntersectOnlyOperation(Shape currShape, Shape secondShape, ref CollisionResult result)
    {
        if(currShape == secondShape || !secondShape.IsActive) return;

        bool intersects = currShape.Intersect(secondShape);
        if(!intersects) return;
        result.Intersects = intersects;

        currShape.ObjectUsingIt.OnCollision(secondShape.ObjectUsingIt, ref result);
        secondShape.ObjectUsingIt.OnCollision(currShape.ObjectUsingIt, ref result);

        return;
    }

    private void CollisionOperation(Shape currShape, Shape secondShape, ref CollisionResult result)
    {
        currShape.IntersectsInfo(secondShape, ref result);

        if(!result.Intersects || currShape == secondShape || !secondShape.IsActive) return;

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

    private void IntersectOnlyOperation_Segmented(Shape currShape, Shape secondShape, ref CollisionResult result)
    {
        if(currShape == secondShape || !secondShape.IsActive || SegmentedMovementHeap.Contains(secondShape)) return;

        bool intersects = currShape.Intersect(secondShape);
        if(!intersects) return;
        result.Intersects = intersects;

        currShape.ObjectUsingIt.OnCollision(secondShape.ObjectUsingIt, ref result);
        secondShape.ObjectUsingIt.OnCollision(currShape.ObjectUsingIt, ref result);

        SegmentedMovementHeap.Add(secondShape);
    }

    private void CollisionOperation_Segmented(Shape currShape, Shape secondShape, ref CollisionResult result)
    {
        currShape.IntersectsInfo(secondShape, ref result);

        if(!result.Intersects || currShape == secondShape || !secondShape.IsActive) return;

        if(secondShape.ObjectUsingIt == null) throw new Exception("No object using the shape.");

        var currUsingIt = currShape.ObjectUsingIt;
        var secondUsingIt = secondShape.ObjectUsingIt;

        currUsingIt.ResolveOverlap(secondUsingIt, ref result);

        result.Separation *= -1;
        result.SeparationDirection *= -1;

        secondUsingIt.ResolveOverlap(currUsingIt, ref result);

        result.Separation *= -1;
        result.SeparationDirection *= -1;

        if(SegmentedMovementHeap.Contains(secondShape)) return;

        currUsingIt.OnCollision(secondUsingIt, ref result);

        result.Separation *= -1;
        result.SeparationDirection *= -1;

        secondUsingIt.OnCollision(currUsingIt, ref result);

        SegmentedMovementHeap.Add(secondShape);
    }
}