﻿// This file was auto-generated by ML.NET Model Builder. 
using Microsoft.ML;
using Microsoft.ML.Data;
namespace BreganTwitchBot_Domain
{
    public partial class RankBeggar
    {
        /// <summary>
        /// model input class for RankBeggar.
        /// </summary>
        #region model input class
        public class ModelInput
        {
            [ColumnName(@"Message")]
            public string Message { get; set; }

            [ColumnName(@"AiResult")]
            public float AiResult { get; set; }

        }

        #endregion

        /// <summary>
        /// model output class for RankBeggar.
        /// </summary>
        #region model output class
        public class ModelOutput
        {
            [ColumnName(@"Message")]
            public float[] Message { get; set; }

            [ColumnName(@"AiResult")]
            public uint AiResult { get; set; }

            [ColumnName(@"Features")]
            public float[] Features { get; set; }

            [ColumnName(@"PredictedLabel")]
            public float PredictedLabel { get; set; }

            [ColumnName(@"Score")]
            public float[] Score { get; set; }

        }

        #endregion

        private static string MLNetModelPath = Path.GetFullPath("RankBeggar.zip");

        public static readonly Lazy<PredictionEngine<ModelInput, ModelOutput>> PredictEngine = new Lazy<PredictionEngine<ModelInput, ModelOutput>>(() => CreatePredictEngine(), true);

        /// <summary>
        /// Use this method to predict on <see cref="ModelInput"/>.
        /// </summary>
        /// <param name="input">model input.</param>
        /// <returns><seealso cref=" ModelOutput"/></returns>
        public static ModelOutput Predict(ModelInput input)
        {
            var predEngine = PredictEngine.Value;
            return predEngine.Predict(input);
        }

        private static PredictionEngine<ModelInput, ModelOutput> CreatePredictEngine()
        {
            var mlContext = new MLContext();
            ITransformer mlModel = mlContext.Model.Load(MLNetModelPath, out var _);
            return mlContext.Model.CreatePredictionEngine<ModelInput, ModelOutput>(mlModel);
        }
    }
}
