using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Blazortastics.DB.Tables
{
    public class Ranking
    {
        public int Id { get; set; }
        public int Fieldsize { get; set; }
        public int? ShuffleGrade { get; set; }
        public string Name { get; set; }
        public TimeSpan Time { get; set; }
        public long Moves { get; set; }
        public DateTime Timestamp { get; set; }
        public string Comment { get; set; }

        #region NotMapped
        [NotMapped, JsonIgnore]
        public int Position { get; set; }
        [NotMapped, JsonIgnore]
        public string FormattedTime { get; set; }
        #endregion NotMapped
    }
}
