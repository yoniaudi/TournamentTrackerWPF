﻿namespace TrackerLibrary.Models
{
    public class PrizeModel
    {
        /// <summary>
        /// The unique identifier for the prize.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The numeric identifier for the place (2 or second place, etc.)
        /// </summary>
        public int PlaceNumber { get; set; }

        /// <summary>
        /// The friendly name for the place (second place, first runner up, etc.)
        /// </summary>
        public string PlaceName { get; set; }

        /// <summary>
        /// The fixed amount this place earns or zero if it is not used.
        /// </summary>
        public decimal PrizeAmount { get; set; }

        /// <summary>
        /// The number that represents the percentage of the overall take or
        /// zero if it is not used. The percentage is a fraction of 1 (so 0.5 for
        /// 50%).
        /// </summary>
        public double PrizePercentage { get; set; }

        public PrizeModel() { }
        public PrizeModel(string placeNumber, string placeName, string prizeAmount, string prizePercentage)
        {
            int.TryParse(placeNumber, out var placeNumberValue);
            decimal.TryParse(prizeAmount, out var prizeAmountValue);
            double.TryParse(prizePercentage, out var prizePercentageValue);

            PlaceNumber = placeNumberValue;
            PlaceName = placeName;
            PrizeAmount = prizeAmountValue;
            PrizePercentage = prizePercentageValue;
        }
    }
}
