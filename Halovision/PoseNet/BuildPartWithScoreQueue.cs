public partial class PoseNet
{
    bool ScoreIsMaximumInLocalWindow(
    int keypointId, float score, int heatmapY, int heatmapX,
    int localMaximumRadius, float[,,,] scores)
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
        var localMaximum = true;
        var yStart = Mathf.Max(heatmapY - localMaximumRadius, 0);
        var yEnd = Mathf.Min(heatmapY + localMaximumRadius + 1, height);

        for (var yCurrent = yStart; yCurrent < yEnd; ++yCurrent)
        {
            var xStart = Mathf.Max(heatmapX - localMaximumRadius, 0);
            var xEnd = Mathf.Min(heatmapX + localMaximumRadius + 1, width);
            for (var xCurrent = xStart; xCurrent < xEnd; ++xCurrent)
            {
                if (!DNN)
                {
                   if (scores[0, yCurrent, xCurrent, keypointId] > score)
                   {
                       localMaximum = false;
                       break;
                   }
                }
                else
                {
                   if (scores[0, keypointId, yCurrent, xCurrent] > score)
                   {
                       localMaximum = false;
                       break;
                   }
                }
            }
            if (!localMaximum)
            {
                break;
            }
        }
        return localMaximum;
    }

    PriorityQueue<float, PartWithScore> BuildPartWithScoreQueue(
        float scoreThreshold, int localMaximumRadius,
        float[,,,] scores)
    {
        var queue = new PriorityQueue<float, PartWithScore>();

        var height = 0;
        var width = 0;
        var numKeypoints = 0;
        if (!DNN)
        {
            height = scores.GetLength(1);
            width = scores.GetLength(2);
            // change to get only first point (eye and nose)
            //numKeypoints = scores.GetLength(3);
            numKeypoints = 3;
        }
        else
        {
            height = scores.GetLength(2);
            width = scores.GetLength(3);
            // change to get only first point (eye and nose)
            //numKeypoints = scores.GetLength(1);
            numKeypoints = 3;
        }

        for (int heatmapY = 0; heatmapY < height; ++heatmapY)
        {
            for (int heatmapX = 0; heatmapX < width; ++heatmapX)
            {
                for (int keypointId = 0; keypointId < numKeypoints; ++keypointId)
                {
                    float score = 0;
                    if (!DNN)
                    {
                        score = scores[0, heatmapY, heatmapX, keypointId];
                    }
                    else
                    {
                        score = scores[0, keypointId, heatmapY, heatmapX];
                    }

                    // Only consider parts with score greater or equal to threshold as
                    // root candidates.
                    if (score < scoreThreshold)
                    {
                        continue;
                    }

                    // Only consider keypoints whose score is maximum in a local window.
                    if (ScoreIsMaximumInLocalWindow(
                            keypointId, score, heatmapY, heatmapX, localMaximumRadius,
                            scores))
                    {
                        queue.Push(score, new PartWithScore(score,
                            new Part(heatmapX, heatmapY, keypointId)
                        ));
                    }
                }
            }
        }

        return queue;
    }

}
