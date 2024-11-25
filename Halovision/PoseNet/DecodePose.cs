using System;

public partial class PoseNet
{

    System.Drawing.PointF GetDisplacement(int edgeId, System.Drawing.Point point, float[,,,] displacements)
    {

        var numEdges = (int)(displacements.GetLength(3) / 2);
        if (!DNN)
        {
          return new System.Drawing.PointF(
              displacements[0, point.Y, point.X, numEdges + edgeId],
              displacements[0, point.Y, point.X, edgeId]);
        }

        return new System.Drawing.PointF(
            displacements[0, numEdges + edgeId, point.Y, point.X],
            displacements[0, edgeId, point.Y, point.X]
        );
    }

    System.Drawing.Point GetStridedIndexNearPoint(
        System.Drawing.PointF point, int outputStride, int height,
        int width)
    {

        return new System.Drawing.Point(
            (int)Mathf.Clamp(Mathf.Round(point.X / outputStride), 0, width - 1),
            (int)Mathf.Clamp(Mathf.Round(point.Y / outputStride), 0, height - 1)
        );
    }

    /**
     * We get a new keypoint along the `edgeId` for the pose instance, assuming
     * that the position of the `idSource` part is already known. For this, we
     * follow the displacement vector from the source to target part (stored in
     * the `i`-t channel of the displacement tensor).
     */

    Keypoint TraverseToTargetKeypoint(
        int edgeId, Keypoint sourceKeypoint, int targetKeypointId,
        float[,,,] scores, float[,,,] offsets, int outputStride,
        float[,,,] displacements)
    {

        var height = 0;
        var width = 0;
        if (!DNN)
        {
          height = scores.GetLength(1);
          width = scores.GetLength(2);
        }
        else
        {
          height = scores.GetLength(2);
          width = scores.GetLength(3);
        }

        // Nearest neighbor interpolation for the source->target displacements.
        var sourceKeypointIndices = GetStridedIndexNearPoint(
            sourceKeypoint.position, outputStride, height, width);

        var displacement =
            GetDisplacement(edgeId, sourceKeypointIndices, displacements);

        var displacedPoint = AddVectors(sourceKeypoint.position, displacement);

        var displacedPointIndices =
            GetStridedIndexNearPoint(displacedPoint, outputStride, height, width);

        var offsetPoint = GetOffsetPoint(
                displacedPointIndices.Y, displacedPointIndices.X, targetKeypointId,
                offsets);

        float score;
        if (!DNN)
        {
          score = scores[0,
                  displacedPointIndices.Y, displacedPointIndices.X, targetKeypointId];
        }
        else
        {
          score = scores[0,
                  targetKeypointId, displacedPointIndices.Y, displacedPointIndices.X];
        }

        var targetKeypoint =
            AddVectors(
                new System.Drawing.PointF(
                    x: displacedPointIndices.X * outputStride,
                    y: displacedPointIndices.Y * outputStride)
                , new System.Drawing.PointF(x: offsetPoint.X, y: offsetPoint.Y));

        return new Keypoint(score, targetKeypoint, partNames[targetKeypointId]);
    }

    Keypoint[] DecodePose(PartWithScore root, float[,,,] scores, float[,,,] offsets,
        int outputStride, float[,,,] displacementsFwd,
        float[,,,] displacementsBwd)
    {

        var numParts = 0;
        if (!DNN)
        {
          numParts = scores.GetLength(3);
        }
        else
        {
          numParts = scores.GetLength(1);
        }
        var numEdges = parentToChildEdges.Length;

        var instanceKeypoints = new Keypoint[numParts];

        // Start a new detection instance at the position of the root.
        var rootPart = root.part;
        var rootScore = root.score;
        var rootPoint = GetImageCoords(rootPart, outputStride, offsets);

        instanceKeypoints[rootPart.id] = new Keypoint(
            rootScore,
            rootPoint,
            partNames[rootPart.id]
        );

        // Decode the part positions upwards in the tree, following the backward
        // displacements.
        for (var edge = numEdges - 1; edge >= 0; --edge)
        {
            var sourceKeypointId = parentToChildEdges[edge];
            var targetKeypointId = childToParentEdges[edge];
            if (instanceKeypoints[sourceKeypointId].score > 0.0f &&
                instanceKeypoints[targetKeypointId].score == 0.0f)
            {
                instanceKeypoints[targetKeypointId] = TraverseToTargetKeypoint(
                    edge, instanceKeypoints[sourceKeypointId], targetKeypointId, scores,
                    offsets, outputStride, displacementsBwd);
            }
        }

        // Decode the part positions downwards in the tree, following the forward
        // displacements.
        for (var edge = 0; edge < numEdges; ++edge)
        {
            var sourceKeypointId = childToParentEdges[edge];
            var targetKeypointId = parentToChildEdges[edge];
            if (instanceKeypoints[sourceKeypointId].score > 0.0f &&
                instanceKeypoints[targetKeypointId].score == 0.0f)
            {
                instanceKeypoints[targetKeypointId] = TraverseToTargetKeypoint(
                    edge, instanceKeypoints[sourceKeypointId], targetKeypointId, scores,
                    offsets, outputStride, displacementsFwd);
            }
        }

        return instanceKeypoints;
    }
}