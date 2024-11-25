using System.Linq;
using System;

public partial class PoseNet
{
    System.Drawing.PointF GetOffsetPoint(int y, int x, int keypoint, float[,,,] offsets)
    {
        if (!DNN)
        {
          return new System.Drawing.PointF(
              offsets[0, y, x, keypoint + NUM_KEYPOINTS],
              offsets[0, y, x, keypoint]);
        }
          return new System.Drawing.PointF(
              offsets[0, keypoint + NUM_KEYPOINTS,y , x],
              offsets[0, keypoint, y, x]
        );
    }

    float SquaredDistance(
        float y1, float x1, float y2, float x2)
    {
        var dy = y2 - y1;
        var dx = x2 - x1;
        return dy * dy + dx * dx;
    }

    System.Drawing.PointF AddVectors(System.Drawing.PointF a, System.Drawing.PointF b)
    {
        return new System.Drawing.PointF(x: a.X + b.X, y: a.Y + b.Y);
    }

    System.Drawing.PointF GetImageCoords(
        Part part, int outputStride, float[,,,] offsets)
    {
        var vec = GetOffsetPoint(part.heatmapY, part.heatmapX,
                                 part.id, offsets);
        return new System.Drawing.PointF(
            (float)(part.heatmapX * outputStride) + vec.X,
            (float)(part.heatmapY * outputStride) + vec.Y
        );
    }

    public Tuple<Keypoint, Keypoint>[] GetAdjacentKeyPoints(
           Keypoint[] keypoints, float minConfidence)
    {

        return connectedPartIndices
            .Where(x => !EitherPointDoesntMeetConfidence(
                keypoints[x.Item1].score, keypoints[x.Item2].score, minConfidence))
           .Select(x => new Tuple<Keypoint, Keypoint>(keypoints[x.Item1], keypoints[x.Item2])).ToArray();

    }

    bool EitherPointDoesntMeetConfidence(
        float a, float b, float minConfidence)
    {
        return (a < minConfidence || b < minConfidence);
    }

    public static double mean(float[,,,] tensor)
    {
        double sum = 0f;
        var x = tensor.GetLength(1);
        var y = tensor.GetLength(2);
        var z = tensor.GetLength(3);
        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                for (int k = 0; k < z; k++)
                {
                    sum += tensor[0, i, j, k];
                }
            }
        }
        var mean = sum / (x * y * z);
        return mean;
    }

    //Pose ScalePose(Pose pose, int scale) {

    //    var s = (float)scale;

    //    return new Pose(
    //        pose.keypoints.Select( x => 
    //            new Keypoint( 
    //                x.score,
    //                new System.Drawing.PointF(x.position.x * s, x.position.y * s),
    //                x.part)
    //         ).ToArray(),
    //         pose.score
    //     );
    //}

    //Pose[] ScalePoses(Pose[] poses, int scale) {
    //    if (scale == 1) {
    //        return poses;
    //    }
    //    return poses.Select(x => ScalePose(pose: x, scale: scale)).ToArray();
    //}

    //int GetValidResolution(float imageScaleFactor,
    //                       int inputDimension,
    //                       int outputStride) {
    //    var evenResolution = (int)(inputDimension * imageScaleFactor) - 1;
    //    return evenResolution - (evenResolution % outputStride) + 1;
    //}

    //int Half(int k)
    //{
    //    return (int)Mathf.Floor((float)(k / 2));
    //}


}